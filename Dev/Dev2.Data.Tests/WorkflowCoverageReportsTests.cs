/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Warewolf.Data;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class WorkflowCoverageReportsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_TestNodesCovered_TestStepType_Is_Assert()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "old name",
                ReportName = "new name",
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert
                    }
                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(1, sut.TotalCoverage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_TestNodesCovered_ShouldFailSafe()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                //Should the undesirable happen
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "old name",
                ReportName = "new name",
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert
                    }
                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(-1, sut.NotCoveredNodesCount, "We might need a deferent approach for handling this matter, with the previous version the one return for safety add on to the count, thus inaccurate count yields");
            Assert.AreEqual(0, sut.TotalCoverage);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_TestNodesCovered_TestStepType_Is_Assert_AND_ActivityIDIsEmpty()
        {
            var testUniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e");
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Empty,
                    UniqueID = testUniqueID, //most of our activities are still using the ActivityID,
                                             //tools like the Gate and newer have started moving towards UniqueID
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "old name",
                ReportName = "new name",
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert
                    }
                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(1, sut.TotalCoverage);
        }
        
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_TestNodesCovered_TestStepType_Is_Mock()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "old name",
                ReportName = "new name",
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Mock
                    }
                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(1, sut.TotalCoverage, "design change: mocked nodes should now included with the test coverage calculation");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_ChildNodes_ShouldInheritParentCoverageStatus()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    ChildNodes = new List<IWorkflowNode>
                    {
                        new WorkflowNode
                        {
                            ActivityID = Guid.Parse("37f61bbe-c77b-4066-be7e-e91706382e82"),
                            UniqueID = Guid.Parse("37f61bbe-c77b-4066-be7e-e91706382e82"),
                            MockSelected = false
                        }
                    }
                },
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("fc647f71-9879-4823-8b2c-04b63e395ba2"),
                    UniqueID = Guid.Parse("fc647f71-9879-4823-8b2c-04b63e395ba2"),
                    ChildNodes = new List<IWorkflowNode>
                    {
                        new WorkflowNode
                        {
                            ActivityID = Guid.Parse("7615414c-8f33-4175-96a7-3f961918c4d4"),
                            UniqueID = Guid.Parse("7615414c-8f33-4175-96a7-3f961918c4d4"),
                            MockSelected = false
                        }
                    }
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "old name",
                ReportName = "new name",
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Mock,
                        Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                        {
                           new ServiceTestStepTO
                           {
                                ActivityID = Guid.Parse("37f61bbe-c77b-4066-be7e-e91706382e82"),
                                UniqueID = Guid.Parse("37f61bbe-c77b-4066-be7e-e91706382e82"),
                                MockSelected = false
                           }
                        }
                    },

                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(.5, sut.TotalCoverage, "design change: child nodes should now included with the test coverage calculation");
        }



        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_GetTotalCoverage_Given_()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                },
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                    UniqueID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                },
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                    UniqueID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            //passing test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "passing test old name",
                ReportName = "passing test new name",
                TotalCoverage = .25,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestPassed
                        }
                    }
                })}
            });

            //add invalid test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "invalid test old name",
                ReportName = "invalid test new name",
                TotalCoverage = 0,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        //this makes for invalid test
                    },
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                        UniqueID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                        Type = StepType.Mock,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestInvalid
                        }
                    }
                })}
            });

            //failing test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "failing test old name",
                ReportName = "failing test new name",
                TotalCoverage = 0.1,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                        UniqueID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                        Type = StepType.Assert,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestFailed
                        }
                    }
                })}
            });

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(1, sut.TotalCoverage, "design change: mocked nodes should now included with the test coverage calculation");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_TryExecute_GetTotalCoverage_Given_SomethingFails_ShouldContinue()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Throws(new Exception("false exception: this should be logged into serverlog"));
            
            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            var result = sut.TryExecute();
            
            //prints to serverlog on fail
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowCoverageReports))]
        public void WorkflowCoverageReports_TryExecute_GetTotalCoverage_Given_ShouldSuccess()
        {
            var sdf = Guid.NewGuid();
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                    UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                },
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                    UniqueID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                },
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                    UniqueID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                }
            });

            var sut = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);

            //passing test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "passing test old name",
                ReportName = "passing test new name",
                TotalCoverage = .25,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestPassed
                        }
                    }
                })}
            });

            //add invalid test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "invalid test old name",
                ReportName = "invalid test new name",
                TotalCoverage = 0,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        //this makes for invalid test
                    },
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                        UniqueID = Guid.Parse("981ff0e1-5604-44ac-ad63-d82eff392882"),
                        Type = StepType.Mock,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestInvalid
                        }
                    }
                })}
            });

            //failing test
            sut.Add(new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "failing test old name",
                ReportName = "failing test new name",
                TotalCoverage = 0.1,
                AllTestNodesCovered = new ISingleTestNodesCovered[] { new SingleTestNodesCovered("Test", new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                        UniqueID = Guid.Parse("58df5745-c1c7-43fa-bb12-30c890dd7223"),
                        Type = StepType.Assert,
                        Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestFailed
                        }
                    }
                })}
            });

            var result = sut.TryExecute();

            Assert.IsNotNull(result.Resource);
            Assert.IsTrue(result.HasTestReports);
            Assert.AreEqual(1, result.TotalCoverage, "design change: mocked nodes should now included with the test coverage calculation");
        }
    }
}
