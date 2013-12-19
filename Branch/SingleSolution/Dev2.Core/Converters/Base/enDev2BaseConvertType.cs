using System.ComponentModel;

namespace Dev2.Converters
{
    /// <summary>
    /// The base convert types available in the system
    /// </summary>
    public enum enDev2BaseConvertType
    {
        [Description("Text")]
        Text,

        [Description("Binary")]
        Binary,

        [Description("Hex")]
        Hex,

        [Description("Base 64")]
        Base64
    }
}
