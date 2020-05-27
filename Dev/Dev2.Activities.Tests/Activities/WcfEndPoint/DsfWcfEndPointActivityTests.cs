/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Dev2.Activities.WcfEndPoint;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.Activities.WcfEndPoint
{
    [TestClass]
    public class DsfComDllActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_Method_IsNull_Expect_Error()
        {
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new TestDsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            dsfWcfEndPointActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("No Method Selected", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_ExecutionImpl_Catch_Common_IsNotNull_Expect_NoError()
        {
            //-----------------------Arrange---------------------
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockOutputDescription = new Mock<IOutputDescription>();
            var mockPath = new Mock<IPath>();
            var mockDataSourceShape = new Mock<IDataSourceShape>();

            var wcfSource = new WcfSource(new FakeWcfProxyService()) { Name = "WcfSource", EndpointUrl = "TestUrl" };
            
            var wcfAction = new WcfAction()
            {
                FullName = "MethodName",
                Method = "MethodName",
            };

            Thread.CurrentPrincipal = null;
            var identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var dataListID = Guid.NewGuid();
            var environment = new ExecutionEnvironment();
            var outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping() };
            var dsfWcfEndPointActivity = new TestDsfWcfEndPointActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = wcfAction,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = outputs,
                OutputDescription = mockOutputDescription.Object,
                Source = wcfSource,
            };
            
            mockDataSourceShape.Setup(o => o.Paths).Returns(new List<IPath> { mockPath.Object }).Verifiable();
            mockResourceCatalog.Setup(o => o.GetResource<WcfSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(wcfSource).Verifiable();
            mockOutputDescription.Setup(o => o.DataSourceShapes).Returns(new List<IDataSourceShape> { mockDataSourceShape.Object }).Verifiable();
            mockOutputDescription.Setup(o => o.Format).Returns(OutputFormats.ShapedXML).Verifiable();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment).Verifiable();
            //-----------------------Act-------------------------
            dsfWcfEndPointActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            mockDataSourceShape.VerifyAll();
            mockResourceCatalog.VerifyAll();
            mockOutputDescription.VerifyAll();
            mockDSFDataObject.VerifyAll();

            Assert.AreEqual(0, errorResult.FetchErrors().Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_ExecutionImpl_Catch_GetResource_IsNull_Expect_Error()
        {
            //-----------------------Arrange---------------------
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockOutputDescription = new Mock<IOutputDescription>();
            var mockPath = new Mock<IPath>();

            var wcfSource = new WcfSource(new FakeWcfProxyService()) { Name = "WcfSource", EndpointUrl = "TestUrl" };

            var wcfAction = new WcfAction()
            {
                FullName = "MethodName",
                Method = "MethodName",
            };

            Thread.CurrentPrincipal = null;
            var identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var dataListID = Guid.NewGuid();
            var environment = new ExecutionEnvironment();
            var outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping() };
            var dsfWcfEndPointActivity = new TestDsfWcfEndPointActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = wcfAction,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = outputs,
                OutputDescription = mockOutputDescription.Object,
                Source = wcfSource,
            };
            
            mockOutputDescription.Setup(o => o.Format).Returns(OutputFormats.ShapedXML).Verifiable();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment).Verifiable();
            //-----------------------Act-------------------------
            dsfWcfEndPointActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            mockDSFDataObject.VerifyAll();
            mockOutputDescription.VerifyAll();
            mockResourceCatalog.Verify(o => o.GetResource<WcfSource>(It.IsAny<Guid>(), It.IsAny<Guid>()));

            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_GetFindMissingType_WhenCalled_Expect_DataGridActivity()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var FindMissingType = dsfWcfEndPointActivity.GetFindMissingType();
            //-----------------------Assert----------------------;
            Assert.AreEqual(enFindMissingType.DataGridActivity, FindMissingType);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_GetHashCode_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var hashCode = dsfWcfEndPointActivity.GetHashCode();
            //-----------------------Assert----------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_GetHashCode_MethodOutputDescriptionSource_NotNull_Expect_IsNotNull()
        {
            //-----------------------Arrange---------------------
            var mockOutputDescription = new Mock<IOutputDescription>();

            var wcfSource = new WcfSource(new FakeWcfProxyService()) { Name = "WcfSource", EndpointUrl = "TestUrl" };
            var wcfAction = new WcfAction()
            {
                FullName = "MethodName",
                Method = "MethodName",
            };
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity()
            {
                Method = wcfAction,
                OutputDescription = mockOutputDescription.Object,
                Source = wcfSource,
            };
            //-----------------------Act-------------------------
            var hashCode = dsfWcfEndPointActivity.GetHashCode();
            //-----------------------Assert----------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_Equals_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(dsfWcfEndPointActivity);
            //-----------------------Assert----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_Equals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(null);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_Equals_InstancesWithNoParams_NotSame_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(new DsfWcfEndPointActivity());
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_Equals_InstancesWithSameParams_NotSame_Expect_False()
        {
            //-----------------------Arrange---------------------
            var mockOutputDescription = new Mock<IOutputDescription>();

            var wcfSource = new WcfSource(new FakeWcfProxyService()) { Name = "WcfSource", EndpointUrl = "TestUrl" };

            var wcfAction = new WcfAction()
            {
                FullName = "MethodName",
                Method = "MethodName",
            };

            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity()
            {
                Method = wcfAction,
                OutputDescription = mockOutputDescription.Object,
                Source = wcfSource,
            };

            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(new DsfWcfEndPointActivity()
            {
                Method = wcfAction,
                OutputDescription = mockOutputDescription.Object,
                Source = wcfSource,
            });
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_ObjectEquals_IsNotSameInstance_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            var obj = new object();
            obj = new DsfWcfEndPointActivity();
            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_ObjectEquals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfWcfEndPointActivity = new DsfWcfEndPointActivity();
            var obj = new object();
            obj = null;
            //-----------------------Act-------------------------
            var equals = dsfWcfEndPointActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWcfEndPointActivity))]
        public void DsfWcfEndPointActivity_TryExecute_injectVal_IsNull_Expect_NoError()
        {
            //-----------------------Arrange---------------------
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockOutputDescription = new Mock<IOutputDescription>();
            var mockPath = new Mock<IPath>();
            var mockDataSourceShape = new Mock<IDataSourceShape>();
            var mockWcfAction = new Mock<IWcfAction>();

            var wcfSource = new WcfSource(new FakeWcfProxyService()) { Name = "WcfSource", EndpointUrl = "TestUrl" };
            
            Thread.CurrentPrincipal = null;
            var identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var dataListID = Guid.NewGuid();
            var environment = new ExecutionEnvironment();
            var outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping() };
            var dsfWcfEndPointActivity = new TestDsfWcfEndPointActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = mockWcfAction.Object,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "") },
                Outputs = outputs,
                OutputDescription = mockOutputDescription.Object,
            };
            
            mockDataSourceShape.Setup(o => o.Paths).Returns(new List<IPath> { mockPath.Object });
            mockWcfAction.Setup(o => o.Method).Returns("TestMethod");
            mockResourceCatalog.Setup(o => o.GetResource<WcfSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(wcfSource);
            mockOutputDescription.Setup(o => o.DataSourceShapes).Returns(new List<IDataSourceShape> { mockDataSourceShape.Object }).Verifiable();
            mockOutputDescription.Setup(o => o.Format).Returns(OutputFormats.ShapedXML).Verifiable();
            mockDSFDataObject.Setup(o => o.DataListID).Returns(dataListID);
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment).Verifiable();
            //-----------------------Act-------------------------
            dsfWcfEndPointActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            mockOutputDescription.VerifyAll();
            mockDataSourceShape.VerifyAll();
            mockResourceCatalog.Verify(o => o.GetResource<WcfSource>(It.IsAny<Guid>(), It.IsAny<Guid>()));
            Assert.AreEqual(0, errorResult.FetchErrors().Count);
        }

        class TestDsfWcfEndPointActivity : DsfWcfEndPointActivity
        {
            public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
            {
                base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
            }
        }

        public class FakeWcfProxyService : IWcfProxyService
        {
            public IOutputDescription ExecuteWebService(Runtime.ServiceModel.Data.WcfService src)
            {
                return new OutputDescription();
            }

            public object ExcecuteMethod(IWcfAction action, string endpointUrl)
            {
                return new object();
            }

            public Dictionary<MethodInfo, ParameterInfo[]> GetMethods(string endpoint)
            {
                return new Dictionary<MethodInfo, ParameterInfo[]>();
            }
        }
    }
}
