using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

/// <summary>
/// Persistence boundary for Exercise Library list/detail and edit operations.
/// </summary>
public interface IExerciseRepository
{
    Task<IReadOnlyList<ExerciseListItem>> ListAsync(ExerciseListFilter filter, CancellationToken ct);
    Task<ExerciseDetailVm?> GetAsync(ulong exerciseId, CancellationToken ct);
    Task<IReadOnlyList<ExerciseCategoryListItem>> ListCategoriesAsync(CancellationToken ct);
    Task<bool> CategoryExistsAsync(ulong exerciseCategoryId, CancellationToken ct);
    Task<ulong> CreateAsync(SaveExerciseRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(ulong exerciseId, SaveExerciseRequest request, CancellationToken ct);
    Task<bool> SetActiveAsync(ulong exerciseId, bool isActive, CancellationToken ct);
}
