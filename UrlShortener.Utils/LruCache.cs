namespace UrlShortenerApi.Utils;

/// <summary>
/// A thread-safe Least Recently Used (LRU) cache implementation with optional time-to-live (TTL) expiration.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached items. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache. Must be a reference type.</typeparam>
public class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    /// <summary>
    /// The maximum number of items the cache can hold before evicting the least recently used entry.
    /// </summary>
    private readonly int size;

    /// <summary>
    /// Internal list used to track usage order and expiration metadata.
    /// Most recently used items are at the front.
    /// </summary>
    private readonly LinkedList<(TKey key, TValue value, DateTime? expiration)> list = new();

    /// <summary>
    /// Dictionary for fast key-based lookup of cache entries.
    /// </summary>
    private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value, DateTime? expiration)>> dict = new();

    /// <summary>
    /// Synchronization object to ensure thread-safe access.
    /// </summary>
    private readonly object syncRoot = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LruCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="size">The maximum number of items the cache can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than 1.</exception>
    public LruCache(int size)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        this.size = size;
    }

    /// <inheritdoc />
    public TValue? Get(TKey key)
    {
        lock (syncRoot)
        {
            return this.GetInternal(key);
        }
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue value, TimeSpan? timeToLive)
    {
        lock (syncRoot)
        {
            this.SetInternal(key, value, timeToLive);
        }
    }

    /// <inheritdoc />
    public void Remove(TKey key)
    {
        lock (syncRoot)
        {
            this.RemoveInternal(key);
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (syncRoot)
        {
            this.ClearInternal();
        }
    }

    /// <summary>
    /// Retrieves the cached value for the specified key, updating its usage order if valid.
    /// </summary>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>The cached value if present and not expired; otherwise, <c>null</c>.</returns>
    public TValue? GetInternal(TKey key)
    {
        if (dict.TryGetValue(key, out var node))
        {
            if (node.Value.expiration == null ||
                DateTime.UtcNow < node.Value.expiration.Value)
            {
                // Move to front to mark as recently used
                list.Remove(node);
                list.AddFirst(node);
                return node.Value.value;
            }
            else
            {
                // Expired entry
                RemoveInternal(key);
            }
        }
        return null;
    }

    /// <summary>
    /// Adds or updates a cache entry, evicting the least recently used item if capacity is exceeded.
    /// </summary>
    /// <param name="key">The key of the item to cache.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="timeToLive">Optional expiration duration. If <c>null</c>, the item does not expire.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
    /// <exception cref="NullReferenceException">Thrown if eviction fails due to unexpected internal state.</exception>
    public void SetInternal(TKey key, TValue value, TimeSpan? timeToLive)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (dict.ContainsKey(key))
        {
            list.Remove(dict[key]);
            dict.Remove(key);
        }
        else if (list.Count >= this.size)
        {
            var last = list.Last;
            if (last == null)
            {
                throw new NullReferenceException("No value to evict!");
            }
            dict.Remove(last.Value.key);
            list.RemoveLast();
        }

        var node = new LinkedListNode<(TKey key, TValue value, DateTime? expiration)>(
            (key,
            value,
            timeToLive == null ? null : DateTime.UtcNow.Add(timeToLive.Value)));
        list.AddFirst(node);
        dict[key] = node;
    }

    /// <summary>
    /// Removes the cache entry associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    public void RemoveInternal(TKey key)
    {
        if (dict.TryGetValue(key, out var node))
        {
            list.Remove(node);
            dict.Remove(key);
        }
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    private void ClearInternal()
    {
        list.Clear();
        dict.Clear();
    }
}
