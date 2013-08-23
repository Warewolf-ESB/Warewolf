using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Diagnostics;

namespace Dev2.Tests.Activities.ActivityTests
{
    public abstract class TestActivityAbstract : DsfNativeActivity<string>
    {
        protected TestActivityAbstract()
            : this(DebugDispatcher.Instance)
        {
        }

        protected TestActivityAbstract(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {          
        }
    }
}