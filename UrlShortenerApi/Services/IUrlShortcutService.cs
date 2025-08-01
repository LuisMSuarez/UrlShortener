namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Common;
    using UrlShortenerApi.Contracts;

    public interface IUrlShortcutService
    {
        Task<Result<UrlShortcut>> GetUrlShortcutAsync(string shortcut);
    }
}
