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
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CartController> _logger;
        private readonly KafkaProducerService _testService;
        private readonly CartService _cartService;

        public CartController(ILogger<CartController> logger, KafkaProducerService testService, CartService cartService)
        {
            _logger = logger;
            _testService = testService;
            _cartService = cartService;
        }


        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Guid: " + Guid.NewGuid().ToString());
            _testService.DoStuff();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("{username}")]
        public ActionResult<ShoppingCart> GetCart(string username)
        {
            var cart = _cartService.GetCart(username);
            return Ok(cart);
        }

        [HttpPost("{username}/add-one")]
        public ActionResult<ShoppingCart> AddOneToCart(string username, [FromBody] Dish dish)
        {
            if (dish == null) return BadRequest("Dish cannot be null.");

            var cart = _cartService.AddOneToCart(username, dish);
            return Ok(cart);
        }

        [HttpDelete("{username}/remove-one/{dishId}")]
        public ActionResult<ShoppingCart> RemoveOneFromCart(string username, Guid dishId)
        {
            try
            {
                var cart = _cartService.RemoveOneFromCart(username, dishId);
                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{username}/remove-all/{dishId}")]
        public ActionResult<ShoppingCart> RemoveAllFromCart(string username, Guid dishId)
        {
            try
            {
                var cart = _cartService.RemoveAllFromCart(username, dishId);
                return Ok(cart);
            }
            catch (ItemNotInCartException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
