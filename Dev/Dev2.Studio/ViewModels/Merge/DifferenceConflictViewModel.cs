using System.Activities.Presentation.Model;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.ViewModels.Merge
{
    public class DifferenceConflictViewModel : ConflictViewModelBase
    {
        public DifferenceConflictViewModel(ModelItem modelItem,IContextualResourceModel contextualResource)
            : base(modelItem)
        {          
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(contextualResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }
    }
}