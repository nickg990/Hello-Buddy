using System;
using System.Collections.Generic;
using HelloBuddy.Admin.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Admin.Core.Data;

public partial class CaninePhysioDbContext : DbContext
{
    public CaninePhysioDbContext(DbContextOptions<CaninePhysioDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appcontentblock> Appcontentblocks { get; set; }

    public virtual DbSet<Appsetting> Appsettings { get; set; }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Exercisecategory> Exercisecategories { get; set; }

    public virtual DbSet<Exercisecompletion> Exercisecompletions { get; set; }

    public virtual DbSet<Exerciseinstruction> Exerciseinstructions { get; set; }

    public virtual DbSet<Notificationpreference> Notificationpreferences { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Passwordresetrequest> Passwordresetrequests { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<Practitioner> Practitioners { get; set; }

    public virtual DbSet<Practitionerlogin> Practitionerlogins { get; set; }

    public virtual DbSet<PractitionerPet> PractitionerPets { get; set; }

    public virtual DbSet<Programme> Programmes { get; set; }

    public virtual DbSet<Programmetemplate> Programmetemplates { get; set; }

    public virtual DbSet<Programmeversion> Programmeversions { get; set; }

    public virtual DbSet<Registrationcode> Registrationcodes { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Sessioncontenttype> Sessioncontenttypes { get; set; }

    public virtual DbSet<Sessionexercise> Sessionexercises { get; set; }

    public virtual DbSet<Sessionoccurrence> Sessionoccurrences { get; set; }

    public virtual DbSet<Sessionskip> Sessionskips { get; set; }

    public virtual DbSet<Sessionskipreason> Sessionskipreasons { get; set; }

    public virtual DbSet<Termsacceptance> Termsacceptances { get; set; }

    public virtual DbSet<Termsdocument> Termsdocuments { get; set; }

    public virtual DbSet<Treatmentcase> Treatmentcases { get; set; }

    public virtual DbSet<Treatmentcasenote> Treatmentcasenotes { get; set; }

    public virtual DbSet<Useraccount> Useraccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Appcontentblock>(entity =>
        {
            entity.HasKey(e => e.AppContentBlockId).HasName("PRIMARY");

            entity.ToTable("appcontentblock");

            entity.HasIndex(e => e.LinkedTermsDocumentId, "FK_AppContentBlock_TermsDocument");

            entity.HasIndex(e => new { e.ContentGroup, e.ContentKey }, "UQ_AppContentBlock_Group_Key").IsUnique();

            entity.Property(e => e.ContentGroup).HasColumnType("enum('information','termsConditions','warnings')");
            entity.Property(e => e.ContentKey).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.HeaderText).HasMaxLength(255);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("'1'");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.LinkedTermsDocument).WithMany(p => p.Appcontentblocks)
                .HasForeignKey(d => d.LinkedTermsDocumentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AppContentBlock_TermsDocument");
        });

        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PRIMARY");

            entity.ToTable("auditlog");

            entity.HasIndex(e => e.PractitionerId, "FK_AuditLog_Practitioner");

            entity.HasIndex(e => e.UserAccountId, "FK_AuditLog_UserAccount");

            entity.Property(e => e.ActionDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.NewValuesJson).HasColumnType("json");
            entity.Property(e => e.OldValuesJson).HasColumnType("json");

            entity.HasOne(d => d.Practitioner).WithMany(p => p.Auditlogs)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLog_Practitioner");

            entity.HasOne(d => d.UserAccount).WithMany(p => p.Auditlogs)
                .HasForeignKey(d => d.UserAccountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLog_UserAccount");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("PRIMARY");

            entity.ToTable("exercise");

            entity.HasIndex(e => e.ExerciseCategoryId, "FK_Exercise_ExerciseCategory");

            entity.HasIndex(e => e.ExerciseKey, "UQ_Exercise_ExerciseKey").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ExerciseKey).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.ObjectiveSummary).HasColumnType("text");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.VideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.ExerciseCategory).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.ExerciseCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Exercise_ExerciseCategory");
        });

        modelBuilder.Entity<Exercisecategory>(entity =>
        {
            entity.HasKey(e => e.ExerciseCategoryId).HasName("PRIMARY");

            entity.ToTable("exercisecategory");

            entity.HasIndex(e => e.CategoryKey, "UQ_ExerciseCategory_CategoryKey").IsUnique();

            entity.HasIndex(e => e.CategoryName, "UQ_ExerciseCategory_CategoryName").IsUnique();

            entity.Property(e => e.CategoryKey).HasMaxLength(100);
            entity.Property(e => e.CategoryName).HasMaxLength(150);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Exercisecompletion>(entity =>
        {
            entity.HasKey(e => e.ExerciseCompletionId).HasName("PRIMARY");

            entity.ToTable("exercisecompletion");

            entity.HasIndex(e => e.SessionExerciseId, "FK_ExerciseCompletion_SessionExercise");

            entity.HasIndex(e => new { e.SessionOccurrenceId, e.ExerciseKeySnapshot }, "UQ_ExerciseCompletion_Occurrence_ExerciseKey").IsUnique();

            entity.Property(e => e.Comments).HasColumnType("text");
            entity.Property(e => e.CompletedDateTime).HasColumnType("datetime");
            entity.Property(e => e.CompletionStatus)
                .HasDefaultValueSql("'completed'")
                .HasColumnType("enum('completed','partial','not_done')");
            entity.Property(e => e.DeviceRecordedDateTime).HasColumnType("datetime");
            entity.Property(e => e.ExerciseKeySnapshot).HasMaxLength(100);
            entity.Property(e => e.ExerciseTitleSnapshot).HasMaxLength(255);
            entity.Property(e => e.SyncedDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.SessionExercise).WithMany(p => p.Exercisecompletions)
                .HasForeignKey(d => d.SessionExerciseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ExerciseCompletion_SessionExercise");

            entity.HasOne(d => d.SessionOccurrence).WithMany(p => p.Exercisecompletions)
                .HasForeignKey(d => d.SessionOccurrenceId)
                .HasConstraintName("FK_ExerciseCompletion_SessionOccurrence");
        });

        modelBuilder.Entity<Exerciseinstruction>(entity =>
        {
            entity.HasKey(e => e.ExerciseInstructionId).HasName("PRIMARY");

            entity.ToTable("exerciseinstruction");

            entity.HasIndex(e => new { e.ExerciseId, e.StepNumber }, "UQ_ExerciseInstruction_Exercise_Step").IsUnique();

            entity.Property(e => e.InstructionText).HasColumnType("text");

            entity.HasOne(d => d.Exercise).WithMany(p => p.Exerciseinstructions)
                .HasForeignKey(d => d.ExerciseId)
                .HasConstraintName("FK_ExerciseInstruction_Exercise");
        });

        modelBuilder.Entity<Notificationpreference>(entity =>
        {
            entity.HasKey(e => e.NotificationPreferenceId).HasName("PRIMARY");

            entity.ToTable("notificationpreference");

            entity.HasIndex(e => e.UserAccountId, "UQ_NotificationPreference_UserAccountId").IsUnique();

            entity.Property(e => e.NotificationTime).HasColumnType("time");
            entity.Property(e => e.NotificationsEnabled)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.UserAccount).WithOne(p => p.Notificationpreference)
                .HasForeignKey<Notificationpreference>(d => d.UserAccountId)
                .HasConstraintName("FK_NotificationPreference_UserAccount");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PRIMARY");

            entity.ToTable("owner");

            entity.HasIndex(e => e.Email, "UQ_Owner_Email").IsUnique();

            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.Postcode).HasMaxLength(20);
            entity.Property(e => e.Town).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Passwordresetrequest>(entity =>
        {
            entity.HasKey(e => e.PasswordResetRequestId).HasName("PRIMARY");

            entity.ToTable("passwordresetrequest");

            entity.HasIndex(e => e.UserAccountId, "FK_PasswordResetRequest_UserAccount");

            entity.HasIndex(e => e.ResetToken, "UQ_PasswordResetRequest_ResetToken").IsUnique();

            entity.Property(e => e.ConsumedDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.RequestedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'requested'")
                .HasColumnType("enum('requested','used','expired','cancelled')");

            entity.HasOne(d => d.UserAccount).WithMany(p => p.Passwordresetrequests)
                .HasForeignKey(d => d.UserAccountId)
                .HasConstraintName("FK_PasswordResetRequest_UserAccount");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PRIMARY");

            entity.ToTable("pet");

            entity.HasIndex(e => e.IsActive, "IX_Pet_IsActive");

            entity.HasIndex(e => e.OwnerId, "IX_Pet_OwnerId");

            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Sex)
                .HasDefaultValueSql("'unknown'")
                .HasColumnType("enum('male','female','unknown')");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Weight).HasPrecision(5, 2);

            entity.HasOne(d => d.Owner).WithMany(p => p.Pets)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pet_Owner");
        });

        modelBuilder.Entity<Practitioner>(entity =>
        {
            entity.HasKey(e => e.PractitionerId).HasName("PRIMARY");

            entity.ToTable("practitioner");

            entity.HasIndex(e => e.Email, "UQ_Practitioner_Email").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<PractitionerPet>(entity =>
        {
            entity.HasKey(e => e.PractitionerPetId).HasName("PRIMARY");

            entity.ToTable("practitioner_pet");

            entity.HasIndex(e => e.PetId, "FK_Practitioner_Pet_Pet");

            entity.HasIndex(e => e.PractitionerId, "FK_Practitioner_Pet_Practitioner");

            entity.Property(e => e.AssignedFrom)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.AssignedTo).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.ReferralSource).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('planned','active','ended','suspended')");

            entity.HasOne(d => d.Pet).WithMany(p => p.PractitionerPets)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Practitioner_Pet_Pet");

            entity.HasOne(d => d.Practitioner).WithMany(p => p.PractitionerPets)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Practitioner_Pet_Practitioner");
        });

        modelBuilder.Entity<Programme>(entity =>
        {
            entity.HasKey(e => e.ProgrammeId).HasName("PRIMARY");

            entity.ToTable("programme");

            entity.HasIndex(e => e.CurrentProgrammeVersionId, "FK_Programme_CurrentProgrammeVersion");

            entity.HasIndex(e => e.ProgrammeTemplateId, "FK_Programme_ProgrammeTemplate");

            entity.HasIndex(e => e.TreatmentCaseId, "FK_Programme_TreatmentCase");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsCurrent)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.ProgrammeName).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'planned'")
                .HasColumnType("enum('planned','active','completed','cancelled')");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CurrentProgrammeVersion).WithMany(p => p.Programmes)
                .HasForeignKey(d => d.CurrentProgrammeVersionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Programme_CurrentProgrammeVersion");

            entity.HasOne(d => d.ProgrammeTemplate).WithMany(p => p.Programmes)
                .HasForeignKey(d => d.ProgrammeTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Programme_ProgrammeTemplate");

            entity.HasOne(d => d.TreatmentCase).WithMany(p => p.Programmes)
                .HasForeignKey(d => d.TreatmentCaseId)
                .HasConstraintName("FK_Programme_TreatmentCase");
        });

        modelBuilder.Entity<Programmetemplate>(entity =>
        {
            entity.HasKey(e => e.ProgrammeTemplateId).HasName("PRIMARY");

            entity.ToTable("programmetemplate");

            entity.HasIndex(e => e.PractitionerId, "FK_ProgrammeTemplate_Practitioner");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.TemplateName).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Practitioner).WithMany(p => p.Programmetemplates)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgrammeTemplate_Practitioner");
        });

        modelBuilder.Entity<Programmeversion>(entity =>
        {
            entity.HasKey(e => e.ProgrammeVersionId).HasName("PRIMARY");

            entity.ToTable("programmeversion");

            entity.HasIndex(e => e.CreatedByPractitionerId, "FK_ProgrammeVersion_Practitioner");

            entity.HasIndex(e => new { e.ProgrammeId, e.VersionNumber }, "UQ_ProgrammeVersion_Programme_Version").IsUnique();

            entity.Property(e => e.ChangeSummary).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.PayloadJson).HasColumnType("json");
            entity.Property(e => e.PayloadSchemaVersion)
                .HasMaxLength(50)
                .HasDefaultValueSql("'1.0'");
            entity.Property(e => e.PublishedDate).HasColumnType("datetime");
            entity.Property(e => e.RetiredDate).HasColumnType("datetime");
            entity.Property(e => e.SupersededDate).HasColumnType("datetime");
            entity.Property(e => e.VersionStatus)
                .HasDefaultValueSql("'draft'")
                .HasColumnType("enum('draft','published','superseded','retired')");

            entity.HasOne(d => d.CreatedByPractitioner).WithMany(p => p.Programmeversions)
                .HasForeignKey(d => d.CreatedByPractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgrammeVersion_Practitioner");

            entity.HasOne(d => d.Programme).WithMany(p => p.Programmeversions)
                .HasForeignKey(d => d.ProgrammeId)
                .HasConstraintName("FK_ProgrammeVersion_Programme");
        });

        modelBuilder.Entity<Registrationcode>(entity =>
        {
            entity.HasKey(e => e.RegistrationCodeId).HasName("PRIMARY");

            entity.ToTable("registrationcode");

            entity.HasIndex(e => e.PetId, "FK_RegistrationCode_Pet");

            entity.HasIndex(e => e.PractitionerId, "FK_RegistrationCode_Practitioner");

            entity.HasIndex(e => e.Code, "UQ_RegistrationCode_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IssuedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'issued'")
                .HasColumnType("enum('issued','used','expired','cancelled')");
            entity.Property(e => e.UsedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Pet).WithMany(p => p.Registrationcodes)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_RegistrationCode_Pet");

            entity.HasOne(d => d.Practitioner).WithMany(p => p.Registrationcodes)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistrationCode_Practitioner");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity.ToTable("session");

            entity.HasIndex(e => e.SessionContentTypeId, "FK_Session_SessionContentType");

            entity.HasIndex(e => new { e.ProgrammeId, e.Period }, "UQ_Session_Programme_Period").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Objective).HasColumnType("text");
            entity.Property(e => e.Period).HasColumnType("enum('single','AM','PM')");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("'1'");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'planned'")
                .HasColumnType("enum('planned','active','completed','cancelled')");

            entity.HasOne(d => d.Programme).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.ProgrammeId)
                .HasConstraintName("FK_Session_Programme");

            entity.HasOne(d => d.SessionContentType).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.SessionContentTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Session_SessionContentType");
        });

        modelBuilder.Entity<Sessioncontenttype>(entity =>
        {
            entity.HasKey(e => e.SessionContentTypeId).HasName("PRIMARY");

            entity.ToTable("sessioncontenttype");

            entity.HasIndex(e => e.ContentKey, "UQ_SessionContentType_ContentKey").IsUnique();

            entity.HasIndex(e => e.DisplayName, "UQ_SessionContentType_DisplayName").IsUnique();

            entity.Property(e => e.ContentKey).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(150);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.MobileDescription).HasColumnType("text");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Sessionexercise>(entity =>
        {
            entity.HasKey(e => e.SessionExerciseId).HasName("PRIMARY");

            entity.ToTable("sessionexercise");

            entity.HasIndex(e => e.ExerciseId, "FK_SessionExercise_Exercise");

            entity.HasIndex(e => new { e.SessionId, e.ExerciseId }, "UQ_SessionExercise_Session_Exercise").IsUnique();

            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("'1'");

            entity.HasOne(d => d.Exercise).WithMany(p => p.Sessionexercises)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionExercise_Exercise");

            entity.HasOne(d => d.Session).WithMany(p => p.Sessionexercises)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK_SessionExercise_Session");
        });

        modelBuilder.Entity<Sessionoccurrence>(entity =>
        {
            entity.HasKey(e => e.SessionOccurrenceId).HasName("PRIMARY");

            entity.ToTable("sessionoccurrence");

            entity.HasIndex(e => e.ProgrammeVersionId, "FK_SessionOccurrence_ProgrammeVersion");

            entity.HasIndex(e => e.SessionId, "FK_SessionOccurrence_Session");

            entity.HasIndex(e => new { e.PetId, e.ProgrammeVersionId, e.ScheduledDate, e.Period }, "UQ_SessionOccurrence_Pet_ProgrammeVersion_Date_Period").IsUnique();

            entity.Property(e => e.Comments).HasColumnType("text");
            entity.Property(e => e.CompletedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DeviceRecordedDateTime).HasColumnType("datetime");
            entity.Property(e => e.Period).HasColumnType("enum('single','AM','PM')");
            entity.Property(e => e.SkippedDateTime).HasColumnType("datetime");
            entity.Property(e => e.StartedDateTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'planned'")
                .HasColumnType("enum('planned','active','completed','skipped','cancelled')");
            entity.Property(e => e.SyncedDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Pet).WithMany(p => p.Sessionoccurrences)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK_SessionOccurrence_Pet");

            entity.HasOne(d => d.ProgrammeVersion).WithMany(p => p.Sessionoccurrences)
                .HasForeignKey(d => d.ProgrammeVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionOccurrence_ProgrammeVersion");

            entity.HasOne(d => d.Session).WithMany(p => p.Sessionoccurrences)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_SessionOccurrence_Session");
        });

        modelBuilder.Entity<Sessionskip>(entity =>
        {
            entity.HasKey(e => e.SessionSkipId).HasName("PRIMARY");

            entity.ToTable("sessionskip");

            entity.HasIndex(e => e.SessionSkipReasonId, "FK_SessionSkip_SessionSkipReason");

            entity.HasIndex(e => e.SessionOccurrenceId, "UQ_SessionSkip_SessionOccurrenceId").IsUnique();

            entity.Property(e => e.Comments).HasColumnType("text");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.SessionOccurrence).WithOne(p => p.Sessionskip)
                .HasForeignKey<Sessionskip>(d => d.SessionOccurrenceId)
                .HasConstraintName("FK_SessionSkip_SessionOccurrence");

            entity.HasOne(d => d.SessionSkipReason).WithMany(p => p.Sessionskips)
                .HasForeignKey(d => d.SessionSkipReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionSkip_SessionSkipReason");
        });

        modelBuilder.Entity<Sessionskipreason>(entity =>
        {
            entity.HasKey(e => e.SessionSkipReasonId).HasName("PRIMARY");

            entity.ToTable("sessionskipreason");

            entity.HasIndex(e => e.ReasonName, "UQ_SessionSkipReason_ReasonName").IsUnique();

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.ReasonName).HasMaxLength(100);
        });

        modelBuilder.Entity<Termsacceptance>(entity =>
        {
            entity.HasKey(e => e.TermsAcceptanceId).HasName("PRIMARY");

            entity.ToTable("termsacceptance");

            entity.HasIndex(e => e.TermsDocumentId, "FK_TermsAcceptance_TermsDocument");

            entity.HasIndex(e => e.UserAccountId, "FK_TermsAcceptance_UserAccount");

            entity.Property(e => e.AcceptanceMethod).HasMaxLength(50);
            entity.Property(e => e.AcceptedDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.AcceptedVersionText).HasMaxLength(100);

            entity.HasOne(d => d.TermsDocument).WithMany(p => p.Termsacceptances)
                .HasForeignKey(d => d.TermsDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TermsAcceptance_TermsDocument");

            entity.HasOne(d => d.UserAccount).WithMany(p => p.Termsacceptances)
                .HasForeignKey(d => d.UserAccountId)
                .HasConstraintName("FK_TermsAcceptance_UserAccount");
        });

        modelBuilder.Entity<Termsdocument>(entity =>
        {
            entity.HasKey(e => e.TermsDocumentId).HasName("PRIMARY");

            entity.ToTable("termsdocument");

            entity.HasIndex(e => new { e.DocumentType, e.VersionNumber }, "UQ_TermsDocument_Type_Version").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentType).HasColumnType("enum('terms_of_service','privacy_policy','acceptable_use')");
            entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");
            entity.Property(e => e.EffectiveTo).HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.VersionNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<Treatmentcase>(entity =>
        {
            entity.HasKey(e => e.TreatmentCaseId).HasName("PRIMARY");

            entity.ToTable("treatmentcase");

            entity.HasIndex(e => e.PetId, "FK_TreatmentCase_Pet");

            entity.HasIndex(e => e.PractitionerId, "FK_TreatmentCase_Practitioner");

            entity.Property(e => e.CaseTitle).HasMaxLength(255);
            entity.Property(e => e.ClinicalSummary).HasColumnType("text");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'planned'")
                .HasColumnType("enum('planned','active','completed','cancelled')");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Pet).WithMany(p => p.Treatmentcases)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TreatmentCase_Pet");

            entity.HasOne(d => d.Practitioner).WithMany(p => p.Treatmentcases)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TreatmentCase_Practitioner");
        });

        modelBuilder.Entity<Treatmentcasenote>(entity =>
        {
            entity.HasKey(e => e.TreatmentCaseNoteId).HasName("PRIMARY");

            entity.ToTable("treatmentcasenote");

            entity.HasIndex(e => e.PractitionerId, "FK_TreatmentCaseNote_Practitioner");

            entity.HasIndex(e => e.TreatmentCaseId, "FK_TreatmentCaseNote_TreatmentCase");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.NoteText).HasColumnType("text");
            entity.Property(e => e.NoteType).HasMaxLength(50);

            entity.HasOne(d => d.Practitioner).WithMany(p => p.Treatmentcasenotes)
                .HasForeignKey(d => d.PractitionerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TreatmentCaseNote_Practitioner");

            entity.HasOne(d => d.TreatmentCase).WithMany(p => p.Treatmentcasenotes)
                .HasForeignKey(d => d.TreatmentCaseId)
                .HasConstraintName("FK_TreatmentCaseNote_TreatmentCase");
        });

        modelBuilder.Entity<Useraccount>(entity =>
        {
            entity.HasKey(e => e.UserAccountId).HasName("PRIMARY");

            entity.ToTable("useraccount");

            entity.HasIndex(e => e.OwnerId, "IX_UserAccount_OwnerId");

            entity.HasIndex(e => e.Email, "UQ_UserAccount_Email").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Owner).WithMany(p => p.Useraccounts)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAccount_Owner");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
