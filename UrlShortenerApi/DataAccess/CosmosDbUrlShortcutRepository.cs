namespace UrlShortenerApi.DataAccess
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using UrlShortenerApi.Common;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess.Contracts;

    public class CosmosDbUrlShortcutRepository : IUrlShortcutRepository
    {
        private readonly CosmosClient client;
        private readonly Configuration configuration;

        public CosmosDbUrlShortcutRepository(
            CosmosClient client,
            IOptions<Configuration> configurationOptions)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.configuration = configurationOptions?.Value ?? throw new ArgumentNullException(nameof(configurationOptions));
        }

        public async Task<Result<UrlShortcut>> GetUrlShortcutAsync(string shortcut)
        {
            Database database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
            database = await database.ReadAsync();
            Container container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
            var response = await container.ReadItemAsync<RepositoryUrlShortcut>(shortcut, new PartitionKey(shortcut));

            if (response == null || response.Resource == null)
            {
                return Result<UrlShortcut>.Failure($"No URL shortcut found for '{shortcut}'.");
            }

            return Result<UrlShortcut>.Success(
                new UrlShortcut
                {
                    Url = response.Resource.Url,
                    Shortcut = response.Resource.Id
                });
        }
    }
}
