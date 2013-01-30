using System.Collections.Generic;
using Dev2.Diagnostics;

namespace Dev2.Studio.Diagnostics
{
    public class DebugOuputInputOutputGroup
    {
        public DebugOuputInputOutputGroup(string name)
        {
            Name = name;
            Items = new List<IDebugItem>();
        }

        public string Name { get; set; }
        public List<IDebugItem> Items { get; set; }
    }
}
