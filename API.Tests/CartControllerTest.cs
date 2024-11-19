using Cart.API.Controllers;
using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Infrastructure.Kafka;
using Cart.Infrastructure.Repositories;
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

            // Kafka Mock 
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var kafkaService = new KafkaProducerService(kafkaProducerMock.Object);

            // Repository Mock 
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
                    cartDataStore[cart.Username] = cart;
                })
                .Returns(Task.CompletedTask);

            cartRepositoryMock
                .Setup(repo => repo.DeleteCartAsync(It.IsAny<string>()))
                .Callback((string username) =>
                {
                    cartDataStore.Remove(username);
                })
                .Returns(Task.CompletedTask);

            var cartService = new CartService(cartRepositoryMock.Object);

            return new CartController(
                loggerMock.Object,
                kafkaService,
                cartService);
        }



        [Fact]
        public async void GetCart_UserHasNoCart_ShouldReturnEmptyCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "nonexistentUser";

            // Act
            var result = await controller.GetCartAsync(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.NotNull(cart);
            Assert.Equal(username, cart.Username);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public async void GetCart_UserAlreadyHasCart_ShouldReturnUsersCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            await controller.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.GetCartAsync(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void AddOneToCart_ItemNotInCart_ShouldAddItemToCart()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };

            // Act
            var result = await controller.AddOneToCartAsync(username, dish);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void AddOneToCart_ItemAlreadyInCart_ShouldAddOneToQuantity()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            await controller.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.AddOneToCartAsync(username, dish);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(2, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.TotalPrice);
        }

        [Fact]
        public async void AddOneToCart_ItemIsNull_ReturnBadRequest()
        {
            // Arrange
            var controller = SetUp();

            var username = "TestUser1";

            // Act
            var result = await controller.AddOneToCartAsync(username, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Dish cannot be null.", badRequestResult.Value);

        }

        [Fact]
        public async void RemoveOneFromCart_ItemAlreadyInCart_ShouldDecreaseQuantityByOne()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await controller.AddOneToCartAsync(username, dish);
            await controller.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.RemoveOneFromCartAsync(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Equal(1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public async void RemoveOneFromCart_OnlyOneItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await controller.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.RemoveOneFromCartAsync(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public async void RemoveOneFromCart_ItemNotInCart_ShouldReturnNotFound()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act
            var result = await controller.RemoveOneFromCartAsync(username, dishId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Item not found in cart", notFoundResult.Value);
        }

        [Fact]
        public async void RemoveAllFromCart_ItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            await controller.AddOneToCartAsync(username, dish);
            await controller.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.RemoveAllFromCartAsync(username, dishId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cart = Assert.IsType<ShoppingCart>(okResult.Value);
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public async void RemoveAllFromCart_ItemNotInCart_ShouldReturnNotFound()
        {
            // Arrange
            var controller = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act
            var result = await controller.RemoveAllFromCartAsync(username, dishId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Item not found in cart", notFoundResult.Value);
        }

        [Fact]
        public async void Order_UserHasCart_ShouldSendKafkaMessageAndReturnAccepted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CartController>>();

            // Kafka Mock
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var kafkaService = new KafkaProducerService(kafkaProducerMock.Object);

            kafkaProducerMock
                .Setup(p => p.ProduceAsync<ShoppingCart>(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShoppingCart>()))
                .Returns(Task.CompletedTask);

            // Repository Mock
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
                    cartDataStore[cart.Username] = cart;
                })
                .Returns(Task.CompletedTask);

            var cartService = new CartService(cartRepositoryMock.Object);
            var controller = new CartController(
                    loggerMock.Object,
                    kafkaService,
                    cartService);

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            var cart = await cartService.AddOneToCartAsync(username, dish);

            // Act
            var result = await controller.OrderAsync(username);

            // Assert
            kafkaProducerMock.Verify(p => p.ProduceAsync<ShoppingCart>(
                "topic", "Key", It.Is<ShoppingCart>(c => c.Username == username && c.CartItems.Count == 1)),
                Times.Once);

            kafkaProducerMock.VerifyNoOtherCalls();

            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            Assert.Equal("Your order is being processed", acceptedResult.Location);
        }

        [Fact]
        public async void Order_UserHasEmptyCart_ShouldReturnBadRequest()
        {
            // Arrange
            var controller = SetUp();
            var username = "nonexistentUser";

            // Act
            var result = await controller.OrderAsync(username);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot place an order with an empty cart", badRequestResult.Value);
        }
    }
}
