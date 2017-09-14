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
            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
        }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }
    }
}
