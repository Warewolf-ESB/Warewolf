using System.Xml.Serialization;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public interface IDebugItemResult : IXmlSerializable
    {
        DebugItemResultType Type { get; set; }
        string Label { get; set; }
        string Operator { get; set; }
        string Variable { get; set; }
        string Value { get; set; }
        string GroupName { get; set; }
        int GroupIndex { get; set; }
        string MoreLink { get; set; }

        string GetMoreLinkItem();
    }
}