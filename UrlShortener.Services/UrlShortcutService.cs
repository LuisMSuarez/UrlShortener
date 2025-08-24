namespace UrlShortenerApi.Services
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess;
    using UrlShortenerApi.DataAccess.Contracts;
    using UrlShortenerApi.Services.Contracts;

    public class UrlShortcutService : IUrlShortcutService
    {
        private readonly IUrlShortcutRepository urlShortcutRepository;
        private readonly IUrlShortcutGenerationService urlShortcutGenerationService;
        private readonly ILogger<UrlShortcutService> logger;
        private const int MaxRetriesConflictResolution = 5;

        public UrlShortcutService(
            IUrlShortcutRepository urlShortcutRepository,
            IUrlShortcutGenerationService urlShortcutGenerationService,
            ILogger<UrlShortcutService> logger
            )
        {
            this.urlShortcutRepository = urlShortcutRepository ?? throw new ArgumentNullException(nameof(urlShortcutRepository));
            this.urlShortcutGenerationService = urlShortcutGenerationService ?? throw new ArgumentNullException(nameof(urlShortcutGenerationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
        {
            if (shortcut == null ||  string.IsNullOrWhiteSpace(shortcut.Url))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
            }

            return await this.RetriableShortcutCreation(MaxRetriesConflictResolution, string.Empty, shortcut);
        }

        public async Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
            }

            try
            {
                var repositoryShortcut = await this.urlShortcutRepository.GetShortcutAsync(shortcut);
                if (repositoryShortcut == null)
                {
                    return null;
                }

                return ToServiceUrlShortcut(repositoryShortcut);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while fetching the URL shortcut {shortcut}.", ex);
            }
        }

        public async Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ServiceException(ServiceResultCode.BadRequest, "Url cannot be null or empty.");
            }

            var normalizedUrl = url.ToLowerInvariant();
            try
            {
                var shortcuts = await this.urlShortcutRepository.GetUrlShortcutsByUrlAsync(normalizedUrl);
                if (shortcuts == null)
                {
                    return [];
                }
                return shortcuts.Select(repoShortcut => ToServiceUrlShortcut(repoShortcut));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while fetching the URL shortcut by Url {url}.", ex);
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

        private async Task<UrlShortcut> RetriableShortcutCreation(int retriesLeft, string previousHash, UrlShortcut shortcut)
        {
            if (retriesLeft == 0)
            {
                throw new ServiceException(ServiceResultCode.Conflict, $"Could not resolve conflict in creation of shortcut with URL {shortcut.Url}.");
            }
            
            // We retry the call to generate a url by prepending the first 3 characters from the previous hash (salt) to the URL
            var salt = retriesLeft == MaxRetriesConflictResolution ?
                string.Empty :
                previousHash.Substring(0, Math.Min(3, previousHash.Length));

            var newShortcutId = this.urlShortcutGenerationService.GenerateUrlShortcutId(
                new UrlShortcut
                {
                    Url = string.Concat(salt, shortcut.Url)
                });
            try
            {
                var createdShortcut= await this.urlShortcutRepository.CreateShortcutAsync(
                    new RepositoryUrlShortcut 
                    {
                        Id = newShortcutId,
                        Url = shortcut.Url
                    });
                return ToServiceUrlShortcut(createdShortcut);
            }
            catch (DataAccessException ex) when (ex.ResultCode == DataAccessResultCode.Conflict)
            {
                // If a conflict occurs, it means the shortcut with the same ID already exists
                // There are 2 possible scenarios:
                // 1. The URL also matches.  In this case, we don't attempt to resolve the conflict and return the existing shortcut.
                // 2. The URL is different (collision).  In this case, we attempt to resolve the collision

                logger.Log(LogLevel.Warning, $"RetryShortcutCreationConflict: A shortcut with the same key {newShortcutId} already exists. Retries left {retriesLeft}");
                
                // TODO: handle case where below line throws not found exception
                var conflictingShortcut = await this.urlShortcutRepository.GetShortcutAsync(newShortcutId);
                if (conflictingShortcut != null && string.Equals(conflictingShortcut.Url, shortcut.Url, StringComparison.OrdinalIgnoreCase))
                {
                    // If the URL also, matches, we return the existing shortcut
                    // Returning a conflict to the caller would not be helpful in this case
                    // as the caller would not be able to know the shortcut for the url
                    return ToServiceUrlShortcut(conflictingShortcut);
                }
                return await RetriableShortcutCreation(retriesLeft - 1, newShortcutId, shortcut);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ServiceResultCode.InternalServerError, $"RetryShortcutCreationConflict: An unexpected error occurred while creating the URL shortcut {shortcut}.", ex);
            }
        }
    }
}