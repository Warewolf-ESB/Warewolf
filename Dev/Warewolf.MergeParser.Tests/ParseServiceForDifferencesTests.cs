//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Dev2.Studio.Core.Activities.Utils;
//using System.Activities.Statements;
//using Unlimited.Applications.BusinessDesignStudio.Activities;
//using Dev2.Activities;
//using System.Linq;
//using Dev2;
//using Dev2.Communication;
//using Dev2.Utilities;
//using Dev2.Studio.Interfaces;
//using Moq;

//namespace Warewolf.MergeParser.Tests
//{
//    [TestClass]
//    public class ParseServiceForDifferencesTests
//    {

//        [TestMethod]
//        public void GetDifferences_WhenSame_ShouldReturnNoConflictItems()
//        {
//            var activityParser = new ActivityParser();
//            var shellView = new Mock<IShellViewModel>();
//            var serverMock = new Mock<IServer>();
//            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
//            CustomContainer.Register(shellView.Object);


//            var randomActivityUniqueId = Guid.NewGuid().ToString();
//            var calculateUniqueId = Guid.NewGuid().ToString();
//            var chart = new Flowchart
//            {
//                StartNode = new FlowStep
//                {
//                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
//                    Next =
//                    new FlowStep
//                    {
//                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
//                    }
//                }
//            };

//            var otherChart = new Flowchart
//            {
//                StartNode = new FlowStep
//                {
//                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
//                    Next =
//                    new FlowStep
//                    {
//                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
//                    }
//                }
//            };

//            var current = CreateContextualResourceModel(chart);
//            var diff = CreateContextualResourceModel(otherChart);

//            var psd = new ServiceDifferenceParser(activityParser);
//            var diffs = psd.GetDifferences(current, diff);

//            var currentChart = diffs.current;
//            var differenceChart = diffs.difference;
//            var differencrStore = diffs.differenceStore;
//            Assert.IsNotNull(diffs);

//            Assert.AreEqual(2, diffs.differenceStore.Count);
//            Assert.IsTrue(diffs.differenceStore.All(d => !d.Value));
//            Assert.AreEqual(randomActivityUniqueId, differencrStore[1].Key.ToString());
//            Assert.AreEqual(calculateUniqueId, differencrStore[0].Key.ToString());

//            //Current chart
//            var topLevelActivity = currentChart.GetCurrentValue<IDev2Activity>();
//            var toFlatList = activityParser.ParseToLinkedFlatList(topLevelActivity).ToList();
//            var dev2Activity = toFlatList[0];
//            var dev2Activity1 = toFlatList[1];
//            Assert.IsNotNull(dev2Activity);
//            Assert.IsNotNull(dev2Activity1);
//            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);
//            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);

//            //difference chart
//            var topLevelDiffActivity = differenceChart.GetCurrentValue<IDev2Activity>();
//            var toFlatList1 = activityParser.ParseToLinkedFlatList(topLevelDiffActivity).ToList();
//            var dev2ActivityD = toFlatList1[0];
//            var dev2Activity1D = toFlatList1[1];
//            Assert.IsNotNull(dev2ActivityD);
//            Assert.IsNotNull(dev2Activity1D);
//            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
//            Assert.AreEqual(randomActivityUniqueId, dev2Activity1D.UniqueID);
//        }

//        private static IContextualResourceModel CreateContextualResourceModel(Flowchart chart)
//        {
//            var workflowHelper = new WorkflowHelper();
//            var tempResource = new Mock<IContextualResourceModel>();

//            var builder = workflowHelper.CreateWorkflow("bob");
//            builder.Implementation = chart;
//            var tempResourceWorkflowXaml = workflowHelper.GetXamlDefinition(builder);
//            var mock = new Mock<IServer>();
//            mock.Setup(server => server.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(),
//                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
//                .Returns(new ExecuteMessage()
//                {
//                    HasError = false,
//                    Message = tempResourceWorkflowXaml
//                });
//            tempResource.Setup(model => model.Environment).Returns(mock.Object);
//            tempResource.Setup(model => model.Category).Returns(@"Unassigned\" + "bob");
//            tempResource.Setup(model => model.ResourceName).Returns("bob");
//            tempResource.Setup(model => model.DisplayName).Returns("bob");
//            tempResource.Setup(model => model.IsNewWorkflow).Returns(true);
//            tempResource.Setup(model => model.WorkflowXaml).Returns(tempResourceWorkflowXaml);

