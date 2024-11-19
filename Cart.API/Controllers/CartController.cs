using Microsoft.AspNetCore.Mvc;
using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ILogger<CartController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly CartService _cartService;

        public CartController(ILogger<CartController> logger, KafkaProducerService kafkaProducerService, CartService cartService)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _cartService = cartService;
        }

        [HttpGet("{username}")]
        public ActionResult<ShoppingCart> GetCart(string username)
        {
            _logger.LogInformation("Fetching cart for user: {Username}", username);

            var cart = _cartService.GetCart(username);
            _logger.LogInformation("Cart fetched successfully for user: {Username}", username);

            return Ok(cart);
        }

        [HttpPost("{username}/add-one")]
        public ActionResult<ShoppingCart> AddOneToCart(string username, [FromBody] Dish dish)
        {
            if (dish == null)
            {
                _logger.LogWarning("AddOneToCart failed: Dish is null for user: {Username}", username);
                return BadRequest("Dish cannot be null.");
            }

            _logger.LogInformation("Adding dish to cart for user: {Username}, Dish: {DishName}", username, dish.Name);
            var cart = _cartService.AddOneToCart(username, dish);
            _logger.LogInformation("Dish added to cart successfully for user: {Username}, Dish: {DishName}", username, dish.Name);

            return Ok(cart);
        }

        [HttpDelete("{username}/remove-one/{dishId}")]
        public ActionResult<ShoppingCart> RemoveOneFromCart(string username, Guid dishId)
        {
            try
            {
                _logger.LogInformation("Removing one instance of dish from cart for user: {Username}, DishId: {DishId}", username, dishId);
                var cart = _cartService.RemoveOneFromCart(username, dishId);
                _logger.LogInformation("Dish removed successfully from cart for user: {Username}, DishId: {DishId}", username, dishId);

                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                _logger.LogWarning("RemoveOneFromCart failed for user: {Username}, DishId: {DishId}. Reason: {Error}", username, dishId, ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{username}/remove-all/{dishId}")]
        public ActionResult<ShoppingCart> RemoveAllFromCart(string username, Guid dishId)
        {
            try
            {
                _logger.LogInformation("Removing all instances of dish from cart for user: {Username}, DishId: {DishId}", username, dishId);
                var cart = _cartService.RemoveAllFromCart(username, dishId);
                _logger.LogInformation("All instances of dish removed successfully from cart for user: {Username}, DishId: {DishId}", username, dishId);

                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                _logger.LogWarning("RemoveAllFromCart failed for user: {Username}, DishId: {DishId}. Reason: {Error}", username, dishId, ex.Message);
                return NotFound(ex.Message);
            }
        }
    }
}
