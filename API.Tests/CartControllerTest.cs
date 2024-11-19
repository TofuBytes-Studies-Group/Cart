using Cart.API.Controllers;
using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Infrastructure.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    public class CartControllerTest
    {
        internal CartController SetUp()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CartController>>();
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var kafkaService = new KafkaProducerService(kafkaProducerMock.Object);
            var cartService = new CartService(kafkaService);
            var controller = new CartController(
                    loggerMock.Object,
                    kafkaService,
                    cartService);
            return controller;
        }


        [Fact]
        public void GetCart_UserHasNoCart_ShouldReturnEmptyCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "nonexistentUser";

            // Act
            var result = controller.GetCart(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.NotNull(cart);
            Assert.Equal(username, cart.Username);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public void GetCart_UserAlreadyHasCart_ShouldReturnUsersCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            controller.AddOneToCart(username, dish);

            // Act
            var result = controller.GetCart(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void AddOneToCart_ItemNotInCart_ShouldAddItemToCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };

            // Act
            var result = controller.AddOneToCart(username, dish);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void AddOneToCart_ItemAlreadyInCart_ShouldAddOneToQuantity()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            controller.AddOneToCart(username, dish);

            // Act
            var result = controller.AddOneToCart(username, dish);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(2, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_ItemAlreadyInCart_ShouldDecreaseQuantityByOne()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            controller.AddOneToCart(username, dish);
            controller.AddOneToCart(username, dish);

            // Act
            var result = controller.RemoveOneFromCart(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_OnlyOneItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            controller.AddOneToCart(username, dish);

            // Act
            var result = controller.RemoveOneFromCart(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_ItemNotInCart_ShouldReturnNotFound()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act
            var result = controller.RemoveOneFromCart(username, dishId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Item not found in cart", notFoundResult.Value);
        }

        [Fact]
        public void RemoveAllFromCart_ItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1"; 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            controller.AddOneToCart(username, dish);
            controller.AddOneToCart(username, dish);

            // Act
            var result = controller.RemoveAllFromCart(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public void RemoveAllFromCart_ItemNotInCart_ShouldReturnNotFound()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act
            var result = controller.RemoveAllFromCart(username, dishId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Item not found in cart", notFoundResult.Value);
        }
    }
}
