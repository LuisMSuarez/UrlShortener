using UrlShortenerApi.Contracts;
using UrlShortenerApi.DataAccess.Contracts;

namespace UrlShortenerApi.Services
{
    public interface IUrlShortcutService
    {
        Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut);
        Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut);
        Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
    }
}
