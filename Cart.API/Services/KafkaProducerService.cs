﻿using Cart.Domain.Aggregates;
using Cart.Infrastructure.Kafka;

namespace Cart.API.Services
{
    public class KafkaProducerService  
    {
        private readonly IKafkaProducer _kafkaProducer;
        public KafkaProducerService(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public async Task Produce(ShoppingCart cart)
        {
            await _kafkaProducer.ProduceAsync<ShoppingCart>("topic", "Key", cart);
        }
    }
}
