using Cart.API.Kafka.DTOs;

namespace Cart.API.Services
{
    public interface IKafkaConsumerService
    {
        void ProcessMessageAsync(CatalogDTO catalogDTO);
    }
}
