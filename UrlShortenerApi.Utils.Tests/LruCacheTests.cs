namespace UrlShortenerApi.Utils.Tests;

public class LruCacheTests
{
    [Fact]
    public void Constructor_WithInvalidSize_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<string, string>(0));
    }

    [Fact]
    public void SetAndGet_ReturnsCachedValue()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", null);

        var result = cache.Get("key1");

        Assert.Equal("value1", result);
    }

    [Fact]
    public void Get_NonExistentKey_ReturnsNull()
    {
        var cache = new LruCache<string, string>(2);

        var result = cache.Get("missing");

        Assert.Null(result);
    }

    [Fact]
    public void Set_ExceedsCapacity_EvictsLeastRecentlyUsed()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", null);
        cache.Set("key2", "value2", null);
        cache.Set("key3", "value3", null); // should evict key1

        Assert.Null(cache.Get("key1"));
        Assert.Equal("value2", cache.Get("key2"));
        Assert.Equal("value3", cache.Get("key3"));
    }

    [Fact]
    public void Get_UpdatesUsageOrder()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", null);
        cache.Set("key2", "value2", null);
        cache.Get("key1"); // key1 becomes most recently used
        cache.Set("key3", "value3", null); // should evict key2

        Assert.Equal("value1", cache.Get("key1"));
        Assert.Null(cache.Get("key2"));
        Assert.Equal("value3", cache.Get("key3"));
    }

    [Fact]
    public void Set_WithTTL_ExpiresCorrectly()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", TimeSpan.FromMilliseconds(100));

        Thread.Sleep(150);

        var result = cache.Get("key1");

        Assert.Null(result);
    }

    [Fact]
    public void Remove_DeletesEntry()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", null);
        cache.Remove("key1");

        var result = cache.Get("key1");

        Assert.Null(result);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new LruCache<string, string>(2);
        cache.Set("key1", "value1", null);
        cache.Set("key2", "value2", null);
        cache.Clear();

        Assert.Null(cache.Get("key1"));
        Assert.Null(cache.Get("key2"));
    }

    [Fact]
    public void Set_NullKey_ThrowsException()
    {
        var cache = new LruCache<string, string>(2);

        Assert.Throws<ArgumentNullException>(() => cache.Set(null!, "value", null));
    }

    [Fact]
    public void ConcurrentAccess_DoesNotThrow()
    {
        var cache = new LruCache<string, string>(100);

        var threads = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            int index = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var key = $"key-{index}-{j}";
                    cache.Set(key, $"value-{j}", null);
                    var _ = cache.Get(key);
                }
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        Assert.True(true); // If no exception, test passes
    }
}
