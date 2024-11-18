using System.Threading.Tasks;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.UnitTests
{
    public class ShoppingCartItemTest
    {
        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 2, 4)]
        [InlineData(10, 5, 50)]
        public void New_ShouldCalculateCorrectSumPrice(int price, int quantity, int expectedSum)
        {
            // Arrange 
            var dish = new Dish
            {
                Id = Guid.NewGuid(),
                Name = "Dish1",
                Price = price
            };

            var cartItem = new ShoppingCartItem
            {
                Dish = dish,
                Quantity = quantity
            };

            // Act
            var actualSum = cartItem.SumPrice;

            // Assert
            Assert.Equal(expectedSum, actualSum);
        }

        [Theory]
        [InlineData(int.MaxValue, 2, "Cannot calculate price of items")]
        [InlineData(2, int.MaxValue, "Cannot calculate price of items")]
        public void New_WithMaxIntValue_ShouldThrowSumPriceMaxValueException(int price, int quantity, string expectedErrorMessage)
        {
            // Arrange
            var dish = new Dish
            {
                Id = Guid.NewGuid(),
                Name = "Dish1",
                Price = price
            };

            var cartItem = new ShoppingCartItem
            {
                Dish = dish,
                Quantity = quantity
            };

            // Act & Assert
            var exception = Assert.Throws<SumPriceMaxValueException>(() => cartItem.SumPrice);
            Assert.Equal(expectedErrorMessage, exception.Message);
        }
    }
}