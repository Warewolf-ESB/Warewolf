using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Activities.Statements;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private IConflictViewModel _currentConflictViewModel;
        private IConflictViewModel _differenceConflictViewModel;

        public MergeWorkflowViewModel(List<ActivityDesignerViewModel> currentMergeList)
        {


        }

        public MergeWorkflowViewModel()
        {
            CurrentConflictViewModel = new CurrentConflictViewModel {WorkflowName = "Current WorkflowName"};
            DifferenceConflictViewModel = new DifferenceConflictViewModel {WorkflowName = "Difference WorkflowName"};

            DataListSingleton.SetDataList(new DataListViewModel());
            var act = new DsfMultiAssignActivity();
            act.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));


            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(tempResource);
            AddAnItem = new DelegateCommand(o =>
            {

                var step = new FlowStep { Action = act };
                WorkflowDesignerViewModel.AddItem(step);
            });
        }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }
    }

    public class CurrentConflictViewModel : BindableBase, ICurrentConflictViewModel
    {
        private string _workflowName;
        
        public CurrentConflictViewModel()
        {
            #region MockToolSetup
            MergeConflicts = new ObservableCollection<IMergeViewModel>();

            DataListSingleton.SetDataList(new DataListViewModel());
            var assign = new DsfMultiAssignActivity();
            assign.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));

            //ASSIGN
            MergeViewModel = new MergeViewModel();
            var mergeAssignVM = MergeViewModel as MergeViewModel;
            mergeAssignVM.IsMergeExpanded = true;
            mergeAssignVM.IsMergeExpanderEnabled = true;
            mergeAssignVM.MergeDescription = "Current Assign (0)";
            mergeAssignVM.SetMergeIcon(typeof(DsfMultiAssignActivity));
            mergeAssignVM.ActivityDesignerViewModel = new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(assign));

            MergeConflicts.Add(mergeAssignVM);

            //DECISION
            var decision = new DsfFlowDecisionActivity();
            MergeViewModel = new MergeViewModel();
            var mergeDecisionVM = MergeViewModel as MergeViewModel;
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
                _currentConflictViewModel = value;
                OnPropertyChanged(() => CurrentConflictViewModel);

            }
        }

        public IConflictViewModel DifferenceConflictViewModel           
        {
            get => _differenceConflictViewModel;
            set
            {
                _differenceConflictViewModel = value; 
                OnPropertyChanged(() => DifferenceConflictViewModel);
            }
        }
        public bool IsVariablesChecked
        {
            get { return _isVariablesChecked; }
            set
            {
                _isVariablesChecked = value;
                OnPropertyChanged(() => IsVariablesChecked);
            }
        }
    }
}
