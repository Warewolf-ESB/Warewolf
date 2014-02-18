namespace Dev2.Diagnostics
{
    public interface IDebugItemResult
    {
        DebugItemResultType Type { get; set; }
        string Label { get; set; }
        string Operator { get; set; }
        string Variable { get; set; }
        string Value { get; set; }
        string GroupName { get; set; }
        int GroupIndex { get; set; }
        string MoreLink { get; set; }
    }
}