using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition.Hosting;
using Dev2.Studio.Core.ViewModels;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.DataList;

namespace Dev2.Studio.Core.Factories {
    public static class DataListViewModelFactory 
    {     
        public static IDataListViewModel CreateDataListViewModel(IResourceModel resourceModel)
        {
            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);

            return dataListViewModel;
        }
    }
}
