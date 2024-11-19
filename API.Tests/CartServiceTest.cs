using Cart.API.Services;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;
using Cart.Infrastructure.Kafka;
using Moq;

namespace API.Tests
{
    public class CartServiceTest
    {
        internal CartService SetUp()
        {
            // Arrange 
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var kafkaService = new KafkaProducerService(kafkaProducerMock.Object);
            var service = new CartService(kafkaService);
            return service;
        }

        [Fact]
        public void GetCart_UserHasNoCart_ShouldReturnEmptyCart()
        {
            // Arrange 
            var service = SetUp();

            var username = "nonexistentUser";

            // Act 
            var cart = service.GetCart(username);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.Username);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public void GetCart_UserAlreadyHasCart_ShouldReturnUsersCart()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            service.AddOneToCart(username, dish);

            // Act
            var cart = service.GetCart(username);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void AddOneToCart_ItemNotInCart_ShouldAddItemToCart()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };

            // Act
            var cart = service.AddOneToCart(username, dish);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(username, cart.Username);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void AddOneToCart_ItemAlreadyInCart_ShouldAddOneToQuantity()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dish = new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 1 };
            service.AddOneToCart(username, dish);

            // Act
            var cart = service.AddOneToCart(username, dish);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(2, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(2, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_ItemAlreadyInCart_ShouldDecreaseQuantityByOne()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1"; 
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            service.AddOneToCart(username, dish);
            service.AddOneToCart(username, dish);

            // Act
            var cart = service.RemoveOneFromCart(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Single(cart.CartItems); 
            Assert.Equal(1, cart.CartItems.Sum(item => item.Quantity));
            Assert.Equal(1, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_OnlyOneItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            service.AddOneToCart(username, dish);

            // Act
            var cart = service.RemoveOneFromCart(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart.CartItems);
            Assert.Equal(0, cart.TotalPrice);
        }

        [Fact]
        public void RemoveOneFromCart_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => service.RemoveOneFromCart(username, dishId));
            Assert.Equal("Item not found in cart", exception.Message);
        }

        [Fact]
        public void RemoveAllFromCart_ItemInCart_ShouldRemoveItemCompletely()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();
            var dish = new Dish { Id = dishId, Name = "Dish1", Price = 1 };
            service.AddOneToCart(username, dish);
            service.AddOneToCart(username, dish);

            // Act
            var cart = service.RemoveAllFromCart(username, dishId);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart.CartItems);
        }

        [Fact]
        public void RemoveAllFromCart_ItemNotInCart_ShouldThrowItemNotInCartException()
        {
            // Arrange
            var service = SetUp();

            string username = "TestUser1";
            var dishId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ItemNotInCartException>(() => service.RemoveAllFromCart(username, dishId));
            Assert.Equal("Item not found in cart", exception.Message);
        }
    }
}
