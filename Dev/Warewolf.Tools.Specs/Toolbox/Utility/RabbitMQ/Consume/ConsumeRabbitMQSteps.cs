using Dev2.Activities.Designers2.RabbitMQ.Consume;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Runtime.Interfaces;
using TechTalk.SpecFlow;

namespace Warewolf.ToolsSpecs.Toolbox.RabbitMQ.Consum
{
    [Binding]
    public sealed class ConsumeRabbitMQSteps
    {
        private readonly ScenarioContext scenarioContext;

        public ConsumeRabbitMQSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"I create New Workflow")]
        public void GivenICreateNewWorkflow()
        {
            Assert.IsTrue(true);
        }

        [Given(@"I drag RabbitMQConsume tool onto the design surface")]
        public void GivenIDragRabbitMQConsumeToolOntoTheDesignSurface()
        {
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new RabbitMQSource());
            var consumeActivity = new DsfConsumeRabbitMQActivity(resourceCatalog.Object);

            var modelItem = ModelItemUtils.CreateModelItem(consumeActivity);
            var model = new Mock<IRabbitMQSourceModel>();

            var viewModel = new RabbitMQConsumeDesignerViewModel(modelItem, model.Object);
            var privateObject = new PrivateObject(consumeActivity);

            scenarioContext.Add("ViewModel", viewModel);
            scenarioContext.Add("Model", model);
            scenarioContext.Add("Activity", consumeActivity);
            scenarioContext.Add("PrivateObj", privateObject);
        }

        [Given(@"RabbitMq Source is Enabled")]
        public void GivenRabbitMqSourceIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"EditButton is Disabled")]
        public void GivenEditButtonIsDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Then(@"EditButton is Enabled")]
        public void ThenEditButtonIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.EditRabbitMQSourceCommand.CanExecute(null));
        }

        [Given(@"Queue Name is disabled")]
        public void GivenQueueNameIsDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"Prefech is disabled")]
        public void GivenPrefechIsDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"ReQueue is disabled")]
        public void GivenReQueueIsDisabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(vm.IsRabbitMQSourceSelected);
        }

        [Given(@"New button is Enabled")]
        public void GivenNewButtonIsEnabled()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsTrue(vm.NewRabbitMQSourceCommand.CanExecute(null));
        }

        [When(@"I click new source")]
        public void WhenIClickNewSource()
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.NewRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"the RabbitMQ Source window is opened")]
        public void ThenTheRabbitMQSourceWindowIsOpened()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.CreateNewSource(), Times.AtLeastOnce);
        }

        [When(@"I select ""(.*)"" from source list as the source")]
        public void WhenISelectFromSourceListAsTheSource(string p0)
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
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
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.EditRabbitMQSourceCommand.Execute(null);
        }

        [Then(@"the RabbitMQ Source window is opened with localhost source")]
        public void ThenTheRabbitMQSourceWindowIsOpenedWithLocalhostSource()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Verify(a => a.EditSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [Then(@"I set QueueName to ""(.*)""")]
        public void ThenISetQueueNameTo(string p0)
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            vm.QueueName = p0;
        }

        [When(@"I change the source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeTheSourceFromTo(string p0, string p1)
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
            Assert.IsFalse(string.IsNullOrEmpty(vm.QueueName));
            vm.SelectedRabbitMQSource = new RabbitMQServiceSourceDefinition() { ResourceName = p1 };
        }

        [Then(@"QueueName equals ""(.*)""")]
        public void ThenQueueNameEquals(string p0)
        {
            var vm = scenarioContext.Get<RabbitMQConsumeDesignerViewModel>("ViewModel");
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
         

            var consumeRabbitMQActivity = scenarioContext.Get<DsfConsumeRabbitMQActivity>("Activity");
            var _privateObject = scenarioContext.Get<PrivateObject>("PrivateObj");
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());
        }

        [Then(@"Empty source error is Returned")]
        public void ThenEmptySourceErrorIsReturned()
        {
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(default(RabbitMQSource));

            var _privateObject = scenarioContext.Get<PrivateObject>("PrivateObj");
            _privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;
            Assert.IsTrue(executeResults != null && string.Equals("Failure: Source has been deleted.", executeResults[0]));
        }

        [Then(@"No queue error is Returned")]
        public void ThenNoQueueErrorIsReturned()
        {
            var consumeRabbitMQActivity = scenarioContext.Get<DsfConsumeRabbitMQActivity>("Activity");

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var _privateObject = scenarioContext.Get<PrivateObject>("PrivateObj");
            _privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            _privateObject.SetProperty("RabbitSource", rabbitMQSource.Object);

            var result = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;
            Assert.IsTrue(result != null && Equals("Failure: Queue Name is required.", result[0]));
        }

        [Then(@"I add the new source")]
        public void ThenIAddTheNewSource()
        {
            var model = scenarioContext.Get<Mock<IRabbitMQSourceModel>>("Model");
            model.Setup(sourceModel => sourceModel.CreateNewSource());
        }

        [Then(@"Nothing in the queue error is Returned")]
        public void ThenNothingInTheQueueErrorIsReturned()
        {
            var consumeRabbitMQActivity = scenarioContext.Get<DsfConsumeRabbitMQActivity>("Activity");
            consumeRabbitMQActivity.RabbitMQSourceResourceId = new Guid();
            var _privateObject = scenarioContext.Get<PrivateObject>("PrivateObj");
            var executeResults = _privateObject.Invoke("PerformExecution", new Dictionary<string, string>());
            Assert.IsTrue(Equals(string.Format("Nothing in the Queue : {0}", consumeRabbitMQActivity.QueueName), executeResults));
        }
    }
}