using Cart.API.Kafka.DTOs;
using Cart.API.Services;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Card.API.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumer> _logger;

        private readonly IConsumer<string, string> _consumer;
        private readonly IKafkaConsumerService _consumerService;

        public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger, IKafkaConsumerService consumerService)
        {
            _configuration = configuration;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                // TODO
                GroupId = "groupId",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumerService = consumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("add.to.cart");

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

                            _logger.LogInformation($"Received Message: {message}, Key: {key}");
                            
                            var catalogDto = JsonConvert.DeserializeObject<CatalogDTO>(message);
                            if (catalogDto != null)
                            {
                                _consumerService.ProcessMessageAsync(catalogDto);
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Error consuming Kafka message: {ex.Message}");
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
