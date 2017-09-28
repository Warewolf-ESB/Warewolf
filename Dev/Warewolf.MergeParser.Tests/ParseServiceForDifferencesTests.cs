using System;
using System.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;
using Dev2;
using Dev2.Communication;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Utilities;
using Dev2.Studio.Interfaces;
using Moq;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {

        [TestMethod]
        public void GetDifferences_WhenSame_ShouldReturnNohasConflictItems()
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
            Assert.IsTrue(diffs.All(d => !d.hasConflict));
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());

            var tupleDifference = diffs[0];
            var currentChart = tupleDifference.Item2;
            var differenceChart = tupleDifference.Item3;
            Assert.IsNotNull(diffs);

            //First Node
            var dev2Activity = currentChart.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = differenceChart.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);



            //Second Node
            var tupleDifference1 = diffs[1];
            var currentChart1 = tupleDifference1.Item2;
            var differenceChart1 = tupleDifference1.Item3;
            Assert.IsNotNull(diffs);

            var dev2ActivityD = currentChart1.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = differenceChart1.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1D.UniqueID);

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
        public void GetDifferences_WhenDifferent_ShouldReturnhasConflictItems()
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
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(calculateUniqueId, dev2Activity1.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);

            //difference chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.Item3.GetCurrentValue<IDev2Activity>();
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
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_WhenFlatToolAddedOnRemote_ShouldNullOnLocalChart()
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
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };
            var baseCOnvertId = Guid.NewGuid().ToString();
            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId
                    },
                    Next =
                    new FlowStep()
                    {
                        Action = new DsfBaseConvertActivity() { UniqueID = baseCOnvertId }
                        ,
                        Next = new FlowStep
                        {
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                        }
                    }
                }

            };

            var current = CreateContextualResourceModel(chart);
            var diff = CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(3, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity.UniqueID);

            //Second chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1D.UniqueID);

            //Third node
            //difference chart
            var valueTuple2 = diffs[2];
            var dev3Activity1D = valueTuple2.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNull(valueTuple2.Item2);
            Assert.IsNotNull(dev3Activity1D);
            Assert.AreEqual(baseCOnvertId, dev3Activity1D.UniqueID);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_WhenFlatToolAddedOnLocal_ShouldNullOnRemoteChart()
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
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };
            var baseCOnvertId = Guid.NewGuid().ToString();
            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId
                    },
                    Next =
                    new FlowStep()
                    {
                        Action = new DsfBaseConvertActivity() { UniqueID = baseCOnvertId }
                        ,
                        Next = new FlowStep
                        {
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                        }
                    }
                }
            };

            var current = CreateContextualResourceModel(otherChart);
            var diff = CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(3, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity.UniqueID);

            //Second chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1D.UniqueID);

            //Third node
            //difference chart
            var valueTuple2 = diffs[2];
            var dev3Activity1D = valueTuple2.Item2.GetCurrentValue<IDev2Activity>();
            Assert.IsNull(valueTuple2.Item3);
            Assert.IsNotNull(dev3Activity1D);
            Assert.AreEqual(baseCOnvertId, dev3Activity1D.UniqueID);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_WhenDecisionAddedOnLocal_ShouldNullOnRemoteChart()
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
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                TheStack = new List<Dev2Decision>() { },
                TrueArmText = "a",
                FalseArmText = "a",
                DisplayText = "a",
                Mode = Dev2DecisionMode.AND
            };
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serialize = serializer.Serialize(dev2DecisionStack);
            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId,
                    },
                    Next =
                        new FlowStep
                        {
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                            ,
                            Next = new FlowDecision()
                            {
                                True = new FlowStep()
                                {
                                    Action = new DsfMultiAssignActivity()
                                },
                                False = new FlowStep()
                                {
                                    Action = new DsfActivity()
                                },
                                Condition = new DsfFlowDecisionActivity()
                                {
                                    ExpressionText = serialize
                                }
                            }
                        }
                }
            };

            var current = CreateContextualResourceModel(otherChart);
            var diff = CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(3, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity.UniqueID);

            //Second chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1D.UniqueID);

            //Third node
            var valueTuple2 = diffs[2];
            var dev3Activity1D = valueTuple2.Item2.GetCurrentValue<IDev2Activity>();
            Assert.IsNull(valueTuple2.Item3);
            Assert.IsNotNull(dev3Activity1D);
            Assert.AreNotEqual(Guid.Empty.ToString(), dev3Activity1D.UniqueID);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_WhenToolModifiedOnBithSides_ShouldNullOnRemoteChart()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);

            var assignId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity()
                    {
                        FieldsCollection = new List<ActivityDTO>()
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",4),
                            new ActivityDTO("field3","field3",3),
                            new ActivityDTO("field3","field3",2),
                        },
                        UniqueID = assignId

                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity()
                    {
                        FieldsCollection = new List<ActivityDTO>()
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",2),
                            new ActivityDTO("field2","fff",4),
                            new ActivityDTO("field3","field3",3),
                        },
                        UniqueID = assignId

                    }
                }
            };

            var current = CreateContextualResourceModel(otherChart);
            var diff = CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(current, diff);

            Assert.AreEqual(1, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
            Assert.AreEqual(assignId, diffs[0].uniqueId.ToString());


            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.Item2.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.Item3.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(assignId, dev2Activity1.UniqueID);
            Assert.AreEqual(assignId, dev2Activity.UniqueID);

        }
    }
}
