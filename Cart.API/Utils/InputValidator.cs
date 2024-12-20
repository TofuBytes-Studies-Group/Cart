using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Cart.Domain.Entities;
using Cart.API.Kafka.DTOs;

namespace Cart.API.Utils
{
    public static class InputValidator
    {
        public static bool IsValidUsername(string username)
        {
            var usernameRegex = new Regex("^[a-zA-Z0-9]{3,12}$");
            return usernameRegex.IsMatch(username);
        }

        public static bool IsValidDish(Dish dish)
        {
            return !string.IsNullOrWhiteSpace(dish.Name) && dish.Price > 0;
        }

        public static bool IsValidDishId(Guid? dishId)
        {
            return dishId != null && dishId != Guid.Empty;
        }

        public static bool IsValidCatalogDTO(CatalogDTO dto)
        {
            return dto.CustomerId != Guid.Empty
                   && dto.RestaurantId != Guid.Empty
                   && !string.IsNullOrWhiteSpace(dto.CustomerUsername)
                   && dto.Dishes != null
                   && dto.Dishes.Count != 0;
        }
    }
}
