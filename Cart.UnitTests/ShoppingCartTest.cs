using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.UnitTests.Helpers;
using System.Diagnostics;

namespace Cart.UnitTests
{
    public class ShoppingCartTest
    {

        [Theory]
        // Arrange
        [ClassData(typeof(ShoppingCartTestData))]
        public void New_ShouldCalculateTotalPrice(ShoppingCart shoppingCart, int expectedTotalPrice)
        {
            // Act
            var actualTotalPrice = shoppingCart.TotaltPrice;

            // Assert
            Assert.Equal(expectedTotalPrice, actualTotalPrice);
        }

        [Theory]
        [InlineData(1, 1, 1, 1)]
        [InlineData(100, 2, 2, 200)]
        [InlineData(100, 100, 100, 10000)]
        public void AddToCart_AddToEmptyCart_ShouldAddCartItemsAndCalculateTotalPrice(int price, int quantity,
            int expectedSizeOfCart, int expectedTotalPrice)
        {
            // Arrange
            var cart = new ShoppingCart { Username = "TestUser1" };
            var dish = new Dish { Id = new Guid(), Name = "Dish1", Price = price };

            // Act
            cart.AddToCart(dish, quantity);

            // Assert
            Assert.Equal(expectedSizeOfCart, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(expectedTotalPrice, cart.TotaltPrice);
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == quantity);

        }

        [Theory]
        [InlineData(1, 1, 1, 1, 2, 2)]
        [InlineData(100, 2, 50, 1, 250, 3)]
        [InlineData(100, 10, 200, 2, 1400, 12)]
        public void AddToCart_AddNewItemToCartWithItems_ShouldAddToCartItemsAndUpdateTotalPrice(int priceOfDishInCart, 
            int quantityOfDishInCart,
            int priceOfNewDish, int quantityOfNewDish,
            int expectedTotalPrice, int expectedTotalQuantityOfCartItems)
        {
            // Arrange
            var cart = new ShoppingCart
            {
                Username = "TestUser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = priceOfDishInCart },
                        Quantity = quantityOfDishInCart
                    }
                }
            };
            var dish = new Dish { Id = new Guid(), Name = "Dish2", Price = priceOfNewDish };

            // Act
            cart.AddToCart(dish, quantityOfNewDish);

            // Assert
            Assert.Equal(expectedTotalQuantityOfCartItems, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.CartItems.Count);
            Assert.Equal(expectedTotalPrice, cart.TotaltPrice);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == quantityOfDishInCart);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish2" && item.Quantity == quantityOfNewDish);
        }


        [Theory]
        [InlineData(1, 1, 1, 2, 2)]
        [InlineData(100, 1, 4, 500, 5)]
        [InlineData(100, 10, 10, 2000, 20)]
        public void AddToCart_AddSameItemToCartWithItems_ShouldAddToCartItemsAndUpdateTotalPrice(int price, 
            int quantityOfDishInCart, int quantityToAdd,
            int expectedTotalPrice, int expectedTotalQuantityOfCartItems)
        {
            // Arrange
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = price };

            var cart = new ShoppingCart
            {
                Username = "TestUser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = quantityOfDishInCart
                    }
                }
            };

            // Act
            cart.AddToCart(dish, quantityToAdd);

            // Assert
            Assert.Equal(expectedTotalQuantityOfCartItems, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(expectedTotalPrice, cart.TotaltPrice);
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == expectedTotalQuantityOfCartItems);
        }
    }
}