//            return tempResource.Object;
//        }

//        [TestInitialize]
//        public void Init()
//        {

//        }

//        [TestCleanup]
//        public void CleanUp()
//        {
//            CustomContainer.DeRegister<IActivityParser>();
//            CustomContainer.DeRegister<IShellViewModel>();
//        }

//        [TestMethod]
//        public void GetDifferences_WhenDifferent_ShouldReturnConflictItems()
//        {
//            var activityParser = new ActivityParser();
//            var shellView = new Mock<IShellViewModel>();
//            var serverMock = new Mock<IServer>();
//            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
//            CustomContainer.Register(shellView.Object);
//            CustomContainer.Register<IActivityParser>(activityParser);

//            var randomActivityUniqueId = Guid.NewGuid().ToString();
//            var calculateUniqueId = Guid.NewGuid().ToString();
//            var chart = new Flowchart
//            {
//                StartNode = new FlowStep
//                {
//                    Action = new DsfRandomActivity
//                    {
//                        UniqueID = randomActivityUniqueId,
//                        DisplayName = "DisplayName"
//                    },
//                    Next =
//                    new FlowStep
//                    {
//                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
//                    }
//                }
//            };

//            var otherChart = new Flowchart
//            {
//                StartNode = new FlowStep
//                {
//                    Action = new DsfRandomActivity
//                    {
//                        UniqueID = randomActivityUniqueId
//                    },
//                    Next =
//                    new FlowStep
//                    {
//                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
//                    }
//                }
//            };

//            var current = CreateContextualResourceModel(chart);
//            var diff = CreateContextualResourceModel(otherChart);

//            var psd = new ServiceDifferenceParser();
//            var diffs = psd.GetDifferences(current, diff);
//            var currentChart = diffs.current;
//            var differenceChart = diffs.difference;
//            var differencrStore = diffs.differenceStore;
//            Assert.IsNotNull(diffs);
//            Assert.AreEqual(2, diffs.differenceStore.Count);
//            Assert.IsFalse(diffs.differenceStore[0].Value);
//            Assert.IsTrue(diffs.differenceStore[1].Value);
//            Assert.AreEqual(randomActivityUniqueId, differencrStore[1].Key.ToString());
//            Assert.AreEqual(calculateUniqueId, differencrStore[0].Key.ToString());

//            //Current chart
//            var topLevelActivity = currentChart.GetCurrentValue<IDev2Activity>();
//            var toFlatList = CustomContainer.Get<IActivityParser>().ParseToLinkedFlatList(topLevelActivity).ToList();
//            var dev2Activity = toFlatList[0];
//            var dev2Activity1 = toFlatList[1];
//            Assert.IsNotNull(dev2Activity);
//            Assert.IsNotNull(dev2Activity1);
//            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);
//            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);

//            //difference chart
//            var topLevelDiffActivity = differenceChart.GetCurrentValue<IDev2Activity>();
//            var toFlatList1 = CustomContainer.Get<IActivityParser>().ParseToLinkedFlatList(topLevelDiffActivity).ToList();
//            var dev2ActivityD = toFlatList1[0];
//            var dev2Activity1D = toFlatList1[1];
//            Assert.IsNotNull(dev2ActivityD);
//            Assert.IsNotNull(dev2Activity1D);
//            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
//            Assert.AreEqual(randomActivityUniqueId, dev2Activity1D.UniqueID);
//        }

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void Constructor_GivenNullParser_ShouldThrowException()
//        {
//            //---------------Set up test pack-------------------
//            var psd = new ServiceDifferenceParser();
//            //---------------Assert Precondition----------------

//            //---------------Execute Test ----------------------

//            //---------------Test Result -----------------------

//        }
//    }
//}
