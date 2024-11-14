using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return Dish.Price * Quantity;
            }
        }

    }
}
