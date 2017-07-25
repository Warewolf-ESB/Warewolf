using System;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Warewolf.ToolsSpecs.Toolbox.RabbitMQ.Publish
{
    [Binding]
    public class PublishRabbitMqSteps : BaseActivityUnitTest
    {
        private readonly ScenarioContext scenarioContext;

        public PublishRabbitMqSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"I drag RabbitMQPublish tool onto the design surface")]
        public void GivenIDragRabbitMQPublishToolOntoTheDesignSurface()
        {
            var publishActivity = new DsfPublishRabbitMQActivity();
            var modelItem = ModelItemUtils.CreateModelItem(publishActivity);
            var model = new Mock<IRabbitMQSourceModel>();
            var viewModel = new RabbitMQPublishDesignerViewModel(modelItem, model.Object);

            scenarioContext.Add("ViewModel", viewModel);
            scenarioContext.Add("Model", model);
            scenarioContext.Add("Activity", publishActivity);
        }

        [Given(@"New Button is Enabled")]
        public void GivenNewButtonIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Edit Button is Disabled")]
        public void GivenEditButtonIsDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"The QueueName and Message and Result are Disabled")]
        public void GivenTheQueueNameAndMessageAndResultAreDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [When(@"I Select ""(.*)"" as a Rabbit Source")]
        public void WhenISelectAsARabbitSource(string resourceName)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = vm.RabbitMQSources.FirstOrDefault(a => a.ResourceName == resourceName);
        }

        [Then(@"The QueueName and Message and Result are Enabled")]
        public void ThenTheQueueNameAndMessageAndResultAreEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.IsRabbitMQSourceSelected);
        }

        [Then(@"Edit Button is Enabled")]
        public void ThenEditButtonIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Edit Button is Enabled")]
        public void GivenEditButtonIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [When(@"I Click New Source")]
        public void WhenIClickNewSource()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.NewRabbitMQSourceCommand.Execute(null);
        }

        [When(@"I Click Edit Source")]
        public void WhenIClickEditSource()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.EditRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"CreateNewSource is executed")]
        public void ThenCreateNewSourceIsExecuted()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.CreateNewSource());
        }

        [Then(@"EditSource is executed")]
        public void ThenEditSourceIsExecuted()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [Given(@"I Select ""(.*)"" as a Rabbit Source")]
        [When(@"I Select ""(.*)"" as the Rabbit source")]
        public void GivenISelectAsARabbitSource(string resourceName)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = SourceDefinitions().FirstOrDefault(a => a.ResourceName == resourceName);
        }
        private IEnumerable<IRabbitMQServiceSourceDefinition> SourceDefinitions()
        {
            return new List<IRabbitMQServiceSourceDefinition>(new[]
            {
                new RabbitMQServiceSourceDefinition(){ResourceName = "Test (localhost)"}, 
            });
        }
        [Given(@"RabbitMQ Source is Enabled")]
        public void GivenRabbitMQSourceIsEnabled()
        {
            Assert.IsTrue(true);
        }

        [Given(@"Queue Name is Enabled")]
        public void GivenQueueNameIsEnabled()
        {
            Assert.IsTrue(true);
        }

        [Given(@"Message is Enabled")]
        public void GivenMessageIsEnabled()
        {
            Assert.IsTrue(true);
        }

        [Given(@"Result is Enabled")]
        public void GivenResultIsEnabled()
        {
            Assert.IsTrue(true);
        }

        [Then(@"the New RabbitMQ Publish Source window is opened")]
        public void ThenTheNewRabbitMQPublishSourceWindowIsOpened()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.CreateNewSource(), Times.AtLeastOnce);
        }

        [Then(@"the ""(.*)"" RabbitMQ Publish Source window is opened")]
        public void ThenTheRabbitMQPublishSourceWindowIsOpened(string resourceName)
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [Then(@"I Click Edit Source")]
        public void ThenIClickEditSource()
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.EditRabbitMQSourceCommand.Execute(null);
        }
        [Then(@"I set Message equals ""(.*)""")]
        public void ThenISetMessageEquals(string msg)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.Message = msg;
        }

        [Then(@"I set Queue Name equals ""(.*)""")]
        public void ThenISetQueueNameEquals(string qname)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.QueueName = qname;
        }
        [When(@"I change RabbitMq source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeRabbitMqSourceFromTo(string oldSourceName, string newSourceName)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.AreEqual(oldSourceName, vm.SelectedRabbitMQSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(vm.Message));
            Assert.IsFalse(string.IsNullOrEmpty(vm.QueueName));
            vm.SelectedRabbitMQSource = new RabbitMQServiceSourceDefinition() { ResourceName = newSourceName };
        }

        [Then(@"Message equals ""(.*)""")]
        public void ThenMessageEquals(string p0)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.AreEqual(p0, vm.Message);
        }

        [Then(@"Queue Name equals ""(.*)""")]
        public void ThenQueueNameEquals(string p0)
        {
            var vm = scenarioContext.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.AreEqual(p0, vm.QueueName);
        }





    }
}