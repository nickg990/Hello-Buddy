using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Api.Services;

public sealed class ExerciseLibrarySeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExerciseLibrarySeedHostedService> _logger;

    public ExerciseLibrarySeedHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ExerciseLibrarySeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue("Seed:ExerciseLibrary:Enabled", false))
        {
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();

        if (string.Equals(db.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            await ApplySeedAsync(db, cancellationToken);
            _logger.LogInformation("Exercise Library seed applied.");
            return;
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        await ApplySeedAsync(db, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        _logger.LogInformation("Exercise Library seed applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task<Dictionary<string, ulong>> UpsertCategoriesAsync(CaninePhysioDbContext db, CancellationToken ct)
    {
        var output = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in GetCategorySeeds())
        {
            var entity = await db.Exercisecategories.FirstOrDefaultAsync(x => x.CategoryKey == category.CategoryKey, ct);
            if (entity is null)
            {
                entity = new Exercisecategory
                {
                    CategoryKey = category.CategoryKey
                };
                db.Exercisecategories.Add(entity);
            }

            entity.CategoryName = category.CategoryName;
            entity.Description = category.Description;
            entity.IsActive = true;

            await db.SaveChangesAsync(ct);
            output[category.CategoryKey] = entity.ExerciseCategoryId;
        }

        return output;
    }

    private static async Task ApplySeedAsync(CaninePhysioDbContext db, CancellationToken cancellationToken)
    {
        var categoryByKey = await UpsertCategoriesAsync(db, cancellationToken);

        foreach (var seed in GetExerciseSeeds())
        {
            var categoryId = categoryByKey[seed.CategoryKey];
            var exercise = await db.Exercises.FirstOrDefaultAsync(x => x.ExerciseKey == seed.ExerciseKey, cancellationToken);
            if (exercise is null)
            {
                exercise = new Exercise
                {
                    ExerciseKey = seed.ExerciseKey
                };
                db.Exercises.Add(exercise);
            }

            exercise.ExerciseCategoryId = categoryId;
            exercise.Title = seed.Title;
            exercise.ObjectiveSummary = seed.ObjectiveSummary;
            exercise.ImageUrl = seed.ImageUrl;
            exercise.VideoUrl = seed.VideoUrl;
            exercise.DefaultReps = seed.DefaultReps;
            exercise.DefaultSets = seed.DefaultSets;
            exercise.DefaultHoldSeconds = seed.DefaultHoldSeconds;
            exercise.IsActive = seed.IsActive;

            await db.SaveChangesAsync(cancellationToken);

            var existingSteps = await db.Exerciseinstructions
                .Where(x => x.ExerciseId == exercise.ExerciseId)
                .ToListAsync(cancellationToken);
            db.Exerciseinstructions.RemoveRange(existingSteps);

            for (ushort i = 0; i < seed.Instructions.Count; i++)
            {
                db.Exerciseinstructions.Add(new Exerciseinstruction
                {
                    ExerciseId = exercise.ExerciseId,
                    StepNumber = (ushort)(i + 1),
                    InstructionText = seed.Instructions[i]
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<(string CategoryKey, string CategoryName, string Description)> GetCategorySeeds()
    {
        return
        [
            ("strength", "Strength", "Strengthening exercises for controlled load."),
            ("mobility", "Mobility", "Range-of-motion and mobility work."),
            ("balance", "Balance", "Balance and proprioception progression."),
            ("functional", "Functional", "Everyday movement rehabilitation.")
        ];
    }

    private static IReadOnlyList<ExerciseSeed> GetExerciseSeeds()
    {
        return
        [
            new ExerciseSeed(
                "stepUp",
                "strength",
                "Step-ups (low)",
                "Controlled step-ups to build hind-limb strength.",
                "https://cdn.hellobuddy.example/exercises/step-up-low.jpg",
                "https://cdn.hellobuddy.example/exercises/step-up-low.mp4",
                5,
                3,
                null,
                true,
                [
                    "Lead your dog onto the low step with a treat.",
                    "Pause briefly in standing.",
                    "Guide a slow, controlled step back down."
                ]),
            new ExerciseSeed(
                "baitedBackStretch",
                "mobility",
                "Baited back stretch",
                "Encourage spinal flexion and extension with gentle lure control.",
                null,
                "https://cdn.hellobuddy.example/exercises/baited-back-stretch.mp4",
                3,
                1,
                7,
                true,
                [
                    "Guide the head toward each shoulder using a treat.",
                    "Guide toward each hip while keeping movement smooth.",
                    "Reward calm completion and stop if discomfort appears."
                ]),
            new ExerciseSeed(
                "sitToStand",
                "functional",
                "Sit-to-stand",
                "Functional hind-limb strengthening from a stable sit.",
                "https://cdn.hellobuddy.example/exercises/sit-to-stand.jpg",
                null,
                6,
                3,
                null,
                true,
                [
                    "Ask for a square sit position.",
                    "Lure to standing without jumping forward.",
                    "Return to sit with control and repeat."
                ]),
            new ExerciseSeed(
                "weightShiftStanding",
                "balance",
                "Weight shifts in standing",
                "Improve controlled weight transfer and postural stability.",
                null,
                "https://cdn.hellobuddy.example/exercises/weight-shifts.mp4",
                10,
                2,
                null,
                true,
                [
                    "Stand dog square on a non-slip surface.",
                    "Apply gentle lateral cue to encourage controlled shift.",
                    "Return to neutral and repeat both sides."
                ]),
            new ExerciseSeed(
                "cavalettiWalk",
                "functional",
                "Cavaletti walk",
                "Promote coordinated stepping and range control.",
                "https://cdn.hellobuddy.example/exercises/cavaletti.jpg",
                "https://cdn.hellobuddy.example/exercises/cavaletti.mp4",
                4,
                2,
                null,
                false,
                [
                    "Walk slowly over evenly spaced poles.",
                    "Keep pace controlled and avoid rushing.",
                    "Rest between sets as advised."
                ])
        ];
    }

    private sealed record ExerciseSeed(
        string ExerciseKey,
        string CategoryKey,
        string Title,
        string ObjectiveSummary,
        string? ImageUrl,
        string? VideoUrl,
        ushort? DefaultReps,
        ushort? DefaultSets,
        ushort? DefaultHoldSeconds,
        bool IsActive,
        IReadOnlyList<string> Instructions);
}
