using UrlShortenerApi.Contracts;

namespace UrlShortenerApi.Services
{
    public interface IUrlShortcutGenerationService
    {
        string GenerateUrlShortcut(UrlShortcut urlShortcut);
    }
}
