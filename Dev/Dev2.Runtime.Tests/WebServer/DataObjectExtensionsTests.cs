/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Principal;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.Data;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using static Dev2.Runtime.WebServer.DataObjectExtensions;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class DataObjectExtensionsTests
    {
        private readonly static Guid _workflowOne = Guid.Parse("fbda8700-2717-4879-88cd-6abdea4560da");
        private readonly static Guid _workflowTwo = Guid.Parse("f46600a6-20e8-4e35-89b7-8e55a4560939");
        private readonly static Guid _workspaceGuid = Guid.Parse("bed398ed-9042-49d0-9270-f0436540445d");
        private readonly static string _reportName = "test: False report";
        private readonly static Guid _testStepOne = Guid.Parse("ce9144ac-005f-41f4-bdb1-44817a3c287f");

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DataObjectExtensions))]
        //SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IResource resource)
        public void DataObjectExtensions_SetResourceNameAndId_GivenResourceNameIsBad_ShouldFixAndLoadResource()
        {
            //---------------Set up test pack-------------------
            var mockResource = new Mock<IResource>();
            var mockWarewolfResource = mockResource.As<IWarewolfResource>();
            var expectedResourceId = Guid.NewGuid();
            mockResource.SetupGet(o => o.ResourceID).Returns(expectedResourceId);
            mockResource.SetupGet(o => o.ResourceName).Returns("Hello World");
            mockResource.Setup(o => o.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            mockWarewolfResource.SetupGet(o => o.ResourceID).Returns(expectedResourceId);
            mockWarewolfResource.SetupGet(o => o.ResourceName).Returns("Hello World");

            var resourceCatalog = new Mock<IResourceCatalog>();
            const string ResourceId = "acb75027-ddeb-47d7-814e-a54c37247ec1";
            var objSourceResourceID = ResourceId.ToGuid();
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), objSourceResourceID)).Returns(mockResource.Object);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ResourceID);
            dataObject.SetupProperty(o => o.TestsResourceIds);
            const string ResourceName = "acb75027-ddeb-47d7-814e-a54c37247ec1.xml";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            dataObject.Object.SetResourceNameAndId(resourceCatalog.Object, ResourceName, out var outResource);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(It.IsAny<Guid>(), objSourceResourceID));
            dataObject.VerifySet(o => o.ResourceID = expectedResourceId, Times.Exactly(1));
            dataObject.VerifySet(o => o.ServiceName = "Hello World", Times.Exactly(1));
            dataObject.VerifySet(o => o.SourceResourceID = expectedResourceId, Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnJSON_Given_NoCoverageReports_ShouldReturnEmpty_Results()
        {
            var workspaceGuid = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();

            var mockCoverageDataObject = new Mock<ICoverageDataObject>();
            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var sut = DataObjectExtensions.RunCoverageAndReturnJSON(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, workspaceGuid, serializer, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, "\r\n  \"Results\": []\r\n");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnJSON_Given_CoverageReport_HasTestReports_False_ShouldReturnEmpty_Results()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne)).Returns(new List<IServiceTestCoverageModelTo>());

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowOne))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO { } });

            MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject,
                      out Mock<IResourceCatalog> mockResourceCatalog);

            var sut = DataObjectExtensions.RunCoverageAndReturnJSON(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, _workspaceGuid, serializer, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, "\r\n  \"Results\": []\r\n");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnJSON_Given_CoverageReport_HasTestReports_True_ShouldReturnNotEmpty_Results()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne))
            .Returns(new List<IServiceTestCoverageModelTo> { new ServiceTestCoverageModelTo
            {
                WorkflowId = _workflowOne,
                OldReportName = "test 1",
                ReportName =  _reportName,
                TotalCoverage = 0.3,
                AllTestNodesCovered = new ISingleTestNodesCovered[]
                {
                    new SingleTestNodesCovered(_reportName, new List<IServiceTestStep>{ })
                }
            }});

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowOne))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO { } });

            MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject,
                      out Mock<IResourceCatalog> mockResourceCatalog);

            var sut = DataObjectExtensions.RunCoverageAndReturnJSON(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, _workspaceGuid, serializer, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, "\"Results\": [\r\n    {\r\n      \"ResourceID\": \"fbda8700-2717-4879-88cd-6abdea4560da\",\r\n  ");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnHTML_TestInvalid()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne)).Returns(new List<IServiceTestCoverageModelTo>());

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowOne))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO { } });

            MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject,
                      out Mock<IResourceCatalog> mockResourceCatalog);

            var sut = DataObjectExtensions.RunCoverageAndReturnHTML(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, _workspaceGuid, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            StringAssert.Contains(executePayload, "Total Test Count: 1");
            StringAssert.Contains(executePayload, "Tests Passed: 0");
            StringAssert.Contains(executePayload, "Tests Failed: 0");
            StringAssert.Contains(executePayload, "Tests Invalid: 1");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnHTML_TestPassed()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne))
            .Returns(new List<IServiceTestCoverageModelTo> { new ServiceTestCoverageModelTo
            {
                WorkflowId = _workflowOne,
                OldReportName = "test 1",
                ReportName =  _reportName,
                TotalCoverage = 0.3,
                AllTestNodesCovered = new ISingleTestNodesCovered[]
                {
                    new SingleTestNodesCovered(_reportName, new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO
                        {
                            ActivityID = _testStepOne,
                            UniqueID = _testStepOne,
                            Type = StepType.Assert,
                            StepDescription = "StepType Assert",
                        }
                    })
                }
            }});


            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowOne))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO
                {
                    TestPassed = true,
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestPassed
                    }
                }});

            MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject,
                      out Mock<IResourceCatalog> mockResourceCatalog);

            var sut = DataObjectExtensions.RunCoverageAndReturnHTML(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, _workspaceGuid, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            StringAssert.Contains(executePayload, "Total Test Count: 1");
            StringAssert.Contains(executePayload, "Tests Passed: 1");
            StringAssert.Contains(executePayload, "Tests Failed: 0");
            StringAssert.Contains(executePayload, "Tests Invalid: 0");
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunCoverageAndReturnHTML_TestFailed()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne))
            .Returns(new List<IServiceTestCoverageModelTo> { new ServiceTestCoverageModelTo
            {
                WorkflowId = _workflowOne,
                OldReportName = "test 1",
                ReportName =  _reportName,
                TotalCoverage = 0.3,
                AllTestNodesCovered = new ISingleTestNodesCovered[]
                {
                    new SingleTestNodesCovered(_reportName, new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO
                        {
                            ActivityID = _testStepOne,
                            UniqueID = _testStepOne,
                            Type = StepType.Assert,
                            StepDescription = "StepType Assert",
                        }
                    })
                }
            }});


            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowOne))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO
                {
                    TestFailing = true,
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestFailed
                    }
                }});

            MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject,
                      out Mock<IResourceCatalog> mockResourceCatalog);

            var sut = DataObjectExtensions.RunCoverageAndReturnHTML(mockCoverageDataObject.Object, mockTestCoverageCatalog.Object, mockTestCatalog.Object, mockResourceCatalog.Object, _workspaceGuid, out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            StringAssert.Contains(executePayload, "Total Test Count: 1");
            StringAssert.Contains(executePayload, "Tests Passed: 0");
            StringAssert.Contains(executePayload, "Tests Failed: 1");
            StringAssert.Contains(executePayload, "Tests Invalid: 0");
        }

        

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnJSON_TestInvalid()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.Fetch(_workflowOne))
            .Returns(new List<IServiceTestCoverageModelTo> { _serviceTestCoverageModelTo });
            mockTestCoverageCatalog.Setup(o => o.FetchReport(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new ServiceTestCoverageModelTo
                {
                });

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowTwo))
                .Returns(new List<IServiceTestModelTO> { _serviceTestModelTO });

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.TestsResourceIds)
                .Returns(new Guid[] { _workflowOne, _workflowTwo });
            mockDSFDataObject.Setup(o => o.Clone()).Returns(new DsfDataObject(string.Empty, Guid.NewGuid()));

            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(r => r.ResourceID).Returns(_workflowTwo);
            mockResource.Setup(r => r.ResourceName).Returns(_reportName);
            mockResource.Setup(r => r.GetResourcePath(_workspaceGuid)).Returns("test/folder/" + _reportName);
            mockResourceCatalog.Setup(o => o.GetResources(It.IsAny<Guid>()))
               .Returns(new List<IResource>
               {
                    mockResource.Object
               });

            var mockServiceTestExecutorWrapper = new Mock<IServiceTestExecutorWrapper>();
            mockServiceTestExecutorWrapper.Setup(o => o.ExecuteTestAsync(It.IsAny<string>(), It.IsAny<IPrincipal>(), It.IsAny<Guid>(), It.IsAny<Dev2JsonSerializer>(), It.IsAny<IDSFDataObject>()))
                .Returns(System.Threading.Tasks.Task.FromResult(new ServiceTestModelTO
                {
                    TestName = "test one re-ran",
                    TestFailing = true,
                    FailureMessage = "test: failure mesage",
                    TestSteps = new List<IServiceTestStep>
                    {
                        //Empty TestSteps makes the report Invalid and override the TestFailing = true
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestInvalid,
                    }
                } as IServiceTestModelTO));


            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnJSON(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, _workspaceGuid, serializer, mockResourceCatalog.Object, mockTestCatalog.Object, out string executePayload, mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, " \"Test Name\": \"test one re-ran\",\r\n");
            StringAssert.Contains(executePayload, "\"Result\": \"Invalid\",\r\n");
            StringAssert.Contains(executePayload, "\"Message\": \"Test has no selected nodes\"\r\n");
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnJSON_TestFailed()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.FetchReport(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(_serviceTestCoverageModelTo);

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowTwo))
                .Returns(new List<IServiceTestModelTO> { new ServiceTestModelTO
                {
                    TestName = "test one saved",
                    TestFailing = true,
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestFailed
                    }
                }});

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.TestsResourceIds)
                .Returns(new Guid[] { _workflowOne, _workflowTwo });
            mockDSFDataObject.Setup(o => o.Clone()).Returns(new DsfDataObject(string.Empty, Guid.NewGuid()));

            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(r => r.ResourceID).Returns(_workflowTwo);
            mockResource.Setup(r => r.ResourceName).Returns(_reportName);
            mockResource.Setup(r => r.GetResourcePath(_workspaceGuid)).Returns("test/folder/" + _reportName);
            mockResourceCatalog.Setup(o => o.GetResources(It.IsAny<Guid>()))
               .Returns(new List<IResource>
               {
                    mockResource.Object
               });

            var mockServiceTestExecutorWrapper = new Mock<IServiceTestExecutorWrapper>();
            mockServiceTestExecutorWrapper.Setup(o => o.ExecuteTestAsync(It.IsAny<string>(), It.IsAny<IPrincipal>(), It.IsAny<Guid>(), It.IsAny<Dev2JsonSerializer>(), It.IsAny<IDSFDataObject>()))
                .Returns(System.Threading.Tasks.Task.FromResult(new ServiceTestModelTO 
                {
                    TestName = "test one re-ran",
                    TestFailing = true,
                    FailureMessage = "test: failure mesage",
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestFailed,
                    }
                } as IServiceTestModelTO));

            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnJSON(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, _workspaceGuid, serializer, mockResourceCatalog.Object, mockTestCatalog.Object, out string executePayload, mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, " \"Test Name\": \"test one re-ran\",\r\n");
            StringAssert.Contains(executePayload, "\"Result\": \"Failed\",\r\n");
            StringAssert.Contains(executePayload, "\"Message\": \"test: failure mesage\"\r\n");
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnJSON_TestPassed()
        {
            var serializer = new Dev2JsonSerializer();

            var mockTestCoverageCatalog = new Mock<ITestCoverageCatalog>();
            mockTestCoverageCatalog.Setup(o => o.FetchReport(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(_serviceTestCoverageModelTo);


            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(o => o.Fetch(_workflowTwo))
                .Returns(new List<IServiceTestModelTO> { _serviceTestModelTO });

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.TestsResourceIds)
                .Returns(new Guid[] { _workflowOne, _workflowTwo });
            mockDSFDataObject.Setup(o => o.Clone()).Returns(new DsfDataObject(string.Empty, Guid.NewGuid()));

            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(r => r.ResourceID).Returns(_workflowTwo);
            mockResource.Setup(r => r.ResourceName).Returns(_reportName);
            mockResource.Setup(r => r.GetResourcePath(_workspaceGuid)).Returns("test/folder/" + _reportName);
            mockResourceCatalog.Setup(o => o.GetResources(It.IsAny<Guid>()))
               .Returns(new List<IResource>
               {
                    mockResource.Object
               });

            var mockServiceTestExecutorWrapper = new Mock<IServiceTestExecutorWrapper>();
            mockServiceTestExecutorWrapper.Setup(o => o.ExecuteTestAsync(It.IsAny<string>(), It.IsAny<IPrincipal>(), It.IsAny<Guid>(), It.IsAny<Dev2JsonSerializer>(), It.IsAny<IDSFDataObject>()))
                .Returns(System.Threading.Tasks.Task.FromResult(new ServiceTestModelTO
                {
                    TestName = "test one re-ran",
                    TestPassed = true,
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestPassed,
                    }
                } as IServiceTestModelTO));

            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnJSON(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, _workspaceGuid, serializer, mockResourceCatalog.Object, mockTestCatalog.Object, out string executePayload, mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("application/json", sut.ContentType);
            StringAssert.Contains(executePayload, " \"Test Name\": \"test one re-ran\",\r\n");
            StringAssert.Contains(executePayload, "\"Result\": \"Passed\"\r\n");
        }


        private static void MockSetup(out Mock<ICoverageDataObject> mockCoverageDataObject, out Mock<IResourceCatalog> mockResourceCatalog)
        {
            mockCoverageDataObject = new Mock<ICoverageDataObject>();
            mockCoverageDataObject.Setup(o => o.CoverageReportResourceIds)
                .Returns(new Guid[] { _workflowOne, _workflowTwo });


            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            mockWarewolfWorkflow.Setup(o => o.ResourceID).Returns(_workflowOne);
            mockWarewolfWorkflow.Setup(o => o.WorkflowNodes).Returns(new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = _testStepOne,
                    UniqueID = _testStepOne
                }
            });

            mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResources<IWarewolfWorkflow>(_workspaceGuid))
                .Returns(new List<IWarewolfWorkflow> { mockWarewolfWorkflow.Object });

        }

        private readonly IServiceTestModelTO _serviceTestModelTO = new ServiceTestModelTO
        {
            TestName = "test one saved",
            TestFailing = true,
            TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
            Result = new TestRunResult
            {
                RunTestResult = RunResult.TestFailed
            }
        };

        private readonly IServiceTestCoverageModelTo _serviceTestCoverageModelTo = new ServiceTestCoverageModelTo
        {
            WorkflowId = _workflowOne,
            OldReportName = "test 1",
            ReportName = _reportName,
            TotalCoverage = 0.3,
            AllTestNodesCovered = new ISingleTestNodesCovered[]
            {
                new SingleTestNodesCovered(_reportName, new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = _testStepOne,
                        UniqueID = _testStepOne,
                        Type = StepType.Assert,
                        StepDescription = "StepType Assert",
                    }
                })
            }
        };
    }

}
