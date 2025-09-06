using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using UrlShortenerApi.Services;
using UrlShortenerApi.Services.Contracts;
using UrlShortenerApi.DataAccess.Contracts;
using UrlShortenerApi.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace UrlShortenerApi.Services.Tests;

public class UrlShortcutServiceTests
{
    private readonly Mock<IUrlShortcutRepository> repositoryMock = new();
    private readonly Mock<IUrlShortcutGenerationService> generationServiceMock = new();
    private readonly Mock<ILogger<UrlShortcutService>> loggerMock = new();
    private readonly UrlShortcutService service;

    public UrlShortcutServiceTests()
    {
        service = new UrlShortcutService(repositoryMock.Object, generationServiceMock.Object, loggerMock.Object);
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
    public async Task GetUrlShortcutAsync_NullOrEmpty_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutAsync(null!));
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutAsync(" "));
    }

    [Fact]
    public async Task GetUrlShortcutAsync_NotFound_ReturnsNull()
    {
        repositoryMock.Setup(r => r.GetShortcutAsync("abc")).ReturnsAsync((RepositoryUrlShortcut?)null);
        var result = await service.GetUrlShortcutAsync("abc");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUrlShortcutAsync_Valid_ReturnsMappedShortcut()
    {
        var repoShortcut = new RepositoryUrlShortcut { Id = "abc", Url = "https://example.com" };
        repositoryMock.Setup(r => r.GetShortcutAsync("abc")).ReturnsAsync(repoShortcut);

        var result = await service.GetUrlShortcutAsync("abc");

        Assert.NotNull(result);
        Assert.Equal("abc", result!.Shortcut);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_EmptyInput_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<ServiceException>(() => service.GetUrlShortcutsByUrlAsync(""));
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_NoResults_ReturnsEmpty()
    {
        repositoryMock.Setup(r => r.GetUrlShortcutsByUrlAsync("https://example.com"))
                      .ReturnsAsync((IEnumerable<RepositoryUrlShortcut>)[]);

        var result = await service.GetUrlShortcutsByUrlAsync("https://example.com");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_Valid_ReturnsMappedShortcuts()
    {
        var repoShortcuts = new List<RepositoryUrlShortcut>
        {
            new RepositoryUrlShortcut { Id = "abc", Url = "https://example.com" },
            new RepositoryUrlShortcut { Id = "xyz", Url = "https://example.com" }
        };

        repositoryMock.Setup(r => r.GetUrlShortcutsByUrlAsync("https://example.com"))
                      .ReturnsAsync(repoShortcuts);

        var result = await service.GetUrlShortcutsByUrlAsync("https://example.com");

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateUrlShortcutAsync_ConflictResolved_ReturnsExistingShortcut()
    {
        var input = new UrlShortcut { Url = "https://example.com" };
        var generatedId = "abc";

        generationServiceMock.Setup(g => g.GenerateUrlShortcutId(It.IsAny<UrlShortcut>()))
                             .Returns(generatedId);

        repositoryMock.Setup(r => r.CreateShortcutAsync(It.IsAny<RepositoryUrlShortcut>()))
                      .ThrowsAsync(new DataAccessException(DataAccessResultCode.Conflict, "Conflict"));

        repositoryMock.Setup(r => r.GetShortcutAsync(generatedId))
                      .ReturnsAsync(new RepositoryUrlShortcut { Id = generatedId, Url = input.Url });

        var result = await service.CreateUrlShortcutAsync(input);

        Assert.Equal(generatedId, result.Shortcut);
        Assert.Equal(input.Url, result.Url);
    }

    [Fact]
    public async Task CreateUrlShortcutAsync_ConflictUnresolved_ThrowsAfterMaxRetries()
    {
        var input = new UrlShortcut { Url = "https://example.com" };
        var generatedId = "abc";

        generationServiceMock.Setup(g => g.GenerateUrlShortcutId(It.IsAny<UrlShortcut>()))
                             .Returns(generatedId);

        repositoryMock.Setup(r => r.CreateShortcutAsync(It.IsAny<RepositoryUrlShortcut>()))
                      .ThrowsAsync(new DataAccessException(DataAccessResultCode.Conflict, "Conflict"));

        repositoryMock.Setup(r => r.GetShortcutAsync(generatedId))
                      .ReturnsAsync(new RepositoryUrlShortcut { Id = generatedId, Url = "https://other.com" });

        await Assert.ThrowsAsync<ServiceException>(() => service.CreateUrlShortcutAsync(input));
    }
}
