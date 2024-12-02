using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;
using Cart.Infrastructure.Repositories;
using Moq;

namespace API.Tests
{
    public class CartServiceTest
    {
        internal CartService SetUp()
        {
            // Arrange 
            var cartDataStore = new Dictionary<string, ShoppingCart>();
            var cartRepositoryMock = new Mock<ICartRepository>();

            cartRepositoryMock
                .Setup(repo => repo.GetCartAsync(It.IsAny<string>()))
                .ReturnsAsync((string username) =>
                {
                    cartDataStore.TryGetValue(username, out var cart);
                    return cart;
                });

            cartRepositoryMock
                .Setup(repo => repo.SaveCartAsync(It.IsAny<ShoppingCart>()))
                .Callback((ShoppingCart cart) =>
                {
                    cartDataStore[cart.CustomerUsername] = cart;
                })
                .Returns(Task.CompletedTask);

            cartRepositoryMock
                .Setup(repo => repo.DeleteCartAsync(It.IsAny<string>()))
                .Callback((string username) =>
                {
                    cartDataStore.Remove(username);
                })
                .Returns(Task.CompletedTask);

            return new CartService(cartRepositoryMock.Object);
        }

        [Fact]
        public async void GetCartAsync_UserHasNoCart_ShouldReturnEmptyCart()
        {
            // Arrange 
            var service = SetUp();

            var username = "nonexistentUser";

            // Act 
            var cart = await service.GetCartAsync(username);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.CustomerUsername);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public async void GetCartAsync_UserAlreadyHasCart_ShouldReturnUsersCart()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            await service.AddOneToCartAsync(username, dish);

            // Act
            var cart = await service.GetCartAsync(username);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.CustomerUsername);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void AddOneToCartAsync_ItemNotInCart_ShouldAddItemToCart()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };

            // Act
            var cart = await service.AddOneToCartAsync(username, dish);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.CustomerUsername);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void AddOneToCartAsync_ItemAlreadyInCart_ShouldAddOneToQuantity()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            await service.AddOneToCartAsync(username, dish);

            // Act
            var cart = await service.AddOneToCartAsync(username, dish);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(2, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.TotalPrice);
        }

        [Fact]
        public async void RemoveOneFromCartAsync_ItemAlreadyInCart_ShouldDecreaseQuantityByOne()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await service.AddOneToCartAsync(username, dish);
            await service.AddOneToCartAsync(username, dish);

            // Act
            var cart = await service.RemoveOneFromCartAsync(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void RemoveOneFromCartAsync_OnlyOneItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await service.AddOneToCartAsync(username, dish);

            // Act
            var cart = await service.RemoveOneFromCartAsync(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public async void RemoveOneFromCartAsync_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ItemNotInCartException>(
                async () => await service.RemoveOneFromCartAsync(username, dishId));
            Assert.Equal("Item not found in cart", exception.Message);
        }

        [Fact]
        public async void RemoveAllFromCartAsync_ItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await service.AddOneToCartAsync(username, dish);
            await service.AddOneToCartAsync(username, dish);

            // Act
            var cart = await service.RemoveAllFromCartAsync(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public async void RemoveAllFromCartAsync_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ItemNotInCartException>(
                async () => await service.RemoveAllFromCartAsync(username, dishId));
            Assert.Equal("Item not found in cart", exception.Message);
        }
    }
}
