namespace Dev2.Diagnostics
{
    public class DebugItemResult : IDebugItemResult
    {
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
    }
}