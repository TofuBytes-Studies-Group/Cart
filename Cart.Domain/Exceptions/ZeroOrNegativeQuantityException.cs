using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cart.Domain.Exceptions
{
    public class ZeroOrNegativeQuantityException : Exception
    {
        public ZeroOrNegativeQuantityException(string message) : base(message) { }
    }
}
