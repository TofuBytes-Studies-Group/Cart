using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.Domain.Aggregates
{
    public class ShoppingCart
    {
        public required string Username { get; set; }
        public List<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public int TotalPrice
        {
            get
            {
                return CartItems.Sum(cartItem => cartItem.SumPrice);
            }
        }

        public void AddOneToCart(Dish dish)
        {
            var existingCartItem = CartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dish.Id);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
            }
            else
            {
                CartItems.Add(new ShoppingCartItem { Dish = dish, Quantity = 1 });
            }
        }

        public void RemoveOneFromCart(Guid dishId)
        {
            var item = CartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                item.Quantity -= 1;
                if (item.Quantity <= 0)
                {
                    CartItems.Remove(item);
                }
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }

        public void RemoveAllFromCart(Guid dishId)
        {
            var item = CartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                CartItems.Remove(item);
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }
    }
}
