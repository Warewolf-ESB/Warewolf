using System;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IComplexObjectItemModel : IDataListItemModel, IEquatable<IComplexObjectItemModel>
    {
        ObservableCollection<IComplexObjectItemModel> Children { get; set; }
        bool IsArray { get; set; }
        IComplexObjectItemModel Parent { get; set; }
        string GetJson();

        void Filter(string searchText);
    }
}