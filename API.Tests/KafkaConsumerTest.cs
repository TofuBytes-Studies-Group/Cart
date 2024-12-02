using Confluent.Kafka;
using Moq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Card.API.Kafka;
using Cart.API.Services;
using Cart.API.Kafka.DTOs;
using Cart.Domain.Entities;

namespace API.Tests
{
    public class KafkaConsumerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<KafkaConsumer>> _mockLogger;
        private readonly Mock<IKafkaConsumerService> _mockConsumerService;
        private readonly Mock<IConsumer<string, string>> _mockConsumer;

        public KafkaConsumerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<KafkaConsumer>>();
            _mockConsumerService = new Mock<IKafkaConsumerService>();
            _mockConsumer = new Mock<IConsumer<string, string>>();

            _mockConfiguration.Setup(config => config["Kafka:BootstrapServers"]).Returns("localhost:9092");
        }

        private KafkaConsumer CreateKafkaConsumer()
        {
            var kafkaConsumer = new KafkaConsumer(
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockConsumerService.Object
            );

            typeof(KafkaConsumer)
                .GetField("_consumer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(kafkaConsumer, _mockConsumer.Object);

            return kafkaConsumer;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldProcesMessageCorrect()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();
            var sampleCatalogDto = new CatalogDTO
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                CustomerUsername = "Test User",
                Dishes = new List<Dish>
                {
                    new Dish { Id = Guid.NewGuid(), Name = "Dish1", Price = 100 },
                    new Dish { Id = Guid.NewGuid(), Name = "Dish2", Price = 60 }
                }
            };
            var sampleMessage = JsonConvert.SerializeObject(sampleCatalogDto);

            _mockConsumer
                .Setup(consumer => consumer.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string>
                    {
                        Key = "key",
                        Value = sampleMessage
                    }
                });

            var kafkaConsumer = CreateKafkaConsumer();

            // Act
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await kafkaConsumer.StartAsync(cts.Token);

            // Assert
            _mockConsumerService.Verify(
                service => service.ProcessMessageAsync(It.Is<CatalogDTO>(dto =>
                    dto.CustomerId == customerId &&
                    dto.CustomerUsername == "Test User"
                )),
                Times.Once
            );

            _mockConsumer.Verify(consumer => consumer.Subscribe("add.to.cart"), Times.Once);
            _mockConsumer.Verify(consumer => consumer.Consume(It.IsAny<TimeSpan>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleConsumeException()
        {
            // Arrange
            _mockLogger.Setup(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _mockConsumer
                .Setup(consumer => consumer.Consume(It.IsAny<TimeSpan>()))
                .Throws(new ConsumeException(new ConsumeResult<byte[], byte[]>(), new Error(ErrorCode.InvalidMsg)));

            var kafkaConsumer = CreateKafkaConsumer();

            // Act
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await kafkaConsumer.StartAsync(cts.Token);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Error consuming Kafka message:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleOperationCanceledException()
        {
            // Arrange
            _mockLogger.Setup(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _mockConsumer
                .Setup(consumer => consumer.Consume(It.IsAny<TimeSpan>()))
                .Throws(new OperationCanceledException());

            var kafkaConsumer = CreateKafkaConsumer();

            // Act
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await kafkaConsumer.StartAsync(cts.Token);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Kafka consumer stopping gracefully.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Kafka consumer has stopped.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
