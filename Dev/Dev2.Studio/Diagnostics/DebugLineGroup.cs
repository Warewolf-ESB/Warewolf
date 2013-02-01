using System.Collections.Generic;

namespace Dev2.Studio.Diagnostics
{
    public class DebugLineGroup : IDebugLineItem
    {
        public DebugLineGroup(string groupName)
        {
            GroupName = groupName;
            Rows = new Dictionary<int, DebugLineGroupRow>();
        }

        public string GroupName { get; set; }
        public Dictionary<int, DebugLineGroupRow> Rows { get; private set; }

        public string MoreText { get; set; }
        public string MoreLink { get; set; }
    }
}