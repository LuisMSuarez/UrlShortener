using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerApi.Controllers;
using UrlShortenerApi.Services.Contracts;
using UrlShortenerApi.Services;

public class UrlControllerTests
{
    private readonly Mock<IUrlShortcutService> mockService;
    private readonly UrlsController controller;

    public UrlControllerTests()
    {
        mockService = new Mock<IUrlShortcutService>();
        controller = new UrlsController(mockService.Object);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsRedirect()
    {
        var shortcut = new UrlShortcut { Shortcut = "xyz", Url = "https://example.com" };
        mockService.Setup(s => s.GetUrlShortcutAsync("xyz")).ReturnsAsync(shortcut);

        var result = await controller.Get("xyz");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://example.com", redirect.Url);
    }

    [Fact]
    public async Task Get_WithNullOrWhitespaceId_ReturnsBadRequest()
    {
        var result = await controller.Get("   ");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("   ", badRequest.Value);
    }

    [Fact]
    public async Task Get_WithMissingShortcut_ReturnsNotFound()
    {
        mockService.Setup(s => s.GetUrlShortcutAsync("abc")).ReturnsAsync((UrlShortcut)null);

        var result = await controller.Get("abc");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("abc", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Get_WhenServiceThrowsInternalError_Returns500()
    {
        mockService.Setup(s => s.GetUrlShortcutAsync("fail"))
            .ThrowsAsync(new ServiceException(ServiceResultCode.InternalServerError, "Internal error"));

        var result = await controller.Get("fail");

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task RedirectByShortcut_DelegatesToGet()
    {
        var shortcut = new UrlShortcut { Shortcut = "xyz", Url = "https://example.com" };
        mockService.Setup(s => s.GetUrlShortcutAsync("xyz")).ReturnsAsync(shortcut);

        var result = await controller.RedirectByShortcut("xyz");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://example.com", redirect.Url);
    }

    [Fact]
    public async Task GetByUrl_WithValidUrl_ReturnsOk()
    {
        var shortcuts = new List<UrlShortcut> { new UrlShortcut { Shortcut = "abc", Url = "https://example.com" } };
        mockService.Setup(s => s.GetUrlShortcutsByUrlAsync("https://example.com")).ReturnsAsync(shortcuts);

        var result = await controller.GetByUrl("https://example.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(shortcuts, ok.Value);
    }

    [Fact]
    public async Task GetByUrl_WithNullOrWhitespace_ReturnsBadRequest()
    {
        var result = await controller.GetByUrl(" ");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(" ", badRequest.Value);
    }

    [Fact]
    public async Task GetByUrl_WhenServiceThrowsBadRequest_ReturnsBadRequest()
    {
        mockService.Setup(s => s.GetUrlShortcutsByUrlAsync("bad"))
            .ThrowsAsync(new ServiceException(ServiceResultCode.BadRequest, "Invalid URL"));

        var result = await controller.GetByUrl("bad");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid URL", badRequest.Value);
    }

    [Fact]
    public async Task Post_WithValidShortcut_ReturnsCreated()
    {
        var input = new UrlShortcut { Shortcut = "new", Url = "https://new.com" };
        mockService.Setup(s => s.CreateUrlShortcutAsync(input)).ReturnsAsync(input);

        var result = await controller.Post(input);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal("new", created.Location);
        Assert.Equal(input, created.Value);
    }

    [Fact]
    public async Task Post_WhenServiceThrows_Returns500()
    {
        var input = new UrlShortcut { Shortcut = "fail", Url = "https://fail.com" };
        mockService.Setup(s => s.CreateUrlShortcutAsync(input)).ThrowsAsync(new Exception());

        var result = await controller.Post(input);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task Put_WithMismatchedId_ReturnsBadRequest()
    {
        var shortcut = new UrlShortcut { Shortcut = "abc", Url = "https://abc.com" };
        var result = await controller.Put("xyz", shortcut);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Put_AlwaysThrowsNotImplemented()
    {
        var shortcut = new UrlShortcut { Shortcut = "xyz", Url = "https://xyz.com" };
        await Assert.ThrowsAsync<NotImplementedException>(() => controller.Put("xyz", shortcut));
    }

    [Fact]
    public async Task Delete_AlwaysThrowsNotImplemented()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => controller.Delete("xyz"));
    }
}
