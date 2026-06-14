using MySqlConnector;

namespace HelloBuddy.Api.IntegrationTests;

internal static class IntegrationTestDatabaseReset
{
    public static async Task ResetToSeedAsync(string appConnectionString, CancellationToken ct = default)
    {
        var resetConnection = Environment.GetEnvironmentVariable("HELLOBUDDY_TEST_DB_RESET_CONNECTION");
        if (string.IsNullOrWhiteSpace(resetConnection))
        {
            resetConnection = BuildAdminConnectionString(appConnectionString);
        }

        var scripts = LocateScripts();

        await using var connection = new MySqlConnection(resetConnection);
        await connection.OpenAsync(ct);

        foreach (var path in scripts)
        {
            var sql = await File.ReadAllTextAsync(path, ct);
            await using var cmd = new MySqlCommand(sql, connection)
            {
                CommandTimeout = 300
            };
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await IntegrationTestSchemaUpgrade.ApplyIncrement8CompatAsync(connection, ct);
    }

    private static string BuildAdminConnectionString(string appConnectionString)
    {
        var source = new MySqlConnectionStringBuilder(appConnectionString)
        {
            AllowPublicKeyRetrieval = true,
            AllowUserVariables = true,
            SslMode = MySqlSslMode.None,
            Database = string.Empty,
            UserID = "root"
        };

        return source.ConnectionString;
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
