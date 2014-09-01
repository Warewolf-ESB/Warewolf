using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    // BUG 9304 - 2013.05.08 - TWR - Created this test class

    public class TestFlowNodeActivity<TResult> : DsfFlowNodeActivity<TResult>
    {
        public TestFlowNodeActivity()
            : base("TestFlowNode", new Mock<IDebugDispatcher>().Object, false)
        {
            UniqueID = Guid.NewGuid().ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        #region Overrides of DsfNativeActivity<TResult>

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        #endregion

    }
}
