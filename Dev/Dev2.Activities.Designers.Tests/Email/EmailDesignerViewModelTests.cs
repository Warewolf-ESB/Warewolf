using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Email;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_AsyncWorkerIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new EmailDesignerViewModel(CreateModelItem(), null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new EmailDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_EventAggregatorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new EmailDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_ModelItemIsNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            const int EmailSourceCount = 2;
            var sources = CreateEmailSources(EmailSourceCount);

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(sources, modelItem);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.EditEmailSourceCommand);
            Assert.IsNotNull(viewModel.TestPasswordCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.EmailSources);
            Assert.IsNotNull(viewModel.Priorities);
            Assert.IsFalse(viewModel.IsEmailSourceSelected);
            Assert.IsFalse(viewModel.IsRefreshing);
            Assert.IsTrue(viewModel.CanTestEmailAccount);

            Assert.AreEqual(EmailSourceCount + 2, viewModel.EmailSources.Count);
            Assert.AreEqual(viewModel.EmailSources[0], viewModel.SelectedEmailSource);
            Assert.AreEqual("Select an Email Source...", viewModel.EmailSources[0].ResourceName);

            Assert.IsNull(viewModel.EmailSource);

            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_ModelItemIsNotNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int EmailSourceCount = 2;
            var sources = CreateEmailSources(EmailSourceCount);
            var selectedEmailSource = sources.First();

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedEmailSource", selectedEmailSource);

            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(sources, modelItem);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.EditEmailSourceCommand);
            Assert.IsNotNull(viewModel.TestPasswordCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.EmailSources);
            Assert.IsNotNull(viewModel.Priorities);
            Assert.IsTrue(viewModel.IsEmailSourceSelected);
            Assert.IsFalse(viewModel.IsRefreshing);
            Assert.IsTrue(viewModel.CanTestEmailAccount);

            Assert.AreEqual(EmailSourceCount + 1, viewModel.EmailSources.Count);
            Assert.AreEqual(selectedEmailSource, viewModel.SelectedEmailSource);

            Assert.AreEqual("New Email Source...", viewModel.EmailSources[0].ResourceName);
            Assert.AreNotEqual(Guid.Empty, viewModel.EmailSources[0].ResourceID);

            Assert.IsNotNull(viewModel.EmailSource);

            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_Constructor")]
        public void EmailDesignerViewModel_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount()
        {
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("test@mydomain.com");
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("");
        }

        void Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount(string expectedFromAccount)
        {
            //------------Setup for test--------------------------
            var activity = new DsfSendEmailActivity { FromAccount = expectedFromAccount };

            var emailSource = new EmailSource
            {
                UserName = "bob@mydomain.com",
                Password = "MyPassword",
                EnableSsl = false,
                Host = "mx.mydomain.com",
                Port = 25,
                ResourceID = Guid.NewGuid()
            };
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var viewModel = CreateViewModel(null, modelItem);

            //------------Execute Test---------------------------
            viewModel.SelectedEmailSource = emailSource;

            //------------Assert Results-------------------------
            var fromAccount = modelItem.GetProperty<string>("FromAccount");
            Assert.AreEqual(expectedFromAccount, fromAccount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_EditEmailSource")]
        public void EmailDesignerViewModel_EditEmailSource_PublishesShowEditResourceWizardMessage()
        {
            //------------Setup for test--------------------------
            var emailSources = CreateEmailSources(2);

            var selectedEmailSource = emailSources.First();

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedEmailSource", selectedEmailSource);

            ShowEditResourceWizardMessage message = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>())).Callback((object m) => message = m as ShowEditResourceWizardMessage).Verifiable();

            var resourceModel = new Mock<IResourceModel>();

            var viewModel = CreateViewModel(emailSources, modelItem, eventPublisher.Object, resourceModel.Object);

            //------------Execute Test---------------------------
            viewModel.EditEmailSourceCommand.Execute(null);


            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>()));
            Assert.AreSame(resourceModel.Object, message.ResourceModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailDesignerViewModel_CreateEmailSource")]
        public void EmailDesignerViewModel_CreateEmailSource_PublishesShowNewResourceWizard()
        {
            //------------Setup for test--------------------------
            var emailSources = CreateEmailSources(2);

            var selectedEmailSource = emailSources.First();

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedEmailSource", selectedEmailSource);

            ShowNewResourceWizard message = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowNewResourceWizard>())).Callback((object m) => message = m as ShowNewResourceWizard).Verifiable();

            var resourceModel = new Mock<IResourceModel>();

            var viewModel = CreateViewModel(emailSources, modelItem, eventPublisher.Object, resourceModel.Object);

            var createEmailSource = viewModel.EmailSources[0];
            Assert.AreEqual("New Email Source...", createEmailSource.ResourceName);

            //------------Execute Test---------------------------
            viewModel.SelectedEmailSource = createEmailSource;

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowNewResourceWizard>()));
            Assert.AreSame("EmailSource", message.ResourceType);
        }


        // TODO: TestPassword, ChooseAttachments, Validate

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSendEmailActivity());
        }

        static List<EmailSource> CreateEmailSources(int count)
        {
            var result = new List<EmailSource>();

            for(var i = 0; i < count; i++)
            {
                result.Add(new EmailSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "Email" + i,
                });
            }

            return result;
        }

        static TestEmailDesignerViewModel CreateViewModel(List<EmailSource> sources, ModelItem modelItem)
        {
            return CreateViewModel(sources, modelItem, new Mock<IEventAggregator>().Object, new Mock<IResourceModel>().Object);
        }

        static TestEmailDesignerViewModel CreateViewModel(List<EmailSource> sources, ModelItem modelItem, IEventAggregator eventPublisher, IResourceModel resourceModel)
        {
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ResourceRepository.FindSourcesByType<EmailSource>(It.IsAny<IEnvironmentModel>(), enSourceType.EmailSource))
                .Returns(sources);
            environment.Setup(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>()))
                .Returns(resourceModel);

            return new TestEmailDesignerViewModel(modelItem, environment.Object, eventPublisher);
        }
    }
}