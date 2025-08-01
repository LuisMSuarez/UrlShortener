using UrlShortenerApi.Contracts;

namespace UrlShortenerApi.DataAccess
{
    public interface IUrlShortcutRepository
    {
        Task<UrlShortcut> GetUrlShortcutAsync(string shortcut);
    }
}
