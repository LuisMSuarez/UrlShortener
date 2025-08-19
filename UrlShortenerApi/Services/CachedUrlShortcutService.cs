namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.Services.Contracts;
    using UrlShortenerApi.Utils;

    // Technical note 1: Deliberately caching at the service layer
    // This allows to vary caching based on business rules, user roles, or request context.
    // This compares to caching at the provider layer, where the data would need to be universally cacheable
    // not depending on business context.
    // Technical note 2: Using decorator pattern to allow for easy extension or replacement of caching logic
    // The decorator adds caching behavior while delegating core logic to the original service.
    // It allows allows dynamically adding behavior or responsibilities to individual objects without modifying
    // their original code or affecting other instances of the same class.
    public class CachedUrlShortcutService : IUrlShortcutService
    {
        private readonly ILruCache<string, UrlShortcut> shortcutCache;
        private readonly IUrlShortcutService innerService;
        private readonly ILogger<CachedUrlShortcutService> logger;  

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

        public async Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
        {
            if (shortcut == null || string.IsNullOrWhiteSpace(shortcut.Url))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
            }

            // Note: In the case of create, we defer to the inner service for all logic.
            // We do not cache the result here, instead rely on the getter to cache the shortcut.
            return await this.innerService.CreateUrlShortcutAsync(shortcut);
        }

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

        public async Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
        {
            // Reverse lookup is a less common scenario
            // We choose not to optimize this and instead call the inner service
            // This can always be changed later by adding a cache for reverse lookup.

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Url cannot be null or empty.");
            }

            return await this.innerService.GetUrlShortcutsByUrlAsync(url);
        }
    }
}
