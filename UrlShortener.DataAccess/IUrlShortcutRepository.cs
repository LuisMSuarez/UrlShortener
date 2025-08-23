namespace UrlShortenerApi.DataAccess
{
    using UrlShortenerApi.DataAccess.Contracts;

    public interface IUrlShortcutRepository
    {
        Task<RepositoryUrlShortcut?> GetShortcutAsync(string shortcutId);
        Task<RepositoryUrlShortcut> CreateShortcutAsync(RepositoryUrlShortcut shortcut);
        Task<IEnumerable<RepositoryUrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
    }
}
