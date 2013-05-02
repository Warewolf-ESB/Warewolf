using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Diagnostics;

namespace ActivityUnitTests.ActivityTests
{
    public abstract class TestActivityAbstract : DsfNativeActivity<string>
    {
        public TestActivityAbstract()
            : this(DebugDispatcher.Instance)
        {
        }

        public TestActivityAbstract(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {          
        }
    }
}