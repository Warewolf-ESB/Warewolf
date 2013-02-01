namespace Dev2.Diagnostics
{
    public class DebugItemResult : IDebugItemResult
    {
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
    }
}