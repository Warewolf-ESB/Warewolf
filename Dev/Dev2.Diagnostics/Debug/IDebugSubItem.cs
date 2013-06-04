using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Diagnostics
{
    public interface IDebugSubItem
    {
        string Data { get; set; }
        Type Type { get; set; }
    }
}
