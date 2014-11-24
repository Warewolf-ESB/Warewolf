
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Services.Communication;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for FeedbackViewModelTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FeedbackViewModelTest
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        [TestInitialize]
        public void TestSetup()
        {
            CustomContainer.Clear();
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
            CustomContainer.Register(mockSysInfo.Object);
            var feedbackViewModel = new FeedbackViewModel();
            string versionNumber = VersionInfo.FetchVersionInfo();
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
            CustomContainer.Register(mockSysInfo.Object);

            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var feedbackViewModel = new FeedbackViewModel();
            feedbackViewModel.Send(mockCommService.Object);

            mockCommService.Verify(c => c.SendCommunication(It.IsAny<EmailCommMessage>()), Times.Once());
        }
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void FeedbackViewModel_Send_LogFilesExistToAttach_FilesAreConcatenatedAsAttachments()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());
            CustomContainer.Register(mockSysInfo.Object);
            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var attacheFiles = new Dictionary<string, string>
            {
                {"RecordingLog", "RecordingLog.log"},
                {"ServerLog", "ServerLog.log"},
                {"StudioLog", "StudioLog.log"}
            };

            var feedbackViewModel = new FeedbackViewModel(attacheFiles) { DoesFileExists = e => true };
            feedbackViewModel.Send(mockCommService.Object);

            Assert.AreEqual("RecordingLog.log;ServerLog.log;StudioLog.log", feedbackViewModel.Attachments);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FeedbackViewModel_IsOutlookInstalled")]
        [Ignore]
        public void FeedbackViewModel_IsOutlookInstalled_IsTrue_SendMessageButtonCaptionIsSetToOpenMail()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());


            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var attacheFiles = new Dictionary<string, string>
            {
                {"RecordingLog", "RecordingLog.log"},
                {"ServerLog", "ServerLog.log"},
                {"StudioLog", "StudioLog.log"}
            };

            var feedbackViewModel = new FeedbackViewModel(attacheFiles) { DoesFileExists = e => true, IsOutlookInstalled = () => true };

            Assert.AreEqual("Open Outlook Mail", feedbackViewModel.SendMessageButtonCaption);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FeedbackViewModel_IsOutlookInstalled")]
        [Ignore]
        public void FeedbackViewModel_IsOutlookInstalled_IsFalse_SendMessageButtonCaptionIsSetToGotoCommunity()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());


            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var attacheFiles = new Dictionary<string, string>
            {
                {"RecordingLog", "RecordingLog.log"},
                {"ServerLog", "ServerLog.log"},
                {"StudioLog", "StudioLog.log"}
            };

            var feedbackViewModel = new FeedbackViewModel(attacheFiles) { DoesFileExists = e => true, IsOutlookInstalled = () => false };

            Assert.AreEqual("Go to Community", feedbackViewModel.SendMessageButtonCaption);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FeedbackViewModel_Send")]
        [Ignore]
        public void FeedbackViewModel_Send_OutlookIsNotInstalled_BrowserIsOpenedToCommunity()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());


            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var attacheFiles = new Dictionary<string, string>
            {
                {"RecordingLog", "RecordingLog.log"},
                {"ServerLog", "ServerLog.log"},
                {"StudioLog", "StudioLog.log"}
            };

            var feedbackViewModel = new FeedbackViewModel(attacheFiles) { DoesFileExists = e => true };

            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(m => m.ShowPopup(It.IsAny<string>())).Verifiable();
            feedbackViewModel.BrowserPopupController = popupController.Object;
            feedbackViewModel.IsOutlookInstalled = () => false;
            feedbackViewModel.Send();

            popupController.Verify(m => m.ShowPopup(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FeedbackViewModel_Send")]
        [Ignore]//Ashley
        public void FeedbackViewModel_Send_OutlookIsInstalled_BrowserIsNotOpenedToCommunity()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(c => c.GetSystemInfo()).Returns(GetMockSysInfo());


            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            var attacheFiles = new Dictionary<string, string>
            {
                {"RecordingLog", "RecordingLog.log"},
                {"ServerLog", "ServerLog.log"},
                {"StudioLog", "StudioLog.log"}
            };

            var feedbackViewModel = new FeedbackViewModel(attacheFiles) { DoesFileExists = e => true };

            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(m => m.ShowPopup(It.IsAny<string>())).Verifiable();
            feedbackViewModel.BrowserPopupController = popupController.Object;
            feedbackViewModel.IsOutlookInstalled = () => true;
            feedbackViewModel.Send();

            popupController.Verify(m => m.ShowPopup(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void FeedbackViewModelSendWithValidCommWithRecordingAttachmentExpectedSendMethodInvokedWithAttachment()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(sysInfo => sysInfo.GetSystemInfo()).Returns(GetMockSysInfo());
            CustomContainer.Register(mockSysInfo.Object);
            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            // PBI 9598 - 2013.06.10 - TWR : fixed paths
            var attachmentPath = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath, @"test text");

            var attachedFiles = new Dictionary<string, string> { { "ServerLog", attachmentPath } };
            var viewModel = new FeedbackViewModel(attachedFiles);

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
            CustomContainer.Register(mockSysInfo.Object);
            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Verifiable();

            // PBI 9598 - 2013.06.10 - TWR : fixed paths
            var attachmentPath1 = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath1, @"test text");

            var attachmentPath2 = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath2, @"test text");

            var attachmentPath3 = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath2, @"test text");

            var attachedFiles = new Dictionary<string, string>
            {
                { "RecordingLog", attachmentPath1 },
                { "ServerLog", attachmentPath2 },
                { "StudioLog", attachmentPath3 }
            };

            var viewModel = new FeedbackViewModel(attachedFiles);

            Assert.AreEqual(attachmentPath1, viewModel.RecordingAttachmentPath);
            Assert.AreEqual(attachmentPath2, viewModel.ServerLogAttachmentPath);
            Assert.AreEqual(attachmentPath3, viewModel.StudioLogAttachmentPath);
            Assert.IsTrue(viewModel.HasServerLogAttachment);
            Assert.IsTrue(viewModel.HasRecordingAttachment);

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


            var feedbackViewModel = new FeedbackViewModel();
            feedbackViewModel.Send(null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("FeedbackViewModel_Send")]
        public void FeedbackViewModel_Send_ServerlogFileAttachment_EmailHasServerLogFileAttachment()
        {
            var mockSysInfo = new Mock<ISystemInfoService>();
            mockSysInfo.Setup(sysInfo => sysInfo.GetSystemInfo()).Returns(GetMockSysInfo());
            CustomContainer.Register(mockSysInfo.Object);
            var mockCommService = new Mock<ICommService<EmailCommMessage>>();
            var actual = new EmailCommMessage();
            mockCommService.Setup(c => c.SendCommunication(It.IsAny<EmailCommMessage>())).Callback<EmailCommMessage>(msg =>
            {
                actual = msg;
            });

            var attachmentPath = Path.Combine(_testDir, string.Format("FeedbackTest_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(attachmentPath, @"test text");

            var attachedFiles = new Dictionary<string, string> { { "ServerLog", attachmentPath } };

            var feedbackViewModel = new FeedbackViewModel(attachedFiles);

            //------------Execute Test---------------------------
            feedbackViewModel.Send(mockCommService.Object);

            // Assert email has server log file attachment
            Assert.AreEqual(attachmentPath, actual.AttachmentLocation, "Wrong file attached");
        }
    }
}
