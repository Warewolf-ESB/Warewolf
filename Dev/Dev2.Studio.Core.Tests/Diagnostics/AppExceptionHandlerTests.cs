
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Dev2.Infrastructure.Tests.Logs;
using Dev2.Interfaces;
using Dev2.Studio;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            new AppExceptionHandler(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppExceptionHandlerConstructorWithNullAppExpectedThrowsArgumentNullException()
        {
            new AppExceptionHandler(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppExceptionHandlerConstructorWithNullMainViewModelExpectedThrowsArgumentNullException()
        {
            var app = new Mock<IApp>();
            new AppExceptionHandler(app.Object, null);
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


        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("AppExceptionHandler_Handle")]
        //public void AppExceptionHandlerHandle_WithOneException_ExpectedHandledLogsException()
        //{
        //    //------------Setup for test--------------------------
        //    var testTraceListner = new TestTraceListner(new StringBuilder());
        //    Trace.Listeners.Add(testTraceListner);
        //    var e = GetException();
        //    var mockHandler = new Mock<AppExceptionHandlerAbstract>();
        //    var popupController = new Mock<IAppExceptionPopupController>();
        //    mockHandler.Protected().Setup<IAppExceptionPopupController>("CreatePopupController").Returns(popupController.Object);
        //    //------------Execute Test---------------------------
        //    var actual = mockHandler.Object.Handle(e);
        //    //------------Assert Results-------------------------
        //    Thread.Sleep(500);
        //    Assert.IsTrue(actual, "AppExceptionHandlerAbstract failed to handle valid exception");
        //    StringAssert.Contains(testTraceListner.CurrentlyLogged, ":: ERROR -> AppExceptionHandlerAbstractProxy Handle : {\"ClassName\":\"System.Exception\",\"Message\":\"Test Exception\",\"Data\":null,\"InnerException\":{\"ClassName\":\"System.Exception\",\"Message\":\"Test inner Exception\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null},\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null}"
        //        + "\r\n" +
        //        "{\"ClassName\":\"System.Exception\",\"Message\":\"Test inner Exception\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null}");

        //}

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
        public void AppExceptionHandlerShutdownAppExpectedShouldRestartIsFalse()
        {
            //Initialize
            var aggregator = new Mock<IEventAggregator>();
            aggregator.Setup(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>())).Verifiable();

            var mockApp = new Mock<IApp>();
            mockApp.Setup(c => c.Shutdown()).Verifiable();
            mockApp.SetupProperty(c => c.ShouldRestart);

            var mainViewModel = new Mock<IMainViewModel>();

            //Execute
            var handler = new MockExceptionHandler(mockApp.Object, mainViewModel.Object);
            handler.TestShutdownApp();

            //Assert            
            Assert.IsFalse(mockApp.Object.ShouldRestart, "App did restart after non critical exception");
            mockApp.Verify(c => c.Shutdown(), "App did not shutdown after non critical exception");
        }

        [TestMethod]
        public void AppExceptionHandlerRestartAppExpectedShouldRestartIsTrue()
        {
            //Initialize
            var aggregator = new Mock<IEventAggregator>();
            aggregator.Setup(c => c.Publish(It.IsAny<SaveAllOpenTabsMessage>())).Verifiable();

            var mockApp = new Mock<IApp>();
            mockApp.Setup(c => c.Shutdown()).Verifiable();
            mockApp.SetupProperty(c => c.ShouldRestart);

            var mainViewModel = new Mock<IMainViewModel>();

            //Execute
            var handler = new MockExceptionHandler(mockApp.Object, mainViewModel.Object);
            handler.TestRestartApp();

            //Assert            
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
