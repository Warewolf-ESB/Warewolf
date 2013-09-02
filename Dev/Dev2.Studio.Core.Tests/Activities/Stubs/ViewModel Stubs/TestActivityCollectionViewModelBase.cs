using System.Activities.Presentation.Model;
using Dev2.Activities.Designers;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Core.Tests.Activities
{
    class TestActivityCollectionViewModelBase<TDev2TOFn> : ActivityCollectionViewModelBase<TDev2TOFn>
        where TDev2TOFn : class, IDev2TOFn, new()
    {

        public TestActivityCollectionViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        #region Overrides of ActivityCollectionViewModelBase

        protected override string CollectionName
        {
            get
            {
                return ModelItemUtils.GetProperty("CollectionName",ModelItem).ToString();
            }
        }

        #endregion
    }
}