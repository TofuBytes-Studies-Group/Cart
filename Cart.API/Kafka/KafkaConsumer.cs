using Cart.API.Kafka.DTOs;
using Cart.API.Services;
using Cart.API.Utils;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Card.API.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly ILogger<KafkaConsumer> _logger;

        private readonly IConsumer<string, string> _consumer;
        private readonly IKafkaConsumerService _consumerService;
        private readonly string? _topic;

        public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger, IKafkaConsumerService consumerService)
        {
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _topic = configuration["Kafka:ConsumerTopic"];

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumerService = consumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Kafka consumer is running.");
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5)); // Is here to not block swagger 

                        if (consumeResult != null)
                        {
                            var message = consumeResult.Message.Value;
                            var key = consumeResult.Message.Key;

                            _logger.LogInformation($"Received Kafka message with key: {key}");

                            var catalogDto = JsonConvert.DeserializeObject<CatalogDTO>(message);
                            if (catalogDto != null && InputValidator.IsValidCatalogDTO(catalogDto))
                            {
                                _logger.LogInformation($"Processing message");
                                _consumerService.ProcessMessageAsync(catalogDto);
                            }
                            else
                            {
                                _logger.LogError("Invalid catalogDto format.");
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Error consuming Kafka message: {ex.Message}");
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Error deserializing message: {ex.Message}");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping gracefully.");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer has stopped.");
            }
        }
    }
}
