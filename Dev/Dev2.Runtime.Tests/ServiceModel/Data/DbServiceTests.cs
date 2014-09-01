using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DbServiceTests
    {
        static Service DeserializeService(string args)
        {
            var service = JsonConvert.DeserializeObject<Service>(args);
            switch(service.ResourceType)
            {
                case ResourceType.DbService:
                    return JsonConvert.DeserializeObject<DbService>(args);

                case ResourceType.PluginService:
                    return JsonConvert.DeserializeObject<PluginService>(args);
            }
            return service;
        }

        #region Ctor Tests

        [TestMethod]
        [TestCategory("DbService_Constructor")]
        // ReSharper disable InconsistentNaming
        public void DbService_Constructor_CorrectDbService()
        // ReSharper restore InconsistentNaming
        {
            // ReSharper disable InconsistentNaming
            const string xmlDataString = @"<Service ID=""af8d2d38-22b5-4599-8357-adce196beb83"" Name=""TravsTestService"" ResourceType=""DbService"">
  <Actions>
    <Action Name=""dbo.InsertDummyUser"" Type=""InvokeStoredProc"" SourceID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" SourceName=""testingDBSrc"" SourceMethod=""dbo.InsertDummyUser"">
      <Inputs>
        <Input Name=""fname"" Source=""fname"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""lname"" Source=""lname"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""username"" Source=""username"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""password"" Source=""password"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""lastAccessDate"" Source=""lastAccessDate"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
      </Inputs>
      <Outputs />
      <OutputDescription><![CDATA[<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:Paths /></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>]]></OutputDescription>
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>TravsTestService</DisplayName>
  <Category>WEBPART_WIZARDS</Category>
</Service>";
            // ReSharper restore InconsistentNaming
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);

            Assert.AreEqual("TravsTestService", dbService.ResourceName);
            Assert.AreEqual(ResourceType.DbService, dbService.ResourceType);
            Assert.AreEqual("af8d2d38-22b5-4599-8357-adce196beb83", dbService.ResourceID.ToString());
            Assert.AreEqual("WEBPART_WIZARDS", dbService.ResourcePath);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbService_Constructor")]
        // ReSharper disable InconsistentNaming
        public void DbService_Constructor_BlankRecordsetName_UsesMethodName()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string ExpectedName = "TestMethod";

            var dbService = DbService.Create();
            dbService.Method = new ServiceMethod { Name = ExpectedName };

            var xml = dbService.ToXml();

            //------------Execute Test---------------------------
            var actual = new DbService(xml);

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedName, actual.Recordset.Name);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbService_Create")]
        // ReSharper disable InconsistentNaming
        public void DbService_Create_ReturnsEmpty()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = DbService.Create();

            //------------Assert Results-------------------------
            Assert.IsNotNull(actual);
            Assert.AreEqual(Guid.Empty, actual.ResourceID);
            Assert.AreEqual(ResourceType.DbService, actual.ResourceType);

            Assert.IsNotNull(actual.Source);
            Assert.IsInstanceOfType(actual.Source, typeof(DbSource));
            Assert.AreEqual(Guid.Empty, actual.Source.ResourceID);
            Assert.AreEqual(ResourceType.DbSource, actual.Source.ResourceType);
        }

        #endregion

        #region ToXml Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbService_ToXml")]
        // ReSharper disable InconsistentNaming
        public void DbService_ToXml_WhenRecordSetHasBlankFields_ExpectNotPartOfOutputDescription()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var dbService = new DbService();
            var dbSource = new DbSource { ResourceName = "Source" };
            var resourceId = Guid.NewGuid();
            dbSource.ResourceID = resourceId;
            dbService.Source = dbSource;
            var serviceMethod = new ServiceMethod { Name = "Method" };
            dbService.Method = serviceMethod;
            var recordset = new Recordset { Name = "SomeRecSet" };
            var recordsetField = new RecordsetField { Alias = "SomeAlias", Name = "" };
            recordset.Fields.Add(recordsetField);
            dbService.Recordset = recordset;

            // ReSharper disable InconsistentNaming
            const string expected = @"<Service ID=""00000000-0000-0000-0000-000000000000"" Name="""" ResourceType=""DbService"" IsValid=""false"">
  <Actions>
    <Action Name=""SomeRecSet"" Type=""InvokeStoredProc"" SourceID=""{0}"" SourceName=""Source"" ExecuteAction="""" SourceMethod=""Method"">
      <Inputs />
      <Outputs />
      <OutputDescription><![CDATA[<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:_x003C_Paths_x003E_k__BackingField /></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>]]></OutputDescription>
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName></DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
</Service>";
            // ReSharper restore InconsistentNaming

            //------------Execute Test---------------------------
            var xElement = dbService.ToXml();
            //------------Assert Results-------------------------
            Assert.AreEqual(string.Format(expected, resourceId), xElement.ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void DbService_ToXml_CorrectXml()
        // ReSharper restore InconsistentNaming
        {
            // ReSharper disable InconsistentNaming
            const string xmlDataString = @"<Service ID=""af8d2d38-22b5-4599-8357-adce196beb83"" Name=""TravsTestService"" ResourceType=""DbService"" IsValid=""true"">
  <Actions>
    <Action Name=""dbo.InsertDummyUser"" Type=""InvokeStoredProc"" SourceID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" SourceName=""testingDBSrc"" ExecuteAction=""dbo.InsertDummyUser"" SourceMethod=""dbo.InsertDummyUser"">
      <Inputs>
        <Input Name=""fname"" Source=""fname"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""lname"" Source=""lname"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""username"" Source=""username"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""password"" Source=""password"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
        <Input Name=""lastAccessDate"" Source=""lastAccessDate"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Object"" />
      </Inputs>
      <Outputs />
      <OutputDescription><![CDATA[<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:_x003C_Paths_x003E_k__BackingField /></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>]]></OutputDescription>
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>TravsTestService</DisplayName>
  <Category>WEBPART_WIZARDS</Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
</Service>";
            // ReSharper restore InconsistentNaming
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);
            XElement returnedXelm = dbService.ToXml();
            string actual = returnedXelm.ToString();

            Assert.AreEqual(xmlDataString, actual);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void DbService_ToXmlWithBlankAliasInService_DoesntBreakXml()
        // ReSharper restore InconsistentNaming
        {
            var dbService = DeserializeService(@"{""resourceID"":""d9aa6906-d2fe-4ba3-89b7-9a22e4fe455a"",""resourceType"":""DbService"",""resourceName"":""IntegrationTestDBNullLogicDBWithNull"",""resourcePath"":""ML-TESTING"",""source"":{""ServerType"":""SqlDatabase"",""Server"":""RSAKLFSVRGENDEV"",""DatabaseName"":""Dev"",""Port"":1433,""AuthenticationType"":""User"",""UserID"":""testuser"",""Password"":""test123"",""ConnectionString"":""Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Dev;User ID=testuser;Password=test123;"",""ResourceID"":""9ccc121a-0279-44e5-9a67-77c25df8551a"",""ResourceType"":""DbSource"",""ResourceName"":""testsrcs"",""ResourcePath"":""SYSTEM"",""Dependencies"":null},""method"":{""Name"":""dbo.proc_get_Rows"",""SourceCode"":""CREATE procedure proc_get_Rows\r<br />&nbsp;&nbsp;&nbsp;&nbsp;(@Rows INT)\r<br />  \r<br />  AS\r<br />&nbsp;&nbsp;&nbsp;&nbsp;  \r<br />&nbsp;&nbsp;&nbsp;&nbsp;WITH cte AS (\r<br />&nbsp;&nbsp;&nbsp;&nbsp; SELECT *,\r<br />&nbsp;&nbsp;&nbsp;&nbsp;  ROW_NUMBER () OVER (ORDER BY BigID) AS rn\r<br />&nbsp;&nbsp;&nbsp;&nbsp; FROM Big)\r<br />&nbsp;&nbsp;&nbsp;&nbsp;SELECT * FROM cte \r<br />&nbsp;&nbsp;&nbsp;&nbsp;WHERE rn <= @Rows"",""Parameters"":[{""Name"":""Rows"",""EmptyToNull"":false,""IsRequired"":false,""Value"":""2"",""DefaultValue"":null,""Type"":null,""TypeName"":""""}]},""recordset"":{""Name"":""dbo_proc_get_Rows"",""Fields"":[{""Name"":""BigID"",""Alias"":""BigID"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.BigID"",""DisplayPath"":""NewDataSet().Table.BigID"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column1"",""Alias"":""Column1"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column1"",""DisplayPath"":""NewDataSet().Table.Column1"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column2"",""Alias"":""Column2"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column2"",""DisplayPath"":""NewDataSet().Table.Column2"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column3"",""Alias"":""Column3"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column3"",""DisplayPath"":""NewDataSet().Table.Column3"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column4"",""Alias"":""Column4"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column4"",""DisplayPath"":""NewDataSet().Table.Column4"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column5"",""Alias"":"""",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column5"",""DisplayPath"":""NewDataSet().Table.Column5"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column6"",""Alias"":""Column6"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column6"",""DisplayPath"":""NewDataSet().Table.Column6"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column7"",""Alias"":""Column7"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column7"",""DisplayPath"":""NewDataSet().Table.Column7"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column8"",""Alias"":""Column8"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column8"",""DisplayPath"":""NewDataSet().Table.Column8"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column9"",""Alias"":""Column9"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column9"",""DisplayPath"":""NewDataSet().Table.Column9"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column10"",""Alias"":""Column10"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column10"",""DisplayPath"":""NewDataSet().Table.Column10"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column11"",""Alias"":""Column11"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column11"",""DisplayPath"":""NewDataSet().Table.Column11"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column12"",""Alias"":""Column12"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column12"",""DisplayPath"":""NewDataSet().Table.Column12"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column13"",""Alias"":""Column13"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column13"",""DisplayPath"":""NewDataSet().Table.Column13"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column14"",""Alias"":""Column14"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column14"",""DisplayPath"":""NewDataSet().Table.Column14"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column15"",""Alias"":""Column15"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column15"",""DisplayPath"":""NewDataSet().Table.Column15"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column16"",""Alias"":""Column16"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column16"",""DisplayPath"":""NewDataSet().Table.Column16"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column17"",""Alias"":""Column17"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column17"",""DisplayPath"":""NewDataSet().Table.Column17"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column18"",""Alias"":""Column18"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column18"",""DisplayPath"":""NewDataSet().Table.Column18"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column19"",""Alias"":""Column19"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column19"",""DisplayPath"":""NewDataSet().Table.Column19"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column20"",""Alias"":""Column20"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column20"",""DisplayPath"":""NewDataSet().Table.Column20"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column21"",""Alias"":""Column21"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column21"",""DisplayPath"":""NewDataSet().Table.Column21"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column22"",""Alias"":""Column22"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column22"",""DisplayPath"":""NewDataSet().Table.Column22"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column23"",""Alias"":""Column23"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column23"",""DisplayPath"":""NewDataSet().Table.Column23"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column24"",""Alias"":""Column24"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column24"",""DisplayPath"":""NewDataSet().Table.Column24"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column25"",""Alias"":""Column25"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column25"",""DisplayPath"":""NewDataSet().Table.Column25"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column26"",""Alias"":""Column26"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column26"",""DisplayPath"":""NewDataSet().Table.Column26"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column27"",""Alias"":""Column27"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column27"",""DisplayPath"":""NewDataSet().Table.Column27"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column28"",""Alias"":""Column28"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column28"",""DisplayPath"":""NewDataSet().Table.Column28"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column29"",""Alias"":""Column29"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column29"",""DisplayPath"":""NewDataSet().Table.Column29"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column30"",""Alias"":""Column30"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column30"",""DisplayPath"":""NewDataSet().Table.Column30"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column31"",""Alias"":""Column31"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column31"",""DisplayPath"":""NewDataSet().Table.Column31"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column32"",""Alias"":""Column32"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column32"",""DisplayPath"":""NewDataSet().Table.Column32"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column33"",""Alias"":""Column33"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column33"",""DisplayPath"":""NewDataSet().Table.Column33"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column34"",""Alias"":""Column34"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column34"",""DisplayPath"":""NewDataSet().Table.Column34"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column35"",""Alias"":""Column35"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column35"",""DisplayPath"":""NewDataSet().Table.Column35"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column36"",""Alias"":""Column36"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column36"",""DisplayPath"":""NewDataSet().Table.Column36"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column37"",""Alias"":""Column37"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column37"",""DisplayPath"":""NewDataSet().Table.Column37"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column38"",""Alias"":""Column38"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column38"",""DisplayPath"":""NewDataSet().Table.Column38"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column39"",""Alias"":""Column39"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column39"",""DisplayPath"":""NewDataSet().Table.Column39"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column40"",""Alias"":""Column40"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column40"",""DisplayPath"":""NewDataSet().Table.Column40"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column41"",""Alias"":""Column41"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column41"",""DisplayPath"":""NewDataSet().Table.Column41"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column42"",""Alias"":""Column42"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column42"",""DisplayPath"":""NewDataSet().Table.Column42"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column43"",""Alias"":""Column43"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column43"",""DisplayPath"":""NewDataSet().Table.Column43"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column44"",""Alias"":""Column44"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column44"",""DisplayPath"":""NewDataSet().Table.Column44"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column45"",""Alias"":""Column45"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column45"",""DisplayPath"":""NewDataSet().Table.Column45"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column46"",""Alias"":""Column46"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column46"",""DisplayPath"":""NewDataSet().Table.Column46"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column47"",""Alias"":""Column47"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column47"",""DisplayPath"":""NewDataSet().Table.Column47"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column48"",""Alias"":""Column48"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column48"",""DisplayPath"":""NewDataSet().Table.Column48"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column49"",""Alias"":""Column49"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column49"",""DisplayPath"":""NewDataSet().Table.Column49"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column50"",""Alias"":""Column50"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column50"",""DisplayPath"":""NewDataSet().Table.Column50"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column51"",""Alias"":""Column51"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column51"",""DisplayPath"":""NewDataSet().Table.Column51"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column52"",""Alias"":""Column52"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column52"",""DisplayPath"":""NewDataSet().Table.Column52"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column53"",""Alias"":""Column53"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column53"",""DisplayPath"":""NewDataSet().Table.Column53"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column54"",""Alias"":""Column54"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column54"",""DisplayPath"":""NewDataSet().Table.Column54"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column55"",""Alias"":""Column55"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column55"",""DisplayPath"":""NewDataSet().Table.Column55"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column56"",""Alias"":""Column56"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column56"",""DisplayPath"":""NewDataSet().Table.Column56"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column57"",""Alias"":""Column57"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column57"",""DisplayPath"":""NewDataSet().Table.Column57"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column58"",""Alias"":""Column58"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column58"",""DisplayPath"":""NewDataSet().Table.Column58"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column59"",""Alias"":""Column59"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column59"",""DisplayPath"":""NewDataSet().Table.Column59"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column60"",""Alias"":""Column60"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column60"",""DisplayPath"":""NewDataSet().Table.Column60"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column61"",""Alias"":""Column61"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column61"",""DisplayPath"":""NewDataSet().Table.Column61"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column62"",""Alias"":""Column62"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column62"",""DisplayPath"":""NewDataSet().Table.Column62"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column63"",""Alias"":""Column63"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column63"",""DisplayPath"":""NewDataSet().Table.Column63"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column64"",""Alias"":""Column64"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column64"",""DisplayPath"":""NewDataSet().Table.Column64"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column65"",""Alias"":""Column65"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column65"",""DisplayPath"":""NewDataSet().Table.Column65"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column66"",""Alias"":""Column66"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column66"",""DisplayPath"":""NewDataSet().Table.Column66"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column67"",""Alias"":""Column67"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column67"",""DisplayPath"":""NewDataSet().Table.Column67"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column68"",""Alias"":""Column68"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column68"",""DisplayPath"":""NewDataSet().Table.Column68"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column69"",""Alias"":""Column69"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column69"",""DisplayPath"":""NewDataSet().Table.Column69"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column70"",""Alias"":""Column70"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column70"",""DisplayPath"":""NewDataSet().Table.Column70"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column71"",""Alias"":""Column71"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column71"",""DisplayPath"":""NewDataSet().Table.Column71"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column72"",""Alias"":""Column72"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column72"",""DisplayPath"":""NewDataSet().Table.Column72"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column73"",""Alias"":""Column73"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column73"",""DisplayPath"":""NewDataSet().Table.Column73"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column74"",""Alias"":""Column74"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column74"",""DisplayPath"":""NewDataSet().Table.Column74"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column75"",""Alias"":""Column75"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column75"",""DisplayPath"":""NewDataSet().Table.Column75"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column76"",""Alias"":""Column76"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column76"",""DisplayPath"":""NewDataSet().Table.Column76"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column77"",""Alias"":""Column77"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column77"",""DisplayPath"":""NewDataSet().Table.Column77"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column78"",""Alias"":""Column78"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column78"",""DisplayPath"":""NewDataSet().Table.Column78"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column79"",""Alias"":""Column79"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column79"",""DisplayPath"":""NewDataSet().Table.Column79"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column80"",""Alias"":""Column80"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column80"",""DisplayPath"":""NewDataSet().Table.Column80"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column81"",""Alias"":""Column81"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column81"",""DisplayPath"":""NewDataSet().Table.Column81"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column82"",""Alias"":""Column82"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column82"",""DisplayPath"":""NewDataSet().Table.Column82"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column83"",""Alias"":""Column83"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column83"",""DisplayPath"":""NewDataSet().Table.Column83"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column84"",""Alias"":""Column84"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column84"",""DisplayPath"":""NewDataSet().Table.Column84"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column85"",""Alias"":""Column85"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column85"",""DisplayPath"":""NewDataSet().Table.Column85"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column86"",""Alias"":""Column86"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column86"",""DisplayPath"":""NewDataSet().Table.Column86"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column87"",""Alias"":""Column87"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column87"",""DisplayPath"":""NewDataSet().Table.Column87"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column88"",""Alias"":""Column88"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column88"",""DisplayPath"":""NewDataSet().Table.Column88"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column89"",""Alias"":""Column89"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column89"",""DisplayPath"":""NewDataSet().Table.Column89"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column90"",""Alias"":""Column90"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column90"",""DisplayPath"":""NewDataSet().Table.Column90"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column91"",""Alias"":""Column91"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column91"",""DisplayPath"":""NewDataSet().Table.Column91"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column92"",""Alias"":""Column92"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column92"",""DisplayPath"":""NewDataSet().Table.Column92"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column93"",""Alias"":""Column93"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column93"",""DisplayPath"":""NewDataSet().Table.Column93"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column94"",""Alias"":""Column94"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column94"",""DisplayPath"":""NewDataSet().Table.Column94"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column95"",""Alias"":""Column95"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column95"",""DisplayPath"":""NewDataSet().Table.Column95"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column96"",""Alias"":""Column96"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column96"",""DisplayPath"":""NewDataSet().Table.Column96"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column97"",""Alias"":""Column97"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column97"",""DisplayPath"":""NewDataSet().Table.Column97"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column98"",""Alias"":""Column98"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column98"",""DisplayPath"":""NewDataSet().Table.Column98"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column99"",""Alias"":""Column99"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column99"",""DisplayPath"":""NewDataSet().Table.Column99"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""Column100"",""Alias"":""Column100"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.Column100"",""DisplayPath"":""NewDataSet().Table.Column100"",""SampleData"":""1,2"",""OutputExpression"":""""}},{""Name"":""rn"",""Alias"":""rn"",""Path"":{""$type"":""Unlimited.Framework.Converters.Graph.String.Xml.XmlPath, Dev2.Core"",""ActualPath"":""NewDataSet().Table.rn"",""DisplayPath"":""NewDataSet().Table.rn"",""SampleData"":""1,2"",""OutputExpression"":""""}}],""Records"":[{""Label"":""dbo_proc_get_Rows(1)"",""Name"":""dbo_proc_get_Rows"",""Count"":10,""Cells"":[{""Name"":""dbo_proc_get_Rows(1).BigID"",""Label"":""BigID"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column1"",""Label"":""Column1"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column2"",""Label"":""Column2"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column3"",""Label"":""Column3"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column4"",""Label"":""Column4"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column5"",""Label"":""Column5"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column6"",""Label"":""Column6"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column7"",""Label"":""Column7"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column8"",""Label"":""Column8"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column9"",""Label"":""Column9"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).Column10"",""Label"":""Column10"",""Value"":""1""},{""Name"":""dbo_proc_get_Rows(1).rn"",""Label"":""rn"",""Value"":""1""}]},{""Label"":""dbo_proc_get_Rows(2)"",""Name"":""dbo_proc_get_Rows"",""Count"":10,""Cells"":[{""Name"":""dbo_proc_get_Rows(2).Column1"",""Label"":""Column1"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column2"",""Label"":""Column2"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column3"",""Label"":""Column3"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column4"",""Label"":""Column4"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column5"",""Label"":""Column5"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column6"",""Label"":""Column6"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column7"",""Label"":""Column7"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column8"",""Label"":""Column8"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column9"",""Label"":""Column9"",""Value"":""2""},{""Name"":""dbo_proc_get_Rows(2).Column10"",""Label"":""Column10"",""Value"":""2""}]}],""HasErrors"":false,""ErrorMessage"":""""}}");
            XElement returnedXelm = dbService.ToXml();
            string actual = returnedXelm.ToString();
            // ReSharper disable ConvertToConstant.Local
            string expected = @"<Output OriginalName=""Column1"" Name=""Column1"" MapsTo=""Column1"" Value=""[[Column1]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column2"" Name=""Column2"" MapsTo=""Column2"" Value=""[[Column2]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column3"" Name=""Column3"" MapsTo=""Column3"" Value=""[[Column3]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column4"" Name=""Column4"" MapsTo=""Column4"" Value=""[[Column4]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column5"" Name="""" MapsTo="""" Value="""" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column6"" Name=""Column6"" MapsTo=""Column6"" Value=""[[Column6]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column7"" Name=""Column7"" MapsTo=""Column7"" Value=""[[Column7]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column8"" Name=""Column8"" MapsTo=""Column8"" Value=""[[Column8]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column9"" Name=""Column9"" MapsTo=""Column9"" Value=""[[Column9]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />
        <Output OriginalName=""Column10"" Name=""Column10"" MapsTo=""Column10"" Value=""[[Column10]]"" RecordsetName=""dbo_proc_get_Rows"" RecordsetAlias="""" Recordset="""" />";
            // ReSharper restore ConvertToConstant.Local
            Assert.IsTrue(actual.Contains(expected));
        }

        #endregion
    }
}
