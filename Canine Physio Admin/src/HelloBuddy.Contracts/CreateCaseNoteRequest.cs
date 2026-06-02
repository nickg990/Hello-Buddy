using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Contracts;

public sealed class CreateCaseNoteRequest
{
    [StringLength(50)]
    public string? NoteType { get; set; }

    [Required]
    public string NoteText { get; set; } = string.Empty;
}
