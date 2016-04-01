using ActivityUnitTests;
using Caliburn.Micro;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.RabbitMQ.Publish
{
    [Binding]
    public class PublishRabbitMQSteps : BaseActivityUnitTest
    {
        [Given(@"I drag RabbitMQPublish tool onto the design surface")]
        public void GivenIDragRabbitMQPublishToolOntoTheDesignSurface()
        {
            var publishActivity = new DsfPublishRabbitMQActivity();
            var modelItem = ModelItemUtils.CreateModelItem(publishActivity);
            var model = new Mock<IRabbitMQSourceModel>();
            var viewModel = new RabbitMQPublishDesignerViewModel(modelItem, model.Object);

            ScenarioContext.Current.Add("ViewModel", viewModel);
            ScenarioContext.Current.Add("Model", model);
            ScenarioContext.Current.Add("Activity", publishActivity);
        }

        [Given(@"New Button is Enabled")]
        public void GivenNewButtonIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Edit Button is Disabled")]
        public void GivenEditButtonIsDisabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"The QueueName and Message and Result are Disabled")]
        public void GivenTheQueueNameAndMessageAndResultAreDisabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [When(@"I Select ""(.*)"" as a Rabbit Source")]
        public void WhenISelectAsARabbitSource(string resourceName)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = vm.RabbitMQSources.FirstOrDefault(a => a.ResourceName == resourceName);
        }

        [Then(@"The QueueName and Message and Result are Enabled")]
        public void ThenTheQueueNameAndMessageAndResultAreEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.IsRabbitMQSourceSelected);
        }

        [Then(@"Edit Button is Enabled")]
        public void ThenEditButtonIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Edit Button is Enabled")]
        public void GivenEditButtonIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [When(@"I Click New Source")]
        public void WhenIClickNewSource()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.NewRabbitMQSourceCommand.Execute(null);
        }

        [When(@"I Click Edit Source")]
        public void WhenIClickEditSource()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.EditRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"CreateNewSource is executed")]
        public void ThenCreateNewSourceIsExecuted()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.CreateNewSource());
        }

        [Then(@"EditSource is executed")]
        public void ThenEditSourceIsExecuted()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [Given(@"I Select ""(.*)"" as a Rabbit Source")]
        public void GivenISelectAsARabbitSource(string resourceName)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = vm.RabbitMQSources.FirstOrDefault(a => a.ResourceName == resourceName);
        }
    }
}