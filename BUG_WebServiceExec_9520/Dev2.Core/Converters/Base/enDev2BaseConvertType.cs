using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;

namespace Dev2.Converters
{
    /// <summary>
    /// The base convert types available in the system
    /// </summary>
    [EnumDisplayString("Text", "Binary", "Hex", "Base 64")]
    public enum enDev2BaseConvertType
    {
        Text,
        Binary,
        Hex,
        Base64
    }
}
