namespace HelloBuddy.Ui.Models;

public sealed class MediaSearchOptions
{
    public List<MediaSearchProviderOption> VideoProviders { get; set; } = [];
}

public sealed class MediaSearchProviderOption
{
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
