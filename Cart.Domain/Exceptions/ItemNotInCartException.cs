using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cart.Domain.Exceptions
{
    public class ItemNotInCartException : Exception
    {
        public ItemNotInCartException(string message) : base(message) { }
    }
}
