using Cart.Domain.Aggregates;

namespace Cart.API.Services
{
    public interface IKafkaProducerService
    {
        Task Produce(ShoppingCart cart);
    }
}
