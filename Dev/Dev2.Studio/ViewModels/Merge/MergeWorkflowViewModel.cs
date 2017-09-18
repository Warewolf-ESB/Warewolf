using System.Activities.Presentation.Model;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using System.Collections.ObjectModel;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        public MergeWorkflowViewModel(IEnumerable<ModelItem> currentMergeList, IEnumerable<ModelItem> differenceModelItems, IApplicationAdaptor applicationAdaptor)
        {
            CurrentConflictViewModel = new CurrentConflictViewModel(applicationAdaptor, currentMergeList);
            DifferenceConflictViewModel = new DifferenceConflictViewModel(applicationAdaptor, differenceModelItems);
            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            CurrentConflictViewModel.WorkflowName = newWorflowName;
            DifferenceConflictViewModel.WorkflowName = newWorflowName;

            Conflicts = new ObservableCollection<CompleteConflict>();
            Conflicts.Add(new CompleteConflict
            {
                DiffViewModel = DifferenceConflictViewModel.MergeToolModel,
                CurrentViewModel = CurrentConflictViewModel.MergeToolModel
            });

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(tempResource);
            AddAnItem = new DelegateCommand(o =>
            {
                //var step = new FlowStep { Action = act };
                //WorkflowDesignerViewModel.AddItem(step);
            });
            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
        }

        public ObservableCollection<CompleteConflict> Conflicts { get; set; }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }
    }
}
