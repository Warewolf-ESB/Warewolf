
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class DebugLineGroupRow
    {
        public DebugLineGroupRow()
        {
            LineItems = new List<IDebugLineItem>();
        }

        public List<IDebugLineItem> LineItems { get; private set; }
    }
}
