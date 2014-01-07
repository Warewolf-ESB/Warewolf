using System.ComponentModel;

namespace Dev2.Data.Binary_Objects
{
    /// <summary>
    /// The Dev2Column direction used for IO Mapping ;)
    /// </summary>
    public enum enDev2ColumnArgumentDirection
    {
        [Description("None")]
        None = 0,
        [Description("Input")]
        Input = 1,
        [Description("Output")]
        Output = 2,
        [Description("Both")]
        Both = 3,
    }
}
