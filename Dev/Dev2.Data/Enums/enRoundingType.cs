using System.ComponentModel;

namespace Dev2.DataList.Contract
{
    public enum enRoundingType
    {
        [Description("None")]
        None,
        [Description("Normal")]
        Normal,
        [Description("Up")]
        Up,
        [Description("Down")]
        Down,
    }
}
