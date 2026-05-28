namespace HelloBuddy.Contracts;

public sealed record PublishResponse(string BlobUri, string FileName, long Bytes);
