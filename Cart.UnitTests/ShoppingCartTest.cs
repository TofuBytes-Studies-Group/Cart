using Cart.Domain.Aggregates;
using Cart.Domain.Entities;
using Cart.UnitTests.Helpers;
using System.Diagnostics;

namespace Cart.UnitTests
{
    public class ShoppingCartTest
    {
        
        [Theory]
        // Arrange
        [ClassData(typeof(ShoppingCartTestData))]
        public void New_ShouldCalculateTotalPrice(ShoppingCart shoppingCart, int expectedTotalPrice)
        {
            // Act
            var actualTotalPrice = shoppingCart.TotaltPrice;

            // Assert
            Assert.Equal(expectedTotalPrice, actualTotalPrice);
        }
    }
}
