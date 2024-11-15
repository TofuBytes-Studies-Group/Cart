using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.Domain.Aggregates
{
    public class ShoppingCart
    {
        public required string Username { get; set; }
        public List<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public int TotaltPrice
        {
            get
            {
                return CartItems.Sum(cartItem => cartItem.SumPrice);
            }
        }

        public void AddToCart(Dish dish, int quantity)
        {
            if (quantity <= 0) throw new ZeroQuantityException("Quantity cannot be zero");

            var existingCartItem = CartItems.FirstOrDefault(cartItem => cartItem.Dish.Id == dish.Id);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                CartItems.Add(new ShoppingCartItem { Dish = dish, Quantity = quantity });
            }
        }
    }
}
