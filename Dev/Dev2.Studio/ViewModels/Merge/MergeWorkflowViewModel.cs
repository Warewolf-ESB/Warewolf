using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.ViewModels.Workflow;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : IMergeWorkflowViewModel
    {
        public MergeWorkflowViewModel()
        {
            DataListSingleton.SetDataList(new DataListViewModel());
            var act = new DsfMultiAssignActivity();
            act.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));
            var mi = ModelItemUtils.CreateModelItem(act);
            CurrentMergeList = new List<ActivityDesignerViewModel>
            {
                new MultiAssignDesignerViewModel(mi)
                {
                    ShowLarge = true
                },
                new CommentDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfCommentActivity())),
                new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFlowDecisionActivity()))
            };

            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(tempResource, true);
            AddAnItem = new DelegateCommand(o => {

                var step = new FlowStep { Action = act };
                WorkflowDesignerViewModel.AddItem(step);
            });
        }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public List<ActivityDesignerViewModel> CurrentMergeList { get; set; }
        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }
        
    }
}
