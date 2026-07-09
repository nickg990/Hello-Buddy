using HelloBuddy.Admin.Pdf;
using HelloBuddy.Contracts;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HelloBuddy.Api.InMemoryTests;

/// <summary>
/// Unit tests for PDF template rendering and contract validation.
/// Tests are grouped by bug-fix story (PDF-S1, PDF-S2).
/// </summary>
public sealed class PdfTemplateTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static ProgrammeVm MinimalVm(IReadOnlyList<ProgrammeVm.SessionRow>? sessions = null) =>
        new(
            ProgrammeId: 1,
            TreatmentCaseId: 1,
            PetId: 1,
            OwnerId: 1,
            ProgrammeName: "Test Programme",
            Status: "active",
            StartDate: new DateOnly(2026, 1, 1),
            EndDate: null,
            Notes: null,
            CaseTitle: "Test Case",
            PetName: "Buddy",
            OwnerName: "Test Owner",
            PractitionerName: "Dr. Test",
            Sessions: sessions ?? Array.Empty<ProgrammeVm.SessionRow>());

    private static ProgrammeVm.SessionRow SessionWithExercise(string period, ProgrammeVm.SessionExerciseRow exercise) =>
        new(1, period, null, "active", 1, new[] { exercise });

    private static ProgrammeVm.SessionExerciseRow ExerciseRow(
        ushort? reps = 10,
        ushort? sets = 3,
        ushort? holdSeconds = null,
        string? notes = null) =>
        new(
            SessionExerciseId: 1,
            ExerciseId: 1,
            ExerciseTitle: "Sit-to-stand",
            ObjectiveSummary: null,
            ImageUrl: null,
            VideoUrl: null,
            Reps: reps,
            Sets: sets,
            HoldSeconds: holdSeconds,
            SortOrder: 1,
            Notes: notes,
            Instructions: Array.Empty<ProgrammeVm.InstructionStep>());

    private static Task<string> RenderAsync(ProgrammeVm vm)
    {
        var template = new RazorProgrammePdfTemplate();
        return template.RenderAsync(vm);
    }

    // ── PDF-S2: Period capitalisation ──────────────────────────────────────

    [Theory]
    [InlineData("single", "Single")]
    [InlineData("AM", "AM")]
    [InlineData("PM", "PM")]
    public async Task PeriodHeader_DisplaysCasedCorrectly(string storedPeriod, string expectedDisplay)
    {
        var vm = MinimalVm(new[] { new ProgrammeVm.SessionRow(1, storedPeriod, null, "active", 1, Array.Empty<ProgrammeVm.SessionExerciseRow>()) });
        var html = await RenderAsync(vm);
        Assert.Contains($"<h2>{expectedDisplay}</h2>", html, StringComparison.Ordinal);
    }

    // ── PDF-S1: Exercise notes on prescription line ────────────────────────

    [Fact]
    public async Task PrescriptionLine_WithNote_RendersNoteAfterHoldSegment()
    {
        var ex = ExerciseRow(reps: 10, sets: 3, holdSeconds: 5, notes: "Keep weight even");
        var vm = MinimalVm(new[] { SessionWithExercise("single", ex) });
        var html = await RenderAsync(vm);
        // Note must appear after the hold segment on the same prescription paragraph
        Assert.Contains("5s hold &middot; Keep weight even", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrescriptionLine_WithNote_NoHold_RendersNoteAfterSets()
    {
        var ex = ExerciseRow(reps: 10, sets: 3, holdSeconds: null, notes: "Short note");
        var vm = MinimalVm(new[] { SessionWithExercise("single", ex) });
        var html = await RenderAsync(vm);
        // Note appended after sets when no hold
        Assert.Contains("3 sets &middot; Short note", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrescriptionLine_WithoutNote_NoTrailingMiddot()
    {
        var ex = ExerciseRow(reps: 10, sets: 3, holdSeconds: null, notes: null);
        var vm = MinimalVm(new[] { SessionWithExercise("single", ex) });
        var html = await RenderAsync(vm);
        // Prescription line must not have a trailing middot or whitespace-only tail
        Assert.Contains("3 sets</p>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrescriptionLine_NoteExceeds60Chars_TruncatedWithEllipsis()
    {
        var longNote = new string('x', 70); // 70 chars — 10 over the limit
        var ex = ExerciseRow(reps: 10, sets: 3, holdSeconds: null, notes: longNote);
        var vm = MinimalVm(new[] { SessionWithExercise("single", ex) });
        var html = await RenderAsync(vm);
        var truncated = new string('x', 60) + "\u2026"; // 60 x's + ellipsis
        Assert.Contains(truncated, html, StringComparison.Ordinal);
        Assert.DoesNotContain(longNote, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrescriptionLine_NoteExactly60Chars_RenderedWithoutTruncation()
    {
        var note60 = new string('y', 60);
        var ex = ExerciseRow(reps: 5, sets: 2, holdSeconds: null, notes: note60);
        var vm = MinimalVm(new[] { SessionWithExercise("single", ex) });
        var html = await RenderAsync(vm);
        Assert.Contains(note60, html, StringComparison.Ordinal);
        // Must NOT have an ellipsis appended
        Assert.DoesNotContain(note60 + "\u2026", html, StringComparison.Ordinal);
    }

    // ── PDF-S1: [StringLength(60)] model validation ────────────────────────

    [Fact]
    public void SessionExerciseEdit_NoteWithin60Chars_PassesValidation()
    {
        var edit = new ProgrammeBuilderForm.SessionExerciseEdit { Notes = new string('a', 60) };
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(edit, new ValidationContext(edit), results, validateAllProperties: true);
        Assert.True(valid, $"Expected valid but got: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    }

    [Fact]
    public void SessionExerciseEdit_NoteExceeds60Chars_FailsValidation()
    {
        var edit = new ProgrammeBuilderForm.SessionExerciseEdit { Notes = new string('a', 61) };
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(edit, new ValidationContext(edit), results, validateAllProperties: true);
        Assert.False(valid, "Expected validation failure for note exceeding 60 chars");
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProgrammeBuilderForm.SessionExerciseEdit.Notes)));
    }
}
