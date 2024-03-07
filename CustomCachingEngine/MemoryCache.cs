namespace CustomCachingEngine
{
    public interface IMemoryCache<TKey, TValue>
    {
        void AddOrUpdate(TKey key, TValue value);
        bool TryGet(TKey key, out TValue value);

        // Event to notify consumers when an item is evicted
        event Action<TKey> ItemEvicted;
    }

    public class MemoryCache<TKey, TVal>
    {
        private readonly int maxNumberOfCachedItems;
        private readonly Dictionary<TKey, TVal> cache;
        private Queue<TKey> evictionQueue;

        public MemoryCache(int maxNumberOfCachedItems = 3)
        {
            if (maxNumberOfCachedItems < 1)
            {
                throw new ArgumentException("Invalid cache capacity", nameof(maxNumberOfCachedItems));
            }

            this.maxNumberOfCachedItems = maxNumberOfCachedItems;
            cache = new Dictionary<TKey, TVal>(maxNumberOfCachedItems);
            evictionQueue = new Queue<TKey>(maxNumberOfCachedItems);
        }

        public void AddOrUpdate(
            TKey key, 
            TVal value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null");
            }

            lock (cache)
            {
                // if item is cached
                if (cache.ContainsKey(key))
                {
                    // update value
                    cache[key] = value;
                    // move the cached item to the back of the queue
                    UpdateEvictionQueue(key);
                }
                else // if item is not cached
                {
                    // if the cache is full
                    if (cache.Count >= maxNumberOfCachedItems)
                    {
                        // evict the least recently used item
                        EvictItem();
                    }

                    // Add new item to the cache and eviction queue
                    cache[key] = value;
                    evictionQueue.Enqueue(key);
                }
            }
        }

        public bool GetItem(TKey key, out TVal value)
        {
            lock (cache)
            {
                if (cache.TryGetValue(key, out var cacheItem))
                {
                    // Move the accessed item to the front of the eviction queue
                    UpdateEvictionQueue(key);

                    value = cacheItem;
                    return true;
                }

                value = default;
                return false;
            }
        }

        public IEnumerable<TKey> GetEvictionQueue()
        {
            return evictionQueue.ToArray();
        }

        private void UpdateEvictionQueue(TKey key)
        {
            // Remove the key from its current position in the queue
            evictionQueue = new Queue<TKey>(evictionQueue.Where(k => !k.Equals(key)));

            // Add the key to the back of the queue as it was used
            evictionQueue.Enqueue(key);
        }

        private void EvictItem()
        {
            var evictedKey = evictionQueue.Dequeue();
            cache.Remove(evictedKey);
            
            // Notify consumers about the eviction
            OnItemEvicted(evictedKey);
        }

        // Event to notify consumers when an item is evicted
        public event Action<TKey> ItemEvicted;

        protected virtual void OnItemEvicted(TKey key)
        {
            ItemEvicted?.Invoke(key);
        }
    }
}