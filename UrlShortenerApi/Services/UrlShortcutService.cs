using UrlShortenerApi.Contracts;
using UrlShortenerApi.DataAccess;

namespace UrlShortenerApi.Services
{
    public class UrlShortcutService : IUrlShortcutService
    {
        private readonly IUrlShortcutRepository urlShortcutRepository;
        public UrlShortcutService(IUrlShortcutRepository urlShortcutRepository) 
        {
            this.urlShortcutRepository = urlShortcutRepository ?? throw new ArgumentNullException(nameof(urlShortcutRepository));
        }

        public Task<UrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            return this.urlShortcutRepository.GetUrlShortcutAsync(shortcut);
        }
    }
}
