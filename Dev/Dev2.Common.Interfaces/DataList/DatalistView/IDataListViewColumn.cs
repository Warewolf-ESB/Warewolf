namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDataListViewColumn : IDataListViewItem
    {
        string ColumnName { get; set; }
        string RecordsetName { get; set; }
    }


}