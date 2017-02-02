using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.RabbitMQ.Publish
{
    [TestClass]
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
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(null, new Mock<IRabbitMQSourceModel>().Object);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel_Constructor_IRabbitMQModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("RabbitMQPublishDesignerViewModel_Handle")]
        public void RabbitMQPublishDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new RabbitMQPublishDesignerViewModel(CreateModelItem(), new Mock<IRabbitMQSourceModel>().Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Constructor")]
        public void RabbitMQPublishDesignerViewModel_Constructor1()
        {
            //------------Setup for test--------------------------
            Mock<IRabbitMQSourceModel> model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            //------------Execute Test---------------------------
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
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
            Assert.IsFalse(vm.ShowLarge);
            Assert.AreEqual(vm.ThumbVisibility, Visibility.Collapsed);
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
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Validate")]
        public void RabbitMQPublishDesignerViewModel_Validate()
        {
            //------------Setup for test--------------------------
            Mock<IRabbitMQSourceModel> model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
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
            StringAssert.Contains(errors[0].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqSourceNotNullErrorTest);
            StringAssert.Contains(errors[1].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqQueueNameNotNullErrorTest);
            StringAssert.Contains(errors[2].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqMessageNotNullErrorTest);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Commands")]
        public void RabbitMQPublishDesignerViewModel_EditRabbitMQSourceCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            Mock<IRabbitMQSourceModel> model = new Mock<IRabbitMQSourceModel>();
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
            //------------Execute Test---------------------------

            vm.EditRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            model.Verify(p => p.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("RabbitMQPublishDesignerViewModelTest_Commands")]
        public void RabbitMQPublishDesignerViewModel_NewRabbitMQSourceCommand_ShouldPublishShowNewResourceWizard()
        {
            Mock<IRabbitMQSourceModel> model = new Mock<IRabbitMQSourceModel>();
            RabbitMQPublishDesignerViewModel vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);

            //------------Execute Test---------------------------
            vm.NewRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNull(vm.SelectedRabbitMQSource);
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
            model.Verify(p => p.CreateNewSource());
        }

        private static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfPublishRabbitMQActivity());
        }
    }
}