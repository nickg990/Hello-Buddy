namespace HelloBuddy.Ui.Services;

public sealed class ApiValidationException : Exception
{
    public ApiValidationException(IDictionary<string, string[]> errors)
        : base("The API rejected the request.")
    {
        Errors = new Dictionary<string, string[]>(errors, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}