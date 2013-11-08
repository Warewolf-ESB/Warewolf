using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Zip
{
    public class ZipDesignerViewModel : FileActivityDesignerViewModel
    {
        public IList<string> CompressionTypes { get; set; }

        public ZipDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", "Destination")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
            CompressionTypes = new List<string> {"None", "Partial", "Normal", "Good", "Max"};
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputAndOutputPaths(true);
        }
    }
}