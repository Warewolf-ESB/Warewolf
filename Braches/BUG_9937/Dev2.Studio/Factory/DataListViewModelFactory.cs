using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Studio.Factory {
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
