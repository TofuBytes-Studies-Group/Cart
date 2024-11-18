using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;
using Cart.UnitTests.Helpers;

namespace Cart.UnitTests
{
    public class ShoppingCartTest
    {

        [Theory]
        // Arrange
        [ClassData(typeof(ShoppingCartTestData))]
        public void New_ShouldCalculateCorrectTotalPrice(ShoppingCart shoppingCart, int expectedTotalPrice)
        {
            // Act
            var actualTotalPrice = shoppingCart.TotalPrice;

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
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
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
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
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
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == expectedTotalQuantityOfCartItems);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void RemoveFromCart_ShouldRemoveEntirelyFromCartNoMatterTheQuantity(int quantity)
        {
            // Arrange  
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                Username = "Testuser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = quantity
                    }
                }
            };

            // Act
            cart.RemoveFromCart(dishId);

            // Assert
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public void RemoveFromCart_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange 
            var cart = new ShoppingCart { Username = "Testuser1" };

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => cart.RemoveFromCart(Guid.NewGuid()));
            Assert.Equal("Item not found in cart", exception.Message);
        }

        [Fact]
        public void RemoveFromCart_OtherItemsInCart_ShouldRemoveOnlySpecificItemFromCart()
        {
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                Username = "Testuser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = 1
                    },
                    new ShoppingCartItem
                    {
                        Dish = new Dish { Id = Guid.NewGuid(), Name = "Dish2", Price = 1 },
                        Quantity = 1
                    }
                }
            };

            // Act
            cart.RemoveFromCart(dishId);

            // Assert
            Assert.DoesNotContain(cart.CartItems, item => item.Dish.Id == dishId && item.Dish.Name == "Dish1");
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish2" && item.Quantity == 1);    
        }

        [Theory]
        [InlineData(1, 2, 2)] 
        [InlineData(2, 4, 4)]
        [InlineData(10, 9, 9)]
        public void UpdateItemQuantity_ShouldUpdateItemQuantity(int quantityInCart, int newQuantity, int expectedQuantityInCart)
        {
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                Username = "Testuser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = quantityInCart
                    }
                }
            };

            // Act
            cart.UpdateItemQuantity(dishId, newQuantity);

            // Assert
            Assert.Equal(expectedQuantityInCart, cart.CartItems.Sum(item => item.Quantity));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void UpdateItemQuantity_ZeroOrNegativeQuantity_ShouldThrowZeroOrNegativeQuantityException(int newQuantity)
        {
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                Username = "Testuser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = 1
                    }
                }
            };

            // Act 6 Assert
            var exception = Assert.Throws<ZeroOrNegativeQuantityException>(() => cart.UpdateItemQuantity(Guid.NewGuid(), newQuantity));
            Assert.Equal("Quantity cannot be zero or less", exception.Message);
        }

        [Fact]
        public void UpdateItemQuantity_ItemNotInCart_ShouldThrowItemNotInCartThrowException()
        {
            //Arrange
            var cart = new ShoppingCart { Username = "TestUser1" };

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => cart.UpdateItemQuantity(Guid.NewGuid(), 1));
            Assert.Equal("Item not found in cart", exception.Message);
        } 
    }
}
