namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IRecordSetFieldItemModel : IDataListItemModel
    {
        IRecordSetItemModel Parent { get; set; }

        void Filter(string searchText);
    }
}