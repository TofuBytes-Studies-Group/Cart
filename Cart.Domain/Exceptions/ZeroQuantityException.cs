using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cart.Domain.Exceptions
{
    public class ZeroQuantityException : Exception
    {
        public ZeroQuantityException(string message) : base(message) { }
    }
}
