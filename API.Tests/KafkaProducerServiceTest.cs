using Cart.API.Services;
using Cart.Infrastructure.Kafka;
using Moq;

namespace API.Tests
{
    public class KafkaProducerServiceTest
    {
        [Fact]
        public void DoStuff_CallsProduceAsyncWithCorrectArguments()
        {
            // Arrange
            var kafkaProducerMock = new Mock<IKafkaProducer>();
            var service = new KafkaProducerService(kafkaProducerMock.Object);

            string? actualTopic = null;
            string? actualKey = null;
            string? actualValue = null;

            kafkaProducerMock
                .Setup(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((topic, key, value) =>
                {
                    actualTopic = topic;
                    actualKey = key;
                    actualValue = value;
                })
                .Returns(Task.CompletedTask);

            // Act
            service.DoStuff();

            // Assert
            kafkaProducerMock.Verify(
                producer => producer.ProduceAsync("topic", "Virker", "From DOSTUFF"),
                Times.Once);

            kafkaProducerMock.VerifyNoOtherCalls();

            Assert.Equal("topic", actualTopic);
            Assert.Equal("Virker", actualKey);
            Assert.Equal("From DOSTUFF", actualValue);
        }
    }
}