using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Converters
{
    public interface IBaseConversionBroker
    {
        string Convert(string payload);
    }
}
