namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewItem
    {
        string Name { get; set; }
        bool Input { get; set; }
        bool Output { get; set; }
        string Notes { get; set; }
        bool Used { get; set; }

    }
}