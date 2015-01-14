namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDataListViewItem
    {
        string Name { get; }
        bool Input { get; set; }
        bool Output { get; set; }
        string Notes { get; }
        bool Used { get; set; }
        bool Visible { get; set; }
    }
}