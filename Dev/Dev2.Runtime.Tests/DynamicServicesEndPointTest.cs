using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Common.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    public class DynamicServicesEndPointTest
    {
        //public static IDataListCompiler Compiler = DataListFactory.CreateDataListCompiler(); 

        const int VersionNo = 9999;

        const string ServiceName = "TestForEachOutput";

        const string ServiceShape = @"<DataList>
  <inputScalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  <outputScalar Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  <bothScalar Description="""" IsEditable=""True"" ColumnIODirection=""Both"" />
  <noneScalar Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
  <recset Description="""" IsEditable=""True"" ColumnIODirection=""None"">
    <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    <f2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <f3 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" />
    <f4 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
  </recset>
  <newrecset Description="""" IsEditable=""True"" ColumnIODirection=""None"">
    <field1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    <field2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  </newrecset>
</DataList>";

        static string _workspacesDir;

        static Guid _workspaceID;

        #region ClassInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _workspacesDir = Path.Combine(testContext.TestDir, "Workspaces");
            Directory.SetCurrentDirectory(testContext.TestDir);

            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(), false, false,
               null,
               new[] { ServiceName },
               out resources);

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
        }

        #endregion

        #region TestInitialize/Cleanup

        static readonly object TestLock = new object();

        [TestInitialize]
        public void TestInitialize()
        {
            Monitor.Enter(TestLock);

            
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Monitor.Exit(TestLock);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DirectoryHelper.CleanUp(_workspacesDir);
            
        }

        #endregion


        #region View In Browser Ouput Format Tests

        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForAllInputRegions()
        {
            IDataListCompiler comp = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = comp.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, ServiceShape, out errors);

            IDSFDataObject dataObj = new DsfDataObject(string.Empty, dlID) { WorkspaceID = _workspaceID, DataListID = dlID, ServiceName = ServiceName };
            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();
            string result = endPoint.FetchExecutionPayload(dataObj, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), out errors);

            DeleteDataList(dlID);

            Assert.IsTrue(result.IndexOf("<inputScalar", StringComparison.Ordinal) < 0, "Output format contains additional tag, <inputScalar>");
            Assert.IsTrue(result.IndexOf("<noneScalar", StringComparison.Ordinal) < 0, "Output format contains additional tag, <noneScalar>");
            Assert.IsTrue(result.IndexOf("<f1", StringComparison.Ordinal) < 0, "Output format contains additional tag, <recset><f1/></recset>");
            Assert.IsTrue(result.IndexOf("<f4", StringComparison.Ordinal) < 0, "Output format contains additional tag, <recset><f4/></recset>");
            Assert.IsTrue(result.IndexOf("<newrecset", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset></newrecset>");
            Assert.IsTrue(result.IndexOf("<field1", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset><f1/></newrecset>");
            Assert.IsTrue(result.IndexOf("<field2", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset><f1/></newrecset>");
            
        }       

        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForOneRecordsetOutputRegion()
        {
            IDataListCompiler comp = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = comp.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, ServiceShape, out errors);

            IDSFDataObject dataObj = new DsfDataObject(string.Empty, dlID) { WorkspaceID = _workspaceID, DataListID = dlID, ServiceName = ServiceName };
            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();
            string result = endPoint.FetchExecutionPayload(dataObj, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), out errors);

            DeleteDataList(dlID);

            Assert.IsTrue(result.IndexOf("<outputScalar", StringComparison.Ordinal) > 0, "Output format missing required tag of <outputScalar>");
            Assert.IsTrue(result.IndexOf("<bothScalar", StringComparison.Ordinal) > 0, "Output format missing required tag of <bothScalar");
            Assert.IsTrue(result.IndexOf("<recset", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset></recset>");
            Assert.IsTrue(result.IndexOf("<f2", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset><f2/></recset>");
            Assert.IsTrue(result.IndexOf("<f3", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset><f3/></recset>");

        }

        #endregion 


        void DeleteDataList(Guid resultGuid)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            compiler.ForceDeleteDataListByID(resultGuid);
        }        
    }
}