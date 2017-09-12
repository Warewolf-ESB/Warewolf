using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.ViewModels.DataList;
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
            CurrentMergeList = new List<ActivityDesignerViewModel>
            {
                new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(act))
                {
                    ShowLarge = true
                },
                new CommentDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfCommentActivity())),
                new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFlowDecisionActivity()))
            };            
        }

        public List<ActivityDesignerViewModel> CurrentMergeList { get; set; }
        
    }
}
