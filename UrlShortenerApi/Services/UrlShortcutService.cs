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

        public async Task<UrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            var repositoryShortcut = await this.urlShortcutRepository.GetUrlShortcutAsync(shortcut);
            if (repositoryShortcut == null)
            {
                throw new NullReferenceException(nameof(repositoryShortcut));
            }

            return new UrlShortcut
            { 
                Url = repositoryShortcut.Url,
                Shortcut = repositoryShortcut.Id
            };
        }
    }
}
