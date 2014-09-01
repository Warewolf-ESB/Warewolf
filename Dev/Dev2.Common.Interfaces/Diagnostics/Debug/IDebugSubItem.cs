using System;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public interface IDebugSubItem
    {
        string Data { get; set; }
        Type Type { get; set; }
    }
}
