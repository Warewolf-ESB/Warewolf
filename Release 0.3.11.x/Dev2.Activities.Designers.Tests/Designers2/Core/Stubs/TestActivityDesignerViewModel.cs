using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class TestActivityDesignerViewModel : ActivityDesignerViewModel
    {
        public TestActivityDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestActivityDesignerViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow)
            : base(modelItem, showExampleWorkflow)
        {
        }

        public bool IsSmallViewActive { get { return ShowSmall; } }

        public override void Validate()
        {
        }

        public void TestAddTitleBarHelpToggle()
        {
            AddTitleBarHelpToggle();
        }

        public void TestAddTitleBarLargeToggle()
        {
            AddTitleBarLargeToggle();
        }
    }
}
