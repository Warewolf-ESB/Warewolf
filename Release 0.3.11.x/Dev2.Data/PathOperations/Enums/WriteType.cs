using System.ComponentModel;

namespace Dev2.PathOperations
{
    public enum WriteType {
        [Description("Append Top")]
        AppendTop,
        [Description("Append Bottom")]
        AppendBottom,
        [Description("Overwrite")]
        Overwrite
    }
}