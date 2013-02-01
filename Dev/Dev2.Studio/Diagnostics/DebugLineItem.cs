using Dev2.Diagnostics;

namespace Dev2.Studio.Diagnostics
{
    public class DebugLineItem : IDebugLineItem
    {
        public DebugLineItem()
        {
        }

        public DebugLineItem(IDebugItemResult result)
        {
            Type = result.Type;
            Value = result.Value;
        }

        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
    }
}