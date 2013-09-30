using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class ActivityDesignerCollectionViewModelDerived : ActivityCollectionDesignerViewModel<ActivityDTO>
    {
        public ActivityDesignerCollectionViewModelDerived(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public bool IsSmallViewActive { get { return ShowSmall; } }

        protected override string CollectionName { get { return "FieldsCollection"; } }

        public void TestAddTitleBarQuickVariableInputToggle()
        {
            AddTitleBarQuickVariableInputToggle();
        }
    }
}