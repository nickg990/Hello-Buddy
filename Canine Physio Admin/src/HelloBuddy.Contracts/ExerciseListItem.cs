namespace HelloBuddy.Contracts;

public sealed record ExerciseListItem(
    ulong ExerciseId,
    string ExerciseKey,
    string Title,
    ulong? ExerciseCategoryId,
    string? CategoryName,
    string? ObjectiveSummary,
    bool IsActive,
    bool HasImage,
    bool HasVideo,
    DateTime UpdatedDate);
