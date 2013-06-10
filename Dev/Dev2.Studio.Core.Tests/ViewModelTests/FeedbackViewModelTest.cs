using System;
using System.IO;
using Dev2.Composition;
using Dev2.Studio.Core.Services.Communication;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.ViewModels.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for FeedbackViewModelTest
    /// </summary>
    [TestClass]
    public class FeedbackViewModelTest
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        private SystemInfoTO GetMockSysInfo()
        {
            return new SystemInfoTO
                {
                    ApplicationExecutionBits = 32,
                    Edition = "Professional",
                    Name = "Windows 7",
                    ServicePack = "ServicePack 1",
                    Version = "1.1.1.1",
                    OsBits = "64-bit"
                };
        }

        [TestMethod]
        public void FeedbackViewModelWithDefaultInitialization()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);
            var feedbackViewModel = new FeedbackViewModel();
            string versionNumber = Dev2.Studio.Core.StringResources.CurrentVersion;
            StringAssert.Contains(feedbackViewModel.Comment, @"Comments : 


I Like the product : YES/NO
I Use the product everyday : YES/NO
My name is Earl : YES/NO
Really, my name is Earl : YES/NO
OS version : ");
            StringAssert.Contains(feedbackViewModel.Comment, "Product Version : " + versionNumber);

            CollectionAssert.AllItemsAreUnique(feedbackViewModel.Categories);

            mockSysInfo.Verify(c => c.GetSystemInfo(), Times.Once());
        }


        [TestMethod]
        public void FeedbackViewModelSendWithValidCommServiceExpectedSendMethodInvokedOnCommService()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);

            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var feedbackViewModel = new FeedbackViewModel();
            feedbackViewModel.Send(mockCommService.Object);

            mockCommService.Verify(c => c.SendCommunication(It.IsAny<EmailCommMessage>()), Times.Once());
        }

        [TestMethod]
        public void FeedbackViewModelSendWithValidCommWithAttachmentExpectedSendMethodInvokedWithAttachment()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(sysInfo => sysInfo.GetSystemInfo()).Returns(GetMockSysInfo());

            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            // PBI 9598 - 2013.06.10 - TWR : fixed paths
            var attachmentPath = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath, "test text");

            var viewModel = new FeedbackViewModel(attachmentPath);

            viewModel.Send(mockCommService.Object);
            mockCommService.Verify(c => c.SendCommunication(It.IsAny<EmailCommMessage>())
                                 , Times.Once()
                                 , "Send Message failed for mail with attachment");
        }

        [TestMethod]
        public void FeedbackViewModelSendWithValidCommWithAttachmentExpectedSendMethodInvokedWithTwoAttachment()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(sysInfo => sysInfo.GetSystemInfo()).Returns(GetMockSysInfo());

            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            // PBI 9598 - 2013.06.10 - TWR : fixed paths
            var attachmentPath1 = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath1, "test text");

            var attachmentPath2 = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath2, "test text");

            var viewModel = new FeedbackViewModel(string.Format("{0};{1}", attachmentPath1, attachmentPath2));

            Assert.AreEqual(attachmentPath1, viewModel.RecordingAttachmentPath);
            Assert.AreEqual(attachmentPath2, viewModel.ServerLogAttachmentPath);
            Assert.AreEqual(true, viewModel.HasServerLogAttachment);
            Assert.AreEqual(true, viewModel.HasRecordingAttachment);

            viewModel.Send(mockCommService.Object);
            mockCommService.Verify(c => c.SendCommunication(It.IsAny<EmailCommMessage>())
                                 , Times.Once()
                                 , "Send Message failed for mail with attachment");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void FeedbackViewModelSendWithNullCommServiceExpectedNullException()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);

            var feedbackViewModel = new FeedbackViewModel();
            feedbackViewModel.Send(null);
        }
    }
}
