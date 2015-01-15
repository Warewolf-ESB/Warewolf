namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewColumn : IVariableListViewItem
    {
        string ColumnName { get; set; }
        string RecordsetName { get; set; }
    }


}