namespace UrlShortenerApi.DataAccess
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
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

        public async Task<RepositoryUrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            try
            {
                var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
                database = await database.ReadAsync();
                var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
                var response = await container.ReadItemAsync<RepositoryUrlShortcut>(shortcut, new PartitionKey(shortcut));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut", ex);
            }
        }
    }
}
