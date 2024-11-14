using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cart.Domain.Exceptions
{
    public class SumPriceMaxValueException : Exception
    {
        public SumPriceMaxValueException(string message) : base(message) { }
    }
}
