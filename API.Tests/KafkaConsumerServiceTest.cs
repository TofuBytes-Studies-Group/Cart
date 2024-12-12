using Cart.API.Kafka.DTOs;
using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Moq;

namespace API.Tests
{
    public class KafkaConsumerServiceTest
    {
        private readonly Mock<IKafkaProducerService> _mockKafkaProducerService;
        private readonly KafkaConsumerService _kafkaConsumerService;

        public KafkaConsumerServiceTest()
        {
            _mockKafkaProducerService = new Mock<IKafkaProducerService>();
            _kafkaConsumerService = new KafkaConsumerService(_mockKafkaProducerService.Object);
        }

        [Fact]
        public void ProcessMessageAsync_ShouldConvertCatalogDtoToShoppingCartAndProduceMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();

            var catalogDTO = new CatalogDTO
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                CustomerUsername = "TestUser",
                Dishes = new List<Dish>
                {
                    new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 100 },
                    new Dish { Id = Guid.NewGuid(), Name = "Dish2", Price = 60 }
                }
            };

            ShoppingCart expectedCart = new ShoppingCart(customerId, restaurantId, catalogDTO.CustomerUsername);
            foreach (var dish in catalogDTO.Dishes)
            {
                expectedCart.AddOneToCart(dish);
            }

            // Act
            _kafkaConsumerService.ProcessMessageAsync(catalogDTO);

            // Assert
            _mockKafkaProducerService.Verify(
                producer => producer.Produce(It.Is<ShoppingCart>(cart =>
                    cart.CustomerId == expectedCart.CustomerId &&
                    cart.RestaurantId == expectedCart.RestaurantId &&
                    cart.CustomerUsername == expectedCart.CustomerUsername &&
                    cart.CartItems.Count == expectedCart.CartItems.Count &&
                    cart.CartItems.All(cartItem =>
                        expectedCart.CartItems.Any(expectedItem =>
                            expectedItem.Dish.Id == cartItem.Dish.Id &&
                            expectedItem.Quantity == cartItem.Quantity)
                        )
                )),
                Times.Once
            );
        }
    }
}
