namespace UrlShortenerApi.Utils;

/// <summary>
/// Defines a generic Least Recently Used (LRU) cache interface with optional time-to-live support.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached items. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache. Must be a reference type.</typeparam>
public interface ILruCache<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    /// <summary>
    /// Retrieves the value associated with the specified key, if present and not expired.
    /// </summary>
    /// <param name="key">The key of the cached item to retrieve.</param>
    /// <returns>
    /// The cached value if found and valid; otherwise, <c>null</c>.
    /// </returns>
    TValue? Get(TKey key);

    /// <summary>
    /// Adds or updates the value associated with the specified key, optionally setting a time-to-live.
    /// </summary>
    /// <param name="key">The key of the item to cache.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="timeToLive">
    /// Optional expiration duration. If <c>null</c>, the item does not expire based on time.
    /// </param>
    void Set(TKey key, TValue value, TimeSpan? timeToLive);

    /// <summary>
    /// Removes the cached item associated with the specified key, if it exists.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void Remove(TKey key);

    /// <summary>
    /// Clears all items from the cache, regardless of expiration or usage.
    /// </summary>
    void Clear();
}
