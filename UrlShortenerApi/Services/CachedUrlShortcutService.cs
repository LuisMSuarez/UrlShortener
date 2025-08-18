namespace UrlShortenerApi.Services
{
    using System.ClientModel;
    using UrlShortenerApi.Contracts;
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

        public CachedUrlShortcutService(
            ILruCache<string, UrlShortcut> shortcutCache,
            Func<string, IUrlShortcutService> shortcutServiceFactory)
        {
            this.shortcutCache = shortcutCache ?? throw new ArgumentNullException(nameof(shortcutCache));
            if (shortcutServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(shortcutServiceFactory));
            }

            this.innerService = shortcutServiceFactory("Base") ?? throw new ArgumentNullException(nameof(shortcutServiceFactory));
        }

        public Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
        {
            throw new NotImplementedException();
        }

        public Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}
