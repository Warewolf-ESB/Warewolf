using System;

namespace Dev2.Diagnostics
{
    public interface IDebugSubItem
    {
        string Data { get; set; }
        Type Type { get; set; }
    }
}
