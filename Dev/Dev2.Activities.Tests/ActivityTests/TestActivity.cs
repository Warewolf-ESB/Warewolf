using System;
using System.Activities;
using Dev2;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    public class TestActivity : DsfNativeActivity<string>
    {
        //public class TestActivity : DsfActivityAbstract<string> {
        //public TestActivity(IDebugDispatcher dispatcher)
        //    : base("TestActivity", dispatcher)
        //{
        //    AmbientDataList.Expression = new DynamicActivity<Location<List<string>>>();
        //    InstructionList.Expression = new DynamicActivity<Location<List<string>>>();
        //    IsValid.Expression = new DynamicActivity<Location<bool>>();
        //    HasError.Expression = new DynamicActivity<Location<bool>>();
        //}

        public TestActivity()
            : this(null)
        {
        }

        public TestActivity(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {
            InstanceGuid = Guid.NewGuid();
            InstanceID = InstanceGuid.ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        public Guid InstanceGuid { get; private set; }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(System.Collections.Generic.IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(System.Collections.Generic.IList<Tuple<string, string>> updates, NativeActivityContext context)
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