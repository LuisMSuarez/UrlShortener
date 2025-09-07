using Microsoft.Extensions.Logging;
using Moq;
using UrlShortenerApi.Services.Contracts;
using UrlShortenerApi.Utils;

namespace UrlShortenerApi.Services.Tests;
public class CachedUrlShortcutServiceTests
{
    private readonly Mock<ILruCache<string, UrlShortcut>> cacheMock = new();
    private readonly Mock<IUrlShortcutService> innerServiceMock = new();
    private readonly Mock<ILogger<CachedUrlShortcutService>> loggerMock = new();
    private readonly CachedUrlShortcutService service;

    public CachedUrlShortcutServiceTests()
    {
        Func<string, IUrlShortcutService> factory = _ => innerServiceMock.Object;
        service = new CachedUrlShortcutService(cacheMock.Object, loggerMock.Object, factory);
    }

    [Fact]
    public async Task CreateUrlShortcutAsync_ValidInput_DelegatesToInnerService()
    {
        var shortcut = new UrlShortcut { Url = "https://example.com" };
        innerServiceMock.Setup(s => s.CreateUrlShortcutAsync(shortcut)).ReturnsAsync(shortcut);

        var result = await service.CreateUrlShortcutAsync(shortcut);

        Assert.Equal(shortcut, result);
        innerServiceMock.Verify(s => s.CreateUrlShortcutAsync(shortcut), Times.Once);
    }

    [Fact]
    public async Task CreateUrlShortcutAsync_NullInput_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<ServiceException>(() => service.CreateUrlShortcutAsync(null!));
    }

    [Fact]
    public async Task CreateUrlShortcutAsync_EmptyUrl_ThrowsBadRequest()
    {
        var shortcut = new UrlShortcut { Url = "" };
        await Assert.ThrowsAsync<ServiceException>(() => service.CreateUrlShortcutAsync(shortcut));
    }

    [Fact]
    public async Task GetUrlShortcutAsync_CacheHit_ReturnsCachedValue()
    {
        var cached = new UrlShortcut { Url = "https://cached.com", Shortcut = "abc" };
        cacheMock.Setup(c => c.Get("abc")).Returns(cached);

        var result = await service.GetUrlShortcutAsync("abc");

        Assert.Equal(cached, result);
        innerServiceMock.Verify(s => s.GetUrlShortcutAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUrlShortcutAsync_CacheMiss_DelegatesAndCachesResult()
    {
        var shortcut = new UrlShortcut { Url = "https://example.com", Shortcut = "abc" };
        cacheMock.Setup(c => c.Get("abc")).Returns((UrlShortcut?)null);
        innerServiceMock.Setup(s => s.GetUrlShortcutAsync("abc")).ReturnsAsync(shortcut);

        var result = await service.GetUrlShortcutAsync("abc");

        Assert.Equal(shortcut, result);
        cacheMock.Verify(c => c.Set("abc", shortcut, TimeSpan.FromDays(1)), Times.Once);
    }

    [Fact]
    public async Task GetUrlShortcutAsync_NullOrWhitespace_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutAsync(null!));
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutAsync(" "));
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_ValidInput_DelegatesToInnerService()
    {
        var url = "https://example.com";
        var shortcuts = new List<UrlShortcut> { new UrlShortcut { Url = url, Shortcut = "abc" } };

        innerServiceMock.Setup(s => s.GetUrlShortcutsByUrlAsync(url)).ReturnsAsync(shortcuts);

        var result = await service.GetUrlShortcutsByUrlAsync(url);

        Assert.Equal(shortcuts, result);
        innerServiceMock.Verify(s => s.GetUrlShortcutsByUrlAsync(url), Times.Once);
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_EmptyInput_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutsByUrlAsync(""));
    }

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CachedUrlShortcutService(null!, loggerMock.Object, _ => innerServiceMock.Object));

        Assert.Throws<ArgumentNullException>(() =>
            new CachedUrlShortcutService(cacheMock.Object, null!, _ => innerServiceMock.Object));

        Assert.Throws<ArgumentNullException>(() =>
            new CachedUrlShortcutService(cacheMock.Object, loggerMock.Object, null!));
    }
}
