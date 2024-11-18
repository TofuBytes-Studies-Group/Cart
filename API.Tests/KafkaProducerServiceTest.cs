using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Infrastructure.Kafka;
using Moq;

namespace API.Tests
{
    public class KafkaProducerServiceTest
    {
        [Fact]
        public async void Produce_ShouldProduceToCorrectTopicAndWithKeyFromService()
        { 
            // Arrange
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var service = new KafkaProducerService(kafkaProducerMock.Object);

            var cart = new ShoppingCart { Username = "TestUser1" };

            kafkaProducerMock
                .Setup(p => p.ProduceAsync<ShoppingCart>(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShoppingCart>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.Produce(cart);

            // Assert
            kafkaProducerMock.Verify(p => p.ProduceAsync<ShoppingCart>("topic", "Key", cart),
                Times.Once);

            kafkaProducerMock.VerifyNoOtherCalls();
        }
    }
}