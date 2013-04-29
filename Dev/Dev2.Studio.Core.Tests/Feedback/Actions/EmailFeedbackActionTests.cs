using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Feedback.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows;

namespace Dev2.Core.Tests.Feedback.Actions
{
    [TestClass]
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

        [TestMethod]
        public void StartFeedBack_Expected_ShowMethodCalledOnWindowManager()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            var mockWindowManager = new Mock<IWindowManager>();
            mockWindowManager.Setup(w => w.ShowWindow(It.IsAny<BaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext =
                CompositionInitializer.InitializeWithWindowManagerTest(mockSysInfo, mockWindowManager);

            var emailAction = new EmailFeedbackAction { WindowManager = mockWindowManager.Object };
            ImportService.SatisfyImports(emailAction);

            emailAction.StartFeedback();
            mockWindowManager.Verify(c => c.ShowWindow(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once());
        }
    }
}
