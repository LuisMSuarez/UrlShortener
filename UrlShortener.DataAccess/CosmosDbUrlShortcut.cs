using Newtonsoft.Json;

namespace UrlShortenerApi.DataAccess;

/// <summary>
/// Represents a URL shortcut entity stored in Azure Cosmos DB.
/// Uses <c>Newtonsoft.Json</c> attributes for compatibility with Cosmos DB's serialization.
/// </summary>
internal class CosmosDbUrlShortcut
{
    /// <summary>
    /// Gets or sets the unique identifier for the shortcut.
    /// This value is also used as the document ID in Cosmos DB.
    /// </summary>
    [JsonProperty("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the partition key used for Cosmos DB storage.
    /// Typically matches the shortcut ID to ensure efficient lookup.
    /// </summary>
    [JsonProperty("partitionKey")]
    public required string PartitionKey { get; set; }

    /// <summary>
    /// Gets or sets the original URL that the shortcut maps to.
    /// </summary>
    [JsonProperty("url")]
    public required string Url { get; set; }
}
