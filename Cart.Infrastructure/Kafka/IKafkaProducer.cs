namespace Cart.Infrastructure.Kafka
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string key, T value);
    }
}
