using DotNet.Testcontainers.Builders;
using MySqlConnector;
using Testcontainers.MySql;
using Xunit;

namespace HelloBuddy.Api.IntegrationTests;

/// <summary>
/// Spins up a MySQL 8 Testcontainer once per test class, applies the
/// Day-1 schema and MSc seed scripts from <c>Canine Physio Database</c>,
/// and exposes the resulting connection string for the <see cref="ApiIntegrationTests.Factory"/>.
/// </summary>
/// <remarks>
/// CR-001 finding #10: Section 12 of the coding standards mandates Testcontainers
/// MySQL for Infrastructure integration tests. The host-side fallback is preserved
/// for local dev convenience and CI environments where Docker isn't available.
/// </remarks>
public sealed class MySqlTestcontainerFixture : IAsyncLifetime
{
    private const string DatabaseName = "canine_physiotherapy";
    private const string Username = "test";
    private const string Password = "test-password!";

    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        // Match Windows dev behaviour where table names are case-insensitive; the
        // production schema uses PascalCase entity names that mix case in queries.
        .WithCommand("--lower-case-table-names=1")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // MySqlContainer.GetConnectionString returns a Pomelo-compatible string for the test user.
        ConnectionString = _container.GetConnectionString() + ";SslMode=None;AllowPublicKeyRetrieval=True";

        await ApplyScriptsAsync();
    }

    public async Task DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            await IntegrationTestDatabaseReset.ResetToSeedAsync(ConnectionString);
        }

        await _container.DisposeAsync();
    }

    private async Task ApplyScriptsAsync()
    {
        var scripts = LocateScripts();
        // The schema script issues DROP DATABASE / CREATE DATABASE, so it must run as root.
        // MySqlContainer sets MYSQL_ROOT_PASSWORD to the same password configured via WithPassword.
        var rootConnectionString =
            $"Server={_container.Hostname};Port={_container.GetMappedPublicPort(3306)};" +
            $"User=root;Password={Password};SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True";

        await using var connection = new MySqlConnection(rootConnectionString);
        await connection.OpenAsync();

        foreach (var path in scripts)
        {
            var sql = await File.ReadAllTextAsync(path);
            await using var cmd = new MySqlCommand(sql, connection)
            {
                CommandTimeout = 300
            };
            await cmd.ExecuteNonQueryAsync();
        }

        // After DROP/CREATE DATABASE the test user's GRANTs on the named DB persist in mysql.* tables.
        // Re-fetch the connection string for ApiTestcontainerIntegrationTests to use.
        ConnectionString = _container.GetConnectionString() + ";SslMode=None;AllowPublicKeyRetrieval=True";
    }

    private static IReadOnlyList<string> LocateScripts()
    {
        var root = FindRepoRoot();
        var dir = Path.Combine(root, "Canine Physio Database", "Build and Initialise");
        return
        [
            Path.Combine(dir, "Canine Physio DB Scripts v2.3 (fresh).sql"),
            Path.Combine(dir, "Canine Physio DB Day 1 Initialise v2.4.sql"),
            Path.Combine(dir, "Canine Physio DB MSc Assessment Seed v1.sql")
        ];
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "Canine Physio Database")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Could not locate 'Canine Physio Database' folder relative to test output directory.");
    }
}
