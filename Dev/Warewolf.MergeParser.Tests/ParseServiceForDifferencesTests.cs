using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    [DoNotParallelize]
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

            var current = ParserTestHelper.CreateContextualResourceModel(chart);
            var diff = ParserTestHelper.CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser(activityParser, new ResourceDefinationCleaner());
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(4, count);

            Assert.IsTrue(diffTree.All(d => !d.IsInConflict));
            Assert.AreEqual(randomActivityUniqueId, diffTree[0].UniqueId);
            Assert.AreEqual(calculateUniqueId, diffTree[1].UniqueId);

            Assert.IsTrue(currentTree.All(d => !d.IsInConflict));
            Assert.AreEqual(randomActivityUniqueId, currentTree[0].UniqueId);
            Assert.AreEqual(calculateUniqueId, currentTree[1].UniqueId);

            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            //First Node
            var devActivityCurr = tupleCurrent.Activity;
            var devActivityDiff = tupleDifference.Activity;
            Assert.IsNotNull(devActivityCurr);
            Assert.IsNotNull(devActivityDiff);
            Assert.AreEqual(randomActivityUniqueId, devActivityCurr.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, devActivityDiff.UniqueID);

            //Second Node
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityCurr1 = tupleCurrent1.Activity;
            var devActivityDiff1 = tupleDifference1.Activity;
            Assert.IsNotNull(devActivityCurr1);
            Assert.IsNotNull(devActivityDiff1);
            Assert.AreEqual(calculateUniqueId, devActivityCurr1.UniqueID);
            Assert.AreEqual(calculateUniqueId, devActivityDiff1.UniqueID);
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

            var current = ParserTestHelper.CreateContextualResourceModel(chart);
            var diff = ParserTestHelper.CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(4, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.AreEqual(randomActivityUniqueId, diffTree[0].UniqueId);
            Assert.AreEqual(calculateUniqueId, diffTree[1].UniqueId);

            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(randomActivityUniqueId, currentTree[0].UniqueId);
            Assert.AreEqual(calculateUniqueId, currentTree[1].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);
            Assert.IsFalse(tupleDifference.IsInConflict);
            Assert.IsFalse(tupleCurrent.IsInConflict);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(randomActivityUniqueId, devActivityCurr.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, devActivityDiff.UniqueID);

            //difference chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);
            Assert.IsFalse(tupleDifference1.IsInConflict);
            Assert.IsFalse(tupleCurrent1.IsInConflict);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.AreEqual(calculateUniqueId, devActivityDiff1.UniqueID);
            Assert.AreEqual(calculateUniqueId, devActivityCurr1.UniqueID);
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
            var baseConvertId = Guid.NewGuid().ToString();
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
                        Action = new DsfBaseConvertActivity { UniqueID = baseConvertId },
                        Next = new FlowStep
                        {
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                        }
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(chart);
            var diff = ParserTestHelper.CreateContextualResourceModel(otherChart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(5, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.AreEqual(calculateUniqueId, diffTree[2].UniqueId);
            Assert.AreEqual(baseConvertId, diffTree[1].UniqueId);

            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(calculateUniqueId, currentTree[1].UniqueId);
            Assert.AreEqual(randomActivityUniqueId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityCurr);
            Assert.IsNotNull(devActivityDiff);
            Assert.AreEqual(randomActivityUniqueId, devActivityDiff.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, devActivityCurr.UniqueID);

            //Second chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.AreEqual(baseConvertId, devActivityDiff1.UniqueID);
            Assert.AreEqual(calculateUniqueId, devActivityCurr1.UniqueID);

            //Third node
            //difference chart
            var tupleDifference2 = diffTree[2];

            var devActivityDiff2 = tupleDifference2.Activity;
            Assert.IsNotNull(devActivityDiff2);
            Assert.AreEqual(calculateUniqueId, devActivityDiff2.UniqueID);
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
            var baseConvertId = Guid.NewGuid().ToString();
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
                        Action = new DsfBaseConvertActivity { UniqueID = baseConvertId }
                        ,
                        Next = new FlowStep
                        {
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                        }
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(5, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.AreEqual(calculateUniqueId, diffTree[1].UniqueId);
            Assert.AreEqual(randomActivityUniqueId, diffTree[0].UniqueId);

            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(baseConvertId, currentTree[1].UniqueId);
            Assert.AreEqual(randomActivityUniqueId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(randomActivityUniqueId, devActivityCurr.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, devActivityDiff.UniqueID);

            //Second chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.AreEqual(calculateUniqueId, devActivityDiff1.UniqueID);
            Assert.AreEqual(baseConvertId, devActivityCurr1.UniqueID);
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
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>(),
                TrueArmText = "a",
                FalseArmText = "a",
                DisplayText = "a",
                Mode = Dev2DecisionMode.AND
            };
            var serializer = new Dev2JsonSerializer();
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
                            Action = new DsfCalculateActivity { UniqueID = calculateUniqueId },
                            Next = new FlowDecision
                            {
                                True = new FlowStep
                                {
                                    Action = new DsfMultiAssignActivity()
                                },
                                False = new FlowStep
                                {
                                    Action = new DsfActivity()
                                },
                                Condition = new DsfFlowDecisionActivity
                                {
                                    ExpressionText = serialize
                                }
                            }
                        }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(7, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.AreEqual(calculateUniqueId, diffTree[1].UniqueId);
            Assert.AreEqual(randomActivityUniqueId, diffTree[0].UniqueId);

            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(calculateUniqueId, currentTree[1].UniqueId);
            Assert.AreEqual(randomActivityUniqueId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(randomActivityUniqueId, devActivityCurr.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, devActivityDiff.UniqueID);

            //Second chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.AreEqual(calculateUniqueId, devActivityDiff1.UniqueID);
            Assert.AreEqual(calculateUniqueId, devActivityCurr1.UniqueID);
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
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
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
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
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

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(2, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.AreEqual(assignId, diffTree[0].UniqueId);

            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(assignId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(assignId, devActivityCurr.UniqueID);
            Assert.AreEqual(assignId, devActivityDiff.UniqueID);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FlowDecision_GetDifferences_WhenFlowArmsModifiedOnBithSides_DecisionToolHasNoConflict()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);
            var bb = new DsfFlowDecisionActivity();
            var jsonSerializer = new Dev2JsonSerializer();
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            bb.ExpressionText = jsonSerializer.Serialize(dev2DecisionStack);

            var assignId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",4),
                            new ActivityDTO("field3","field3",3),
                            new ActivityDTO("field3","field3",2),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity)),
                            Next = new FlowStep
                            {
                                Action = ActivityBuilderFactory.BuildActivity(typeof(DsfBaseConvertActivity))
                            }
                        },
                        Condition = bb
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",2),
                            new ActivityDTO("field2","fff",4),
                            new ActivityDTO("field3","field3",3),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity)),
                            Next = new FlowStep
                            {
                                Action = ActivityBuilderFactory.BuildActivity(typeof(DsfBaseConvertActivity))
                            }
                        },
                        Condition = bb
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(8, count);
            Assert.IsFalse(diffTree.First().IsInConflict);
            Assert.IsFalse(currentTree.First().IsInConflict);
            Assert.IsFalse(diffTree.Last().IsInConflict);
            Assert.IsFalse(currentTree.Last().IsInConflict);
            Assert.AreEqual(assignId, diffTree[0].UniqueId);
            Assert.AreEqual(assignId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(assignId, devActivityCurr.UniqueID);
            Assert.AreEqual(assignId, devActivityDiff.UniqueID);

            //Decision Node chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.IsTrue(devActivityDiff1.Equals(devActivityCurr1));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FlowDecision_GetDifferences_WhenMainDecisionModified_DecisionToolHasConflict()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);
            var deicisionId = Guid.NewGuid().ToString();
            var bb = new DsfFlowDecisionActivity { UniqueID = deicisionId };
            var jsonSerializer = new Dev2JsonSerializer();
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            bb.ExpressionText = jsonSerializer.Serialize(dev2DecisionStack);

            var bb2 = new DsfFlowDecisionActivity { UniqueID = deicisionId };
            var dev2DecisionStack2 = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "changed",
                FalseArmText = "false",
                TrueArmText = "true",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            bb2.ExpressionText = jsonSerializer.Serialize(dev2DecisionStack2);

            var assignId = Guid.NewGuid().ToString();
            var calcActivity = new DsfCalculateActivity { UniqueID = assignId };
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = calcActivity,
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep { Action = calcActivity },
                        Condition = bb
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = calcActivity,
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep { Action = calcActivity },
                        Condition = bb2
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(4, count);
            Assert.IsFalse(diffTree.First().IsInConflict);
            Assert.IsFalse(currentTree.First().IsInConflict);
            Assert.IsFalse(diffTree.Last().IsInConflict);
            Assert.IsFalse(currentTree.Last().IsInConflict);
            Assert.AreEqual(assignId, diffTree[0].UniqueId);
            Assert.AreEqual(assignId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(assignId, devActivityCurr.UniqueID);
            Assert.AreEqual(assignId, devActivityDiff.UniqueID);

            //Decision Node chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.IsFalse(devActivityDiff1.Equals(devActivityCurr1));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FlowDecision_GetDifferences_WhenArmToolsTheSame_DecisionHasNoConflict()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);
            var bb = new DsfFlowDecisionActivity();
            var jsonSerializer = new Dev2JsonSerializer();
            var toolId1 = Guid.NewGuid().ToString();
            var toolId2 = Guid.NewGuid().ToString();
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            bb.ExpressionText = jsonSerializer.Serialize(dev2DecisionStack);

            var assignId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",4),
                            new ActivityDTO("field3","field3",3),
                            new ActivityDTO("field3","field3",2),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity), toolId1),
                            Next = new FlowStep
                            {
                                Action = ActivityBuilderFactory.BuildActivity(typeof(DsfBaseConvertActivity), toolId2)
                            }
                        },
                        Condition = bb
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",2),
                            new ActivityDTO("field2","fff",4),
                            new ActivityDTO("field3","field3",3),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowDecision
                    {
                        DisplayName = "DisplayName",
                        True = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity), toolId1),
                            Next = new FlowStep
                            {
                                Action = ActivityBuilderFactory.BuildActivity(typeof(DsfBaseConvertActivity), toolId2)
                            }
                        },
                        Condition = bb
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(8, count);

            Assert.IsFalse(diffTree.Any(d => d.IsInConflict));
            Assert.IsFalse(currentTree.Any(d => d.IsInConflict));
            Assert.AreEqual(assignId, diffTree[0].UniqueId);
            Assert.AreEqual(assignId, currentTree[0].UniqueId);

            //First Node chart
            var tupleDifference = diffTree[0];
            var tupleCurrent = currentTree[0];
            Assert.IsNotNull(tupleDifference);
            Assert.IsNotNull(tupleCurrent);

            var devActivityDiff = tupleDifference.Activity;
            var devActivityCurr = tupleCurrent.Activity;
            Assert.IsNotNull(devActivityDiff);
            Assert.IsNotNull(devActivityCurr);
            Assert.AreEqual(assignId, devActivityCurr.UniqueID);
            Assert.AreEqual(assignId, devActivityDiff.UniqueID);

            //Decision Node chart
            var tupleDifference1 = diffTree[1];
            var tupleCurrent1 = currentTree[1];
            Assert.IsNotNull(tupleDifference1);
            Assert.IsNotNull(tupleCurrent1);

            var devActivityDiff1 = tupleDifference1.Activity;
            var devActivityCurr1 = tupleCurrent1.Activity;
            Assert.IsNotNull(devActivityDiff1);
            Assert.IsNotNull(devActivityCurr1);
            Assert.IsTrue(devActivityDiff1.Equals(devActivityCurr1));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FlowSwitch_GetDifferences_WhenCasesTheSame_SwitchHasNoConflict()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);
            var switchActivity = new DsfFlowSwitchActivity("MyName", new Mock<IDebugDispatcher>().Object, It.IsAny<bool>())
            {
                UniqueID = Guid.NewGuid().ToString(),
            };
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND,
            };
            var jsonSerializer = new Dev2JsonSerializer();
            switchActivity.ExpressionText = jsonSerializer.Serialize(dev2DecisionStack);
            var assignId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",4),
                            new ActivityDTO("field3","field3",3),
                            new ActivityDTO("field3","field3",2),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowSwitch<string>
                    {
                        DisplayName = "DisplayName",
                        Default = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity))
                        },
                        Expression = switchActivity
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new ActivityDTO("field1","field1",1),
                            new ActivityDTO("field2","field2",2),
                            new ActivityDTO("field2","fff",4),
                            new ActivityDTO("field3","field3",3),
                        },
                        UniqueID = assignId
                    },
                    Next = new FlowSwitch<string>
                    {
                        DisplayName = "DisplayName",
                        Default = new FlowStep
                        {
                            Action = ActivityBuilderFactory.BuildActivity(typeof(DsfCalculateActivity))
                        },
                        Expression = switchActivity
                    }
                }
            };

            var current = ParserTestHelper.CreateContextualResourceModel(otherChart);
            var diff = ParserTestHelper.CreateContextualResourceModel(chart);

            var psd = new ServiceDifferenceParser();
            var (currentTree, diffTree) = psd.GetDifferences(current, diff);

            var currConflicts = currentTree;
            var diffConflicts = diffTree;

            var count = currConflicts.Count + diffConflicts.Count;

            Assert.AreEqual(6, count);

            Assert.IsFalse(diffTree.First().IsInConflict);
            Assert.IsFalse(currentTree.First().IsInConflict);

            Assert.IsFalse(diffTree.Last().IsInConflict);
            Assert.IsFalse(currentTree.Last().IsInConflict);

            Assert.AreEqual(assignId, diffTree[0].UniqueId);
            Assert.AreEqual(assignId, currentTree[0].UniqueId);
        }
    }

    public static class ActivityBuilderFactory
    {
        public static System.Activities.Activity BuildActivity(Type actvityType, string uniqueId = "")
        {
            var objectInstance = Activator.CreateInstance(actvityType) as IDev2Activity;
            if (!string.IsNullOrEmpty(uniqueId))
            {
                objectInstance.UniqueID = uniqueId;
            }
            return objectInstance as System.Activities.Activity;
        }
    }
}
