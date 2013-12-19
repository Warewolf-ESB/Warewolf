using System;
using System.Collections.Generic;
using Dev2.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.Utils
{
    /// <summary>
    /// Dummy activity for testing ;)
    /// </summary>
    public class DsfFakeActivity : DsfActivityAbstract<bool>
    {
        protected override void OnExecute(System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }
    }
}
