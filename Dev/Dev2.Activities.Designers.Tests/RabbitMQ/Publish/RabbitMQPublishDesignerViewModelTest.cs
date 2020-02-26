/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.RabbitMQ.Publish2;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
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
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.UI;
using Moq.Protected;
using System.Linq;
using System.Threading;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Activities.Designers.Tests.RabbitMQ.Publish
{
    [TestClass]
    public class RabbitMQPublishDesignerViewModelTest
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel2_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel2(null, new Mock<IRabbitMQSourceModel>().Object);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RabbitMQPublishDesignerViewModel2_Constructor_IRabbitMQModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), null);

            //------------Assert Results-------------------------
            Assert.IsNull(vm);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), new Mock<IRabbitMQSourceModel>().Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_Constructor1()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());
            //------------Execute Test---------------------------

            var result = new List<RabbitMqPublishOptions>();
            var CorrelationID = new RabbitMqPublishOptions() {  };
            var options = OptionConvertor.Convert(CorrelationID);
            var basicProperties = new OptionsWithNotifier { Options = options };

            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), model.Object);
            vm.QueueName = "Q1";
            vm.BasicProperties = basicProperties;
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
            Assert.AreEqual(vm.BasicProperties.Options[0].Name, "CorrelationID");
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
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_Validate()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), model.Object);
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
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_Constructor2()
        {
            var modelSource = new Mock<IRabbitMQSourceModel>();
            modelSource.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());
                      
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();

            var server = new Mock<IServer>();           
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);

            var mockMainViewModel = new Mock<IShellViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(RabbitMQServiceSourceDefinition)
            };
            //------------Setup for test--------------------------
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem());
            vm.QueueName = "";
            vm.Message = null;
            vm.SelectedRabbitMQSource = null;

            //------------Execute Test---------------------------
            vm.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);           
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_LoadBasicProperties()
        {
            var properties = new RabbitMqPublishOptions();
            var basicProperties = CreateModelProperty("BasicProperties", properties).Object;
            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "BasicProperties", true).Returns(basicProperties);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            var rabbitMQPublishDesignerViewModel2 = new RabbitMQPublishDesignerViewModel2(mockModelItem.Object, new Mock<IRabbitMQSourceModel>().Object);
            var options = rabbitMQPublishDesignerViewModel2.BasicProperties.Options.ToList();

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(typeof(OptionAutocomplete), options[0].GetType());
            Assert.AreEqual("CorrelationID", options[0].Name);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_EditRabbitMQSourceCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), model.Object);
            //------------Execute Test---------------------------

            vm.EditRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            model.Verify(p => p.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_NewRabbitMQSourceCommand_ShouldPublishShowNewResourceWizard()
        {
            var model = new Mock<IRabbitMQSourceModel>();
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), model.Object);

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
            return ModelItemUtils.CreateModelItem(new PublishRabbitMQActivity());
        }
        private Mock<ModelProperty> CreateModelProperty(string name, object value)
        {
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.Name).Returns(name);
            prop.Setup(p => p.ComputedValue).Returns(value);
            return prop;
        }
    }
}