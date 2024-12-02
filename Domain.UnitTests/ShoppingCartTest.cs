using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;
using Domain.UnitTests.Helpers;

namespace Domain.UnitTests
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
        [InlineData(1, 1)]
        [InlineData(100, 100)]
        [InlineData(500, 500)]
        public void AddToCart_AddToEmptyCart_ShouldAddCartItemsAndTotalPriceShouldEqualPrice(int price, int expectedTotalPrice)
        {
            // Arrange
            var cart = new ShoppingCart { CustomerUsername = "TestUser1" };
            var dish = new Dish { Id = new Guid(), Name = "Dish1", Price = price };

            // Act
            cart.AddOneToCart(dish);

            // Assert
            Assert.Equal(1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == 1);

        }

        [Theory]
        [InlineData(1, 1, 1, 2)]
        [InlineData(100, 2, 50, 250)]
        [InlineData(100, 10, 200, 1200)]
        public void AddToCart_AddNewItemToCartWithItems_ShouldAddOneNewItemSeparatelyAndUpdateTotalPrice(int priceOfDishInCart,
            int quantityOfDishInCart,
            int priceOfNewDish,
            int expectedTotalPrice)
        {
            // Arrange
            var cart = new ShoppingCart 
            {
                CustomerUsername = "TestUser1",
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
            cart.AddOneToCart(dish);

            // Assert
            Assert.Equal(quantityOfDishInCart + 1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.CartItems.Count);
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == quantityOfDishInCart);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish2" && item.Quantity == 1);
        }


        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(100, 4, 500)]
        [InlineData(100, 10, 1100)]
        public void AddToCart_AddSameItemToCartWithItems_ShouldAddOneToCartItemsAndUpdateTotalPrice(int price,
            int quantityOfDishInCart,
            int expectedTotalPrice)
        {
            // Arrange
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = price };

            var cart = new ShoppingCart
            {
                CustomerUsername = "TestUser1",
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
            cart.AddOneToCart(dish);

            // Assert
            Assert.Equal(quantityOfDishInCart + 1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(expectedTotalPrice, cart.TotalPrice);
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish1" && item.Quantity == quantityOfDishInCart + 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void RemoveAllFromCart_ShouldRemoveEntirelyFromCartNoMatterTheQuantity(int quantity)
        {
            // Arrange  
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                CustomerUsername = "Testuser1",
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
            cart.RemoveAllFromCart(dishId);

            // Assert
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public void RemoveAllFromCart_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange 
            var cart = new ShoppingCart { CustomerUsername = "Testuser1" };

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => cart.RemoveAllFromCart(Guid.NewGuid()));
            Assert.Equal("Item not found in cart", exception.Message);
        }

        [Fact]
        public void RemoveAllFromCart_OtherItemsAlsoInCart_ShouldRemoveOnlySpecificItemFromCart()
        {
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                CustomerUsername = "Testuser1",
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
            cart.RemoveAllFromCart(dishId);

            // Assert
            Assert.DoesNotContain(cart.CartItems, item => item.Dish.Id == dishId && item.Dish.Name == "Dish1");
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish2" && item.Quantity == 1);    
        }

        [Theory]
        [InlineData(2)] 
        [InlineData(10)]
        [InlineData(100)]
        public void RemoveOneFromCart_MoreThanOneItemAlreadyInCart_ShouldUpdateItemQuantityToOneLess
            (int quantityInCart)
        { 
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                CustomerUsername = "Testuser1",
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
            cart.RemoveOneFromCart(dishId);

            // Assert
            Assert.Equal(quantityInCart - 1, cart.CartItems.Sum(item => item.Quantity));
        }

        [Fact]
        public void RemoveOneFromCart_ExactlyOneItemInCart_ShouldRemoveShoppingCartItemFromCartItems()
        {
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                CustomerUsername = "Testuser1",
                CartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Dish = dish,
                        Quantity = 1
                    }
                }
            };

            // Act
            cart.RemoveOneFromCart(dishId);

            // Assert
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public void RemoveOneFromCart_OtherItemsAlsoInCart_ShouldRemoveOnlySpecificItemFromCart()
        {
            // Arrange 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            var cart = new ShoppingCart
            {
                CustomerUsername = "Testuser1",
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
            cart.RemoveOneFromCart(dishId);

            // Assert
            Assert.DoesNotContain(cart.CartItems, item => item.Dish.Id == dishId && item.Dish.Name == "Dish1");
            Assert.Single(cart.CartItems);
            Assert.Contains(cart.CartItems, item => item.Dish.Name == "Dish2" && item.Quantity == 1);
        }

        [Fact]
        public void RemoveOneFromCart_ItemNotInCart_ShouldThrowItemNotInCartThrowException()
        {
            //Arrange
            var cart = new ShoppingCart { CustomerUsername = "TestUser1" }; 

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => cart.RemoveOneFromCart(Guid.NewGuid()));
            Assert.Equal("Item not found in cart", exception.Message);
        } 
    }
}
