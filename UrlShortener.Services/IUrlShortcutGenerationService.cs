using UrlShortenerApi.Contracts;

namespace UrlShortenerApi.Services
{
    public interface IUrlShortcutGenerationService
    {
        string GenerateUrlShortcutId(UrlShortcut urlShortcut);
    }
}
