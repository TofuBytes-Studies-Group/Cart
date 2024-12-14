using Cart.Domain.Exceptions;

namespace Cart.Domain.Entities
{
    public class ShoppingCartItem
    {
        public Dish Dish { get; }
        public int Quantity { get; private set; }
        public int SumPrice => Dish.Price * Quantity;

        public ShoppingCartItem(Dish dish, int quantity = 1)
        {
            if (dish == null)
                throw new ArgumentNullException("Dish cannot be null");
            if (quantity <= 0)
                throw new NegativeOrZeroAmountException("Quantity must be greater than zero.");
            
            Dish = dish;
            Quantity = quantity;
        }

        public void IncreaseQuantity(int amount = 1)
        {
            if (amount <= 0)
                throw new NegativeOrZeroAmountException("Amount must be greater than zero.");

            Quantity += amount;
        }

        public void DecreaseQuantity(int amount = 1)
        {
            if (amount <= 0)
                throw new NegativeOrZeroAmountException("Amount must be greater than zero.");

            Quantity -= amount;
        }
    }
}
