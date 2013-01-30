using Dev2.Diagnostics;
using System.Activities;
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
            InstanceID = "InstanceID";
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }
        
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(System.Collections.Generic.IList<System.Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateForEachOutputs(System.Collections.Generic.IList<System.Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}