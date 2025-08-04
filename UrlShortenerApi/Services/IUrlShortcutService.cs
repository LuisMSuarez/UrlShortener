using UrlShortenerApi.Contracts;

namespace UrlShortenerApi.Services
{
    public interface IUrlShortcutService
    {
        Task<UrlShortcut> GetUrlShortcutAsync(string shortcut);
        Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut);
    }
}
