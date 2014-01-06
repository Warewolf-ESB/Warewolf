using System.ComponentModel;

namespace Dev2.Common.Lookups
{  
    public enum CompressionRatios
    {
        [Description("None (No Compression)")]
        NoCompression,
        [Description("Partial (Best Speed)")]
        BestSpeed,
        [Description("Normal (Default)")]
        Default,
        [Description("Max (Best Compression)")]
        BestCompression
    }
}
