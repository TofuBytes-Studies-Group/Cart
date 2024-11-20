using System.Text.Json;
using Cart.Domain.Aggregates;
using StackExchange.Redis;

namespace Cart.Infrastructure.Repositories
{
    public class RedisCartRepository : ICartRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly TimeSpan expiration = TimeSpan.FromMinutes(30);

        public RedisCartRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task<ShoppingCart?> GetCartAsync(string username)
        {
            var jsonCart = await _database.StringGetAsync(username);
            if (jsonCart.IsNullOrEmpty)
            {
                return null;
            }
#pragma warning disable CS8604 // Possible null reference argument. // TODO: I don't know why it keeps saying it might be null here
            return JsonSerializer.Deserialize<ShoppingCart>(jsonCart);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public async Task SaveCartAsync(ShoppingCart cart)
        {
            var jsonCart = JsonSerializer.Serialize(cart);
            await _database.StringSetAsync(cart.Username, jsonCart, expiration);
        }

        public async Task DeleteCartAsync(string username)
        {
            await _database.KeyDeleteAsync(username);
        }
    }
}