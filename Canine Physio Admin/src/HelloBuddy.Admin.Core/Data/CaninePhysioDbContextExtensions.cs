using HelloBuddy.Admin.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Admin.Core.Data;

public partial class CaninePhysioDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // -------------------------------------------------------------------
        // AppSetting — admin-managed key/value settings (Release 2 R2-S3)
        // -------------------------------------------------------------------
        modelBuilder.Entity<Appsetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PRIMARY");
            entity.ToTable("appsetting");
            entity.Property(e => e.SettingKey).HasMaxLength(255);
            entity.Property(e => e.SettingValue).HasColumnType("text");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        // -------------------------------------------------------------------
        // PractitionerLogin — credentials table (Increment 8)
        // -------------------------------------------------------------------
        modelBuilder.Entity<Practitionerlogin>(entity =>
        {
            entity.HasKey(e => e.PractitionerId).HasName("PRIMARY");
            entity.ToTable("practitionerlogin");

            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'physiotherapist'")
                .HasColumnType("enum('physiotherapist','administrator')");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.MustChangePassword)
                .HasDefaultValueSql("'0'");
            entity.Property(e => e.FailedAttemptCount)
                .HasDefaultValueSql("'0'");
            entity.Property(e => e.LockedUntil).HasColumnType("datetime");
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Practitioner)
                .WithOne(p => p.PractitionerLogin)
                .HasForeignKey<Practitionerlogin>(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PractitionerLogin_Practitioner");
        });

        // -------------------------------------------------------------------
        // Attribution columns (Increment 8) — all nullable, not FK-constrained
        // so historical records survive practitioner renames / soft-deletes.
        // -------------------------------------------------------------------
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
            entity.Property(e => e.UpdatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
            entity.Property(e => e.UpdatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Treatmentcase>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
            entity.Property(e => e.UpdatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Treatmentcasenote>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Programme>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
            entity.Property(e => e.UpdatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
            entity.Property(e => e.UpdatedByPractitionerName).HasMaxLength(255);
        });

        modelBuilder.Entity<Programmeversion>(entity =>
        {
            entity.Property(e => e.CreatedByPractitionerName).HasMaxLength(255);
        });

    }
}
