using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class DebugLineGroup : IDebugLineItem
    {
        public DebugLineGroup(string groupName)
        {
            GroupName = groupName;
            Rows = new Dictionary<int, DebugLineGroupRow>();
        }

        public string MoreLink { get; set; }
        public string GroupName { get; set; }
        public Dictionary<int, DebugLineGroupRow> Rows { get; private set; }
    }
}