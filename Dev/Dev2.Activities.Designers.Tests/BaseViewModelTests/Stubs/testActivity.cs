using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Activities
{
    public class testActivity : DsfNativeActivity<bool>
    {
        public testActivity()
            : this(null)
        {
        }

        public testActivity(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {
            UniqueID = Guid.NewGuid().ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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
