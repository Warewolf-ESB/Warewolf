using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Interfaces;
using Dev2.Providers.Validation;

namespace Dev2.Core.Tests.Activities
{
    class TestActivityCollectionViewModelBase<TDev2TOFn> : ActivityCollectionDesignerViewModel<TDev2TOFn>
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {

        public TestActivityCollectionViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        protected override string CollectionName
        {
            get
            {
                return GetProperty<string>();
            }
        }
    }
}