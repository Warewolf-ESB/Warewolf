using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Activities.Designers2.RabbitMQ.Consume;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces.Core;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.RabbitMQ.Consume
{
    [TestClass]
    public class RabbitMQConsumeDesignerViewModelTest
    {
        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Constructor")]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RabbitMQConsumeDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var vm = new RabbitMQConsumeDesignerViewModel(null, new Mock<IRabbitMQSourceModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Constructor")]
        public void RabbitMQConsumeDesignerViewModel_Constructor_Given_A_Model_Should_SetupCommonViewModelProperties()
        {
            //------------Setup for test--------------------------
            var model = CreateModelItem();
            //------------Execute Test---------------------------
            var vm = new RabbitMQConsumeDesignerViewModel(model, new Mock<IRabbitMQSourceModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsTrue(vm.HasLargeView);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(Visibility.Visible, vm.ThumbVisibility);
            Assert.IsNotNull(vm.EditRabbitMQSourceCommand);
            Assert.IsNotNull(vm.NewRabbitMQSourceCommand);
            Assert.IsNotNull(vm.RabbitMQSources);
        }

        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Constructor")]
        public void RabbitMQConsumeDesignerViewModel_Constructor1()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            //------------Execute Test---------------------------
            RabbitMQConsumeDesignerViewModel vm = new RabbitMQConsumeDesignerViewModel(CreateModelItem(), model.Object);
            vm.QueueName = "Q1";
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
            Assert.AreEqual(vm.Result, "Success");
            Assert.AreEqual(vm.IsRabbitMQSourceFocused, false);
            Assert.AreEqual(vm.IsQueueNameFocused, false);
            Assert.AreEqual(vm.IsMessageFocused, false);
        }

        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Validate")]
        public void RabbitMQConsumeDesignerViewModel_Validate_With_No_QueueName_ShouldBreakRule()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            var vm = new RabbitMQConsumeDesignerViewModel(CreateModelItem(), model.Object);
            vm.QueueName = "";
            vm.SelectedRabbitMQSource = new RabbitMQServiceSourceDefinition();
            //------------Execute Test---------------------------
            vm.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            var errors = vm.Errors;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any(info => info.Message == "'Queue Name' cannot be empty or only white space"));
        }

        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Validate")]
        public void RabbitMQConsumeDesignerViewModel_Validate_With_No_RabbitMQ_Source_ShouldBreakRule()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());

            var vm = new RabbitMQConsumeDesignerViewModel(CreateModelItem(), model.Object);
            vm.SelectedRabbitMQSource = null;
            //------------Execute Test---------------------------
            vm.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            var errors = vm.Errors;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any(info => info.Message == "'RabbitMQ Source' cannot be null"));
        }

        [TestMethod]
        [Owner("Sanele")]
        [TestCategory("RabbitMQConsumeDesignerViewModelTest_Commands")]
        public void RabbitMQConsumeDesignerViewModel_EditRabbitMQSourceCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            var vm = new RabbitMQConsumeDesignerViewModel(CreateModelItem(), model.Object);
            //------------Execute Test---------------------------
            vm.EditRabbitMQSourceCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            model.Verify(p => p.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        private static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfConsumeRabbitMQActivity());
        }
    }
}