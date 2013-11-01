using System;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Activities.Designers.Tests.Email
{
    [TestClass][ExcludeFromCodeCoverage]
    public class EmailDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EmailDesignerViewModel_CreateNewEmailSource")]
        public void EmailDesignerViewModel_CreateNewEmailSource_MailSourceCreated_EventPublishedTwice()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<IMessage>())).Verifiable();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            viewModel.CreateNewEmailSource();
            mockEventPublisher.Verify(m => m.Publish(It.IsAny<IMessage>()), Times.Once());
            mockEventPublisher.Verify(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>()), Times.AtLeast(2));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EmailDesignerViewModel_EditEmailSource")]
        public void EmailDesignerViewModel_EditEmailSource_MailSourceEdited_EventPublisherCalledOnce()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            viewModel.EditEmailSource();
            mockEventPublisher.Verify(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>()), Times.AtLeast(2));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EmailDesignerViewModel_GetSources")]
        public void EmailDesignerViewModel_GetSources_TwoMailSourcesOnServer_TwoSourcesAreReturned()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            var sources = viewModel.GetSources(new Mock<IEnvironmentModel>().Object);
            Assert.AreEqual(2, sources.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EmailDesignerViewModel_UpdateEnvironmentResourcesCallback")]
        public void EmailDesignerViewModel_UpdateEnvironmentResourcesCallback_FetchEmailSourcesWithNoSelectedEmailSource_FirstEmailSourceSetAsSelected()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.SetupGet(p => p.WorkspaceID).Returns(Guid.NewGuid);
            mockConnection.Setup(m => m.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            mockEnvironmentModel.SetupGet(p => p.Connection).Returns(mockConnection.Object);
            viewModel.UpdateEnvironmentResourcesCallback(mockEnvironmentModel.Object);
            Assert.IsTrue(viewModel.SelectedEmailSource == viewModel.EmailSourceList[1]);
        }
        
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_KeepsUserData_UserDataStillPersent()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            const string TestMailAccount = "test@mydomain.com";
            viewModel.FromAccount = TestMailAccount;
            
            viewModel.SelectedSelectedEmailSource = new EmailSource
            {
                UserName = "MyUser",
                Password = "MyPassword",
                EnableSsl = false,
                Host = "mx.mydomain.com",
                Port = 25,
                TestFromAddress = "bob@mydomain.com",
                ResourceID = Guid.NewGuid()
            };

            Assert.AreEqual(TestMailAccount, viewModel.FromAccount);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_HydratesFromSource_SourceDataOverwritesBlankUserData()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            const string TestMailAccount = "";
            viewModel.FromAccount = TestMailAccount;

            viewModel.SelectedSelectedEmailSource = new EmailSource
            {
                UserName = "MyUser",
                Password = "MyPassword",
                EnableSsl = false,
                Host = "mx.mydomain.com",
                Port = 25,
                TestFromAddress = "bob@mydomain.com",
                ResourceID = Guid.NewGuid()
            };

            Assert.AreEqual("MyUser", viewModel.FromAccount);
        }
        
        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSendEmailActivity());
        }
    }
}