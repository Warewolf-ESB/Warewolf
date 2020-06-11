/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Dev2.Runtime.Interfaces;
using Warewolf.Data;
using Dev2.Runtime.ServiceModel.Data;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Serializers;
using Dev2.Data.SystemTemplates.Models;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    //TODO: Can this not be added here instead of added to every test? [DoNotParallelize]
    public class TestCoverageCatalogTests
    {
        public Mock<IResourceCatalog> GetMockResourceCatalog(IWarewolfWorkflow warewolfWorkflow)
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetWorkflow(_workflowId)).Returns(warewolfWorkflow);

            return mockResourceCatalog;
        }

        private Mock<IWarewolfWorkflow> GetMockWorkflowBuilder()
        {
            var workflowNodes = GetWorkflowNodes();

            var mockWorkflowBuilder = new Mock<IWarewolfWorkflow>();
            mockWorkflowBuilder.Setup(o => o.ResourceID).Returns(_workflowId);
            mockWorkflowBuilder.Setup(o => o.Name).Returns(_workflowName);
            mockWorkflowBuilder.Setup(o => o.WorkflowNodes).Returns(workflowNodes);
            return mockWorkflowBuilder;
        }

        private static readonly Guid _workflowId = Guid.Parse("99c23a82-aaf8-46a5-8746-4ff2d251daf2");
        private static readonly string _workflowName = "wf-send-cached-client-an-email";
        private static readonly ITestCoverageCatalog _testCoverageCatalog = TestCoverageCatalog.Instance;

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_ExpectPartialCoverageReport()
        {
            var mockResourceCatalog = GetMockResourceCatalog(GetMockWorkflowBuilder().Object);
            var test = GetFalseBranchTest();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);

            var coverage = sut.GenerateSingleTestCoverage(_workflowId, test);

            var report = sut.FetchReport(_workflowId, _falseBranchTest.TestName);

            Assert.AreEqual(.5, coverage.CoveragePercentage);

            Assert.AreEqual(_falseBranchTest.TestName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);
        }

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GenerateSingleTestCoverage_With_MockNodes_ExpectPartialCoverage()
        {
            var mockResourceCatalog = GetMockResourceCatalog(GetMockWorkflowBuilder().Object);
            var tests = GetTrueBranchTest();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);

            var coverage = sut.GenerateSingleTestCoverage(_workflowId, tests);

            Assert.AreEqual(.33, Math.Round(coverage.CoveragePercentage, 2));
        }

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_When_FetchReport_ExpectFullCoverageReport()
        {
            var mockResourceCatalog = GetMockResourceCatalog(GetMockWorkflowBuilder().Object);
            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);
            var coverage = sut.GenerateAllTestsCoverage(_workflowName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _workflowName);

            Assert.AreEqual(.5, coverage.CoveragePercentage);

            Assert.AreEqual(_workflowName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);
        }

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_Using_FlowSwitch_HaveMockedNode_When_FetchReport_ExpectCoverageWitoutMockedNode()
        {
            var workflowMock = GetMockResourceCatalog(GetRealWorkflowBuilder());
            var tests = GetRealWorkflowTests();

            var sut = new TestCoverageCatalog(workflowMock.Object);
            var coverage = sut.GenerateAllTestsCoverage(_workflowName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _workflowName);

            Assert.AreEqual(.125, report.CoveragePercentage);

            Assert.AreEqual(_workflowName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);
        }

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_When_DeleteCoverageReport_ExpectFullCoverageRemoved()
        {
            var mockResourceCatalog = GetMockResourceCatalog(GetMockWorkflowBuilder().Object);
            //Arrange
            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);
            var coverage = sut.GenerateAllTestsCoverage(_workflowName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _workflowName);

            Assert.AreEqual(.5, coverage.CoveragePercentage);

            Assert.AreEqual(_workflowName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);

            //Act
            sut.DeleteCoverageReport(_workflowId, _falseBranchTest.TestName);

            //Assert
            var afterDeleteReport = sut.FetchReport(_workflowId, _falseBranchTest.TestName);
            Assert.IsNull(afterDeleteReport);
        }

        [TestMethod]
        [DoNotParallelize]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenTestCoverage_When_ReloadAllReports_ExpectFullCoverageRemoved()
        {
            var mockResourceCatalog = GetMockResourceCatalog(GetMockWorkflowBuilder().Object);

            //Arrange
            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);
            Assert.AreEqual(0, sut.TestCoverageReports.Count);

            _ = sut.GenerateSingleTestCoverage(_workflowId, _falseBranchTest);
            //Act
            sut.ReloadAllReports();

            //Assert
            Assert.IsTrue(sut.TestCoverageReports.Count > 0);
        }


        private IWarewolfWorkflow GetRealWorkflowBuilder()
        {
            var jsonSerializer = new Dev2JsonSerializer();
            var dev2DecisionStackParent = GetDecisionStackParent();
            var dev2DecisionStackChild = GetDecisionChild();

            var switchId = Guid.Parse("0d723deb-402d-43ec-bdbc-97c645a3a8f1");
            var flowSwitch = new FlowSwitch<string>()
            {
                Expression = new DsfFlowSwitchActivity
                {
                    ActivityId = switchId,
                    UniqueID = switchId.ToString(),
                    DisplayName = "Switch (dsf)",
                }
            };
            flowSwitch.Cases
                .Add("Case1", new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        ActivityId = Guid.Parse("f8aec437-38c3-47c8-be1c-e5f408efa3bc"),
                        UniqueID = Guid.Parse("f8aec437-38c3-47c8-be1c-e5f408efa3bc").ToString(),
                        DisplayName = "Assign (case 1)"
                    }
                });
            flowSwitch.Cases
                .Add("Case2", new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        ActivityId = Guid.Parse("a8c186a2-bb5e-4382-84a6-699622034e8d"),
                        UniqueID = Guid.Parse("a8c186a2-bb5e-4382-84a6-699622034e8d").ToString(),
                        DisplayName = "Assign (case 2)"
                    }
                });
            flowSwitch.Cases
                .Add("Case3", new FlowDecision()
                {
                    Condition = new DsfFlowDecisionActivity
                    {
                        ExpressionText = jsonSerializer.Serialize(dev2DecisionStackParent)
                    },
                    DisplayName = "Decision (parent)",
                    True = new FlowStep
                    {
                        Action = new DsfMultiAssignActivity
                        {
                            DisplayName = "Assign (success)"
                        }
                    },
                    False = new FlowDecision()
                    {
                        Condition = new DsfFlowDecisionActivity
                        {
                            ExpressionText = jsonSerializer.Serialize(dev2DecisionStackChild)
                        },
                        DisplayName = "Decision (child)",
                        True = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-True Arm"
                            }
                        },
                        False = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-False Arm"
                            }
                        }
                    }
                });
            flowSwitch.Default = new FlowStep();

            var flowNodes = new Collection<FlowNode>
            {
                flowSwitch
            };

            return new Workflow(flowNodes);
        }

        private static Dev2DecisionStack GetDecisionChild()
        {
            return new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("aChild")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("aChild")
                        }
                    }
                },
                DisplayText = "aChild",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
        }

        private static Dev2DecisionStack GetDecisionStackParent()
        {
            return new Dev2DecisionStack
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
        }

        private readonly IServiceTestModelTO _flowSwitch_Case1_Test = new ServiceTestModelTO
        {
            ResourceId = _workflowId,
            TestName = "Flow Switch Case 1 test",
            Password = "p@ssw0rd",
            TestSteps = new List<IServiceTestStep>
            {
                new ServiceTestStepTO
                {
                    ActivityID = Guid.Parse("0d723deb-402d-43ec-bdbc-97c645a3a8f1"),
                    UniqueID = Guid.Parse("0d723deb-402d-43ec-bdbc-97c645a3a8f1"),
                    StepDescription = "Switch (dsf)",
                    Type = StepType.Assert
                },
                new ServiceTestStepTO
                {
                    ActivityID = Guid.Parse("f8aec437-38c3-47c8-be1c-e5f408efa3bc"),
                    UniqueID = Guid.Parse("f8aec437-38c3-47c8-be1c-e5f408efa3bc"),
                    StepDescription = "Assign (case 1)",
                    Type = StepType.Mock
                }
            }
        };

        private List<IServiceTestModelTO> GetRealWorkflowTests()
        {
            var tests = new List<IServiceTestModelTO>
            {
                _flowSwitch_Case1_Test
            };

            return tests;
        }

        private IServiceTestModelTO GetFalseBranchTest()
        {
            return _falseBranchTest;
        }

        private IServiceTestModelTO GetTrueBranchTest()
        {
            return _trueBranchTest;
        }

        public List<IWorkflowNode> GetWorkflowNodes()
        {
            return new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                    UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                    StepDescription = "Assign(input)",
                    MockSelected = false
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                    UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                    StepDescription = "If [[data_ttl_expired]] Is = false",
                    MockSelected = false
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                    UniqueID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                    StepDescription = "Assign(error)",
                    MockSelected = false
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                    UniqueID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                    StepDescription = "SQL(get person)",
                    MockSelected = true
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                    UniqueID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                    StepDescription = "Assign(sql person)",
                    MockSelected = true
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                    UniqueID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                    StepDescription = "Email(sql person)",
                    MockSelected = true
                }
            };
        }


        private readonly IServiceTestModelTO _falseBranchTest = new ServiceTestModelTO
        {
            ResourceId = _workflowId,
            TestName = "False branch test",
            Password = "p@ssw0rd",

            TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        StepDescription = "Assign(input)",
                        Type = StepType.Assert
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = false",
                        Type = StepType.Assert
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        UniqueID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        StepDescription = "Assign(error)",
                        Type = StepType.Assert
                    }
                },
        };

        private readonly IServiceTestModelTO _trueBranchTest = new ServiceTestModelTO
        {
            ResourceId = _workflowId,
            TestName = "True branch test",
            Password = "p@ssw0rd",
            TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        StepDescription = "Assign(input)",
                        Type = StepType.Assert
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = true",
                        Type = StepType.Assert
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        UniqueID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        StepDescription = "SQL(get person)",
                        Type = StepType.Mock
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                        UniqueID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                        StepDescription = "Assign(sql person)",
                        MockSelected = true
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                        UniqueID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                        StepDescription = "Email(sql person)",
                        MockSelected = true
                    }
                },
        };

        private List<IServiceTestModelTO> GetTests()
        {
            var tests = new List<IServiceTestModelTO>
            {
                _falseBranchTest,
                _trueBranchTest
            };

            return tests;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testCoverageCatalog.DeleteAllCoverageReports(_workflowId);
        }
    }
}
