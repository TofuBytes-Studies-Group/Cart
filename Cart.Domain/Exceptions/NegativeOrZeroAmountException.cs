namespace Cart.Domain.Exceptions
{
    public class NegativeOrZeroAmountException : Exception
    {
        public NegativeOrZeroAmountException(string message) : base(message) { }
    }
}
