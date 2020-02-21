/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
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
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers.Tests.RabbitMQ.Publish
{
    [TestClass]
    public class DsfRabbitMQPublishDesignerViewModelTest
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfRabbitMQPublishDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel(null, new Mock<IRabbitMQSourceModel>().Object);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfRabbitMQPublishDesignerViewModel_Constructor_IRabbitMQModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), null);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        public void DsfRabbitMQPublishDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
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
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        public void DsfRabbitMQPublishDesignerViewModel_Constructor1()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
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
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        public void DsfRabbitMQPublishDesignerViewModel_Validate()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            var vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
            vm.QueueName = "";
            vm.Message = null;
            vm.SelectedRabbitMQSource = null;

            //------------Execute Test---------------------------
            vm.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            var errors = vm.Errors;
            Assert.IsNotNull(errors);
            Assert.AreEqual(3, errors.Count);
            StringAssert.Contains(errors[0].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqSourceNotNullErrorTest);
            StringAssert.Contains(errors[1].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqQueueNameNotNullErrorTest);
            StringAssert.Contains(errors[2].Message, Warewolf.Resource.Errors.ErrorResource.RabbitMqMessageNotNullErrorTest);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        public void DsfRabbitMQPublishDesignerViewModel_EditRabbitMQSourceCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            var vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);
            //------------Execute Test---------------------------

            vm.EditRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            model.Verify(p => p.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RabbitMQPublishDesignerViewModel")]
        public void DsfRabbitMQPublishDesignerViewModel_NewRabbitMQSourceCommand_ShouldPublishShowNewResourceWizard()
        {
            var model = new Mock<IRabbitMQSourceModel>();
            var vm = new RabbitMQPublishDesignerViewModel(CreateModelItem(), model.Object);

            //------------Execute Test---------------------------
            vm.NewRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNull(vm.SelectedRabbitMQSource);
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
            model.Verify(p => p.CreateNewSource());
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfPublishRabbitMQActivity());
        }
    }
}