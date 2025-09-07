using UrlShortenerApi.Services.Contracts;

namespace UrlShortenerApi.Services.Tests;

public class Sha256UrlShortcutGenerationServiceTests
{
    private readonly Sha256UrlShortcutGenerationService _service = new();

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/page?id=123")]
    [InlineData("https://EXAMPLE.com")] // Case sensitivity
    public void GenerateUrlShortcutId_ShouldReturnFixedLengthBase62(string url)
    {
        // Arrange
        var shortcut = new UrlShortcut { Url = url };

        // Act
        var result = _service.GenerateUrlShortcutId(shortcut);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.True(result.Length <= 6);
    }

    [Fact]
    public void GenerateUrlShortcutId_ShouldReturnSameResultForSameInput()
    {
        // Arrange
        var shortcut = new UrlShortcut { Url = "https://example.com" };

        // Act
        var result1 = _service.GenerateUrlShortcutId(shortcut);
        var result2 = _service.GenerateUrlShortcutId(shortcut);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GenerateUrlShortcutId_ShouldReturnDifferentResultsForDifferentInputs()
    {
        // Arrange
        var shortcut1 = new UrlShortcut { Url = "https://example.com" };
        var shortcut2 = new UrlShortcut { Url = "https://example.org" };

        // Act
        var result1 = _service.GenerateUrlShortcutId(shortcut1);
        var result2 = _service.GenerateUrlShortcutId(shortcut2);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Theory]
    [MemberData(nameof(GetSimilarUrls))]
    public void GenerateUrlShortcutId_ShouldBeSensitiveToMinorChanges(string url1, string url2)
    {
        var shortcut1 = new UrlShortcut { Url = url1 };
        var shortcut2 = new UrlShortcut { Url = url2 };

        var result1 = _service.GenerateUrlShortcutId(shortcut1);
        var result2 = _service.GenerateUrlShortcutId(shortcut2);

        Assert.NotEqual(result1, result2);
    }

    public static IEnumerable<object[]> GetSimilarUrls()
    {
        yield return new object[] { "https://example.com", "https://example.com/" };
        yield return new object[] { "https://example.com", "https://example.com/page" };
        yield return new object[] { "https://example.com", "https://example.com?query=1" };
    }
}
