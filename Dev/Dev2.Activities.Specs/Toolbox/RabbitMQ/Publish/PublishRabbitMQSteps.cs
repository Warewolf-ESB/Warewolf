using ActivityUnitTests;
using Caliburn.Micro;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
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
            var model = new Mock<IRabbitMQModel>();
            var viewModel = new RabbitMQPublishDesignerViewModel(modelItem, model.Object, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);

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

        [When(@"I Click New Source")]
        public void WhenIClickNewSource()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.NewRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"A new tab is opened")]
        public void ThenANewTabIsOpened()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQModel>>("Model");
            model.Verify(a => a.CreateNewSource());
        }

        [Given(@"I Select ""(.*)"" as a Rabbit Source")]
        public void GivenISelectAsARabbitSource(string resourceName)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = vm.RabbitMQSources.FirstOrDefault(a => a.ResourceName == resourceName);
        }
    }
}