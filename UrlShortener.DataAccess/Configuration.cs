namespace UrlShortenerApi.DataAccess
{
    /// <summary>
    /// Represents the root configuration object for data access settings.
    /// Bound from application settings and injected via <see cref="Microsoft.Extensions.Options.IOptions{T}"/>.
    /// </summary>
    public record Configuration
    {
        /// <summary>
        /// Gets the Azure Cosmos DB configuration section.
        /// </summary>
        public required AzureCosmosDB AzureCosmosDB { get; init; }
    }

    /// <summary>
    /// Contains Azure Cosmos DB connection and container details.
    /// Used to initialize the Cosmos DB client and access the correct database and container.
    /// </summary>
    public record AzureCosmosDB
    {
        /// <summary>
        /// Gets the endpoint URI for the Cosmos DB account.
        /// </summary>
        public required string Endpoint { get; init; }

        /// <summary>
        /// Gets the connection string used to authenticate with Cosmos DB.
        /// </summary>
        public required string ConnectionString { get; init; }

        /// <summary>
        /// Gets the name of the Cosmos DB database to use.
        /// </summary>
        public required string DatabaseName { get; init; }

        /// <summary>
        /// Gets the name of the container within the database where URL shortcuts are stored.
        /// </summary>
        public required string ContainerName { get; init; }
    }
}
