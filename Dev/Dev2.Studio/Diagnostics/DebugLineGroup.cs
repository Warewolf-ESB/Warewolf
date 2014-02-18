using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class DebugLineGroup : IDebugLineItem
    {
        public DebugLineGroup(string groupName, string groupLabel)
        {
            GroupName = groupName;
            Rows = new Dictionary<int, DebugLineGroupRow>();
            GroupLabel = groupLabel;
        }

        public string MoreLink { get; set; }
        public string GroupName { get; set; }
        public string GroupLabel { get; set; }
        public Dictionary<int, DebugLineGroupRow> Rows { get; private set; }
    }
}