namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Services.Contracts;

    public interface IUrlShortcutGenerationService
    {
        string GenerateUrlShortcutId(UrlShortcut urlShortcut);
    }
}
