using System;
using System.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class AppExceptionPopupControllerTests
    {
        static AppExceptionPopupController _controller;

        [ClassInitialize]
        public static void InitClass(TestContext testContext)
        {
            _controller = new AppExceptionPopupController();
        }

        static object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #region Default Exception Popup Controller

        [TestMethod]
        public void ShowPopupWithDefaultErrorExpectedAppShutDownCalledWithIsCriticalFalse()
        {
            //Initialize
            var handler = new Mock<AppExceptionHandler>(It.IsAny<AppExceptionPopupController>(), It.IsAny<App>(), It.IsAny<IEnvironmentModel>(), It.IsAny<EventAggregator>());
            handler.Setup(c=>c.ShutdownApp(false)).Verifiable();
            var e = GetException();

            //Execute
            _controller.ShowPopup(e, ErrorSeverity.Default, null, handler.Object);

            //Assert
            handler.Verify(c=>c.ShutdownApp(false));
        }

        [TestMethod]
        public void ShowPopupWithCriticalErrorExpectedAppShutDownCalledWithIsCriticalTrue()
        {
            //Initialize
            var handler = new Mock<AppExceptionHandler>(It.IsAny<AppExceptionPopupController>(), It.IsAny<App>(), It.IsAny<IEnvironmentModel>(), It.IsAny<EventAggregator>());
            handler.Setup(c => c.ShutdownApp(false)).Verifiable();
            var e = GetException();

            //Execute
            _controller.ShowPopup(e, ErrorSeverity.Critical, null, handler.Object);

            //Assert
            handler.Verify(c => c.ShutdownApp(true));
        }

        #endregion

        #region Private Test Methods

        private static Exception GetException()
        {
            return new Exception("Test Exception", new Exception("Test inner Exception"));
        }

        #endregion
    }
}
