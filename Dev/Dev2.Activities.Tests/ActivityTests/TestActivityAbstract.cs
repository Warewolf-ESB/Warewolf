using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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