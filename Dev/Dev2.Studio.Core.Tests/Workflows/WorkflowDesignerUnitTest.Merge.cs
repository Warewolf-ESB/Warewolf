using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public partial class WorkflowDesignerUnitTest
    {


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_AddItem")]
        public void AddItem_Given_MergeToolModel_VerifyCalls()
        {
            //------------Setup for test--------------------------
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.SaveToServer(It.IsAny<IResourceModel>())).Verifiable();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));


            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });


            var eventArgs = new Mock<ModelChangedEventArgs>();
            eventArgs.Setup(c => c.ItemsAdded).Returns(new List<ModelItem> { source.Object });

            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();
            Mock<WorkflowDesigner> _moq = new Mock<WorkflowDesigner>();
            var modelService = new Mock<ModelService>();
            var chart = new Flowchart();
            var flowChart = ModelItemUtils.CreateModelItem(chart);
            modelService.Setup(p => p.Root).Returns(flowChart);
            _moq.Setup(a => a.Context.Services.GetService<ModelService>()).Returns(modelService.Object);
            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object, false, _moq.Object);
            var obj = new Mock<IMergeToolModel>();
            wd.AddItem(obj.Object);


        }
    }
}
