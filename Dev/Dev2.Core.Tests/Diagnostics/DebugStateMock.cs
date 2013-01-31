using Dev2.Diagnostics;
using System.Collections.Generic;

namespace Dev2.Tests.Diagnostics
{
    public class DebugStateMock : DebugState
    {
        public int SaveGroupHitCount { get; set; }
        public Dictionary<string, IList<IDebugItem>> SaveGroupItems { get; private set; }

        public DebugStateMock()
        {
            SaveGroupItems = new Dictionary<string, IList<IDebugItem>>();
        }

        public override string SaveGroup(IList<IDebugItem> items, string group)
        {
            SaveGroupHitCount++;
            var arr = new IDebugItem[items.Count];
            items.CopyTo(arr, 0);
            SaveGroupItems.Add(group, arr);
            return null;
        }
    }
}
