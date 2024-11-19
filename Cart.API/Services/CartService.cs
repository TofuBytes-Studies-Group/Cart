using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Infrastructure.Repositories;

namespace Cart.API.Services
{
    public class CartService
    {

        // Temporary untill redis:
        private readonly Dictionary<string, ShoppingCart> _carts = new Dictionary<string, ShoppingCart>();
        private readonly KafkaProducerService _kafkaProducerService;
        //private readonly ICartRepository _cartRepository; //TODO Add this

        public CartService(KafkaProducerService kafkaProducerService)
        {
            _kafkaProducerService = kafkaProducerService;
        }

        // Will always return a cart, just an empty one for a new username
        public ShoppingCart GetCart(string username)
        {
            if (!_carts.TryGetValue(username, out var cart))
            {
                cart = new ShoppingCart { Username = username }; // Temporary will add db later 
                _carts[username] = cart;
                return cart;
            }
            return cart;
        }

        public ShoppingCart AddOneToCart(string username, Dish dish)
        {
            var cart = GetCart(username);
            cart.AddOneToCart(dish);
            _carts[username] = cart;
            return cart;
        }

        public ShoppingCart RemoveOneFromCart(string username, Guid dishId)
        {
            var cart = GetCart(username);
            cart.RemoveOneFromCart(dishId);
            _carts[username] = cart;
            return cart;
        }

        public ShoppingCart RemoveAllFromCart(string username, Guid dishId)
        {
            var cart = GetCart(username);
            cart.RemoveAllFromCart(dishId);
            _carts[username] = cart;
            return cart;
        }
    }
}
