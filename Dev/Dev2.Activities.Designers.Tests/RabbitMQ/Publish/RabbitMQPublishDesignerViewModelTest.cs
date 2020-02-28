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
using Dev2.Data.ServiceModel;
using Warewolf.Studio.ViewModels;

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
            vm.BasicPropertiesOptions = basicProperties;
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
            Assert.AreEqual(vm.BasicPropertiesOptions.Options[0].Name, "AutoCorrelation");
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_Validate_Expect_NoErrors()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IRabbitMQSourceModel>();
            model.Setup(m => m.RetrieveSources()).Returns(new List<IRabbitMQServiceSourceDefinition>());
            var vm = new RabbitMQPublishDesignerViewModel2(CreateModelItem(), model.Object)
            {
                QueueName = "Test",
                Message = "msg",
                SelectedRabbitMQSource = new Mock<IRabbitMQServiceSourceDefinition>().Object
            };

            //------------Execute Test---------------------------
            vm.Validate();

            //------------Assert Results-------------------------
            var errors = vm.Errors;
            Assert.IsNull(errors);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_Constructor2()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageRabbitMQSourceModel)
            };

            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            
            var mockUpdateManager = new Mock<IStudioUpdateManager>();
            var mockQueryManager = new Mock<IQueryManager>();
            
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();

            var rabbitMqSourceOne = new RabbitMQSource {ResourceID = guidOne, ResourceName = "ResourceOne", HostName = "HostOne"};
            var rabbitMqSourceTwo = new RabbitMQSource {ResourceID = guidTwo, ResourceName = "ResourceTwo", HostName = "HostTwo"};
            var rabbitMqServiceSourceDefinitions = new List<IRabbitMQServiceSourceDefinition>
            {
                new RabbitMQServiceSourceDefinition(rabbitMqSourceOne),
                new RabbitMQServiceSourceDefinition(rabbitMqSourceTwo),
            };
            mockQueryManager.Setup(o => o.FetchRabbitMQServiceSources()).Returns(rabbitMqServiceSourceDefinitions);

            var server = new Mock<IServer>();           
            server.Setup(server1 => server1.UpdateRepository).Returns(mockUpdateManager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(mockQueryManager.Object);
            
            var mockMainViewModel = new Mock<IShellViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            
            var publishRabbitMqActivity = new PublishRabbitMQActivity();
            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel2(ModelItemUtils.CreateModelItem(publishRabbitMqActivity));
            //------------Assert Results-------------------------
            Assert.AreEqual(2, vm.RabbitMQSources.Count);
            Assert.AreEqual(guidOne, vm.RabbitMQSources[0].ResourceID);
            Assert.AreEqual("ResourceOne", vm.RabbitMQSources[0].ResourceName);
            Assert.AreEqual("HostOne", vm.RabbitMQSources[0].HostName);
            Assert.AreEqual(guidTwo, vm.RabbitMQSources[1].ResourceID);
            Assert.AreEqual("ResourceTwo", vm.RabbitMQSources[1].ResourceName);
            Assert.AreEqual("HostTwo", vm.RabbitMQSources[1].HostName);
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
            var options = rabbitMQPublishDesignerViewModel2.BasicPropertiesOptions.Options.ToList();

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(typeof(OptionRadioButtons), options[0].GetType());
            Assert.AreEqual("AutoCorrelation", options[0].Name);
            
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_LoadBasicProperties_IsNull_SetBasicProperties()
        {
            var properties = new RabbitMqPublishOptions();
            var basicProperties = CreateModelProperty("BasicProperties", null).Object;
            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "BasicProperties", true).Returns(basicProperties);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            var rabbitMQPublishDesignerViewModel2 = new RabbitMQPublishDesignerViewModel2(mockModelItem.Object, new Mock<IRabbitMQSourceModel>().Object);
            var options = rabbitMQPublishDesignerViewModel2.BasicPropertiesOptions.Options.ToList();

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(typeof(OptionRadioButtons), options[0].GetType());
            Assert.AreEqual("AutoCorrelation", options[0].Name);
            
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_BasicProperties_UpdateBasicPropertiesModelItem()
        {
            var properties = new RabbitMqPublishOptions();
            var basicProperties = CreateModelProperty("BasicProperties", properties).Object;
            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "BasicProperties", true).Returns(basicProperties);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            var rabbitMQPublishDesignerViewModel2 = new RabbitMQPublishDesignerViewModel2(mockModelItem.Object, new Mock<IRabbitMQSourceModel>().Object);
            var options = rabbitMQPublishDesignerViewModel2.BasicPropertiesOptions.Options.ToList();
            rabbitMQPublishDesignerViewModel2.BasicPropertiesOptions.Notify();
            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(typeof(OptionRadioButtons), options[0].GetType());
            Assert.AreEqual("AutoCorrelation", options[0].Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RabbitMQPublishDesignerViewModel2))]
        public void RabbitMQPublishDesignerViewModel2_EditRabbitMQSourceCommand_ExistingSourceId()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageRabbitMQSourceModel)
            };

            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();

            var mockUpdateManager = new Mock<IStudioUpdateManager>();
            var mockQueryManager = new Mock<IQueryManager>();

            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();

            var rabbitMqSourceOne = new RabbitMQSource { ResourceID = guidOne, ResourceName = "ResourceOne", HostName = "HostOne" };
            var rabbitMqSourceTwo = new RabbitMQSource { ResourceID = guidTwo, ResourceName = "ResourceTwo", HostName = "HostTwo" };
            var rabbitMqServiceSourceDefinitionOne = new RabbitMQServiceSourceDefinition(rabbitMqSourceOne);
            var rabbitMqServiceSourceDefinitionTwo = new RabbitMQServiceSourceDefinition(rabbitMqSourceTwo);

            var rabbitMqServiceSourceDefinitions = new List<IRabbitMQServiceSourceDefinition>
            {
                rabbitMqServiceSourceDefinitionOne,
                rabbitMqServiceSourceDefinitionTwo,
            };
            mockQueryManager.Setup(o => o.FetchRabbitMQServiceSources()).Returns(rabbitMqServiceSourceDefinitions);

            var server = new Mock<IServer>();
            server.Setup(server1 => server1.UpdateRepository).Returns(mockUpdateManager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(mockQueryManager.Object);

            var mockMainViewModel = new Mock<IShellViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var publishRabbitMqActivity = new PublishRabbitMQActivity();
            //------------Execute Test---------------------------
            var vm = new RabbitMQPublishDesignerViewModel2(ModelItemUtils.CreateModelItem(publishRabbitMqActivity));
            //------------Execute Test---------------------------

            vm.SelectedRabbitMQSource = rabbitMqServiceSourceDefinitionOne;

            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));

            vm.EditRabbitMQSourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(rabbitMqServiceSourceDefinitionOne, vm.SelectedRabbitMQSource);
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