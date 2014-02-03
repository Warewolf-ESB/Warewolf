using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class TestValidationActivityDesignerCollectionViewModel : ActivityCollectionDesignerViewModel<ActivityDTO>
    {
        public TestValidationActivityDesignerCollectionViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            dynamic mi = ModelItem;
            InitializeItems(mi.FieldsCollection);
        }

        public override string CollectionName { get { return "FieldsCollection"; } }

        public int ValidateThisHitCount { get; private set; }
        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            ValidateThisHitCount++;
            yield return new ActionableErrorInfo();
        }

        public int ValidateCollectionItemHitCount { get; private set; }
        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            ValidateCollectionItemHitCount++;
            yield return new ActionableErrorInfo();
        }

    }
}