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

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class TestCoverageCatalogTests
    {
        public Mock<IResourceCatalog> GetMockResourceCatalog()
        {
            var workflowNodes = GetWorkflowNodes();

            var mockWorkflowBuilder = new Mock<IWarewolfWorkflow>();
            mockWorkflowBuilder.Setup(o => o.ResourceID).Returns(_workflowId);
            mockWorkflowBuilder.Setup(o => o.Name).Returns(_workflowName);
            mockWorkflowBuilder.Setup(o => o.WorkflowNodesForHtml).Returns(workflowNodes);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetWorkflow(_workflowId)).Returns(mockWorkflowBuilder.Object);

            return mockResourceCatalog;
        }

        private static readonly Guid _workflowId = Guid.Parse("99c23a82-aaf8-46a5-8746-4ff2d251daf2");
        private static readonly string _workflowName = "wf-send-cached-client-an-email";
        private static readonly ITestCoverageCatalog _testCoverageCatalog = TestCoverageCatalog.Instance;

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_ExpectPartialCoverageReport()
        {
            var mockResourceCatalog = GetMockResourceCatalog();
            var test = GetFalseBranchTest();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);

            var coverage = sut.GenerateSingleTestCoverage(_workflowId, test);

            var report = sut.FetchReport(_workflowId, _falseBranchTest.TestName);

            Assert.AreEqual(.5, coverage.CoveragePercentage);

            Assert.AreEqual(_falseBranchTest.TestName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GenerateSingleTestCoverage_With_MockNodes_ExpectPartialCoverage()
        {
            var mockResourceCatalog = GetMockResourceCatalog();
            var tests = GetTrueBranchTest();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);

            var coverage = sut.GenerateSingleTestCoverage(_workflowId, tests);

            Assert.AreEqual(.33, Math.Round(coverage.CoveragePercentage, 2));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_When_FetchReport_ExpectFullCoverageReport()
        {
            var mockResourceCatalog = GetMockResourceCatalog();
            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);
            var coverage = sut.GenerateAllTestsCoverage(_workflowName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _workflowName);

            Assert.AreEqual(.5, coverage.CoveragePercentage);

            Assert.AreEqual(_workflowName, report.ReportName);
            Assert.AreEqual(coverage.CoveragePercentage, report.CoveragePercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenGenerateAllTestsCoverageExecuted_When_DeleteCoverageReport_ExpectFullCoverageRemoved()
        {
            var mockResourceCatalog = GetMockResourceCatalog();
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenTestCoverage_When_ReloadAllReports_ExpectFullCoverageRemoved()
        {
            var mockResourceCatalog = GetMockResourceCatalog();

            //Arrange
            var sut = new TestCoverageCatalog(mockResourceCatalog.Object);
            Assert.AreEqual(0, sut.TestCoverageReports.Count);

            _ = sut.GenerateSingleTestCoverage(_workflowId, _falseBranchTest);
            //Act
            sut.ReloadAllReports();

            //Assert
            Assert.IsTrue(sut.TestCoverageReports.Count > 0);
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
                        MockSelected = false
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = false",
                        MockSelected = false
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        UniqueID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        StepDescription = "Assign(error)",
                        MockSelected = false
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
                        MockSelected = false
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = true",
                        MockSelected = false
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        UniqueID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        StepDescription = "SQL(get person)",
                        MockSelected = true
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
