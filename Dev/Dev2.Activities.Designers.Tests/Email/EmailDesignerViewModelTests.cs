using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Email;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Email
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EmailDesignerViewModelTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new EmailDesignerViewModel(CreateModelItem());

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.EditEmailSourceCommand);
            Assert.IsNotNull(viewModel.EmailSourceList);
            Assert.AreEqual(1, viewModel.EmailSourceList.Count);
        }

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
            mockConnection.Setup(m => m.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            mockEnvironmentModel.SetupGet(p => p.Connection).Returns(mockConnection.Object);
            viewModel.UpdateEnvironmentResourcesCallback(mockEnvironmentModel.Object);
            Assert.IsTrue(viewModel.SelectedEmailSource == viewModel.EmailSourceList[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_KeepsUserData_UserDataStillPresent()
        {
            var modelItem = CreateModelItem();
            var mockEventPublisher = new Mock<IEventAggregator>();
            mockEventPublisher.Setup(m => m.Publish(It.IsAny<ICallBackMessage<IEnvironmentModel>>())).Verifiable();
            var viewModel = new TestEmailDesignerViewModel(modelItem, mockEventPublisher.Object);
            const string TestMailAccount = "test@mydomain.com";
            viewModel.FromAccount = TestMailAccount;

            viewModel.TheSelectedEmailSource = new EmailSource
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

            viewModel.TheSelectedEmailSource = new EmailSource
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_EditEmailSource")]
        public void EmailDesignerViewModel_EditEmailSource_PublishesShowEditResourceWizardMessage()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>();

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceModel.Object);
            env.Setup(e => e.Connection.WorkspaceID).Returns(Guid.Empty);
            env.Setup(e => e.Connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(""));

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<GetActiveEnvironmentCallbackMessage>()))
                .Callback((object message) => ((GetActiveEnvironmentCallbackMessage)message).Callback(env.Object));
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>())).Verifiable();

            var viewModel = new TestEmailDesignerViewModel(CreateModelItem(), eventPublisher.Object);
            viewModel.SelectedEmailSource = new EmailSource();

            //------------Execute Test---------------------------
            viewModel.EditEmailSource();

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_GetSources")]
        public void EmailDesignerViewModel_GetSources_ReturnsListOfObjects()
        {
            //------------Setup for test--------------------------
            var repo = new Mock<IResourceRepository>();

            EmailSource es = new EmailSource();
            List<EmailSource> src = new List<EmailSource> {es};

            repo.Setup(r => r.FindSourcesByType<EmailSource>(It.IsAny<IEnvironmentModel>(), enSourceType.EmailSource)).Returns(src);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            env.Setup(e => e.Connection.WorkspaceID).Returns(Guid.Empty);

            var eventPublisher = new Mock<IEventAggregator>();
            var viewModel = new EmailDesignerViewModel(CreateModelItem(), eventPublisher.Object);

            //------------Execute Test---------------------------
            var sources = viewModel.GetSources(env.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(sources);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSendEmailActivity());
        }
    }
}