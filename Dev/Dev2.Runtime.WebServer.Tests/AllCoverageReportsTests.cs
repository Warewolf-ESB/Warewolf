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
using Dev2.Common.Interfaces.Runtime.WebServer;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class AllCoverageReportsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_CTOR_Empty_Defaults()
        {
            var sut = new AllCoverageReports();

            Assert.IsNotNull(sut.StartTime, "should stamp start time");
            Assert.IsNull(sut.EndTime);
            Assert.AreEqual(0, sut.TotalReportsCoverage);
            Assert.AreEqual(0, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Given_WithTestReports_IsNull_Default_ShouldNotReturnNaN()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();

            var sut = new AllCoverageReports
            {
                EndTime = DateTime.Now
            };

            sut.Add(new WorkflowCoverageReports(mockWarewolfWorkflow.Object)
            {
                //should something unfavorable ever happen, zero make more sense then NaN in this context
            });

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            Assert.AreEqual(0, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(0, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Given_WithTestReports_IsNotNull_AND_HasOneReports_ShouldSuccess()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();

            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
                .Returns(_workflow_Three_Nodes);

            var sut = new AllCoverageReports
            {
                EndTime = DateTime.Now
            };

            var coverageReports = new WorkflowCoverageReports(mockWarewolfWorkflow.Object); 
            coverageReports.Add(_test_One_CoverageModelTo);
            sut.Add(coverageReports);

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            Assert.AreEqual(33, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(1, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Given_WithTestReports_IsNotNull_AND_HasALotOfReports_ShouldSuccess()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();

            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
               .Returns(_workflow_Three_Nodes);

            var sut = new AllCoverageReports
            {
                EndTime = DateTime.Now
            };

            var coverageReports = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);
            coverageReports.Add(_test_One_CoverageModelTo);
            coverageReports.Add(_test_One_CoverageModelTo);
            sut.Add(coverageReports);

            var coverageReports_1 = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);
            coverageReports_1.Add(_test_One_CoverageModelTo);
            coverageReports_1.Add(_test_One_CoverageModelTo);
            sut.Add(coverageReports_1);

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            var sss = sut.TotalReportsCoverage;
            Assert.AreEqual(33, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(2, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Execute_Given_WithTestReports_IsNotNull_AND_HasALotOfReports_ShouldSuccess()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();

            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes)
               .Returns(_workflow_Three_Nodes);

            var sut = new AllCoverageReports
            {
                EndTime = DateTime.Now
            };

            var coverageReports = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);
            coverageReports.Add(_test_One_CoverageModelTo);
            coverageReports.Add(_test_One_CoverageModelTo);
            sut.Add(coverageReports);

            var coverageReports_1 = new WorkflowCoverageReports(mockWarewolfWorkflow.Object);
            coverageReports_1.Add(_test_One_CoverageModelTo);
            coverageReports_1.Add(_test_One_CoverageModelTo);
            sut.Add(coverageReports_1);

            var warewolfCoverageReportsTO = sut.Calcute();

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            Assert.AreEqual(33, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(2, sut.WithTestReports.ToList().Count, "should be initialized");
            Assert.IsInstanceOfType(warewolfCoverageReportsTO, typeof(IEnumerable<IWorkflowCoverageReportsTO>));
        }

        private List<IWorkflowNode> _workflow_Three_Nodes =>
            new List<IWorkflowNode>
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
            };

        private ServiceTestCoverageModelTo _test_One_CoverageModelTo =>
            new ServiceTestCoverageModelTo
            {
                WorkflowId = Guid.NewGuid(),
                LastRunDate = DateTime.Now,
                OldReportName = "passing test old name",
                ReportName = "passing test new name",
                TotalCoverage = .25,
                AllTestNodesCovered = new ISingleTestNodesCovered[]
                {
                    new SingleTestNodesCovered("Test", new List<IServiceTestStep>
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
                    })
                }
            };
    }

}
