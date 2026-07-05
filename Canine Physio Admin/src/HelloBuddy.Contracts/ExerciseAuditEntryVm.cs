namespace HelloBuddy.Contracts;

/// <summary>
/// Read-model for a single Exercise audit history row, projected from
/// <c>AuditLog</c> rows where <c>EntityName = "Exercise"</c>.
/// </summary>
public sealed record ExerciseAuditEntryVm(
    ulong AuditLogId,
    string? Author,
    DateTime ActionDateTime,
    string ActionType,
    IReadOnlyList<ExerciseAuditEntryVm.FieldChangeVm> Changes)
{
    public sealed record FieldChangeVm(string FieldName, string? OldValue, string? NewValue);
}
