namespace Dev2.Diagnostics
{
    public interface IDebugItemResult
    {
        DebugItemResultType Type { get; set; }
        string Value { get; set; }
    }
}