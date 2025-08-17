namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Contracts;

    // Technical note: Deliberately caching at the service layer
    // This allows to vary caching based on business rules, user roles, or request context.
    // Instead of at the provider layer, where the data would need to be universally cacheable
    // not depending on business context.
    public class CachedUrlShortcutService : IUrlShortcutService
    {
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
