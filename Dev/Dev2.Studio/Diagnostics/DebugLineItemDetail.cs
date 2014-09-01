using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Studio.Diagnostics
{
    public class DebugLineItemDetail
    {
        public DebugLineItemDetail()
        {
        }

        public DebugLineItemDetail(DebugItemResultType type, string value, string moreLink, string variable)
        {
            Type = type;
            Value = value;
            MoreLink = moreLink;
            Variable = variable;
        }

        public string MoreLink { get; set; }
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
        public string Variable { get; set; }
    }
}