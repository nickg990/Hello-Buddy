using MySqlConnector;

namespace HelloBuddy.Api.IntegrationTests;

internal static class IntegrationTestSchemaUpgrade
{
    public static async Task ApplyIncrement8CompatAsync(MySqlConnection connection, CancellationToken ct = default)
    {
        await EnsurePractitionerLoginTableAsync(connection, ct);

        await EnsureAttributionColumnsAsync(connection, "Owner", ct);
        await EnsureAttributionColumnsAsync(connection, "Pet", ct);
        await EnsureAttributionColumnsAsync(connection, "TreatmentCase", ct);
        await EnsureAttributionColumnsAsync(connection, "TreatmentCaseNote", ct);
        await EnsureAttributionColumnsAsync(connection, "Programme", ct);
        await EnsureAttributionColumnsAsync(connection, "Exercise", ct);
        await EnsureAttributionColumnsAsync(connection, "ProgrammeVersion", ct);
    }

    private static async Task EnsurePractitionerLoginTableAsync(MySqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS PractitionerLogin (
    PractitionerId BIGINT UNSIGNED NOT NULL,
    PasswordHash VARCHAR(512) NOT NULL,
    Role ENUM('physiotherapist','administrator') NOT NULL DEFAULT 'physiotherapist',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    MustChangePassword TINYINT(1) NOT NULL DEFAULT 0,
    FailedAttemptCount INT NOT NULL DEFAULT 0,
    LockedUntil DATETIME NULL,
    LastLoginDate DATETIME NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (PractitionerId),
    CONSTRAINT FK_PractitionerLogin_Practitioner_PractitionerId
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE CASCADE
);";

        await using var cmd = new MySqlCommand(sql, connection) { CommandTimeout = 300 };
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task EnsureAttributionColumnsAsync(MySqlConnection connection, string tableName, CancellationToken ct)
    {
        await EnsureColumnAsync(connection, tableName, "CreatedByPractitionerId", "BIGINT UNSIGNED NULL", ct);
        await EnsureColumnAsync(connection, tableName, "CreatedByPractitionerName", "VARCHAR(255) NULL", ct);
        await EnsureColumnAsync(connection, tableName, "UpdatedByPractitionerId", "BIGINT UNSIGNED NULL", ct);
        await EnsureColumnAsync(connection, tableName, "UpdatedByPractitionerName", "VARCHAR(255) NULL", ct);
    }

    private static async Task EnsureColumnAsync(
        MySqlConnection connection,
        string tableName,
        string columnName,
        string columnDefinition,
        CancellationToken ct)
    {
        const string existsSql = @"
SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND LOWER(TABLE_NAME) = LOWER(@tableName)
  AND LOWER(COLUMN_NAME) = LOWER(@columnName);";

        await using var existsCmd = new MySqlCommand(existsSql, connection);
        existsCmd.Parameters.AddWithValue("@tableName", tableName);
        existsCmd.Parameters.AddWithValue("@columnName", columnName);

        var exists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync(ct)) > 0;
        if (exists)
        {
            return;
        }

        var alterSql = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
        await using var alterCmd = new MySqlCommand(alterSql, connection) { CommandTimeout = 300 };
        await alterCmd.ExecuteNonQueryAsync(ct);
    }
}
