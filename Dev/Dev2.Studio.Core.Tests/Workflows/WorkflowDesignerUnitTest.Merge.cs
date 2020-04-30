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
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public partial class WorkflowDesignerUnitTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Studio Workflows Core")]
        [DoNotParallelize]
        public void AddItem_Given_MergeToolModel_VerifyCalls()
        {
            //------------Setup for test--------------------------
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.SaveToServer(It.IsAny<IResourceModel>(), It.IsAny<string>())).Verifiable();
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
            var actId = Guid.NewGuid();
            testAct.UniqueID = actId.ToString();           

            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();
            var _moq = new Mock<WorkflowDesigner>();
            var modelService = new Mock<ModelService>();
            var viewStateService = new Mock<ViewStateService>();
            var chart = new Flowchart();
            chart.Nodes.Add(testAct.GetFlowNode());
            var flowChart = ModelItemUtils.CreateModelItem(chart);
            modelService.Setup(p => p.Root).Returns(flowChart).Verifiable();
            modelService.Setup(p => p.Find(flowChart, typeof(Flowchart))).Returns(() => new List<ModelItem>() { flowChart }).Verifiable();
            var dHelper = new Mock<IWorkflowDesignerWrapper>();
            dHelper.Setup(p => p.GetService<ModelService>(It.IsAny<WorkflowDesigner>())).Returns(modelService.Object).Verifiable();
            dHelper.Setup(p => p.GetService<ViewStateService>(It.IsAny<WorkflowDesigner>())).Returns(viewStateService.Object);
            var wd = new MergePreviewWorkflowDesignerViewModelMock(dHelper.Object, crm.Object, wh.Object, eventAggregator.Object, _moq.Object);
            var obj = new Mock<IToolConflictItem>();
            obj.Setup(o => o.UniqueId).Returns(actId);
            wd.AddItem(obj.Object);
            dHelper.VerifyAll();
            modelService.VerifyAll();
            viewStateService.Verify(p => p.RemoveViewState(It.IsAny<ModelItem>(), It.IsAny<string>()));
            viewStateService.Verify(p => p.StoreViewState(It.IsAny<ModelItem>(), It.IsAny<string>(), It.IsAny<Point>()));
        }
    }
}