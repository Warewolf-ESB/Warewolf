using System.Collections.ObjectModel;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IRecordSetItemModel : IDataListItemModel
    {
        ObservableCollection<IRecordSetFieldItemModel> Children { get; set; }

        void Filter(string searchText);
    }
}