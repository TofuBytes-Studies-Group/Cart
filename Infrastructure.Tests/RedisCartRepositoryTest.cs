using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Infrastructure.Tests
{
    public class RedisCartRepositoryTest : IAsyncLifetime
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisCartRepository _repository;
        private readonly string _testCartKey = "TestUser1";

        public RedisCartRepositoryTest()
        {
            var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _repository = new RedisCartRepository(_redis);
        }

        public async Task InitializeAsync()
        {
            var database = _redis.GetDatabase();
            await database.KeyDeleteAsync(_testCartKey);
        }

        public async Task DisposeAsync()
        {
            var database = _redis.GetDatabase();
            await database.KeyDeleteAsync(_testCartKey);
            _redis.Dispose();
        }

        [Fact]
        public async void GetCartAsync_ShouldSaveCartAndGetCorrectCart()
        {
            //Arrange
            var dishId = Guid.NewGuid();
            var expectedCart = new ShoppingCart
            {
                CustomerUsername = _testCartKey,
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = dishId, Name = "Dish1", Price = 100 },
                        Quantity = 2
                    }
                }
            };

            // Act
            await _repository.SaveCartAsync(expectedCart);
            var actualCart = await _repository.GetCartAsync(_testCartKey);

            // Assert
            Assert.NotNull(actualCart);
            Assert.Equal(expectedCart.CustomerUsername, actualCart.CustomerUsername);
            Assert.Single(actualCart.CartItems);
            Assert.Equal(dishId, actualCart.CartItems[0].Dish.Id);
            Assert.Equal(2, actualCart.CartItems[0].Quantity);
            Assert.Equal(100, actualCart.CartItems[0].Dish.Price);
            Assert.Equal(200, actualCart.TotalPrice);
        }

        [Fact]
        public async Task GetCartAsync_ShouldReturnNullIfCartDoesNotExist()
        {
            // Act
            var cart = await _repository.GetCartAsync("non_existent_cart");

            // Assert
            Assert.Null(cart);
        }

        [Fact]
        public async void DeleteCartAsync_ShouldDeleteCartAndGetShouldReturnNull()
        {
            //Arrange
            var dishId = Guid.NewGuid();
            var cart = new ShoppingCart
            {
                CustomerUsername = _testCartKey,
                CartItems = new List<ShoppingCartItem> 
                {
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = dishId, Name = "Dish1", Price = 100 },
                        Quantity = 2
                    }
                }
            };
            await _repository.SaveCartAsync(cart);

            // Act
            await _repository.DeleteCartAsync(_testCartKey);
            var deletedCart = await _repository.GetCartAsync(_testCartKey);

            // Assert
            Assert.Null(deletedCart);
        }
    }
}