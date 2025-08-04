namespace UrlShortenerApi.DataAccess
{
    using UrlShortenerApi.DataAccess.Contracts;

    public interface IUrlShortcutRepository
    {
        Task<RepositoryUrlShortcut> GetUrlShortcutAsync(string shortcut);
        Task<RepositoryUrlShortcut> CreateUrlShortcutAsync(string url, string shortcut);
    }
}
