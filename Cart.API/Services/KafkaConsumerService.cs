using Cart.API.Kafka.DTOs;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;

namespace Cart.API.Services
{
    public class KafkaConsumerService : IKafkaConsumerService
    {
        private readonly IKafkaProducerService _kafkaProducerService;

        public KafkaConsumerService(IKafkaProducerService kafkaProducerService)
        {
            _kafkaProducerService = kafkaProducerService;
        }

        public async void ProcessMessageAsync(CatalogDTO catalogDTO)
        {
            var cart = new ShoppingCart()
            {
                CustomerId = catalogDTO.CustomerId,
                RestaurantId = catalogDTO.RestaurantId,
                CustomerUsername = catalogDTO.CustomerUsername
            };

            foreach (Dish dish in catalogDTO.Dishes)
            {
                cart.AddOneToCart(dish);
            }

            await _kafkaProducerService.Produce(cart);
        }
    }
}
