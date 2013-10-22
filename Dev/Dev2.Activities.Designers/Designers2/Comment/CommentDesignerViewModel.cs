using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Comment
{
    public class CommentDesignerViewModel : ActivityDesignerViewModel
    {
        public CommentDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}