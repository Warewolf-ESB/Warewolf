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
                AllTestNodesCovered = new ISingleTestNodesCovered [] { new SingleTestNodesCovered("Test", new List<IServiceTestStep> 
                { 
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        UniqueID = Guid.Parse("7ed4ab9c-d227-409a-acc3-18330fe6b84e"),
                        Type = StepType.Assert
                    }
                })}
            });

            (double TotalCoverage, _, _) = sut.GetTotalCoverage();

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(1, TotalCoverage);
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

            (double TotalCoverage, _, _) = sut.GetTotalCoverage();

            Assert.IsNotNull(sut.Resource);
            Assert.IsTrue(sut.HasTestReports);
            Assert.AreEqual(0, TotalCoverage);
        }
    }
}
