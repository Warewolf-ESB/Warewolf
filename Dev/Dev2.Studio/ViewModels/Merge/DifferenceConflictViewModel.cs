using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.ViewModels.Merge
{
    public class DifferenceConflictViewModel : ConflictViewModelBase
    {
        public DifferenceConflictViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            string newWorflowName = NewWorkflowNames.Instance.GetNext();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(shellViewModel.ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(tempResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }
    }
}