namespace UrlShortenerApi.Utils
{
    public class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly int size;
        private readonly LinkedList<(TKey key, TValue value, DateTime? expiration)> list = new();
        private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value, DateTime? expiration)>> dict = new();
        private readonly object syncRoot = new();
        public LruCache(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.size = size;
        }

        public TValue? Get(TKey key)
        {
            lock (syncRoot)
            {
                return this.GetInternal(key);
            }
        }

        public void Set(TKey key, TValue value, TimeSpan? timeToLive)
        {
            lock (syncRoot)
            {
                this.SetInternal(key, value, timeToLive);
            }
        }

        public void Remove(TKey key)
        {
            lock (syncRoot)
            {
                this.RemoveInternal(key);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                this.ClearInternal();
            }
        }

        public TValue? GetInternal(TKey key)
        {
            if (dict.TryGetValue(key, out var node))
            {
                if (node.Value.expiration == null ||
                    DateTime.UtcNow < node.Value.expiration.Value)
                {
                    // cached value exists and still not expired
                    list.Remove(node);
                    list.AddFirst(node);
                    return node.Value.value;
                }
                else
                {
                    // time to live has expired
                    RemoveInternal(key);
                }
            }
            return null;
        }

        public void SetInternal(TKey key, TValue value, TimeSpan? timeToLive)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // note: allowing setting null values

            if (dict.ContainsKey(key))
            {
                // setting the conditions same as case where key is not present
                list.Remove(dict[key]);
                dict.Remove(key);
            }
            else if (list.Count >= this.size)
            {
                // Remove the least recently used item which is the last item of the linked list
                var last = list.Last;
                if (last == null)
                {
                    throw new NullReferenceException("No value to evict!");
                }
                dict.Remove(last.Value.key);
                list.RemoveLast();
            }

            // Add the new item to the front of the list
            var node = new LinkedListNode<(TKey key, TValue value, DateTime? expiration)>(
                (key,
                value,
                timeToLive == null? null : DateTime.UtcNow.Add(timeToLive.Value)));
            list.AddFirst(node);
            dict[key] = node;
        }

        public void RemoveInternal(TKey key)
        {
            if (dict.TryGetValue(key, out var node))
            {
                list.Remove(node);
                dict.Remove(key);
            }
        }

        private void ClearInternal()
        {
            list.Clear();
            dict.Clear();
        }
    }
}
