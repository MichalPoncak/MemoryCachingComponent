
namespace CustomCachingEngine.Test
{
    public class MemoryCacheTests
    {
        [Fact]
        public void TestAddOrUpdate_WithValidData_ShouldReadLastItem()
        {            
            // Arrange
            var cache = new MemoryCache<int, string>(2);

            // Act
            cache.AddOrUpdate(1, "Val1");
            cache.AddOrUpdate(2, "Val2");

            // Assert
            Assert.True(cache.GetItem(2, out var retrievedValue));
            Assert.Equal("Val2", retrievedValue);
        }

        [Fact]
        public void TestAddOrUpdate_WithValidData_ShouldUpdateAnExistingItem()
        {
            // Arrange
            var cache = new MemoryCache<int, string>(6);

            // Act
            cache.AddOrUpdate(1, "Val1");
            cache.AddOrUpdate(2, "Val2");
            cache.AddOrUpdate(3, "Val3");
            cache.AddOrUpdate(4, "Val4");
            cache.AddOrUpdate(3, "Val5");
            cache.AddOrUpdate(6, "Val6");

            // Assert
            Assert.True(cache.GetItem(3, out var retrievedValue));
            Assert.Equal("Val5", retrievedValue);
        }

        [Fact]
        public void TestAddOrUpdate_WithValidData_ShouldEvictItemWhenCacheFull()
        {
            // Arrange
            var cache = new MemoryCache<int, string>(3);
            int evictedKey = -1;

            // Subscribe to the ItemEvicted event
            cache.ItemEvicted += key => evictedKey = key;

            // Act
            cache.AddOrUpdate(1, "Val1");
            cache.AddOrUpdate(2, "Val2");
            cache.AddOrUpdate(3, "Val3");
            cache.AddOrUpdate(4, "Val4");

            // Assert
            Assert.Equal(1, evictedKey);
        }

        [Fact]
        public void TestAddOrUpdate_WithValidData_ShouldOrderItemsCorrectly()
        {
            // Arrange
            var cache = new MemoryCache<int, string>(5);

            // Act
            cache.AddOrUpdate(1, "Val1");
            cache.AddOrUpdate(2, "Val2");
            cache.AddOrUpdate(3, "Val3");

            cache.GetItem(1, out _);

            cache.AddOrUpdate(4, "Val4"); // 2314

            cache.GetItem(3, out _); // 2143

            cache.AddOrUpdate(5, "Val5"); // 21435

            var evictionQueueOrder = cache.GetEvictionQueue();

            // Assert
            Assert.Equal(new[] { 2, 1, 4, 3, 5 }, evictionQueueOrder);
        }

        [Fact]
        public void TestAddOrUpdate_InvalidCacheCapacity_ShouldThrowException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new MemoryCache<string, string>(0));
            Assert.Throws<ArgumentException>(() => new MemoryCache<int, string>(-1));
        }

        [Fact]
        public void TestAddOrUpdate_WithNullKey_ShouldThrowException()
        {
            // Arrange
            var cache = new MemoryCache<string, string>(3);

            // Assert
            Assert.Throws<ArgumentNullException>(() => cache.AddOrUpdate(null, "Value1"));
        }

        [Fact]
        public void TestAddOrUpdate_WithNullValue_ShouldThrowException()
        {
            // Arrange
            var cache = new MemoryCache<string, string>(3);

            // Assert
            Assert.Throws<ArgumentNullException>(() => cache.AddOrUpdate("Key1", null));
        }
    }
}