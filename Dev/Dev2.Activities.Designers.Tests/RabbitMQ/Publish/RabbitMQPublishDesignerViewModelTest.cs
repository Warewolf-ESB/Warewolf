using Caliburn.Micro;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Activities.Designers.Tests.RabbitMQ.Publish
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class RabbitMQPublishDesignerViewModelTest
    {
        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(null, null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel_Constructor_IEnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, null, new Mock<IEventAggregator>().Object);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel_Constructor_IEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, null);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        public void RabbitMQPublishDesignerViewModel_Constructor()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            vm.QueueName = "Q1";
            vm.IsDurable = false;
            vm.IsExclusive = false;
            vm.IsAutoDelete = false;
            vm.Message = "Test Message";
            vm.Result = "Success";
            vm.IsRabbitMQSourceFocused = false;
            vm.IsQueueNameFocused = false;
            vm.IsMessageFocused = false;

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.EditRabbitMQSourceCommand);
            Assert.IsNotNull(vm.NewRabbitMQSourceCommand);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(vm.ThumbVisibility, Visibility.Visible);
            Assert.IsNotNull(vm.RabbitMQSources);
            Assert.IsFalse(vm.IsRabbitMQSourceFocused);
            Assert.IsFalse(vm.IsQueueNameFocused);
            Assert.IsFalse(vm.IsMessageFocused);
            Assert.IsNull(vm.SelectedRabbitMQSource);
            Assert.AreEqual(vm.QueueName, "Q1");
            Assert.AreEqual(vm.IsDurable, false);
            Assert.AreEqual(vm.IsExclusive, false);
            Assert.AreEqual(vm.IsAutoDelete, false);
            Assert.AreEqual(vm.Message, "Test Message");
            Assert.AreEqual(vm.Result, "Success");
            Assert.AreEqual(vm.IsRabbitMQSourceFocused, false);
            Assert.AreEqual(vm.IsQueueNameFocused, false);
            Assert.AreEqual(vm.IsMessageFocused, false);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        public void RabbitMQPublishDesignerViewModel_Validate()
        {
            //------------Setup for test--------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            vm.QueueName = "";
            vm.Message = null;
            vm.SelectedRabbitMQSource = null;

            //------------Execute Test---------------------------
            vm.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            List<IActionableErrorInfo> errors = vm.Errors;
            Assert.IsNotNull(errors);
            Assert.AreEqual(3, errors.Count);
            StringAssert.Contains(errors[0].Message, "'RabbitMQ Source' cannot be null");
            StringAssert.Contains(errors[1].Message, "'Queue Name' cannot be empty or only white space");
            StringAssert.Contains(errors[2].Message, "'Message' cannot be empty or only white space");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Commands")]
        public void SharepointListDesignerViewModelBase_SetSelectedSharepointServer_EditCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            Mock<IShellViewModel> mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>())).Verifiable();
            CustomContainer.Register(mockShellViewModel.Object);
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            vm.SelectedRabbitMQSource = new RabbitMQSource();
            //------------Execute Test---------------------------

            vm.EditRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedRabbitMQSource);
            mockShellViewModel.Verify(model => model.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Commands")]
        public void SharepointListDesignerViewModelBase_SetSelectedSharepointServer_NewCommand_ShouldPublishShowNewResourceWizard()
        {
            ShowNewResourceWizard message = null;
            Mock<IEventAggregator> eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowNewResourceWizard>())).Callback((object m) => message = m as ShowNewResourceWizard).Verifiable();
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, eventPublisher.Object);

            //------------Execute Test---------------------------
            vm.NewRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowNewResourceWizard>()));
        }

        private static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfPublishRabbitMQActivity());
        }
    }

    // ReSharper restore InconsistentNaming
}