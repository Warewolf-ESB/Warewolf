namespace Dev2.Diagnostics
{
    public interface IDebugItemResult
    {
        DebugItemResultType Type { get; set; }
        string Value { get; set; }
        string GroupName { get; set; }
        int GroupIndex { get; set; }
    }
}