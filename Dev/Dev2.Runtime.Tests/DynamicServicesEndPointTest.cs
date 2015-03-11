
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Tests.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;

namespace Dev2.Tests.Runtime
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DynamicServicesEndPointTest
    {
        const int VersionNo = 9999;

        const string ServiceName = "Mo\\TestForEachOutput";
        readonly Guid _serviceId = Guid.NewGuid();

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

        const string ServiceShapeWithEntireRs = @"<DataList>
  <recset Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
    <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <f2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  </recset>
</DataList>";

        const string ServiceShapeWithSingleColumn = @"<DataList>
  <recset Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
    <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <f2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  </recset>
</DataList>";

        Guid _workspaceId;

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            _workspaceId = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceId, VersionNo.ToString(CultureInfo.InvariantCulture), true, false,
               null,
               new[] { "TestForEachOutput" },
               out resources,
               null,
               new[] { _serviceId });

            ResourceCatalog.Instance.LoadWorkspace(_workspaceId);

        }
        #endregion


        #region View In Browser Ouput Format Tests

        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForAllInputRegions()
        {
            IDataListCompiler comp = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlId = comp.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, ServiceShape.ToStringBuilder(), out errors);

            IDSFDataObject dataObj = new DsfDataObject(string.Empty, dlId) { WorkspaceID = _workspaceId, DataListID = dlId, ServiceName = ServiceName };
            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();
            string result = endPoint.FetchExecutionPayload(dataObj, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), out errors);

            DeleteDataList(dlId);

            Assert.IsTrue(result.IndexOf("<inputScalar", StringComparison.Ordinal) < 0, "Output format contains additional tag, <inputScalar>");
            Assert.IsTrue(result.IndexOf("<noneScalar", StringComparison.Ordinal) < 0, "Output format contains additional tag, <noneScalar>");
            Assert.IsTrue(result.IndexOf("<f1", StringComparison.Ordinal) < 0, "Output format contains additional tag, <recset><f1/></recset>");
            Assert.IsTrue(result.IndexOf("<f4", StringComparison.Ordinal) < 0, "Output format contains additional tag, <recset><f4/></recset>");
            Assert.IsTrue(result.IndexOf("<newrecset", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset></newrecset>");
            Assert.IsTrue(result.IndexOf("<field1", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset><f1/></newrecset>");
            Assert.IsTrue(result.IndexOf("<field2", StringComparison.Ordinal) < 0, "Output format contains additional tag, <newrecset><f1/></newrecset>");

        }

        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForOneEntireRecordsetOutputRegion()
        {
            // This test the core of the output shaping based upon IODirection ;)

            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();

            var result = endPoint.ManipulateDataListShapeForOutput(ServiceShapeWithEntireRs);

            Assert.IsTrue(result.IndexOf("<recset", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset></recset>");
            Assert.IsTrue(result.IndexOf("<f1", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset><f1/></recset>");
            Assert.IsTrue(result.IndexOf("<f2", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset><f2/></recset>");

        }

        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForOneColumnInARecordset()
        {
            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();

            var result = endPoint.ManipulateDataListShapeForOutput(ServiceShapeWithSingleColumn);

            Assert.IsTrue(result.IndexOf("<recset", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset></recset>");
            Assert.IsTrue(result.IndexOf("<f2", StringComparison.Ordinal) > 0, "Output format missing required tag of <recset><f2/></recset>");

        }


        [TestMethod]
        public void CheckOutputFormatOfDataListForViewInBrowserForOneRecordsetOutputRegion()
        {
            IDataListCompiler comp = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlId = comp.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), ServiceShape.ToStringBuilder(), out errors);
            var resource = ResourceCatalog.Instance.GetResource(_workspaceId, ServiceName);
            IDSFDataObject dataObj = new DsfDataObject(string.Empty, dlId) { WorkspaceID = _workspaceId, DataListID = dlId, ServiceName = ServiceName, ResourceID = resource.ResourceID };
            EsbServicesEndpoint endPoint = new EsbServicesEndpoint();
            string result = endPoint.FetchExecutionPayload(dataObj, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), out errors);

            DeleteDataList(dlId);

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
