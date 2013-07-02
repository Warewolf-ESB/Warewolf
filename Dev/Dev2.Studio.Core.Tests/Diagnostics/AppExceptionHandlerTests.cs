using System;
using System.Threading;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class AppExceptionHandlerTests
    {
        static AppExceptionHandler Handler;
        static Mock<MockApp> testApp;
        static Mock<AppExceptionPopupController> Controller;
        static Mock<EventAggregator> Aggregator;
        static object _testGuard = new object();

        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);

            //Controller
            Controller = new Mock<AppExceptionPopupController>();
            //App
            testApp = new Mock<MockApp>();
            testApp.Setup(c => c.Shutdown()).Verifiable();
            testApp.SetupProperty(c => c.ShouldRestart);
            //Aggregator
            Aggregator = new Mock<EventAggregator>();
            Aggregator.Setup(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>()));

            //Handler
            Handler = new AppExceptionHandler(Controller.Object, testApp.Object, null, Aggregator.Object);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #region Handle

        [TestMethod]
        public void HandleWithValidExceptionExpectedHandledAndDetectedSeverityAsDefault()
        {
            //Initialize
            var e = GetException();
            Controller.Setup(a => a.ShowPopup(It.IsAny<Exception>(), ErrorSeverity.Default, It.IsAny<IEnvironmentModel>(), Handler)).Verifiable();

            //Execute
            var actual = Handler.Handle(e);

            //Assert
            Controller.Verify(a => a.ShowPopup(It.IsAny<Exception>(), ErrorSeverity.Default, It.IsAny<IEnvironmentModel>(), Handler), "Default popup not displayed for default exception");
            testApp.Verify(c => c.Shutdown(), Times.Never(), "Applicaiton shutdown after default exception");
            Assert.AreEqual(true, actual, "AppExceptionHandler failed to handle valid exception");
        }

        [TestMethod]
        public void HandleWithSameExceptionTwiceExpectedHandledAndDetectedSeverityAsCritical()
        {
            //Initialize
            var e = GetException();
            Controller.Setup(a => a.ShowPopup(It.IsAny<Exception>(), It.IsAny<ErrorSeverity>(), It.IsAny<IEnvironmentModel>(), Handler)).Verifiable();

            //Execute
            Handler.Handle(e);
            var actual = Handler.Handle(e);

            //Assert
            Controller.Verify(a => a.ShowPopup(It.IsAny<Exception>(), ErrorSeverity.Critical, It.IsAny<IEnvironmentModel>(), Handler), "Critical popup not displayed for critical exception");
            Assert.AreEqual(true, actual, "AppExceptionHandler failed to handle valid exception");
        }

        [TestMethod]
        public void HandleWithExceptionWhileHandlingExpectedShutdownCalledAndErrorNotHandled()
        {
            //Initialize
            var e = GetException();
            Controller.Setup(a => a.ShowPopup(It.IsAny<Exception>(), It.IsAny<ErrorSeverity>(), It.IsAny<IEnvironmentModel>(), Handler)).Throws<NullReferenceException>();

            //Execute
            var actual = Handler.Handle(e);

            //Assert
            testApp.Verify(c => c.Shutdown(), "Shutdown not called after exception during exception handling");
            Assert.AreEqual(false, actual, "AppExceptionHandler failed to handle valid exception");
        } 

        #endregion

        #region Get Exception String

        [TestMethod]
        public void ErrorToStringExpectedTypeOfAndMessageOfInnerAndOuterExceptionsInString()
        {
            //Initialize
            var e = GetException();

            //Execute
            var actual = Handler.GetErrorToString(e);

            //Assert
            Assert.AreEqual("System.ExceptionTest ExceptionSystem.ExceptionTest inner Exception", actual, "Error to string returned an incorrect string");
        }

        #endregion

        #region Shutdown App

        [TestMethod]
        public void ShutdownAppWithTrueIsCriticalExpectedSaveMessagePublishedShouldRestartIsTrueAndAppShutdownCalled()
        {
            //Execute Initialization
            Handler.ShutdownApp(true);

            //Assert
            Aggregator.Verify(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>()), "Save all open tabs message not sent on app shutdown");
            Assert.IsTrue(testApp.Object.ShouldRestart, "App did not restart after critical exception");
            testApp.Verify(c => c.Shutdown(), "App did not shutdown after critical exception");
        }

        [TestMethod]
        public void ShutdownAppWithFalseIsCriticalExpectedSaveMessageNotPublishedShouldRestartIsFalseAndAppShutdownisNotCalled()
        {
            //Execute Initialization
            Handler.ShutdownApp(false);

            //Assert
            Aggregator.Verify(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>()), Times.Never(), "Save all open tabs message called on default exception");
            Assert.IsFalse(testApp.Object.ShouldRestart, "App did restart after default exception");
            testApp.Verify(c => c.Shutdown(), Times.Never(), "App did shutdown after default exception");
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
