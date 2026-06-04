namespace HelloBuddy.Contracts;

public sealed class ExerciseListFilter
{
    public string? SearchText { get; set; }
    public ulong? CategoryId { get; set; }
    public bool? HasVideo { get; set; }
    public bool ActiveOnly { get; set; } = true;
}
