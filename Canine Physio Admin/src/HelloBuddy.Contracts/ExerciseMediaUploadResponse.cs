namespace HelloBuddy.Contracts;

public sealed record ExerciseMediaUploadResponse(
    string Url,
    string FileName,
    string ContentType,
    long Bytes);