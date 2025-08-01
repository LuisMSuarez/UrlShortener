namespace UrlShortenerApi.DataAccess
{
    using UrlShortenerApi.Common;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess.Contracts;

    public interface IUrlShortcutRepository
    {
        Task<Result<UrlShortcut>> GetUrlShortcutAsync(string shortcut);
    }
}
