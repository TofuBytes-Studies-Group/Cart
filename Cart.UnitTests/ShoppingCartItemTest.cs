using System.Threading.Tasks;
using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.UnitTests
{
    public class ShoppingCartItemTest
    {
        [Theory]
        [InlineData(1, 1, 1, null)]
        [InlineData(2, 2, 4, null)]
        [InlineData(10, 5, 50, null)]
        [InlineData(int.MaxValue, 2, 0, "Cannot calculate price of items")]
        public void New_ShouldCalculateSumPrice(int price, int quantity, int expectedSum, string expectedErrorMessage)
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

            if (expectedErrorMessage != null)
            {
                // Act & Assert
                var exception = Assert.Throws<SumPriceMaxValueException>(() => cartItem.SumPrice);
                Assert.Equal(expectedErrorMessage, exception.Message);
            }
            else
            {
                // Act
                var actualSum = cartItem.SumPrice;

                // Assert
                Assert.Equal(expectedSum, actualSum);
            }
        }
    }
}