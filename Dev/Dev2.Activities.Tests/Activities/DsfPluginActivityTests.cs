/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.Activities
{
    [TestClass]
    public class DsfPluginActivityTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_IsNull_Expect_Error1()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------

            //-----------------------Assert----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_IsNull_Expect_Error()
        {
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("No Method Selected", errorResult.FetchErrors()[0]);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ExecutionImpl_Inputs_IsNull_Catch_Expect_Error()
        {
            //-----------------------Arrange---------------------
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockPluginAction = new Mock<IPluginAction>();
            var mockComPluginSource = new Mock<ComPluginSource>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var dsfComDllActivity = new TestDsfComDllActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = mockPluginAction.Object
            };

            mockPluginAction.Setup(o => o.Method).Returns("TestMethod");
            mockResourceCatalog.Setup(o => o.GetResource<ComPluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockComPluginSource.Object);
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ExecutionImpl_Catch_Common_IsNotNull_Expect_NoError()
        {
            //-----------------------Arrange---------------------
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockPluginAction = new Mock<IPluginAction>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockServiceInput = new Mock<ICollection<IServiceInput>>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockOutputDescription = new Mock<IOutputDescription>();
            var mockPath = new Mock<IPath>();
            var mockDataSourceShape = new Mock<IDataSourceShape>();

            var comPluginSource = new ComPluginSource()
            {
                ClsId = "some ClsID"
            };
            var dsfActivity = new DsfActivity();

            Thread.CurrentPrincipal = null;
            var identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var dataListID = Guid.NewGuid();
            var environment = new ExecutionEnvironment();
            var outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping() };
            var dsfComDllActivity = new TestDsfComDllActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = mockPluginAction.Object,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = outputs,
                OutputDescription = mockOutputDescription.Object
            };

            mockDataSourceShape.Setup(o => o.Paths).Returns(new List<IPath> { mockPath.Object });
            mockPluginAction.Setup(o => o.Method).Returns("TestMethod");
            mockResourceCatalog.Setup(o => o.GetResource<ComPluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(comPluginSource);
            mockOutputDescription.Setup(o => o.DataSourceShapes).Returns(new List<IDataSourceShape> { mockDataSourceShape.Object });
            mockOutputDescription.Setup(o => o.Format).Returns(OutputFormats.ShapedXML);
            mockDSFDataObject.Setup(o => o.DataListID).Returns(dataListID);
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(comPluginSource.ClsId, dsfComDllActivity.comPluginInvokeArgs.ClsId);
            Assert.AreEqual(0, errorResult.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_GetHashCode_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var hashCode = dsfComDllActivity.GetHashCode();
            //-----------------------Assert----------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(dsfComDllActivity);
            //-----------------------Assert----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(null);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_NotSame_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(new TestDsfComDllActivity());
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ObjectEquals_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            var obj = new object();
            obj = new DsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ObjectEquals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            var obj = new object();
            obj = null;
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }
    }

    class TestDsfComDllActivity : DsfComDllActivity
    {
        public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }

        public ComPluginInvokeArgs comPluginInvokeArgs;
        protected override void ExecuteInsideImpersonatedContext(ComPluginInvokeArgs args)
        {
            comPluginInvokeArgs = args;
        }
    }

    internal class ServiceTestExecutionContainerMock : ServiceTestExecutionContainer
    {
        public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {

        }
        public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, ITestCatalog catalog, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {
            TstCatalog = catalog;
            ResourceCat = resourceCatalog;
        }
    }
}

