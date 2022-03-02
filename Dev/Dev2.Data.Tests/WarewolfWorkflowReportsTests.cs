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
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class WarewolfWorkflowReportsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Add_GIVEN_Null_ShouldNotAddNull()
        {
            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { null } , null);

            Assert.AreEqual(0, sut.TotalWorkflowNodesCount);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Add_GIVEN_Null_ShouldNotAddNull1()
        {
            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { null }, null);

            Assert.AreEqual(0, sut.TotalWorkflowNodesCoveredPercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Add_GIVEN_NotNull_ShouldAddWorkflow()
        {
            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { new Workflow() }, default);
            
            Assert.AreEqual(1, sut.Workflows.Count());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_TotalWorkflowsNodesCount_ShouldSucces()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                });

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(Guid.Empty))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(string.Empty, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = Guid.NewGuid(),
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = Guid.NewGuid()
                                       }
                                   }
                               }
                            })
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, default);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(3, sut.TotalWorkflowNodesCount);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_ResourceNotFound_ShouldNotReturnNaN()
        {
            var workflowID = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>());

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(Guid.Empty))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, string.Empty);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(0, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(0, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(0, sut.TotalWorkflowNodesCoveredPercentage, "NaN my not be Ideal to print to user, should rather returning zero");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_ResourceNotFound_ShouldReturnReport()
        {
            var workflowID = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                });

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(Guid.Empty))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(string.Empty, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = Guid.NewGuid(),
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = Guid.NewGuid()
                                       }
                                   }
                               }
                            })
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, string.Empty);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(3, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(0, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(0, sut.TotalWorkflowNodesCoveredPercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_RecordNameNotFound_ShouldReturnReport()
        {
            var workflowID = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        ActivityID = Guid.NewGuid()
                    },
                });

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(workflowID))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(string.Empty, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = Guid.NewGuid(),
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = Guid.NewGuid()
                                       }
                                   }
                               }
                            })
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, string.Empty);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(3, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(2, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(67,67, sut.TotalWorkflowNodesCoveredPercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_RecordNameFound_ShouldReturnReport()
        {
            var workflowID = Guid.NewGuid();
            var testName = "Test 1";
            var reportName = "First_Report";
            var coveredNode_one = Guid.NewGuid();
            var coveredNode_two = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        UniqueID = coveredNode_one
                    },
                    new WorkflowNode
                    {
                        UniqueID = coveredNode_two
                    },
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid()
                    },
                });
            
            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(workflowID))
                .Returns(new List<IServiceTestCoverageModelTo> { 
                    new ServiceTestCoverageModelTo
                    {
                        ReportName = reportName,
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(testName, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = coveredNode_one,
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = coveredNode_two,
                                           UniqueID = coveredNode_two
                                       }
                                   }
                               }
                            })
                        }
                    } 
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, reportName);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(3, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(2, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(67,67, sut.TotalWorkflowNodesCoveredPercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_RecordNameIsAsterick_ShouldReturnReport()
        {
            var workflowID = Guid.NewGuid();
            var testName = "Test 1";
            var reportName = "*";
            var coveredNode_one = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid() 
                    },
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid()
                    },
                });

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(workflowID))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        ReportName = reportName,
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(testName, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = coveredNode_one,
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO()
                                   }
                               }
                            })
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, reportName);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(3, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(2, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(67,67, sut.TotalWorkflowNodesCoveredPercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWorkflowReports))]
        public void WarewolfWorkflowReports_Calculte_TotalWorkflowNodesCoveredPercentage_GIVEN_RecordNameIsAsterick_ShouldReturnReport100()
        {
            var workflowID = Guid.NewGuid();
            var testName = "Test 1";
            var reportName = "*";
            var coveredNode_one = Guid.NewGuid();

            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID)
                .Returns(workflowID);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(new List<IWorkflowNode>
                {
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid()
                    },
                    new WorkflowNode
                    {
                        UniqueID = Guid.NewGuid()
                    }
                });

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(workflowID))
                .Returns(new List<IServiceTestCoverageModelTo> {
                    new ServiceTestCoverageModelTo
                    {
                        ReportName = reportName,
                        AllTestNodesCovered = new ISingleTestNodesCovered[]
                        {
                            new SingleTestNodesCovered(testName, new List<IServiceTestStep>
                            {
                               new ServiceTestStepTO
                               {
                                   ActivityID = coveredNode_one,
                                   Children = new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>
                                   {
                                       new ServiceTestStepTO()
                                   }
                               }
                            })
                        }
                    }
                });

            var sut = new WarewolfWorkflowReports(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object }, reportName);
            sut.Calculte(mockTestCoverageCatalog.Object, new Mock<ITestCatalog>().Object);

            Assert.AreEqual(2, sut.TotalWorkflowNodesCount);
            Assert.AreEqual(2, sut.TotalWorkflowNodesCoveredCount);
            Assert.AreEqual(100, sut.TotalWorkflowNodesCoveredPercentage);
        }

    }
}
