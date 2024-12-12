using System.Collections;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
namespace Domain.UnitTests.Helpers
{
    public class ShoppingCartTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                CreateShoppingCartWithItems(
                    "TestUser1",
                    new (Dish, int)[] 
                    {
                        (new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 100 }, 2),
                        (new Dish { Id = Guid.NewGuid(), Name = "Dish2", Price = 200 }, 1)
                    }),
                // Expected total price
                400
            };

            yield return new object[]
            {
                CreateShoppingCartWithItems(
                    "TestUser2",
                    new (Dish, int)[]
                    {
                        (new Dish { Id = Guid.NewGuid(), Name = "Dish3", Price = 50 }, 5)
                    }),
                250
            };
        }

        private static ShoppingCart CreateShoppingCartWithItems(string customerUsername, (Dish, int)[] items)
        {
            var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid(), customerUsername);
            foreach (var (dish, quantity) in items)
            {
                for (int i = 0; i < quantity; i++)
                {
                    cart.AddOneToCart(dish);
                }
            }
            return cart;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}