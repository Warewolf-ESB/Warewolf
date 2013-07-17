using System;
using System.Threading;
using Caliburn.Micro;
using Dev2.Studio;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class AppExceptionHandlerTests
    {
        static readonly object TestGuard = new object();

        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(TestGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(TestGuard);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppExceptionHandlerConstructorWithNullEventAggregatorExpectedThrowsArgumentNullException()
        {
            var handler = new AppExceptionHandler(null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppExceptionHandlerConstructorWithNullAppExpectedThrowsArgumentNullException()
        {
            var aggregator = new Mock<IEventAggregator>();
            var handler = new AppExceptionHandler(aggregator.Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppExceptionHandlerConstructorWithNullMainViewModelExpectedThrowsArgumentNullException()
        {
            var aggregator = new Mock<IEventAggregator>();
            var app = new Mock<IApp>();
            var handler = new AppExceptionHandler(aggregator.Object, app.Object, null);

        }

        #region Handle

        [TestMethod]
        public void AppExceptionHandlerHandleWithOneExceptionExpectedHandledAndAppNotRestarted()
        {
            //Initialize
            var e = GetException();
            var mockHandler = new Mock<AppExceptionHandlerAbstract>();
            var popupController = new Mock<IAppExceptionPopupController>();
            mockHandler.Protected().Setup<IAppExceptionPopupController>("CreatePopupController").Returns(popupController.Object);
            mockHandler.Protected().Setup("RestartApp").Verifiable();

            //Execute
            var actual = mockHandler.Object.Handle(e);

            //Assert
            mockHandler.Protected().Verify("RestartApp", Times.Never());
            Assert.IsTrue(actual, "AppExceptionHandlerAbstract failed to handle valid exception");
        }

        [TestMethod]
        public void AppExceptionHandlerHandleWithSameExceptionTwiceExpectedHandledAndAppRestarted()
        {
            //Initialize
            var e = GetException();
            var mockHandler = new Mock<AppExceptionHandlerAbstract>();
            var popupController = new Mock<IAppExceptionPopupController>();
            mockHandler.Protected().Setup<IAppExceptionPopupController>("CreatePopupController").Returns(popupController.Object);
            mockHandler.Protected().Setup("RestartApp").Verifiable();

            //Execute
            mockHandler.Object.Handle(e);
            var actual = mockHandler.Object.Handle(e);

            //Assert
            mockHandler.Protected().Verify("RestartApp", Times.Once());
            Assert.IsTrue(actual, "AppExceptionHandlerAbstract failed to handle valid exception");
        }

        [TestMethod]
        public void AppExceptionHandlerHandleWithExceptionWhileHandlingExpectedShutdownCalled()
        {
            //Initialize
            var e = GetException();
            var mockHandler = new Mock<AppExceptionHandlerAbstract>();

            // DON'T SETUP CreatePopupController to force secondary exception

            mockHandler.Protected().Setup("ShutdownApp").Verifiable();

            //Execute
            mockHandler.Object.Handle(e);

            //Assert
            mockHandler.Protected().Verify("ShutdownApp", Times.Once());
        }

        #endregion

        #region Shutdown/Restart App

        [TestMethod]
        public void AppExceptionHandlerShutdownAppExpectedSaveMessagePublishedAndShouldRestartIsFalse()
        {
            //Initialize
            var aggregator = new Mock<IEventAggregator>();
            aggregator.Setup(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>())).Verifiable();

            var mockApp = new Mock<IApp>();
            mockApp.Setup(c => c.Shutdown()).Verifiable();
            mockApp.SetupProperty(c => c.ShouldRestart);

            var mainViewModel = new Mock<IMainViewModel>();

            //Execute
            var handler = new MockExceptionHandler(aggregator.Object, mockApp.Object, mainViewModel.Object);
            handler.TestShutdownApp();

            //Assert
            aggregator.Verify(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>()), "Save all open tabs message called on non critical exception");
            Assert.IsFalse(mockApp.Object.ShouldRestart, "App did restart after non critical exception");
            mockApp.Verify(c => c.Shutdown(), "App did not shutdown after non critical exception");
        }

        [TestMethod]
        public void AppExceptionHandlerRestartAppExpectedSaveMessagePublishedAndShouldRestartIsTrue()
        {
            //Initialize
            var aggregator = new Mock<IEventAggregator>();
            aggregator.Setup(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>())).Verifiable();

            var mockApp = new Mock<IApp>();
            mockApp.Setup(c => c.Shutdown()).Verifiable();
            mockApp.SetupProperty(c => c.ShouldRestart);

            var mainViewModel = new Mock<IMainViewModel>();

            //Execute
            var handler = new MockExceptionHandler(aggregator.Object, mockApp.Object, mainViewModel.Object);
            handler.TestRestartApp();

            //Assert
            aggregator.Verify(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>()), "Save all open tabs message called on critical exception");
            Assert.IsTrue(mockApp.Object.ShouldRestart, "App did restart after critical exception");
            mockApp.Verify(c => c.Shutdown(), "App did not shutdown after critical exception");
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
