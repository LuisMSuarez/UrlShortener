namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Contracts;

    public interface IUrlShortcutService
    {
        Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut);
        Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut);
        Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
    }
}
