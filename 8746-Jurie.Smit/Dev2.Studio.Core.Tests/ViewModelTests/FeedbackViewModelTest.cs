using Dev2.Composition;
using Dev2.Studio.Core.Services.Communication;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.ViewModels.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for FeedbackViewModelTest
    /// </summary>
    [TestClass]
    public class FeedbackViewModelTest
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        public void Default_Initialization_Test()
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
        public void Send_Given_ValidCommService_Expected_SendMethodInvokedOnCommService()
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
        public void Send_Given_ValidCommWithAttachment_Expected_SendMethodInvokedWithAttachment()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(sysInfo => sysInfo.GetSystemInfo()).Returns(GetMockSysInfo());

            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            #region Create Attachment File

            string fileToCreate = TestContext.TestDir + "\\testingfile.txt";

            StreamWriter sr = File.CreateText(fileToCreate);
            sr.Write("text");
            sr.Close();
            sr.Dispose();

            #endregion Create Attachment File

            FeedbackViewModel feedBackVM = new FeedbackViewModel(fileToCreate);
            
            feedBackVM.Send(mockCommService.Object);
            mockCommService.Verify(c => c.SendCommunication(It.IsAny<EmailCommMessage>())
                                 , Times.Once()
                                 , "Send Message failed for mail with attachment");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Send_Given_NullCommService_Expected_NullException()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());

            ImportService.CurrentContext = CompositionInitializer.InitializeEmailFeedbackTest(mockSysInfo);

            var feedbackViewModel = new FeedbackViewModel();
            feedbackViewModel.Send(null);
        }
    }
}
