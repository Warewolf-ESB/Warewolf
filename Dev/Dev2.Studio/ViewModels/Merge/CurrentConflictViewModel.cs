using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.ViewModels.Merge
{
    public class CurrentConflictViewModel : BindableBase, ICurrentConflictViewModel
    {
        private string _workflowName;
        
        public CurrentConflictViewModel()
        {
            #region MockToolSetup
            MergeConflicts = new ObservableCollection<IMergeToolModel>();

            DataListSingleton.SetDataList(new DataListViewModel());
            var assign = new DsfMultiAssignActivity();
            assign.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));

            //ASSIGN
            MergeToolModel = new MergeToolModel();
            var mergeAssignVM = MergeToolModel as MergeToolModel;
            mergeAssignVM.IsMergeExpanded = true;
            mergeAssignVM.IsMergeExpanderEnabled = true;
            mergeAssignVM.MergeDescription = "Current Assign (0)";
            mergeAssignVM.SetMergeIcon(typeof(DsfMultiAssignActivity));
            mergeAssignVM.ActivityDesignerViewModel = new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(assign));

            MergeConflicts.Add(mergeAssignVM);

            //DECISION
            var decision = new DsfFlowDecisionActivity();
            MergeToolModel = new MergeToolModel();
            var mergeDecisionVM = MergeToolModel as MergeToolModel;
            mergeDecisionVM.IsMergeExpanded = true;
            mergeDecisionVM.IsMergeExpanderEnabled = true;
            mergeDecisionVM.MergeDescription = "Current Decision (0)";
            mergeDecisionVM.SetMergeIcon(typeof(DsfFlowDecisionActivity));
            mergeDecisionVM.ActivityDesignerViewModel = new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(decision));

            MergeConflicts.Add(mergeDecisionVM);

            #endregion

            string newWorflowName = NewWorkflowNames.Instance.GetNext();
            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(tempResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }

        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                OnPropertyChanged(() => WorkflowName);
            }
        }
        
        public DataListViewModel DataListViewModel { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public ObservableCollection<IMergeToolModel> MergeConflicts { get; set; }
    }
}