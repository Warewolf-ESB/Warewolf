using Dev2.Diagnostics;

// ReSharper disable once CheckNamespace
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
            MoreLink = result.MoreLink;
        }

        public string MoreLink { get; set; }
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }


    }
}