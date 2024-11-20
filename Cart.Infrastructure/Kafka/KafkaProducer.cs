using Cart.Infrastructure.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Card.Infrastructure.Kafka
{
    public class KafkaProducer: IKafkaProducer, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<ShoppingCart>(string topic, string key, ShoppingCart value)
        {
            try
            {
                string json = JsonConvert.SerializeObject(value);

                // Construct the message to be sent with the key-value pair (same types as the producer expects).
                var message = new Message<string, string> { Key = key, Value = json };

                // ProduceAsync sends the message to Kafka.
                // The result contains metadata about the message that we can assign to a var if interested
                var deliveryResult = await _producer.ProduceAsync(topic, message);

                // We can log the deliveryResult fx
                _logger.LogInformation($"Message sent to {deliveryResult.Topic}");
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Error producing message: {e.Error.Reason}");
            }
        }

        // Dispose method ensures the producer is closed and resources are released when done.
        // Should be disposed when done by default because of Singleton use in Program.cs but to be sure:
        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
