using HelloBuddy.Application.Media;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class ExerciseMediaKeyTests
{
    [Fact]
    public void TryResolve_SimpleFilename_ReturnsKey()
    {
        var ok = ExerciseMediaKey.TryResolve(
            "https://acct.blob.core.windows.net/exercise-media/images/baited_back_stretch.jpg",
            out var key);

        Assert.True(ok);
        Assert.Equal("exercise-media/images/baited_back_stretch.jpg", key);
    }

    [Fact]
    public void TryResolve_FilenameWithSpaces_ReturnsDecodedKey()
    {
        // Blob URLs percent-encode spaces in the path (%20); the blob name itself
        // has real spaces. TryResolve must return the DECODED key so downstream
        // Url.Action re-encoding produces %20 (not %2520) and blob lookups match.
        var ok = ExerciseMediaKey.TryResolve(
            "https://acct.blob.core.windows.net/exercise-media/images/Stood%20HL%20bicycle%20and%20DV%20PROM%20-%20da001329-74c0-4217-8514-c11507de42c1.jpg",
            out var key);

        Assert.True(ok);
        Assert.Equal(
            "exercise-media/images/Stood HL bicycle and DV PROM - da001329-74c0-4217-8514-c11507de42c1.jpg",
            key);
    }

    [Fact]
    public void TryResolve_NonManagedUrl_ReturnsFalse()
    {
        var ok = ExerciseMediaKey.TryResolve("https://drive.google.com/file/d/abc/view", out var key);

        Assert.False(ok);
        Assert.Equal(string.Empty, key);
    }

    [Fact]
    public void TryResolve_NullOrWhitespace_ReturnsFalse()
    {
        Assert.False(ExerciseMediaKey.TryResolve(null, out _));
        Assert.False(ExerciseMediaKey.TryResolve("   ", out _));
    }
}
