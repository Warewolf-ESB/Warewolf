using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Core.Tests.Activities
{
    class TestActivityViewModelBase : ActivityDesignerViewModel
    {
        public TestActivityViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public override void Validate()
        {
        }
    }
}