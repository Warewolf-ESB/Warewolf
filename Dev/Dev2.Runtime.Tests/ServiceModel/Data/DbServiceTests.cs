/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DbServiceTests
    {
        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
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
</Service>";
            // ReSharper restore InconsistentNaming
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);

            Assert.AreEqual("TravsTestService", dbService.ResourceName);
            Assert.AreEqual("DbService", dbService.ResourceType);
            Assert.AreEqual("af8d2d38-22b5-4599-8357-adce196beb83", dbService.ResourceID.ToString());
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
            Assert.AreEqual("DbService", actual.ResourceType);

            Assert.IsNotNull(actual.Source);
            Assert.IsInstanceOfType(actual.Source, typeof(DbSource));
            Assert.AreEqual(Guid.Empty, actual.Source.ResourceID);
            Assert.AreEqual("DbSource", actual.Source.ResourceType);
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
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
</Service>";
            // ReSharper restore InconsistentNaming

            //------------Execute Test---------------------------
            var xElement = dbService.ToXml();
            //------------Assert Results-------------------------
            string expectedResult = string.Format(expected, resourceId);
            var actual = xElement.ToString();
            FixBreaks(ref expectedResult, ref actual);
            Assert.AreEqual(expectedResult, actual);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void DbService_ToXml_CorrectXml()
        // ReSharper restore InconsistentNaming
        {
            // ReSharper disable InconsistentNaming
            string xmlDataString = @"<Service ID=""af8d2d38-22b5-4599-8357-adce196beb83"" Name=""TravsTestService"" ResourceType=""DbService"" IsValid=""true"">
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
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
</Service>";
            // ReSharper restore InconsistentNaming
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);
            XElement returnedXelm = dbService.ToXml();
            string actual = returnedXelm.ToString();
            FixBreaks(ref xmlDataString, ref actual);
            Assert.AreEqual(xmlDataString, actual);
        }



        #endregion
    }
}
