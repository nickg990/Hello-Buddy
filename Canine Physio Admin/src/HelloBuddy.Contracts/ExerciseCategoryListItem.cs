namespace HelloBuddy.Contracts;

public sealed record ExerciseCategoryListItem(
    ulong ExerciseCategoryId,
    string CategoryName,
    bool IsActive);
