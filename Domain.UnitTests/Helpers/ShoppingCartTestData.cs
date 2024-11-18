using System.Collections;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
namespace Cart.UnitTests.Helpers
{
    public class ShoppingCartTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
            new ShoppingCart
            {
                Username = "TestUser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 100 },
                        Quantity = 2
                    },
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = Guid.NewGuid(), Name = "Dish2", Price = 200 },
                        Quantity = 1
                    }
                }
            },
            // Expected total price
            400
            };

            yield return new object[]
            {
            new ShoppingCart
            {
                Username = "TestUser2",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = Guid.NewGuid(), Name = "Dish3", Price = 50 },
                        Quantity = 5
                    }
                }
            },
            250
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}