using Cart.Domain.Aggregates;

namespace Cart.Infrastructure.Repositories
{
    public interface ICartRepository
    {
        Task<ShoppingCart?> GetCartAsync(string username);
        Task SaveCartAsync(ShoppingCart cart);
        Task DeleteCartAsync(string username);
    }
}
