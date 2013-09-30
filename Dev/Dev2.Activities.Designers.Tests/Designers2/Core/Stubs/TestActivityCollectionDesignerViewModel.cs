using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Interfaces;
using Dev2.Providers.Validation;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    class TestActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel<TDev2TOFn>
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {

        public TestActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        #region Overrides of ActivityCollectionViewModelBase

        protected override string CollectionName
        {
            get
            {
                return GetProperty<string>();
            }
        }

        #endregion
    }
}