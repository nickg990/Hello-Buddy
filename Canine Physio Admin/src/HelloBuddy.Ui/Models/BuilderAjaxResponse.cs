namespace HelloBuddy.Ui.Models;

using System.Text.Json.Serialization;

public sealed record BuilderAjaxResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("error")] string? Error = null)
{
    public static BuilderAjaxResponse Success(string? message = null)
        => new(true, message, null);

    public static BuilderAjaxResponse Failure(string error)
        => new(false, null, error);
}
