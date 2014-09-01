using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Feedback.Actions
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EmailFeedbackActionTests
    {
        private SystemInfoTO GetMockSysInfo()
        {
            return new SystemInfoTO
            {
                ApplicationExecutionBits = 32,
                Edition = "Professional",
                Name = "Windows 7",
                ServicePack = "ServicePack 1",
                Version = "1.1.1.1"
            };
        }

        [TestInitialize]
        public void TestSetup()
        {
            CustomContainer.Clear();
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void StartFeedBack_Expected_ShowMethodCalledOnWindowManager()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            var mockWindowManager = new Mock<IWindowManager>();
            mockWindowManager.Setup(w => w.ShowWindow(It.IsAny<BaseViewModel>(), null, null)).Verifiable();

            CustomContainer.Register(mockSysInfo.Object);
            CustomContainer.Register(mockWindowManager.Object);

            var connection = new Mock<IEnvironmentConnection>();
            var environment = new Mock<IEnvironmentModel>();
            var repo = new Mock<IResourceRepository>();

            repo.Setup(r => r.GetServerLogTempPath(It.IsAny<IEnvironmentModel>())).Returns("");
            environment.Setup(env => env.Connection).Returns(connection.Object);
            environment.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var emailAction = new EmailFeedbackAction(new Dictionary<string, string>(), environment.Object);

            emailAction.StartFeedback();
            mockWindowManager.Verify(c => c.ShowWindow(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EmailFeedbackAction_DisplayFeedbackWindow")]
        public void EmailFeedbackAction_DisplayFeedbackWindow_NullEnvionment_ServerLogFileAttached()
        {
            var expected = new Dictionary<string, string> { { "ServerLog", "ExpectedServerLogFilePath" } };
            string actual = null;

            var windowManager = new Mock<IWindowManager>();
            windowManager.Setup(c => c.ShowWindow(It.IsAny<Object>(), It.IsAny<Object>(), It.IsAny<IDictionary<string, Object>>())).Callback((Object vm, Object obj, IDictionary<string, Object> dictionary) =>
            {
                actual = (vm is FeedbackViewModel) ? (vm as FeedbackViewModel).ServerLogAttachmentPath : null;
            });

            CustomContainer.Register(Dev2MockFactory.CreateIPopup(MessageBoxResult.OK).Object);
            CustomContainer.Register(new Mock<IFeedbackInvoker>().Object);
            CustomContainer.Register(windowManager.Object);
            var mockSysInfoService = new Mock<ISystemInfoService>();
            mockSysInfoService.Setup(service => service.GetSystemInfo()).Returns(new SystemInfoTO());
            CustomContainer.Register(mockSysInfoService.Object);

            var connection = new Mock<IEnvironmentConnection>();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(env => env.Connection).Returns(connection.Object);
            var emailFeedbackAction = new EmailFeedbackAction(expected, environment.Object);

            //------------Execute Test---------------------------
            emailFeedbackAction.DisplayFeedbackWindow();

            //------------Assert Server Log File Attached--------
            Assert.AreEqual(expected.Where(f => f.Key.Equals("ServerLog", StringComparison.CurrentCulture)).Select(v => v.Value).SingleOrDefault(), actual, "No log file attached");
        }
    }
}
