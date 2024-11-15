using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cart.Domain.Entities;

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
                int totalPrice = 0;
                foreach (var cartItem in CartItems)
                {
                    totalPrice += cartItem.SumPrice;
                }
                return totalPrice;
            }
        }
    }
}
