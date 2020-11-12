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
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
using System.IO;
using System.Linq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class TestCoverageCatalogTests
    {
        private static readonly Guid _workflowId = Guid.Parse("99c23a82-aaf8-46a5-8746-4ff2d251daf2");
        private static readonly string _workflowName = "wf-send-cached-client-an-email";
        private static readonly ITestCoverageCatalog _testCoverageCatalog = TestCoverageCatalog.Instance;

        private static readonly string _testCoveragePath = EnvironmentVariables.TestCoveragePath;
        private static readonly string _reportPath = _testCoveragePath + "\\" + _workflowId.ToString();
        private static readonly string _oldReportPath = _reportPath + "\\old_report_name.coverage";
        private static readonly string _newReportPath = _reportPath + "\\False branch test.coverage";


        private readonly ServiceTestCoverageModelTo _testCoverageModelTo = new ServiceTestCoverageModelTo
        {
            OldReportName = "old_report_name",
            ReportName = "False branch test"
        };

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateSingleTestCoverage_Executed_ExpectCoverageReport_On_FetchReport()
        {

            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            var test = GetFalseBranchTest();

            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, new Mock<IFile>().Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            var result = sut.GenerateSingleTestCoverage(_workflowId, test);

            var report = sut.FetchReport(_workflowId, "False branch test");

            Assert.AreEqual(.0, result.TotalCoverage);
            Assert.AreEqual("False branch test", report.ReportName);
            Assert.AreEqual(result.TotalCoverage, report.TotalCoverage);

            mockDirectory.Verify(o => o.CreateIfNotExists(_reportPath), Times.Once);
            mockStreamWriterFactory.Verify(o => o.New(_newReportPath, false), Times.Exactly(2));
            mockServiceTestCoverageModelToFactory.Verify(o => o.New(_workflowId, It.IsAny<ICoverageArgs>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
            mockFilePath.Verify(o => o.Combine(_testCoveragePath, _workflowId.ToString()), Times.Once);
            mockFilePath.Verify(o => o.Combine(_reportPath, "old_report_name.coverage"), Times.Once);
            mockFilePath.Verify(o => o.Combine(_reportPath, "False branch test.coverage"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateSingleTestCoverage_Executed_OnExistingReport_ExpectCoverageReport_On_FetchReport()
        {

            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            var test = GetFalseBranchTest();

            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, new Mock<IFile>().Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);
            
            sut.TestCoverageReports.GetOrAdd(_workflowId, new List<IServiceTestCoverageModelTo> { _testCoverageModelTo });

            var result = sut.GenerateSingleTestCoverage(_workflowId, test);

            var report = sut.FetchReport(_workflowId, "False branch test");

            Assert.AreEqual(.0, result.TotalCoverage);
            Assert.AreEqual("False branch test", report.ReportName);
            Assert.AreEqual(result.TotalCoverage, report.TotalCoverage);

            mockDirectory.Verify(o => o.CreateIfNotExists(_reportPath), Times.Once);
            mockStreamWriterFactory.Verify(o => o.New(_newReportPath, false), Times.Exactly(2));
            mockServiceTestCoverageModelToFactory.Verify(o => o.New(_workflowId, It.IsAny<ICoverageArgs>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
            mockFilePath.Verify(o => o.Combine(_testCoveragePath, _workflowId.ToString()), Times.Once);
            mockFilePath.Verify(o => o.Combine(_reportPath, "old_report_name.coverage"), Times.Once);
            mockFilePath.Verify(o => o.Combine(_reportPath, "False branch test.coverage"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateAllTestsCoverage_Executed_ExpectCoverageReport_On_FetchReport()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, new Mock<IFile>().Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            var coverage = sut.GenerateAllTestsCoverage("False branch test", _workflowId, tests);

            var report = sut.FetchReport(_workflowId, "False branch test");

            Assert.AreEqual(.0, coverage.TotalCoverage);

            Assert.AreEqual("False branch test", report.ReportName);
            Assert.AreEqual(coverage.TotalCoverage, report.TotalCoverage);

            mockStreamWriterFactory.Verify(o => o.New(_newReportPath, false), Times.Exactly(1));
            mockServiceTestCoverageModelToFactory.Verify(o => o.New(_workflowId, It.IsAny<ICoverageArgs>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateAllTestsCoverage_Executed_When_DeleteCoverageReport_ExpectCoverageReportRemoved()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            var mockFileWrapper = new Mock<IFile>();
            mockFileWrapper.Setup(o => o.Exists(_newReportPath)).Returns(true);

            //Arrange
            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, mockFileWrapper.Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            var coverage = sut.GenerateAllTestsCoverage(_testCoverageModelTo.ReportName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _testCoverageModelTo.ReportName);

            Assert.AreEqual(.0, coverage.TotalCoverage);

            Assert.AreEqual(_testCoverageModelTo.ReportName, report.ReportName);
            Assert.AreEqual(coverage.TotalCoverage, report.TotalCoverage);

            //Act
            sut.DeleteCoverageReport(_workflowId, _falseBranchTest.TestName);

            //Assert
            var afterDeleteReport = sut.FetchReport(_workflowId, _falseBranchTest.TestName);
            Assert.IsNull(afterDeleteReport);

            mockFileWrapper.Verify(o => o.Delete(_newReportPath), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateAllTestsCoverage_Executed_When_DeleteAllCoverageReports_ExpectCoverageReportRemoved()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            var mockFileWrapper = new Mock<IFile>();
            mockFileWrapper.Setup(o => o.Exists(_newReportPath)).Returns(true);

            //Arrange
            var tests = GetTests();

            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, mockFileWrapper.Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            var coverage = sut.GenerateAllTestsCoverage(_testCoverageModelTo.ReportName, _workflowId, tests);

            var report = sut.FetchReport(_workflowId, _testCoverageModelTo.ReportName);

            Assert.AreEqual(.0, coverage.TotalCoverage);

            Assert.AreEqual(_testCoverageModelTo.ReportName, report.ReportName);
            Assert.AreEqual(coverage.TotalCoverage, report.TotalCoverage);

            //Act
            sut.DeleteAllCoverageReports(_workflowId);

            //Assert
            var afterDeleteReport = sut.FetchReport(_workflowId, _falseBranchTest.TestName);
            Assert.IsNull(afterDeleteReport);

            mockDirectory.Verify(o => o.Delete(_reportPath, true), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenTestCoverage_When_ReloadAllReports_ExpectCoverageRemoved()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            //Arrange
            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, new Mock<IFile>().Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            Assert.AreEqual(0, sut.TestCoverageReports.Count);

            _ = sut.GenerateSingleTestCoverage(_workflowId, _falseBranchTest);
            //Act
            sut.ReloadAllReports();

            //Assert
            Assert.IsTrue(sut.TestCoverageReports.Count > 0);

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_GivenTestCoverage_When_ReloadAllReports_GetReportList_ExpectCoverageRemoved()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);
            
            var derivedTestPath = _newReportPath.Replace(EnvironmentVariables.TestCoveragePath, EnvironmentVariables.TestPath).Replace(".coverage", ".test");
            var mockFile = new Mock<IFile>();
            mockFile.Setup(o => o.Exists(derivedTestPath)).Returns(true);

            var mockStreamReaderFactory = new Mock<IStreamReaderFactory>();
            //Arrange
            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, mockFile.Object, mockDirectory.Object, mockStreamWriterFactory.Object, mockStreamReaderFactory.Object, mockSerialize.Object);

            Assert.AreEqual(0, sut.TestCoverageReports.Count);

            _ = sut.GenerateSingleTestCoverage(_workflowId, _falseBranchTest);
            //Act
            sut.ReloadAllReports();

            //Assert
            var results = sut.TestCoverageReports;
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual(_workflowId, results.First().Key);

            mockStreamReaderFactory.Verify(o => o.New(_newReportPath), Times.Once);
            mockSerialize.Verify(o => o.Deserialize<ServiceTestCoverageModelTo>(It.IsAny<StreamReader>()), Times.Once);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestCoverageCatalog))]
        public void TestCoverageCatalog_Given_GenerateSingleTestCoverage_Executed_When_Fetch_ExpectCoverageReport()
        {
            SetupMocks(_testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize);

            //Arrange
            var sut = new TestCoverageCatalog(mockServiceTestCoverageModelToFactory.Object, mockFilePath.Object, new Mock<IFile>().Object, mockDirectory.Object, mockStreamWriterFactory.Object, new Mock<IStreamReaderFactory>().Object, mockSerialize.Object);

            Assert.AreEqual(0, sut.TestCoverageReports.Count);

            _ = sut.GenerateSingleTestCoverage(_workflowId, _falseBranchTest);
            //Act
            var result = sut.Fetch(_workflowId);

            //Assert
            Assert.IsTrue(result.Count > 0);

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


        private void SetupMocks(ServiceTestCoverageModelTo testCoverageModelTo, out Mock<IDirectory> mockDirectory, out Mock<IStreamWriterFactory> mockStreamWriterFactory, out Mock<IServiceTestCoverageModelToFactory> mockServiceTestCoverageModelToFactory, out Mock<IFilePath> mockFilePath, out Mock<ISerializer> mockSerialize)
        {
            mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(It.IsAny<string>())).Returns(_newReportPath);
            mockDirectory.Setup(o => o.GetDirectories(_testCoveragePath)).Returns(new string[] { _reportPath });
            mockDirectory.Setup(o => o.GetDirectoryName(_reportPath)).Returns(_workflowId.ToString());
            mockDirectory.Setup(o => o.GetFiles(_reportPath)).Returns(new string[] { _newReportPath });
            mockDirectory.Setup(o => o.Exists(_reportPath)).Returns(true);

            mockStreamWriterFactory = new Mock<IStreamWriterFactory>();
            mockServiceTestCoverageModelToFactory = new Mock<IServiceTestCoverageModelToFactory>();
            mockServiceTestCoverageModelToFactory.Setup(o => o.New(_workflowId, It.IsAny<ICoverageArgs>(), It.IsAny<List<IServiceTestModelTO>>()))
            .Returns(testCoverageModelTo);

            mockFilePath = new Mock<IFilePath>();
            mockFilePath.Setup(o => o.Combine(_testCoveragePath, _workflowId.ToString())).Returns(_reportPath);
            mockFilePath.Setup(o => o.Combine(_testCoveragePath, testCoverageModelTo.OldReportName+ ".coverage")).Returns(_oldReportPath);
            mockFilePath.Setup(o => o.Combine(_testCoveragePath, testCoverageModelTo.ReportName+ ".coverage")).Returns(_newReportPath);

            mockSerialize = new Mock<ISerializer>();
            var streamWriterFactory = mockStreamWriterFactory.Object;
            mockSerialize.Setup(o => o.Serialize(streamWriterFactory.New(_newReportPath, false), testCoverageModelTo));
            mockSerialize.Setup(o => o.Deserialize<ServiceTestCoverageModelTo>(It.IsAny<StreamReader>())).Returns(_testCoverageModelTo);
        }
    }
}
