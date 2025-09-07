using Microsoft.Extensions.Logging;
using UrlShortenerApi.Services.Contracts;
using UrlShortenerApi.Utils;

namespace UrlShortenerApi.Services;

/// <summary>
/// A decorator for <see cref="IUrlShortcutService"/> that adds caching behavior using an LRU cache.
/// Caching is applied selectively based on business context, allowing flexible extension without modifying the core service.
/// </summary>
public class CachedUrlShortcutService : IUrlShortcutService
{
    private readonly ILruCache<string, UrlShortcut> shortcutCache;
    private readonly IUrlShortcutService innerService;
    private readonly ILogger<CachedUrlShortcutService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedUrlShortcutService"/> class.
    /// </summary>
    /// <param name="shortcutCache">The LRU cache used to store shortcut lookups.</param>
    /// <param name="logger">Logger for diagnostics and cache events.</param>
    /// <param name="shortcutServiceFactory">Factory function to resolve the underlying shortcut service.</param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    public CachedUrlShortcutService(
        ILruCache<string, UrlShortcut> shortcutCache,
        ILogger<CachedUrlShortcutService> logger,
        Func<string, IUrlShortcutService> shortcutServiceFactory)
    {
        this.shortcutCache = shortcutCache ?? throw new ArgumentNullException(nameof(shortcutCache));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (shortcutServiceFactory == null)
        {
            throw new ArgumentNullException(nameof(shortcutServiceFactory));
        }

        this.innerService = shortcutServiceFactory("Base") ?? throw new ArgumentNullException(nameof(shortcutServiceFactory));
    }

    /// <summary>
    /// Creates a new shortcut for the specified URL.
    /// Delegates creation to the inner service; caching is not applied at creation time.
    /// </summary>
    /// <param name="shortcut">The shortcut request containing the original URL.</param>
    /// <returns>The created <see cref="UrlShortcut"/>.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid.</exception>
    public async Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
    {
        if (shortcut == null || string.IsNullOrWhiteSpace(shortcut.Url))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
        }

        return await this.innerService.CreateUrlShortcutAsync(shortcut);
    }

    /// <summary>
    /// Retrieves a shortcut by its identifier.
    /// Uses cache if available; otherwise delegates to the inner service and stores the result.
    /// </summary>
    /// <param name="shortcut">The shortcut ID.</param>
    /// <returns>The corresponding <see cref="UrlShortcut"/>, or null if not found.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid.</exception>
    public async Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
        }

        var cachedShortcut = this.shortcutCache.Get(shortcut);
        if (cachedShortcut != null)
        {
            this.logger.LogInformation("Cache hit for GetUrlShortcutAsync with shortcut: {shortcut}", shortcut);
            return await Task.FromResult(cachedShortcut);
        }

        var result = await this.innerService.GetUrlShortcutAsync(shortcut);
        if (result != null)
        {
            this.shortcutCache.Set(shortcut, result, TimeSpan.FromDays(1));
        }

        this.logger.LogInformation("Cache miss for GetUrlShortcutAsync with shortcut: {shortcut}", shortcut);
        return result;
    }

    /// <summary>
    /// Retrieves all shortcuts associated with a given URL.
    /// Delegates to the inner service; caching is not applied for reverse lookups.
    /// </summary>
    /// <param name="url">The original URL to search for.</param>
    /// <returns>A collection of matching <see cref="UrlShortcut"/> instances.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid.</exception>
    public async Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Url cannot be null or empty.");
        }

        return await this.innerService.GetUrlShortcutsByUrlAsync(url);
    }
}
