using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using UrlShortenerApi.DataAccess.Contracts;

namespace UrlShortenerApi.DataAccess.Tests;
public class CosmosDbUrlShortcutRepositoryTests
{
    private readonly Mock<CosmosClient> mockClient = new();
    private readonly Mock<Database> mockDatabase = new();
    private readonly Mock<Microsoft.Azure.Cosmos.Container> mockContainer = new();
    private readonly IOptions<Configuration> configOptions;

    public CosmosDbUrlShortcutRepositoryTests()
    {
        configOptions = Options.Create(new Configuration
        {
            AzureCosmosDB = new AzureCosmosDB
            {
                Endpoint = "https://localhost",
                ConnectionString = "fake-connection",
                DatabaseName = "TestDb",
                ContainerName = "Shortcuts"
            }
        });

        mockClient.Setup(c => c.GetDatabase("TestDb")).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.GetContainer("Shortcuts")).Returns(mockContainer.Object);
    }

    [Fact]
    public async Task CreateShortcutAsync_ValidInput_ReturnsShortcut()
    {
        var shortcut = new RepositoryUrlShortcut { Id = "abc123", Url = "https://example.com" };
        var cosmosEntity = new CosmosDbUrlShortcut { Id = shortcut.Id, Url = shortcut.Url, PartitionKey = shortcut.Id };

        var responseMock = new Mock<ItemResponse<CosmosDbUrlShortcut>>();
        responseMock.Setup(r => r.Resource).Returns(cosmosEntity);

        mockContainer.Setup(c => c.CreateItemAsync(
            It.IsAny<CosmosDbUrlShortcut>(),
            It.IsAny<PartitionKey>(),
            null,
            default)).ReturnsAsync(responseMock.Object);

        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var result = await repo.CreateShortcutAsync(shortcut);

        Assert.Equal(shortcut.Id, result.Id);
        Assert.Equal(shortcut.Url, result.Url);
    }

    [Fact]
    public async Task CreateShortcutAsync_Conflict_ThrowsDataAccessException()
    {
        var shortcut = new RepositoryUrlShortcut { Id = "abc123", Url = "https://example.com" };

        mockContainer.Setup(c => c.CreateItemAsync(
            It.IsAny<CosmosDbUrlShortcut>(),
            It.IsAny<PartitionKey>(),
            null,
            default)).ThrowsAsync(new CosmosException("Conflict", HttpStatusCode.Conflict, 0, "", 0));

        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);

        var ex = await Assert.ThrowsAsync<DataAccessException>(() => repo.CreateShortcutAsync(shortcut));
        Assert.Equal(DataAccessResultCode.Conflict, ex.ResultCode);
    }

    [Fact]
    public async Task GetShortcutAsync_ValidId_ReturnsShortcut()
    {
        var cosmosEntity = new CosmosDbUrlShortcut { Id = "abc123", Url = "https://example.com", PartitionKey = "abc123" };
        var responseMock = new Mock<ItemResponse<CosmosDbUrlShortcut>>();
        responseMock.Setup(r => r.Resource).Returns(cosmosEntity);

        mockContainer.Setup(c => c.ReadItemAsync<CosmosDbUrlShortcut>(
            "abc123",
            It.IsAny<PartitionKey>(),
            null,
            default)).ReturnsAsync(responseMock.Object);

        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var result = await repo.GetShortcutAsync("abc123");

        Assert.NotNull(result);
        Assert.Equal("abc123", result!.Id);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task GetShortcutAsync_NotFound_ReturnsNull()
    {
        mockContainer.Setup(c => c.ReadItemAsync<CosmosDbUrlShortcut>(
            "missing",
            It.IsAny<PartitionKey>(),
            null,
            default)).ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var result = await repo.GetShortcutAsync("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_ValidUrl_ReturnsShortcuts()
    {
        var items = new[]
        {
            new CosmosDbUrlShortcut { Id = "a", Url = "https://example.com", PartitionKey = "a" },
            new CosmosDbUrlShortcut { Id = "b", Url = "https://example.com", PartitionKey = "b" }
        };

        var feedResponseMock = new Mock<FeedResponse<CosmosDbUrlShortcut>>();
        feedResponseMock.Setup(r => r.GetEnumerator()).Returns(items.ToList().GetEnumerator());

        var iteratorMock = new Mock<FeedIterator<CosmosDbUrlShortcut>>();
        iteratorMock.SetupSequence(i => i.HasMoreResults).Returns(true).Returns(false);
        iteratorMock.Setup(i => i.ReadNextAsync(default)).ReturnsAsync(feedResponseMock.Object);

        mockContainer.Setup(c => c.GetItemQueryIterator<CosmosDbUrlShortcut>(
            It.IsAny<QueryDefinition>(),
            null,
            null)).Returns(iteratorMock.Object);

        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var result = await repo.GetUrlShortcutsByUrlAsync("https://example.com");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "a");
        Assert.Contains(result, r => r.Id == "b");
    }

    [Fact]
    public async Task CreateShortcutAsync_NullInput_ThrowsBadRequest()
    {
        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var ex = await Assert.ThrowsAsync<DataAccessException>(() => repo.CreateShortcutAsync(null!));
        Assert.Equal(DataAccessResultCode.BadRequest, ex.ResultCode);
    }

    [Fact]
    public async Task GetShortcutAsync_EmptyId_ThrowsBadRequest()
    {
        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var ex = await Assert.ThrowsAsync<DataAccessException>(() => repo.GetShortcutAsync(""));
        Assert.Equal(DataAccessResultCode.BadRequest, ex.ResultCode);
    }

    [Fact]
    public async Task GetUrlShortcutsByUrlAsync_EmptyUrl_ThrowsBadRequest()
    {
        var repo = new CosmosDbUrlShortcutRepository(mockClient.Object, configOptions);
        var ex = await Assert.ThrowsAsync<DataAccessException>(() => repo.GetUrlShortcutsByUrlAsync(""));
        Assert.Equal(DataAccessResultCode.BadRequest, ex.ResultCode);
    }
}
