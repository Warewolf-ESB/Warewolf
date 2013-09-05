using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers;
using Dev2.Providers.Errors;

namespace Dev2.Core.Tests.Activities
{
    public class TestActivityViewModel : ActivityViewModelBase
    {
        public TestActivityViewModel(ModelItem modelItem) : base(modelItem)
        {
        }

        #region Overrides of ActivityViewModelBase

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            yield break;
        }

        #endregion
    }
}
