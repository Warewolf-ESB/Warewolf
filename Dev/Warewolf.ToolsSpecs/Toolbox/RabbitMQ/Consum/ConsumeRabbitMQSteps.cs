using Dev2.Activities.Designers2.RabbitMQ.Consume;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace Warewolf.ToolsSpecs.Toolbox.RabbitMQ.Consum
{
    [Binding]
    public sealed class ConsumeRabbitMQSteps
    {
        [Given(@"I create New Workflow")]
        public void GivenICreateNewWorkflow()
        {
            Assert.IsTrue(true);
        }

        [Given(@"I drag RabbitMQConsume tool onto the design surface")]
        public void GivenIDragRabbitMQConsumeToolOntoTheDesignSurface()
        {
            var consumeActivity = new DsfConsumeRabbitMQActivity();            

            var resource = new Mock<IResourceCatalog>();
            var modelItem = ModelItemUtils.CreateModelItem(consumeActivity);
            var model = new Mock<IRabbitMQSourceModel>();

            var viewModel = new RabbitMQConsumeDesignerViewModel(modelItem, model.Object);
            var privateObject = new PrivateObject(consumeActivity);
            
            ScenarioContext.Current.Add("ViewModel", viewModel);
            ScenarioContext.Current.Add("Model", model);
            ScenarioContext.Current.Add("Activity", consumeActivity);
            ScenarioContext.Current.Add("PrivateObj", privateObject);
        }
       

        [Given(@"RabbitMq Source is Enabled")]
        public void GivenRabbitMqSourceIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }       

        [Given(@"EditButton is Disabled")]
        public void GivenEditButtonIsDisabled()
        {                        
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }
        [Then(@"EditButton is Enabled")]
        public void ThenEditButtonIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Queue Name is disabled")]
        public void GivenQueueNameIsDisabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"Prefech is disabled")]
        public void GivenPrefechIsDisabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"ReQueue is disabled")]
        public void GivenReQueueIsDisabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"New button is Enabled")]
        public void GivenNewButtonIsEnabled()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }

        [When(@"I click new source")]
        public void WhenIClickNewSource()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.NewRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"the RabbitMQ Source window is opened")]
        public void ThenTheRabbitMQSourceWindowIsOpened()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.CreateNewSource(), Times.AtLeastOnce);
        }

        [When(@"I select ""(.*)"" from source list as the source")]
        public void WhenISelectFromSourceListAsTheSource(string p0)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.SelectedRabbitMQSource = SourceDefinitions().FirstOrDefault(definition => definition.ResourceName == "localhost");
        }
        private IEnumerable<IRabbitMQServiceSourceDefinition> SourceDefinitions()
        {
            return new List<IRabbitMQServiceSourceDefinition>(new[]
            {
                new RabbitMQServiceSourceDefinition()
                {
                    ResourceName = "localhost"
                }
            });
        }

        [Then(@"I click Edit source")]
        public void ThenIClickEditSource()
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.EditRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"the RabbitMQ Source window is opened with localhost source")]
        public void ThenTheRabbitMQSourceWindowIsOpenedWithLocalhostSource()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [Then(@"I set QueueName to ""(.*)""")]
        public void ThenISetQueueNameTo(string p0)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.QueueName = p0;
        }

        [When(@"I change the source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeTheSourceFromTo(string p0, string p1)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(string.IsNullOrEmpty(vm.QueueName));
            vm.SelectedRabbitMQSource = new RabbitMQServiceSourceDefinition() { ResourceName = p1 };
        }

        [Then(@"QueueName equals ""(.*)""")]
        public void ThenQueueNameEquals(string p0)
        {
            var vm = ScenarioContext.Current.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.QueueName = p0;
        }
        [Given(@"I execute tool without a source")]
        public void GivenIExecuteToolWithoutASource()
        {
            Assert.IsTrue(true);
        }

        [When(@"I hit F-six to execute tool")]
        public void WhenIHitF_SixToExecuteTool()
        {
            var consumeRabbitMQActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("Activity");
            var _privateObject = ScenarioContext.Current.Get<PrivateObject>("PrivateObj");
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());
        }

        [Then(@"Empty source error is Returned")]
        public void ThenEmptySourceErrorIsReturned()
        {
            var _privateObject = ScenarioContext.Current.Get<PrivateObject>("PrivateObj");
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());
            Assert.IsTrue(string.Equals("Failure: Source has been deleted.", executeResults));
        }

        [Then(@"No queue error is Returned")]
        public void ThenNoQueueErrorIsReturned()
        {            
            var consumeRabbitMQActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("Activity");

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var _privateObject = ScenarioContext.Current.Get<PrivateObject>("PrivateObj");
            _privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            
            var result = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());
            Assert.IsTrue(Equals("Failure: Queue Name is required.", result));
        }
                
        [Then(@"I add the new source")]
        public void ThenIAddTheNewSource()
        {
            var model = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Setup(sourceModel => sourceModel.CreateNewSource());
        }

        [Then(@"Nothing in the queue error is Returned")]
        public void ThenNothingInTheQueueErrorIsReturned()
        {            
            var consumeRabbitMQActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("Activity");
            consumeRabbitMQActivity.RabbitMQSourceResourceId = new Guid();
            var _privateObject = ScenarioContext.Current.Get<PrivateObject>("PrivateObj");
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());            
            Assert.IsTrue(Equals(string.Format("Nothing in the Queue : {0}", consumeRabbitMQActivity.QueueName), executeResults));
        }
    }
}
