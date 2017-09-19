using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.ViewModels.Merge
{
    public class CurrentConflictViewModel : ConflictViewModelBase
    {
        public CurrentConflictViewModel(ModelItem modelItem,IContextualResourceModel resourceModel) 
            : base(modelItem)
        {
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }
    }
}