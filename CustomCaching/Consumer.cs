using CustomCachingEngine;

namespace CustomCaching
{
    public class Consumer
    {
        private readonly MemoryCache<int, string> memoryCache;

        public Consumer() {
            memoryCache = new MemoryCache<int, string>(4);

            // Subscribe to the ItemEvicted event
            // to be notified when items are evicted
            memoryCache.ItemEvicted += OnItemEvicted;

            // Use the cache
            memoryCache.AddOrUpdate(1, "Val1");
            memoryCache.AddOrUpdate(2, "Val2");
            memoryCache.AddOrUpdate(3, "Val3");
            memoryCache.AddOrUpdate(4, "Val4");
            memoryCache.AddOrUpdate(3, "Val5");
            memoryCache.AddOrUpdate(6, "Val6");

            if (memoryCache.GetItem(3, out var value))
            {
                Console.WriteLine($"Value for item 3: {value}");
            }

            if (memoryCache.GetItem(1, out var value2))
            {
                Console.WriteLine($"Value for item 1: {value2}");
            }

            Console.WriteLine($"Current eviction order: {string.Join(",", memoryCache.GetEvictionQueue())}");
        }

        private void OnItemEvicted(int key)
        {
            Console.WriteLine($"Item evicted from the cache. Key {key}");
        }
    }
}
