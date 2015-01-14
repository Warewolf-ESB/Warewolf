using System.ComponentModel;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDataListViewScalar : IDataListViewItem
    {
    }



    public enum EnDev2ColumnArgumentDirection
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