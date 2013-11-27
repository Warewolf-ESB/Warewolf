using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.DeleteRecords
{
    public class DeleteRecordsDesignerViewModel : ActivityDesignerViewModel
    {
        public DeleteRecordsDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}