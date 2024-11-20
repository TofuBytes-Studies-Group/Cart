namespace Cart.Domain.Entities
{
    public class Dish
    {
        public Guid Id { get; set; } 
        public required string Name { get; set; }
        public int Price { get; set; }
    }
}