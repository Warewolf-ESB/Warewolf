using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public interface IDebugOutputFilterStrategy
    {
        bool Filter(object content, string filterText);
    }
}
