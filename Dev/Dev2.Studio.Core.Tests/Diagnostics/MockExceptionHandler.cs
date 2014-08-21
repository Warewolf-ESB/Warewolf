using Dev2.Interfaces;
using Dev2.Studio;
using Dev2.Studio.Diagnostics;

namespace Dev2.Core.Tests.Diagnostics
{
    public class MockExceptionHandler : AppExceptionHandler
    {
        public MockExceptionHandler(IApp current, IMainViewModel mainViewModel)
            : base(current, mainViewModel)
        {
        }

        #region ShutdownApp

        public void TestShutdownApp()
        {
            base.ShutdownApp();
        }

        #endregion

        #region RestartApp

        public void TestRestartApp()
        {
            base.RestartApp();
        }

        #endregion
    }
}
