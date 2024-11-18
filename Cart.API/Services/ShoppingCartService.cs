namespace Cart.API.Services
{
    public class ShoppingCartService
    {
        private readonly KafkaProducerService _kafkaProducerService;

        public ShoppingCartService(KafkaProducerService kafkaProducerService)
        {
            _kafkaProducerService = kafkaProducerService;
        }


    }
}
