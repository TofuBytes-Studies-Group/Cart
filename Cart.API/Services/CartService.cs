using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Infrastructure.Repositories;

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<ShoppingCart> GetCartAsync(string username)
        {
            var cart = await _cartRepository.GetCartAsync(username);
            if (cart == null)
            {
                cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid(), username);
                await _cartRepository.SaveCartAsync(cart); 
            }
            return cart;
        }

        public async Task<ShoppingCart> AddOneToCartAsync(string username, Dish dish)
        {
            var cart = await GetCartAsync(username);
            cart.AddOneToCart(dish);
            await _cartRepository.SaveCartAsync(cart);
            return cart;
        }

        public async Task<ShoppingCart> RemoveOneFromCartAsync(string username, Guid dishId)
        {
            var cart = await GetCartAsync(username);
            cart.RemoveOneFromCart(dishId);
            await _cartRepository.SaveCartAsync(cart);
            return cart;
        }

        public async Task<ShoppingCart> RemoveAllFromCartAsync(string username, Guid dishId)
        {
            var cart = await GetCartAsync(username);
            cart.RemoveAllFromCart(dishId);
            await _cartRepository.SaveCartAsync(cart);
            return cart;
        }
    }
}