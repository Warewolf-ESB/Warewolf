using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;
using Dev2;
using Dev2.Communication;
using Dev2.Utilities;
using Dev2.Studio.Interfaces;
using Moq;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {

        [TestMethod]
        public void GetDifferences_WhenSame_ShouldReturnNoConflictItems()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);


            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var current = CreateContextualResourceModel(chart);
            var diff = CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser(activityParser);
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(2, diffs.Count);
            Assert.IsTrue(diffs.All(d => !d.conflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());

            var tupleDifference = diffs[0];
            var currentChart = tupleDifference.current;
            var differenceChart = tupleDifference.difference;
            Assert.IsNotNull(diffs);

            //First Node
            var dev2Activity = currentChart.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = differenceChart.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1.UniqueID);



            //Second Node
            var tupleDifference1 = diffs[1];
            var currentChart1 = tupleDifference1.current;
            var differenceChart1 = tupleDifference1.difference;
            Assert.IsNotNull(diffs);
     
            var dev2ActivityD = currentChart1.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = differenceChart1.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(randomActivityUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1D.UniqueID);

        }

        private static IContextualResourceModel CreateContextualResourceModel(Flowchart chart)
        {
            var workflowHelper = new WorkflowHelper();
            var tempResource = new Mock<IContextualResourceModel>();

            var builder = workflowHelper.CreateWorkflow("bob");
            builder.Implementation = chart;
            var tempResourceWorkflowXaml = workflowHelper.GetXamlDefinition(builder);
            var mock = new Mock<IServer>();
            mock.Setup(server => server.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(),
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns(new ExecuteMessage()
                {
                    HasError = false,
                    Message = tempResourceWorkflowXaml
                });
            tempResource.Setup(model => model.Environment).Returns(mock.Object);
            tempResource.Setup(model => model.Category).Returns(@"Unassigned\" + "bob");
            tempResource.Setup(model => model.ResourceName).Returns("bob");
            tempResource.Setup(model => model.DisplayName).Returns("bob");
            tempResource.Setup(model => model.IsNewWorkflow).Returns(true);
            tempResource.Setup(model => model.WorkflowXaml).Returns(tempResourceWorkflowXaml);

            return tempResource.Object;
        }

        [TestInitialize]
        public void Init()
        {

        }

        [TestCleanup]
        public void CleanUp()
        {
            CustomContainer.DeRegister<IActivityParser>();
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        public void GetDifferences_WhenDifferent_ShouldReturnConflictItems()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);

            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId,
                        DisplayName = "DisplayName"
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var current = CreateContextualResourceModel(chart);
            var diff = CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(2, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.conflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.current.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.difference.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(calculateUniqueId, dev2Activity1.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);

            //difference chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.current.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.difference.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(randomActivityUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1D.UniqueID);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_GivenNullParser_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var psd = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------

        }
    }
}
