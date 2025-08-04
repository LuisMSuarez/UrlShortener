namespace UrlShortenerApi.Services
{
    using System.Security.Policy;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess;
    using UrlShortenerApi.DataAccess.Contracts;
    using UrlShortenerApi.Services.Contracts;

    public class UrlShortcutService : IUrlShortcutService
    {
        private readonly IUrlShortcutRepository urlShortcutRepository;
        private readonly IUrlShortcutGenerationService urlShortcutGenerationService;
        public UrlShortcutService(IUrlShortcutRepository urlShortcutRepository, IUrlShortcutGenerationService urlShortcutGenerationService)
        {
            this.urlShortcutRepository = urlShortcutRepository ?? throw new ArgumentNullException(nameof(urlShortcutRepository));
            this.urlShortcutGenerationService = urlShortcutGenerationService ?? throw new ArgumentNullException(nameof(urlShortcutGenerationService));
        }

        public async Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut.Url))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
            }

            try
            {
                var hashedShortcut = this.urlShortcutGenerationService.GenerateUrlShortcut(shortcut);
                var repositoryShortcut = await this.urlShortcutRepository.CreateUrlShortcutAsync(shortcut.Url, hashedShortcut);
                return ToServiceUrlShortcut(repositoryShortcut);
            }
            catch (DataAccessException ex) when (ex.ResultCode == DataAccessResultCode.Conflict)
            {
                throw new ServiceException(ServiceResultCode.Conflict, "A shortcut with the same key already exists.", ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while creating the URL shortcut for url {shortcut.Url}.", ex);
            }
        }

        public async Task<UrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
            }

            try
            {
                var repositoryShortcut = await this.urlShortcutRepository.GetUrlShortcutAsync(shortcut);
                if (repositoryShortcut == null)
                {
                    throw new NullReferenceException(nameof(repositoryShortcut));
                }

                return ToServiceUrlShortcut(repositoryShortcut);
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

        private static UrlShortcut ToServiceUrlShortcut(RepositoryUrlShortcut repositoryUrlShortcut)
        {
            return new UrlShortcut
            {
                Url = repositoryUrlShortcut.Url,
                Shortcut = repositoryUrlShortcut.Id
            };
        }
    }
}