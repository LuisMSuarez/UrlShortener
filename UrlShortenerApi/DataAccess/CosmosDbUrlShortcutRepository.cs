namespace UrlShortenerApi.DataAccess
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using System.Net;
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

        public async Task<RepositoryUrlShortcut> CreateUrlShortcutAsync(string url, string shortcut)
        {
            try
            {
                var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
                database = await database.ReadAsync();
                var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
                var response = await container.CreateItemAsync<CosmosDbUrlShortcut>(
                    new CosmosDbUrlShortcut
                    {
                        Id = shortcut,
                        Url = url,
                        PartitionKey = shortcut
                    },
                    partitionKey: new PartitionKey(shortcut));

                return ToRepositoryUrlShortcut(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new DataAccessException(DataAccessResultCode.Conflict, $"Shortcut with id {shortcut} already exists", ex);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut", ex);
            }
        }

        public async Task<RepositoryUrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            try
            {
                var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
                database = await database.ReadAsync();
                var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
                var response = await container.ReadItemAsync<CosmosDbUrlShortcut>(shortcut, new PartitionKey(shortcut));
                return ToRepositoryUrlShortcut(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new DataAccessException(DataAccessResultCode.NotFound, $"Shortcut with id {shortcut} is not found", ex);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut", ex);
            }
        }

        private static RepositoryUrlShortcut ToRepositoryUrlShortcut(CosmosDbUrlShortcut cosmosDbUrlShortcut)
        {
            return new RepositoryUrlShortcut
            {
                Id = cosmosDbUrlShortcut.Id,
                Url = cosmosDbUrlShortcut.Url
            };
        }
    }
}