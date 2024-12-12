using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.Domain.Aggregates
{
    public class ShoppingCart
    {
        public Guid CustomerId { get; private set; }
        public Guid RestaurantId { get; private set; }
        public string CustomerUsername { get; private set; }

        private readonly List<ShoppingCartItem> _cartItems = new();
        public IReadOnlyList<ShoppingCartItem> CartItems => _cartItems.AsReadOnly();
        public int TotalPrice => _cartItems.Sum(cartItem => cartItem.SumPrice);

        public ShoppingCart(Guid customerId, Guid restaurantId, string customerUsername)
        {
            if (string.IsNullOrWhiteSpace(customerUsername))
                throw new ArgumentException("Customer username cannot be empty.");

            CustomerId = customerId;
            RestaurantId = restaurantId;
            CustomerUsername = customerUsername;
        }

        public void AddOneToCart(Dish dish)
        {
            var existingCartItem = _cartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dish.Id);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
            }
            else
            {
                _cartItems.Add(new ShoppingCartItem { Dish = dish, Quantity = 1 });
            }
        }

        public void RemoveOneFromCart(Guid dishId)
        {
            var item = _cartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                item.Quantity -= 1;
                if (item.Quantity <= 0)
                {
                    _cartItems.Remove(item);
                }
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }

        public void RemoveAllFromCart(Guid dishId)
        {
            var item = _cartItems.SingleOrDefault(cartItem => cartItem.Dish.Id == dishId);
            if (item != null)
            {
                _cartItems.Remove(item);
            }
            else
            {
                throw new ItemNotInCartException("Item not found in cart");
            }
        }
    }
}
