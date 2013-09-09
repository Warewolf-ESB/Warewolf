using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
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
                return ModelItemUtils.GetProperty("CollectionName", ModelItem).ToString();
            }
        }

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            yield break;
        }

        #endregion
    }
}