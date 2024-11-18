using Cart.Domain.Aggregates;

namespace API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var cart = new ShoppingCart()
            { Username = ""};
        }
    }
}