using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class TestActivityDesignerCollectionViewModelItemsInitialized: ActivityCollectionDesignerViewModel<ActivityDTO>
    {
        public TestActivityDesignerCollectionViewModelItemsInitialized(ModelItem modelItem)
            : base(modelItem)
        {
            dynamic mi = ModelItem;
            InitializeItems(mi.FieldsCollection);
        }

        public override string CollectionName { get { return "FieldsCollection"; } }

        public void TestAddTitleBarQuickVariableInputToggle()
        {
            AddTitleBarQuickVariableInputToggle();
        }

        public void TestAddToCollection(IEnumerable<string> source, bool overwrite)
        {
            AddToCollection(source, overwrite);
        }

        #region Overrides of ActivityCollectionDesignerViewModel<ActivityDTO>

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            yield break;
        }

        #endregion
    }
}