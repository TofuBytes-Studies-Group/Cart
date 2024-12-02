using Microsoft.AspNetCore.Mvc;
using Cart.API.Services;
using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;
using Card.API.Kafka;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ILogger<CartController> _logger;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly CartService _cartService;

        public CartController(ILogger<CartController> logger, IKafkaProducerService kafkaProducerService, CartService cartService)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _cartService = cartService;
        }

        [HttpGet("{customerUsername}")]
        public async Task<ActionResult<ShoppingCart>> GetCartAsync(string customerUsername)
        {
            _logger.LogInformation("Fetching cart for user: {customerUsername}", customerUsername);

            var cart = await _cartService.GetCartAsync(customerUsername);
            _logger.LogInformation("Cart fetched successfully for user: {customerUsername}", customerUsername);

            return Ok(cart);
        }

        [HttpPost("{customerUsername}/add-one")]
        public async Task<ActionResult<ShoppingCart>> AddOneToCartAsync(string customerUsername, [FromBody] Dish dish)
        {
            if (dish == null)
            {
                _logger.LogWarning("AddOneToCart failed: Dish is null for user: {customerUsername}", customerUsername);
                return BadRequest("Dish cannot be null.");
            }

            _logger.LogInformation("Adding dish to cart for user: {customerUsername}, Dish: {DishName}", customerUsername, dish.Name);
            var cart = await _cartService.AddOneToCartAsync(customerUsername, dish);
            _logger.LogInformation("Dish added to cart successfully for user: {customerUsername}, Dish: {DishName}", customerUsername, dish.Name);

            return Ok(cart);
        }

        [HttpDelete("{customerUsername}/remove-one/{dishId}")]
        public async Task<ActionResult<ShoppingCart>> RemoveOneFromCartAsync(string customerUsername, Guid dishId)
        {
            try
            {
                _logger.LogInformation("Removing one instance of dish from cart for user: {customerUsername}, DishId: {DishId}", customerUsername, dishId);
                var cart = await _cartService.RemoveOneFromCartAsync(customerUsername, dishId);
                _logger.LogInformation("Dish removed successfully from cart for user: {customerUsername}, DishId: {DishId}", customerUsername, dishId);

                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                _logger.LogWarning("RemoveOneFromCart failed for user: {customerUsername}, DishId: {DishId}. Reason: {Error}", customerUsername, dishId, ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{customerUsername}/remove-all/{dishId}")]
        public async Task<ActionResult<ShoppingCart>> RemoveAllFromCartAsync(string customerUsername, Guid dishId)
        {
            try
            {
                _logger.LogInformation("Removing all instances of dish from cart for user: {customerUsername}, DishId: {DishId}", customerUsername, dishId);
                var cart = await _cartService.RemoveAllFromCartAsync(customerUsername, dishId);
                _logger.LogInformation("All instances of dish removed successfully from cart for user: {customerUsername}, DishId: {DishId}", customerUsername, dishId);

                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                _logger.LogWarning("RemoveAllFromCart failed for user: {customerUsername}, DishId: {DishId}. Reason: {Error}", customerUsername, dishId, ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{customerUsername}/order")]
        public async Task<ActionResult> OrderAsync(string customerUsername)
        {
            try
            {
                _logger.LogInformation("Processing order for user: {customerUsername}", customerUsername);
                var cart = await _cartService.GetCartAsync(customerUsername);
                if (!cart.CartItems.Any())
                {
                    _logger.LogWarning("Order failed: Cart is empty for user: {customerUsername}", customerUsername);
                    return BadRequest("Cannot place an order with an empty cart");
                }
                _logger.LogInformation("Order creation initiated for user: {customerUsername}", customerUsername);
                await _kafkaProducerService.Produce(cart);

                return Accepted("Your order is being processed");
            }
            catch (Exception ex)
            {
                _logger.LogError("Order failed for user: {customerUsername}, Reason: {Error}", customerUsername, ex.Message);
                return StatusCode(500, "An error occurred while processing your order");
            }
        }

    }
}
