namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Services.Contracts;

    public interface IUrlShortcutService
    {
        Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut);
        Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut);
        Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
    }
}
