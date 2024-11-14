using Cart.Domain.Entities;

namespace Cart.UnitTests
{
    public class ShoppingCartItemTest
    {
        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 2, 4)]
        [InlineData(10, 5, 50)]
        public void New_ShouldCalculateSumPrice(int price, int quantity, int expectedSum)
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
    }
}