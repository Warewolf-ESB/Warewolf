﻿using System.Collections.ObjectModel;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IComplexObjectItemModel : IDataListItemModel
    {
        ObservableCollection<IComplexObjectItemModel> Children { get; set; }
        bool IsArray { get; set; }
        IComplexObjectItemModel Parent { get; set; }
        string GetJson();

        void Filter(string searchText);
    }
}