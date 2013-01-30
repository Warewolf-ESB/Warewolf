using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Converters
{
    public class BaseTypeException : Exception
    {
        public BaseTypeException(string msg) : base(msg)
        {
        }
    }
}
