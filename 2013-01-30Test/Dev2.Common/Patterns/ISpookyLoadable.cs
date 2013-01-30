using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common
{
    /// <summary>
    /// Used to represent an class that can be loaded via the spooky action at a distance pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpookyLoadable
    {
        Enum HandlesType();
    }
}
