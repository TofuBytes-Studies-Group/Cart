using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cart.Domain.Exceptions;

namespace Cart.Domain.Entities
{
    public class ShoppingCartItem
    {
        public required Dish Dish { get; set; }
        public int Quantity { get; set; }
        public decimal SumPrice 
        { 
            get
            {
                if (Dish.Price == int.MaxValue || Quantity == int.MaxValue)
                {
                    throw new SumPriceMaxValueException("Cannot calculate price of items");
                }
                return Dish.Price * Quantity;
            }
        }

    }
}
