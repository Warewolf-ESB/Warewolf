using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Studio.Core.Factories;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Interfaces {
    public interface IInputOutputViewModel {
        //ObservableCollection<IDataListItemModel> DataList { get; }
        //IInputOutputViewModelFactory InputOutputViewModelFactory { get; set; }
        string Name { get; set; }
        bool IsSelected { get; set; }
        string Value { get; set; }
        string MapsTo { get; set; }
        string DefaultValue { get; set; }
        bool Required { get; set; }
        string RecordSetName{get;set;}
        string DisplayName { get; set; }
        string DisplayDefaultValue { get; }

        bool IsNew { get; set; }
        bool RequiredMissing { get; set; }

        IDev2Definition GetGenerationTO();
    }
}
