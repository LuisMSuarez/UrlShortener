namespace UrlShortenerApi.Utils
{
    public interface ILruCache<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        public TValue? Get(TKey key);
        public void Set(TKey key, TValue value, TimeSpan? timeToLive);
        public void Remove(TKey key);
        public void Clear();
    }
}