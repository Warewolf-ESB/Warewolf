using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    public class TestActivity : DsfNativeActivity<string>
    {

        public TestActivity()
            : this(null)
        {
        }

        public TestActivity(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {
            UniqueGuid = Guid.NewGuid();
            UniqueID = UniqueGuid.ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        public Guid UniqueGuid { get; private set; }

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

        public IDebugState TestInitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage)
        {
            InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);
            return DebugState;
        }
    }
}