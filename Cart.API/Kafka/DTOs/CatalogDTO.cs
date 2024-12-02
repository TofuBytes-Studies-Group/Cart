using Cart.Domain.Entities;

namespace Cart.API.Kafka.DTOs
{
    public class CatalogDTO
    {
        public Guid CustomerId { get; set; }
        public Guid RestaurantId { get; set; }
        public required string CustomerUsername { get; set; }
        public required List<Dish> Dishes { get; set; }
    }
}
