using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HelloBuddy.Admin.Core.Data;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that stamps audit timestamps
/// (<c>CreatedDate</c> on insert, <c>UpdatedDate</c> on insert and update) for any
/// tracked entity that exposes those properties. Replaces ad-hoc timestamp
/// assignment in repositories/services (coding-standards.md §5).
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added)
            {
                TrySet(entry, "CreatedDate", now, overwriteIfDefault: true);
                TrySet(entry, "UpdatedDate", now, overwriteIfDefault: true);
            }
            else if (entry.State is EntityState.Modified)
            {
                TrySet(entry, "UpdatedDate", now, overwriteIfDefault: false);
            }
        }
    }

    private static void TrySet(EntityEntry entry, string propertyName, DateTime value, bool overwriteIfDefault)
    {
        var property = entry.Metadata.FindProperty(propertyName);
        if (property is null || property.ClrType != typeof(DateTime))
        {
            return;
        }

        var member = entry.Property(propertyName);
        if (overwriteIfDefault && member.CurrentValue is DateTime current && current != default)
        {
            return;
        }

        member.CurrentValue = value;
    }
}
