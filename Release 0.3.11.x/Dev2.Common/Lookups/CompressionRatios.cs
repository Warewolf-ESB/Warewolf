
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
