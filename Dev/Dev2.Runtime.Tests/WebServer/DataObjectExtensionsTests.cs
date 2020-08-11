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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.Data;
using Dev2.DynamicServices;
using Dev2.InterfaceImplementors;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Services;
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
        public void DataObjectExtensions_SetResourceNameAndId_ResourceName_And_Id_Empty_ExpectServiceNotFound()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetResourceNameAndId(sut, new Mock<IResourceCatalog>().Object, string.Empty, out var outResource);

            Assert.IsNull(outResource);
            Assert.AreEqual("Service  not found.", sut.Environment.FetchErrors());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId_ServiceName_NotGuidParse_ExpectServiceNotFound()
        {
            var resourceId = "service_id";
            var resourceName = resourceId + ".json";
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ServiceName = "servicename"
            };

            DataObjectExtensions.SetResourceNameAndId(sut, mockResourceCatalog.Object, resourceName, out var outResource);

            Assert.IsNull(outResource);
            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.AreEqual(Guid.Empty, sut.SourceResourceID);
            Assert.AreEqual("Service " + resourceName + " not found.", sut.Environment.FetchErrors());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId_ServiceName_GuidParse_ExpectServiceNotFound()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = resourceId + ".bite";
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ServiceName = "servicename"   
            };

            DataObjectExtensions.SetResourceNameAndId(sut, mockResourceCatalog.Object, resourceName, out var outResource);

            Assert.IsNull(outResource);
            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.AreEqual(Guid.Empty, sut.SourceResourceID);
            Assert.AreEqual("Service "+ resourceName + " not found.", sut.Environment.FetchErrors());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId_ServiceName_NotGuidParse_ExpectResourceID()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = resourceId + ".bite"; 
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            mockResourceCatalog.Setup(o => o.GetResource(_workspaceGuid, "servicename"))
               .Returns(new Workflow 
               { 
                   ResourceID = resourceId 
               });

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ServiceName = "servicename",
                WorkspaceID = _workspaceGuid
            };

            DataObjectExtensions.SetResourceNameAndId(sut, mockResourceCatalog.Object, resourceName, out var outResource);

            Assert.IsNotNull(outResource);
            Assert.AreEqual("servicename", sut.ServiceName);
            Assert.AreEqual(resourceId, sut.ResourceID);
            Assert.AreEqual(resourceId, sut.SourceResourceID);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId_ServiceName_GuidParse_ExpectResourceID()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = resourceId;

            var mockResourceCatalog = new Mock<IResourceCatalog>();

            mockResourceCatalog.Setup(o => o.GetResource(_workspaceGuid, resourceId))
               .Returns(new Workflow 
               { 
                   ResourceID = resourceId, 
                   ResourceName = resourceName.ToString() 
               });

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ServiceName = "servicename",
                WorkspaceID = _workspaceGuid
            };

            DataObjectExtensions.SetResourceNameAndId(sut, mockResourceCatalog.Object, resourceName.ToString(), out var outResource);

            Assert.IsNotNull(outResource);
            Assert.AreEqual(resourceId.ToString(), sut.ServiceName);
            Assert.AreEqual(resourceId, sut.ResourceID);
            Assert.AreEqual(resourceId, sut.SourceResourceID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId_ServiceName_GuidParse_ExpectServiceNotFound1()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = resourceId + ".bite";

            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResourceCatalog.Setup(o => o.GetResource(_workspaceGuid, "servicename"))
               .Returns(new Workflow { ResourceID = resourceId });

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ServiceName = "servicename",
                WorkspaceID = _workspaceGuid
            };

            DataObjectExtensions.SetResourceNameAndId(sut, mockResourceCatalog.Object, resourceName, out var outResource);

            Assert.IsNotNull(outResource);
            Assert.AreEqual(resourceId, sut.ResourceID);
            Assert.AreEqual(resourceId, sut.SourceResourceID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetResourceNameAndId()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetResourceNameAndId(sut, new Mock<IResourceCatalog>().Object, string.Empty, out var outResource);

            Assert.IsNull(outResource);
            Assert.AreEqual("Service  not found.", sut.Environment.FetchErrors());
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnTRX_NoTestRan()
        { 
            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnTRX(new Mock<IDSFDataObject>().Object, new Mock<IPrincipal>().Object, Guid.NewGuid(),
                new Dev2JsonSerializer(), new Mock<IResourceCatalog>().Object, new Mock<ITestCatalog>().Object, out string executionPayload, 
                new Mock<ITestCoverageCatalog>().Object, new Mock<IServiceTestExecutorWrapper>().Object);

            Assert.IsNotNull(executionPayload);
            Assert.AreEqual("text/xml", sut.ContentType);
            Assert.IsNotNull(sut);
            StringAssert.Contains(executionPayload, "<ResultSummary outcome=\"Completed\">");
            StringAssert.Contains(executionPayload, "<Counters total=\"0\" ");
            StringAssert.Contains(executionPayload, "passed=\"0\" ");
            StringAssert.Contains(executionPayload, "failed=\"0\" ");
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnTRX_TestFailed()
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
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("test/folder/" + _reportName);
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
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestFailed,
                    }
                } as IServiceTestModelTO));

            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnTRX(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, Guid.NewGuid(),
                new Dev2JsonSerializer(), mockResourceCatalog.Object, mockTestCatalog.Object, out string executionPayload,
                mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);


            Assert.IsNotNull(executionPayload);
            Assert.AreEqual("text/xml", sut.ContentType);
            Assert.IsNotNull(sut);
            StringAssert.Contains(executionPayload, "<ResultSummary outcome=\"Completed\">");
            StringAssert.Contains(executionPayload, "<Counters total=\"1\" ");
            StringAssert.Contains(executionPayload, "passed=\"0\" ");
            StringAssert.Contains(executionPayload, "failed=\"1\" ");

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnTRX_TestInvalid()
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
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("test/folder/" + _reportName);
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
                    TestInvalid = true,
                    TestSteps = new List<IServiceTestStep>
                    {
                        new ServiceTestStepTO(_testStepOne, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert){ }
                    },
                    Result = new TestRunResult
                    {
                        RunTestResult = RunResult.TestInvalid,
                    }
                } as IServiceTestModelTO));

            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnTRX(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, Guid.NewGuid(),
                new Dev2JsonSerializer(), mockResourceCatalog.Object, mockTestCatalog.Object, out string executionPayload,
                mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);


            Assert.IsNotNull(executionPayload);
            Assert.AreEqual("text/xml", sut.ContentType);
            Assert.IsNotNull(sut);
            StringAssert.Contains(executionPayload, "<ResultSummary outcome=\"Completed\">");
            StringAssert.Contains(executionPayload, "<Counters total=\"1\" ");
            StringAssert.Contains(executionPayload, "passed=\"1\" "); //TODO: add an Invalid key to ServiceTestModelTRXResultBuilder.BuildTestResultTRX 
            StringAssert.Contains(executionPayload, "failed=\"0\" ");

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_RunMultipleTestBatchesAndReturnTRX_TestPassed()
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
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("test/folder/" + _reportName);
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

            var sut = DataObjectExtensions.RunMultipleTestBatchesAndReturnTRX(mockDSFDataObject.Object, new Mock<IPrincipal>().Object, Guid.NewGuid(),
                new Dev2JsonSerializer(), mockResourceCatalog.Object, mockTestCatalog.Object, out string executionPayload,
                mockTestCoverageCatalog.Object, mockServiceTestExecutorWrapper.Object);


            Assert.IsNotNull(executionPayload);
            Assert.AreEqual("text/xml", sut.ContentType);
            Assert.IsNotNull(sut);
            StringAssert.Contains(executionPayload, "<ResultSummary outcome=\"Completed\">");
            StringAssert.Contains(executionPayload, "<Counters total=\"1\" ");
            StringAssert.Contains(executionPayload, "passed=\"1\" ");
            StringAssert.Contains(executionPayload, "failed=\"0\" ");

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_CanExecuteCurrentResource_Resource_Is_Null_ExpectFalse()
        {
            var sut = DataObjectExtensions.CanExecuteCurrentResource(new Mock<IDSFDataObject>().Object, null, new Mock<IAuthorizationService>().Object);

            Assert.IsFalse(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_CanExecuteCurrentResource_Resource_IsNot_Null_ExpectFalse()
        {
            var sut = DataObjectExtensions.CanExecuteCurrentResource(new Mock<IDSFDataObject>().Object, new Mock<IWarewolfResource>().Object, new Mock<IAuthorizationService>().Object);

            Assert.IsFalse(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_CanExecuteCurrentResource_EmitionTypes_IsNot_TRX_ExpectTrue()
        {
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(o => o.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<Guid>()))
                .Returns(true);

            var sut = DataObjectExtensions.CanExecuteCurrentResource(new Mock<IDSFDataObject>().Object, new Mock<IWarewolfResource>().Object, mockAuthorizationService.Object);

            Assert.IsTrue(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_CanExecuteCurrentResource_EmitionTypes_IsNot_TRX_ExpectFalse()
        {
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(o => o.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.View, It.IsAny<Guid>()))
                .Returns(true);
            mockAuthorizationService.Setup(o => o.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, It.IsAny<Guid>()))
                .Returns(false);

            var sut = DataObjectExtensions.CanExecuteCurrentResource(new Mock<IDSFDataObject>().Object, new Mock<IWarewolfResource>().Object, mockAuthorizationService.Object);

            Assert.IsFalse(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_CanExecuteCurrentResource_EmitionTypes_Is_TRX_ExpectTrue()
        {
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.ReturnType)
                .Returns(Web.EmitionTypes.TRX);

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(o => o.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<Guid>()))
                .Returns(true);


            var sut = DataObjectExtensions.CanExecuteCurrentResource(mockDSFDataObject.Object, new Mock<IWarewolfResource>().Object, mockAuthorizationService.Object);

            Assert.IsTrue(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_Not_IsRunAllCoverageRequest_Headers_Null_ExpectEmitionTypesXML()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, string.Empty, null);

            Assert.AreEqual(Web.EmitionTypes.XML, dSFDataObject.ReturnType);
            Assert.IsFalse(dSFDataObject.IsServiceTestExecution);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_Not_IsRunAllCoverageRequest_SetContentType_From_Headers_Not_Null_ExpectEmitionTypesXML()
        {
            var headers = new NameValueCollection
            {
                { "Content-Type", "test:headerValue.xml" }
            };

            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, "*",  headers);

            Assert.AreEqual(Web.EmitionTypes.XML, dSFDataObject.ReturnType);
            Assert.IsFalse(dSFDataObject.IsServiceTestExecution);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_Not_IsRunAllCoverageRequest_SetContentType_From_Headers_Not_Null_ExpectEmitionTypesJSON()
        {
            var headers = new NameValueCollection
            {
                { "ContentType", "test:headerValue.json" }
            };

            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, ".tests", headers);

            Assert.AreEqual(Web.EmitionTypes.JSON, dSFDataObject.ReturnType);
            Assert.IsFalse(dSFDataObject.IsServiceTestExecution);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_IsRunAllTestsRequest_And_Not_IsRunAllCoverageRequest_SetContentType_ExpectEmitionTypesTRX()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.TRX
            };

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, ".tests.trx", null);

            Assert.AreEqual(Web.EmitionTypes.TRX, dSFDataObject.ReturnType);
            Assert.IsTrue(dSFDataObject.IsServiceTestExecution);
            Assert.AreEqual("*", dSFDataObject.TestName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_IsRunAllTestsRequest_And_Not_IsRunAllCoverageRequest_ExpectEmitionTypesTEST()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.TEST
            };

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, ".tests", null);

            Assert.AreEqual(Web.EmitionTypes.TEST, dSFDataObject.ReturnType);
            Assert.IsTrue(dSFDataObject.IsServiceTestExecution);
            Assert.AreEqual("*", dSFDataObject.TestName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_IsRunAllCoverageRequest_ExpectEmitionTypesCover()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.Cover
            };

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, ".coverage", null);

            Assert.AreEqual(Web.EmitionTypes.Cover, dSFDataObject.ReturnType);
            Assert.IsTrue(dSFDataObject.IsServiceTestExecution);
            Assert.AreEqual("*", dSFDataObject.TestName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_IsRunAllCoverageRequest_ExpectEmitionTypesCoverJson()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.CoverJson
            };

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, ".coverage.json", null);

            Assert.AreEqual(Web.EmitionTypes.CoverJson, dSFDataObject.ReturnType);
            Assert.IsTrue(dSFDataObject.IsServiceTestExecution);
            Assert.AreEqual("*", dSFDataObject.TestName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForTestExecution_Not_IsRunAllTestsRequest_And_IsRunAllCoverageRequest_serviceNameEndsWith_dot_slash_coverage_ExpectEmitionTypesCoverJson()
        {
            var dSFDataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.CoverJson
            };

            DataObjectExtensions.SetupForTestExecution(dSFDataObject, "/.coverage", null);

            Assert.AreEqual(Web.EmitionTypes.CoverJson, dSFDataObject.ReturnType);
            Assert.IsTrue(dSFDataObject.IsServiceTestExecution);
            Assert.AreEqual("*", dSFDataObject.TestName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetHeaders_Headers_Null_ExpectNewExecutionID()
        {
            var dataListId = Guid.NewGuid();
            var sut = new DsfDataObject(string.Empty, dataListId);

            DataObjectExtensions.SetHeaders(sut, null);

            Assert.AreEqual(string.Empty, sut.CustomTransactionID);
            Assert.IsNotNull(sut.ExecutionID);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetHeaders_Headers_NotNull_ExpectNewExecutionID()
        {
            var dataListId = Guid.NewGuid();
            var transId = "test_string_id";
            var sut = new DsfDataObject(string.Empty, dataListId);

            DataObjectExtensions.SetHeaders(sut, new NameValueCollection
            {
                { "Warewolf-Custom-Transaction-Id", transId }
            });

            Assert.AreEqual(transId.ToString(), sut.CustomTransactionID);
            Assert.IsNotNull(sut.ExecutionID);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetHeaders_Headers_NotNull_ExpectSetHeadersExecutionID()
        {
            var transId = "test_string_id";
            var executionId = Guid.NewGuid();
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetHeaders(sut, new NameValueCollection 
            { 
                { "Warewolf-Custom-Transaction-Id", transId }, 
                { "Warewolf-Execution-Id", executionId.ToString() }
            });

            Assert.AreEqual(transId.ToString(), sut.CustomTransactionID);
            Assert.AreNotSame(executionId, sut.ExecutionID);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForWebDebug_WebRequestTO_Null_ExpectIsDebugFalse()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForWebDebug(sut, null);

            Assert.IsFalse(sut.IsDebug);
            Assert.IsFalse(sut.IsDebugFromWeb);
            Assert.AreEqual(Guid.Empty, sut.ClientID);
            Assert.AreEqual(Guid.Empty, sut.DebugSessionID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForWebDebug_WebRequestTO_NotNull_ExpectIsDebugTrue()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForWebDebug(sut, new WebRequestTO
            {
                Variables = new NameValueCollection
                {
                    { "IsDebug", "test:value" }
                }
            });

            Assert.IsTrue(sut.IsDebug);
            Assert.IsTrue(sut.IsDebugFromWeb);
            Assert.AreNotEqual(Guid.Empty, sut.ClientID);
            Assert.AreNotEqual(Guid.Empty, sut.DebugSessionID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForWebDebug_WebRequestTO_NotNull_ExpectIsDebugFalse()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForWebDebug(sut, new WebRequestTO
            {
                Variables = new NameValueCollection
                {
                    { "IsOtherTestValue", "test:value" }
                }
            });

            Assert.IsFalse(sut.IsDebug);
            Assert.IsFalse(sut.IsDebugFromWeb);
            Assert.AreEqual(Guid.Empty, sut.ClientID);
            Assert.AreEqual(Guid.Empty, sut.DebugSessionID);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForRemoteInvoke_Headers_Null_ExpectFalse()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForRemoteInvoke(sut, null);

            Assert.IsFalse(sut.RemoteInvoke);
            Assert.IsFalse(sut.RemoteNonDebugInvoke);
            Assert.IsNull(sut.RemoteInvokerID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForRemoteInvoke_Headers_NotNull_IsRemote_ExpectFalse()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForRemoteInvoke(sut, new NameValueCollection 
            {
                { HttpRequestHeader.Cookie.ToString(), "is remote" }
            });

            Assert.IsFalse(sut.RemoteInvoke);
            Assert.IsFalse(sut.RemoteNonDebugInvoke);
            Assert.IsNull(sut.RemoteInvokerID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForRemoteInvoke_Headers_NotNull_IsRemote_ExpectRemoteServerInvokeTrue()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForRemoteInvoke(sut, new NameValueCollection
            {
                { HttpRequestHeader.Cookie.ToString(), GlobalConstants.RemoteServerInvoke },
                { HttpRequestHeader.From.ToString(), "is from address" },
            });

            Assert.IsTrue(sut.RemoteInvoke);
            Assert.IsFalse(sut.RemoteNonDebugInvoke);
            Assert.AreEqual("is from address", sut.RemoteInvokerID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetupForRemoteInvoke_Headers_NotNull_IsRemote_ExpectRemoteNonDebugInvokeTrue()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetupForRemoteInvoke(sut, new NameValueCollection
            {
                { HttpRequestHeader.Cookie.ToString(), GlobalConstants.RemoteDebugServerInvoke },
                { HttpRequestHeader.From.ToString(), "is from address" },
            });

            Assert.IsFalse(sut.RemoteInvoke);
            Assert.IsTrue(sut.RemoteNonDebugInvoke);
            Assert.AreEqual("is from address", sut.RemoteInvokerID);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetTestResourceIds_IsRunAllTestsRequest_False_ExpectTestsResourceIdsContainsResourceId()
        {
            var resourceId = Guid.NewGuid();

            var mockWarewolfResource = new Mock<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID)
                .Returns(resourceId);

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            DataObjectExtensions.SetTestResourceIds(sut, new Mock<IContextualResourceCatalog>().Object, null, string.Empty, mockWarewolfResource.Object);

            Assert.AreEqual(resourceId, sut.TestsResourceIds.First());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetTestResourceIds_IsRunAllTestsRequest_True_ExpectTestsResourceIdsContainsResourceId()
        {
            var resourceId = Guid.NewGuid();

            var mockWarewolfResource = new Mock<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID)
                .Returns(resourceId);

            var mockContextualResourceCatalog = new Mock<IContextualResourceCatalog>();
            mockContextualResourceCatalog.Setup(o => o.GetExecutableResources("workflow.tests"))
                .Returns(new List<IWarewolfResource>
                {
                    new Workflow
                    {
                        ResourceID = resourceId
                    }
                });

            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ReturnType = Web.EmitionTypes.TEST
            };

            DataObjectExtensions.SetTestResourceIds(sut, mockContextualResourceCatalog.Object, new WebRequestTO { WebServerUrl = @"http://localhost:3210/secure/workflow.tests" }, ".tests", mockWarewolfResource.Object);

            Assert.AreEqual(resourceId, sut.TestsResourceIds.First());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetTestCoverageResourceIds_IsRunAllCoverageRequest_False_ExpectCoverageReportResourceIdsContainsResourceId()
        {
            var resourceId = Guid.NewGuid();

            var mockWarewolfResource = new Mock<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID)
                .Returns(resourceId);

            var sut = new CoverageDataContext(resourceId, Web.EmitionTypes.Cover, "http://localhost:3210/secure/.coverage");

            DataObjectExtensions.SetTestCoverageResourceIds(sut, new Mock<IContextualResourceCatalog>().Object, null, string.Empty, mockWarewolfResource.Object);

            Assert.AreEqual(resourceId, sut.CoverageReportResourceIds.First());
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetTestCoverageResourceIds_IsRunAllCoverageRequest_False_ExpectCoverageReportResourceIdsContainsResourceId1()
        {
            var resourceId = Guid.NewGuid();
            var uri = "http://localhost:3210/secure/.coverage";

            var mockWarewolfResource = new Mock<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID)
                .Returns(resourceId);

            var mockContextualResourceCatalog = new Mock<IContextualResourceCatalog>();
            mockContextualResourceCatalog.Setup(o => o.GetExecutableResources("/"))
                .Returns(new List<IWarewolfResource>
                {
                    new Workflow
                    {
                        ResourceID = resourceId
                    }
                });

            var sut = new CoverageDataContext(resourceId, Web.EmitionTypes.Cover, uri);

            DataObjectExtensions.SetTestCoverageResourceIds(sut, mockContextualResourceCatalog.Object, new WebRequestTO { WebServerUrl = uri }, "*", mockWarewolfResource.Object);

            Assert.AreEqual(resourceId, sut.CoverageReportResourceIds.First());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_EmptyServiceName_And_NullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflow.tests"), string.Empty, null);

            Assert.AreEqual(string.Empty, result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_NotEmptyServiceName_And_NullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflow.tests"), "*", null);

            Assert.AreEqual("*", result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesJSON()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder/.tests"), ".coverage", new NameValueCollection { { "Content-Type", "json" } });

            Assert.AreEqual(".coverage", result);
            Assert.AreEqual(Web.EmitionTypes.JSON, sut.ReturnType);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_TEST_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder/.tests"), ".coverage", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual(".coverage", result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_Cover_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder/.coverage"), ".coverage", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual(".coverage", result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_CoverJson_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder/.coverage.json"), "*", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("*", result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_TRX_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesXML()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid());

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder/.tests.trx"), "*", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("*", result);
            Assert.AreEqual(Web.EmitionTypes.XML, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_API_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesSWAGGER()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                OriginalServiceName = "original/servicename.bite"
            };

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder.tests"), "/.api", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("o", result);
            Assert.IsFalse(sut.IsServiceTestExecution);
            Assert.IsNull(sut.TestName);
            Assert.AreEqual(Web.EmitionTypes.SWAGGER, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_TEST_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesTEST()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                OriginalServiceName = "original/servicename.bite"
            };

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder.tests"), "/.tests", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("o", result);
            Assert.IsTrue(sut.IsServiceTestExecution);
            Assert.AreEqual("SERVICENAME.BITE", sut.TestName);
            Assert.AreEqual(Web.EmitionTypes.TEST, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_TRX_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesTRX()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                OriginalServiceName = "original/servicename.bite"
            };

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder.tests"), "/.trx", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("original/servicename", result);
            Assert.IsTrue(sut.IsServiceTestExecution);
            Assert.AreEqual("SERVICENAME.BITE", sut.TestName);
            Assert.AreEqual(Web.EmitionTypes.TRX, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_Cover_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesCover()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                OriginalServiceName = "original/servicename.coverage"
            };

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder.tests"), "/.coverage", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("o", result);
            Assert.IsFalse(sut.IsServiceTestExecution);
            Assert.IsNull(sut.TestName);
            Assert.AreEqual(Web.EmitionTypes.Cover, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataObjectExtensions))]
        public void DataObjectExtensions_SetEmissionType_CoverJson_NotEmptyServiceName_And_NotNullHeaders_ExpectEmitionTypesCoverJson()
        {
            var sut = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                OriginalServiceName = "original/servicename.coverage.json"
            };

            var result = DataObjectExtensions.SetEmissionType(sut, new Uri("http://localhost:3110/secure/workflowFolder.tests"), "/.coverage.json", new NameValueCollection { { "Content-Type", "xml" } });

            Assert.AreEqual("original/servicename", result);
            Assert.IsFalse(sut.IsServiceTestExecution);
            Assert.IsNull(sut.TestName);
            Assert.AreEqual(Web.EmitionTypes.CoverJson, sut.ReturnType);
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
