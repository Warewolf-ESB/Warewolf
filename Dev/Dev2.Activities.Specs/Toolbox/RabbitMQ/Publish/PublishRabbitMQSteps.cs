using System;
using System.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Studio.Core.Interfaces;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Specs.Toolbox.RabbitMQ.Publish
{
    [Binding]
    public class PublishRabbitMQSteps
    {
        [Given(@"I drag RabbitMQPublish tool onto the design surface")]
        public void GivenIDragRabbitMQPublishToolOntoTheDesignSurface()
        {
            var publishActivity = new DsfPublishRabbitMQActivity();
            var modelItem = ModelItemUtils.CreateModelItem(publishActivity);
            var model = new Mock<IRabbitMQModel>();
            var viewModel = new RabbitMQPublishDesignerViewModel(modelItem, model.Object, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            
            ScenarioContext.Current.Add("ViewModel",viewModel);
            ScenarioContext.Current.Add("Model", model);
        }
        
        //[Given(@"Edit Button is Enabled")]
        //public void GivenEditButtonIsEnabled()
        //{
        //    var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
        //    Assert.IsTrue(vm.NewRabbitMQSourceCommand.is);

        //}
        
        [When(@"I Select ""(.*)"" as a Rabbit Source")]
        public void WhenISelectAsARabbitSource(string resourceName)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = vm.RabbitMQSources.FirstOrDefault(a => a.ResourceName == resourceName);
        }
        
        [Then(@"The QueueName is enabled is Enabled")]
        public void ThenTheQueueNameIsEnabledIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQPublishDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.CanEditFields);
        }
        
        [Then(@"The Message is enabled is Enabled")]
        public void ThenTheMessageIsEnabledIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"The Result is enabled is Enabled")]
        public void ThenTheResultIsEnabledIsEnabled()
        {
            ScenarioContext.Current.Pending();
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
            var model = ScenarioContext.Current.Get< Mock<IRabbitMQModel>>("Model");
            model.Verify(a => a.CreateSource());
        }

    }
}
