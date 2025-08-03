namespace UrlShortenerApi.Services
{
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess;
    using UrlShortenerApi.DataAccess.Contracts;
    using UrlShortenerApi.Services.Contracts;

    public class UrlShortcutService : IUrlShortcutService
    {
        private readonly IUrlShortcutRepository urlShortcutRepository;
        public UrlShortcutService(IUrlShortcutRepository urlShortcutRepository)
        {
            this.urlShortcutRepository = urlShortcutRepository ?? throw new ArgumentNullException(nameof(urlShortcutRepository));
        }

        public async Task<UrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            try
            {
                var repositoryShortcut = await this.urlShortcutRepository.GetUrlShortcutAsync(shortcut);
                if (repositoryShortcut == null)
                {
                    throw new NullReferenceException(nameof(repositoryShortcut));
                }

                return new UrlShortcut
                {
                    Url = repositoryShortcut.Url,
                    Shortcut = repositoryShortcut.Id
                };
            }
            catch (DataAccessException ex) when (ex.ResultCode == DataAccessResultCode.NotFound)
            {
                throw new ServiceException(ServiceResultCode.NotFound, $"Shortcut with id {shortcut} is not found.", ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while fetching the URL shortcut {shortcut}.", ex);
            }
        }
    }
}