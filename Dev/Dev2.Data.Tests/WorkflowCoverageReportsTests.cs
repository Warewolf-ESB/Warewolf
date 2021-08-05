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
            Assert.AreEqual(0, sut.TotalCoverage);
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
            Assert.AreEqual(0.67, sut.TotalCoverage);
        }
    }
}
