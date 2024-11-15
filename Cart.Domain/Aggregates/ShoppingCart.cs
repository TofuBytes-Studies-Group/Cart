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
            if (quantity <= 0) throw new ZeroOrNegativeQuantityException("Quantity cannot be zero or less");

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

        public void RemoveFromCart(Guid dishId)
        {
            var item = CartItems.FirstOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                CartItems.Remove(item);
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }

        public void UpdateItemQuantity(Guid dishId, int newQuantity)
        {
            if (newQuantity <= 0) throw new ZeroOrNegativeQuantityException("Quantity cannot be zero or less");

            var item = CartItems.FirstOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                item.Quantity = newQuantity;
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }
    }
}
