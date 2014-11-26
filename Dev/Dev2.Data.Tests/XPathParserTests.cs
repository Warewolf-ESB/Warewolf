
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Dev2.Data.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XPathParserTests
    {
        readonly XPathParser _xPathParser = new XPathParser();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        const string XMLDocument = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" +
                                   "<dotfuscator version=\"2.3\">\n" +
                                   "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>" +
                                   "</dotfuscator>";

        const string XMLData = "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>";

        [TestMethod]
        public void ExecutePathWhereGivenXMLDocumentExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLDocument, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_Parser_WhenXPathingAttributes_ExpectValidDataWithoutAttributeName()
        {
            //------------Setup for test--------------------------

            #region data

            var data = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestRun id=""069cce00-4b2c-425e-a343-d784dd187adf"" name=""RSAKLFASHLEY$@RSAKLFASHLEY 2013-11-01 13:43:52"" runUser=""NT AUTHORITY\NETWORK SERVICE"">
  <TestSettings name=""Unit Test with Coverage"" id=""3264dd0f-6fc1-4cb9-b44f-c649fef29605"">
    <Description>These are default test settings for a local test run.</Description>
    <Deployment userDeploymentRoot=""D:\Builds\ReleaseGate\TestResults"" useDefaultDeploymentRoot=""false"" runDeploymentRoot=""RSAKLFASHLEY$_RSAKLFASHLEY 2013-11-01 13_43_52"">
      <DeploymentItem filename=""ConsoleAppToTestExecuteCommandLineActivity\bin\Debug\ConsoleAppToTestExecuteCommandLineActivity.exe"" />
      <DeploymentItem filename=""Binaries\IronPython.Modules.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Scripting.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Dynamic.dll"" />
      <DeploymentItem filename=""Binaries\IronPython.dll"" />
      <DeploymentItem filename=""Binaries\CefSharp\"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.Yaml.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.dll"" />
    </Deployment>
    <NamingScheme baseName=""UT"" />
    <Execution>
      <Timeouts runTimeout=""1800000"" testTimeout=""120000"" />
      <TestTypeSpecific>
        <UnitTestRunConfig testTypeId=""13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"">
          <AssemblyResolution>
            <TestDirectory useLoadContext=""true"" />
          </AssemblyResolution>
        </UnitTestRunConfig>
        <WebTestRunConfiguration testTypeId=""4e7599fa-5ecb-43e9-a887-cd63cf72d207"">
          <Browser name=""Internet Explorer 9.0"" MaxConnections=""6"">
            <Headers>
              <Header name=""User-Agent"" value=""Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"" />
              <Header name=""Accept"" value=""*/*"" />
              <Header name=""Accept-Language"" value=""{{$IEAcceptLanguage}}"" />
              <Header name=""Accept-Encoding"" value=""GZIP"" />
            </Headers>
          </Browser>
        </WebTestRunConfiguration>
      </TestTypeSpecific>
      <AgentRule name=""LocalMachineDefaultRole"">
        <DataCollectors>
          <DataCollector uri=""datacollector://microsoft/CodeCoverage/1.0"" assemblyQualifiedName=""Microsoft.VisualStudio.TestTools.CodeCoverage.CoveragePlugIn, Microsoft.VisualStudio.QualityTools.Plugins.CodeCoverage, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" friendlyName=""Code Coverage (Visual Studio 2010)"">
          </DataCollector>
        </DataCollectors>
      </AgentRule>
    </Execution>
  </TestSettings>
  <Times creation=""2013-11-01T13:43:52.8654497+02:00"" queuing=""2013-11-01T13:43:55.0394497+02:00"" start=""2013-11-01T13:43:55.5834497+02:00"" finish=""2013-11-01T13:47:05.6964497+02:00"" />
  <ResultSummary outcome=""Failed"">
    <Counters total=""4241"" executed=""4241"" passed=""4236"" error=""0"" failed=""3"" timeout=""0"" aborted=""0"" inconclusive=""2"" passedButRunAborted=""0"" notRunnable=""0"" notExecuted=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" pending=""0"" />
  </ResultSummary>
  <TestDefinitions>
    <UnitTest name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" storage=""d:\builds\releasegate\binaries\dev2.activities.designers.tests.dll"" id=""6142bab1-509e-a426-59b7-e5d8bc6d62cf"">
      <Owners>
        <Owner name=""Tshepo Ntlhokoa"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand"" />
      </TestCategory>
      <Execution id=""46ecc213-c05f-4b20-9407-0b99ea2cfd18"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Designers.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Activities.Designers.Tests.Designers2.Core.ActivityCollectionDesignerViewModelTests, Dev2.Activities.Designers.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" />
    </UnitTest>
    <UnitTest name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" storage=""d:\builds\releasegate\binaries\dev2.runtime.tests.dll"" id=""9fd0ecee-f50c-e066-0468-ce2b55bda4ab"">
      <Execution id=""98e1e23c-f12a-4415-944b-e6deb57ce44f"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Runtime.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Runtime.ServiceModel.ResourcesTests, Dev2.Runtime.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" />
    </UnitTest>
    <UnitTest name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""7e7e630d-fafe-de84-423b-98111f0c4f10"">
      <Execution id=""dfc91477-fc60-4272-9528-87c83c36ea9e"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.DataSplitActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" />
    </UnitTest>
    <UnitTest name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""edea71e2-ab69-da37-c337-d40ffd7134a1"">
      <Description>When a string is white space, expect false when istrueisempty equals false</Description>
      <Owners>
        <Owner name=""Jurie Smit"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""EmptyStringToBoolConverter"" />
      </TestCategory>
      <Execution id=""7717e8d0-6284-4e03-b460-12414be89064"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.ConverterTests.EmptyStringToBoolConverterTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" />
    </UnitTest>
    <UnitTest name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""ab77f188-f223-34cf-e658-5de592dab87e"">
      <Execution id=""a4180f20-d3bc-414f-8821-eb11e4e705e2"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.RecordsetSearch.RsOpTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" />
    </UnitTest>
    <UnitTest name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" storage=""d:\builds\releasegate\binaries\dev2.data.tests.dll"" id=""ebaa4ad7-7566-977f-f72c-5d9012d134c0"">
      <Execution id=""c82d7c97-24fe-4e4e-abb6-d48adbd36060"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Data.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Data.Tests.SystemTemplates.DecisionTest, Dev2.Data.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" />
    </UnitTest>
    <UnitTest name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""40e1ba7a-d657-55ba-f6db-bfac68b345da"">
      <Description>TcpConnection Connect must disconnect when connection to server fails.</Description>
      <Owners>
        <Owner name=""Trevor Williams-Ros"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""TcpConnection_Connect"" />
      </TestCategory>
      <Execution id=""3e6f82bf-5fb8-4293-8d09-0542747e3c75"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.Network.TcpConnectionTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" />
    </UnitTest>
    <UnitTest name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""cf841a6b-14c1-1575-edba-45b97f0027ad"">
      <Execution id=""368c921f-24c9-4120-9e59-1313ed0c8675"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.IntellisenseProviderTest, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" />
    </UnitTest>
    <UnitTest name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""18db7495-8579-e9df-3f65-223579568fd5"">
      <Execution id=""e819c819-7d63-4e7c-a2f1-97464c7134b8"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.ForEachActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" />
    </UnitTest>
    <UnitTest name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""3a95af49-978b-ffba-8a29-cb7ad2cc403f"">
      <Execution id=""523e1acc-3e26-4100-8e0d-98c8c0705347"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.JsonTest.JsonPathSegmentTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" />
    </UnitTest>
    <UnitTest name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""ae44269d-d5bb-bd9f-52dd-eaec4fbb93d9"">
      <Execution id=""310cc625-6b0f-47d6-90b5-376b97da50e5"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.SortRecordsTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" />
    </UnitTest>
</TestDefinitions>
</TestRun>";
            #endregion

            const string xPath = "//UnitTest/@name";
            
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(data, xPath);
            //------------Assert Results-------------------------
            var dataList = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(11, dataList.Count());
            Assert.AreEqual("ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse", dataList[0]);
            Assert.AreEqual("SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes", dataList[10]);
        }

 
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_Parser_WhenXPathingElements_ExpectValidData()
        {
            //------------Setup for test--------------------------

            #region data

            var data = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestRun id=""069cce00-4b2c-425e-a343-d784dd187adf"" name=""RSAKLFASHLEY$@RSAKLFASHLEY 2013-11-01 13:43:52"" runUser=""NT AUTHORITY\NETWORK SERVICE"">
  <TestSettings name=""Unit Test with Coverage"" id=""3264dd0f-6fc1-4cb9-b44f-c649fef29605"">
    <Description>These are default test settings for a local test run.</Description>
    <Deployment userDeploymentRoot=""D:\Builds\ReleaseGate\TestResults"" useDefaultDeploymentRoot=""false"" runDeploymentRoot=""RSAKLFASHLEY$_RSAKLFASHLEY 2013-11-01 13_43_52"">
      <DeploymentItem filename=""ConsoleAppToTestExecuteCommandLineActivity\bin\Debug\ConsoleAppToTestExecuteCommandLineActivity.exe"" />
      <DeploymentItem filename=""Binaries\IronPython.Modules.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Scripting.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Dynamic.dll"" />
      <DeploymentItem filename=""Binaries\IronPython.dll"" />
      <DeploymentItem filename=""Binaries\CefSharp\"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.Yaml.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.dll"" />
    </Deployment>
    <NamingScheme baseName=""UT"" />
    <Execution>
      <Timeouts runTimeout=""1800000"" testTimeout=""120000"" />
      <TestTypeSpecific>
        <UnitTestRunConfig testTypeId=""13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"">
          <AssemblyResolution>
            <TestDirectory useLoadContext=""true"" />
          </AssemblyResolution>
        </UnitTestRunConfig>
        <WebTestRunConfiguration testTypeId=""4e7599fa-5ecb-43e9-a887-cd63cf72d207"">
          <Browser name=""Internet Explorer 9.0"" MaxConnections=""6"">
            <Headers>
              <Header name=""User-Agent"" value=""Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"" />
              <Header name=""Accept"" value=""*/*"" />
              <Header name=""Accept-Language"" value=""{{$IEAcceptLanguage}}"" />
              <Header name=""Accept-Encoding"" value=""GZIP"" />
            </Headers>
          </Browser>
        </WebTestRunConfiguration>
      </TestTypeSpecific>
      <AgentRule name=""LocalMachineDefaultRole"">
        <DataCollectors>
          <DataCollector uri=""datacollector://microsoft/CodeCoverage/1.0"" assemblyQualifiedName=""Microsoft.VisualStudio.TestTools.CodeCoverage.CoveragePlugIn, Microsoft.VisualStudio.QualityTools.Plugins.CodeCoverage, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" friendlyName=""Code Coverage (Visual Studio 2010)"">
          </DataCollector>
        </DataCollectors>
      </AgentRule>
    </Execution>
  </TestSettings>
  <Times creation=""2013-11-01T13:43:52.8654497+02:00"" queuing=""2013-11-01T13:43:55.0394497+02:00"" start=""2013-11-01T13:43:55.5834497+02:00"" finish=""2013-11-01T13:47:05.6964497+02:00"" />
  <ResultSummary outcome=""Failed"">
    <Counters total=""4241"" executed=""4241"" passed=""4236"" error=""0"" failed=""3"" timeout=""0"" aborted=""0"" inconclusive=""2"" passedButRunAborted=""0"" notRunnable=""0"" notExecuted=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" pending=""0"" />
  </ResultSummary>
  <TestDefinitions>
    <UnitTest name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" storage=""d:\builds\releasegate\binaries\dev2.activities.designers.tests.dll"" id=""6142bab1-509e-a426-59b7-e5d8bc6d62cf"">
      <Owners>
        <Owner name=""Tshepo Ntlhokoa"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand"" />
      </TestCategory>
      <Execution id=""46ecc213-c05f-4b20-9407-0b99ea2cfd18"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Designers.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Activities.Designers.Tests.Designers2.Core.ActivityCollectionDesignerViewModelTests, Dev2.Activities.Designers.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" />
    </UnitTest>
    <UnitTest name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" storage=""d:\builds\releasegate\binaries\dev2.runtime.tests.dll"" id=""9fd0ecee-f50c-e066-0468-ce2b55bda4ab"">
      <Execution id=""98e1e23c-f12a-4415-944b-e6deb57ce44f"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Runtime.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Runtime.ServiceModel.ResourcesTests, Dev2.Runtime.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" />
    </UnitTest>
    <UnitTest name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""7e7e630d-fafe-de84-423b-98111f0c4f10"">
      <Execution id=""dfc91477-fc60-4272-9528-87c83c36ea9e"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.DataSplitActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" />
    </UnitTest>
    <UnitTest name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""edea71e2-ab69-da37-c337-d40ffd7134a1"">
      <Description>When a string is white space, expect false when istrueisempty equals false</Description>
      <Owners>
        <Owner name=""Jurie Smit"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""EmptyStringToBoolConverter"" />
      </TestCategory>
      <Execution id=""7717e8d0-6284-4e03-b460-12414be89064"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.ConverterTests.EmptyStringToBoolConverterTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" />
    </UnitTest>
    <UnitTest name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""ab77f188-f223-34cf-e658-5de592dab87e"">
      <Execution id=""a4180f20-d3bc-414f-8821-eb11e4e705e2"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.RecordsetSearch.RsOpTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" />
    </UnitTest>
    <UnitTest name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" storage=""d:\builds\releasegate\binaries\dev2.data.tests.dll"" id=""ebaa4ad7-7566-977f-f72c-5d9012d134c0"">
      <Execution id=""c82d7c97-24fe-4e4e-abb6-d48adbd36060"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Data.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Data.Tests.SystemTemplates.DecisionTest, Dev2.Data.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" />
    </UnitTest>
    <UnitTest name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""40e1ba7a-d657-55ba-f6db-bfac68b345da"">
      <Description>TcpConnection Connect must disconnect when connection to server fails.</Description>
      <Owners>
        <Owner name=""Trevor Williams-Ros"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""TcpConnection_Connect"" />
      </TestCategory>
      <Execution id=""3e6f82bf-5fb8-4293-8d09-0542747e3c75"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.Network.TcpConnectionTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" />
    </UnitTest>
    <UnitTest name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""cf841a6b-14c1-1575-edba-45b97f0027ad"">
      <Execution id=""368c921f-24c9-4120-9e59-1313ed0c8675"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.IntellisenseProviderTest, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" />
    </UnitTest>
    <UnitTest name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""18db7495-8579-e9df-3f65-223579568fd5"">
      <Execution id=""e819c819-7d63-4e7c-a2f1-97464c7134b8"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.ForEachActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" />
    </UnitTest>
    <UnitTest name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""3a95af49-978b-ffba-8a29-cb7ad2cc403f"">
      <Execution id=""523e1acc-3e26-4100-8e0d-98c8c0705347"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.JsonTest.JsonPathSegmentTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" />
    </UnitTest>
    <UnitTest name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""ae44269d-d5bb-bd9f-52dd-eaec4fbb93d9"">
      <Execution id=""310cc625-6b0f-47d6-90b5-376b97da50e5"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.SortRecordsTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" />
    </UnitTest>
</TestDefinitions>
</TestRun>";
            #endregion

            const string xPath = "TestRun/TestSettings/Deployment";

            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(data, xPath);
            //------------Assert Results-------------------------
            var expected = @"<Deployment userDeploymentRoot=""D:\Builds\ReleaseGate\TestResults"" useDefaultDeploymentRoot=""false"" runDeploymentRoot=""RSAKLFASHLEY$_RSAKLFASHLEY 2013-11-01 13_43_52"">
  <DeploymentItem filename=""ConsoleAppToTestExecuteCommandLineActivity\bin\Debug\ConsoleAppToTestExecuteCommandLineActivity.exe"" />
  <DeploymentItem filename=""Binaries\IronPython.Modules.dll"" />
  <DeploymentItem filename=""Binaries\Microsoft.Scripting.dll"" />
  <DeploymentItem filename=""Binaries\IronRuby.Libraries.dll"" />
  <DeploymentItem filename=""Binaries\Microsoft.Dynamic.dll"" />
  <DeploymentItem filename=""Binaries\IronPython.dll"" />
  <DeploymentItem filename=""Binaries\CefSharp\"" />
  <DeploymentItem filename=""Binaries\IronRuby.Libraries.Yaml.dll"" />
  <DeploymentItem filename=""Binaries\IronRuby.dll"" />
</Deployment>";

            var dataList = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(1, dataList.Count());
            Assert.AreEqual(expected, dataList[0]);
           
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_Parser_WhenXPathingElementsWithAttributes_ExpectValidData()
        {
            //------------Setup for test--------------------------

            #region data

            var data = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestRun id=""069cce00-4b2c-425e-a343-d784dd187adf"" name=""RSAKLFASHLEY$@RSAKLFASHLEY 2013-11-01 13:43:52"" runUser=""NT AUTHORITY\NETWORK SERVICE"">
  <TestSettings name=""Unit Test with Coverage"" id=""3264dd0f-6fc1-4cb9-b44f-c649fef29605"">
    <Description>These are default test settings for a local test run.</Description>
    <Deployment userDeploymentRoot=""D:\Builds\ReleaseGate\TestResults"" useDefaultDeploymentRoot=""false"" runDeploymentRoot=""RSAKLFASHLEY$_RSAKLFASHLEY 2013-11-01 13_43_52"">
      <DeploymentItem filename=""ConsoleAppToTestExecuteCommandLineActivity\bin\Debug\ConsoleAppToTestExecuteCommandLineActivity.exe"" />
      <DeploymentItem filename=""Binaries\IronPython.Modules.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Scripting.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.dll"" />
      <DeploymentItem filename=""Binaries\Microsoft.Dynamic.dll"" />
      <DeploymentItem filename=""Binaries\IronPython.dll"" />
      <DeploymentItem filename=""Binaries\CefSharp\"" />
      <DeploymentItem filename=""Binaries\IronRuby.Libraries.Yaml.dll"" />
      <DeploymentItem filename=""Binaries\IronRuby.dll"" />
    </Deployment>
    <NamingScheme baseName=""UT"" />
    <Execution>
      <Timeouts runTimeout=""1800000"" testTimeout=""120000"" />
      <TestTypeSpecific>
        <UnitTestRunConfig testTypeId=""13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"">
          <AssemblyResolution>
            <TestDirectory useLoadContext=""true"" />
          </AssemblyResolution>
        </UnitTestRunConfig>
        <WebTestRunConfiguration testTypeId=""4e7599fa-5ecb-43e9-a887-cd63cf72d207"">
          <Browser name=""Internet Explorer 9.0"" MaxConnections=""6"">
            <Headers>
              <Header name=""User-Agent"" value=""Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"" />
              <Header name=""Accept"" value=""*/*"" />
              <Header name=""Accept-Language"" value=""{{$IEAcceptLanguage}}"" />
              <Header name=""Accept-Encoding"" value=""GZIP"" />
            </Headers>
          </Browser>
        </WebTestRunConfiguration>
      </TestTypeSpecific>
      <AgentRule name=""LocalMachineDefaultRole"">
        <DataCollectors>
          <DataCollector uri=""datacollector://microsoft/CodeCoverage/1.0"" assemblyQualifiedName=""Microsoft.VisualStudio.TestTools.CodeCoverage.CoveragePlugIn, Microsoft.VisualStudio.QualityTools.Plugins.CodeCoverage, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" friendlyName=""Code Coverage (Visual Studio 2010)"">
          </DataCollector>
        </DataCollectors>
      </AgentRule>
    </Execution>
  </TestSettings>
  <Times creation=""2013-11-01T13:43:52.8654497+02:00"" queuing=""2013-11-01T13:43:55.0394497+02:00"" start=""2013-11-01T13:43:55.5834497+02:00"" finish=""2013-11-01T13:47:05.6964497+02:00"" />
  <ResultSummary outcome=""Failed"">
    <Counters total=""4241"" executed=""4241"" passed=""4236"" error=""0"" failed=""3"" timeout=""0"" aborted=""0"" inconclusive=""2"" passedButRunAborted=""0"" notRunnable=""0"" notExecuted=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" pending=""0"" />
  </ResultSummary>
  <TestDefinitions>
    <UnitTest name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" storage=""d:\builds\releasegate\binaries\dev2.activities.designers.tests.dll"" id=""6142bab1-509e-a426-59b7-e5d8bc6d62cf"">
      <Owners>
        <Owner name=""Tshepo Ntlhokoa"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand"" />
      </TestCategory>
      <Execution id=""46ecc213-c05f-4b20-9407-0b99ea2cfd18"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Designers.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Activities.Designers.Tests.Designers2.Core.ActivityCollectionDesignerViewModelTests, Dev2.Activities.Designers.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse"" />
    </UnitTest>
    <UnitTest name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" storage=""d:\builds\releasegate\binaries\dev2.runtime.tests.dll"" id=""9fd0ecee-f50c-e066-0468-ce2b55bda4ab"">
      <Execution id=""98e1e23c-f12a-4415-944b-e6deb57ce44f"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Runtime.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Runtime.ServiceModel.ResourcesTests, Dev2.Runtime.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString"" />
    </UnitTest>
    <UnitTest name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""7e7e630d-fafe-de84-423b-98111f0c4f10"">
      <Execution id=""dfc91477-fc60-4272-9528-87c83c36ea9e"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.DataSplitActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace"" />
    </UnitTest>
    <UnitTest name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""edea71e2-ab69-da37-c337-d40ffd7134a1"">
      <Description>When a string is white space, expect false when istrueisempty equals false</Description>
      <Owners>
        <Owner name=""Jurie Smit"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""EmptyStringToBoolConverter"" />
      </TestCategory>
      <Execution id=""7717e8d0-6284-4e03-b460-12414be89064"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.ConverterTests.EmptyStringToBoolConverterTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue"" />
    </UnitTest>
    <UnitTest name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""ab77f188-f223-34cf-e658-5de592dab87e"">
      <Execution id=""a4180f20-d3bc-414f-8821-eb11e4e705e2"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.RecordsetSearch.RsOpTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3"" />
    </UnitTest>
    <UnitTest name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" storage=""d:\builds\releasegate\binaries\dev2.data.tests.dll"" id=""ebaa4ad7-7566-977f-f72c-5d9012d134c0"">
      <Execution id=""c82d7c97-24fe-4e4e-abb6-d48adbd36060"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Data.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Data.Tests.SystemTemplates.DecisionTest, Dev2.Data.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True"" />
    </UnitTest>
    <UnitTest name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""40e1ba7a-d657-55ba-f6db-bfac68b345da"">
      <Description>TcpConnection Connect must disconnect when connection to server fails.</Description>
      <Owners>
        <Owner name=""Trevor Williams-Ros"" />
      </Owners>
      <TestCategory>
        <TestCategoryItem TestCategory=""TcpConnection_Connect"" />
      </TestCategory>
      <Execution id=""3e6f82bf-5fb8-4293-8d09-0542747e3c75"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.Network.TcpConnectionTests, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect"" />
    </UnitTest>
    <UnitTest name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" storage=""d:\builds\releasegate\binaries\dev2.studio.core.tests.dll"" id=""cf841a6b-14c1-1575-edba-45b97f0027ad"">
      <Execution id=""368c921f-24c9-4120-9e59-1313ed0c8675"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Studio.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Core.Tests.IntellisenseProviderTest, Dev2.Studio.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" name=""GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults"" />
    </UnitTest>
    <UnitTest name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""18db7495-8579-e9df-3f65-223579568fd5"">
      <Execution id=""e819c819-7d63-4e7c-a2f1-97464c7134b8"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.ForEachActivityTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""GetDebugInputOutputWithScalarCsvsExpectedPass"" />
    </UnitTest>
    <UnitTest name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" storage=""d:\builds\releasegate\binaries\dev2.core.tests.dll"" id=""3a95af49-978b-ffba-8a29-cb7ad2cc403f"">
      <Execution id=""523e1acc-3e26-4100-8e0d-98c8c0705347"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Core.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.JsonTest.JsonPathSegmentTests, Dev2.Core.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat"" />
    </UnitTest>
    <UnitTest name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" storage=""d:\builds\releasegate\binaries\dev2.activities.tests.dll"" id=""ae44269d-d5bb-bd9f-52dd-eaec4fbb93d9"">
      <Execution id=""310cc625-6b0f-47d6-90b5-376b97da50e5"" />
      <TestMethod codeBase=""D:/Builds/ReleaseGate/Binaries/Dev2.Activities.Tests.DLL"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" className=""Dev2.Tests.Activities.ActivityTests.SortRecordsTest, Dev2.Activities.Tests, Version=0.2.8.0, Culture=neutral, PublicKeyToken=null"" name=""SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes"" />
    </UnitTest>
</TestDefinitions>
</TestRun>";
            #endregion

            const string xPath = "TestRun/TestSettings/Deployment/@userDeploymentRoot";

            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(data, xPath);
            //------------Assert Results-------------------------
            var expected = @"D:\Builds\ReleaseGate\TestResults";

            var dataList = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(1, dataList.Count());
            Assert.AreEqual(expected, dataList[0]);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_Parser_WhenXPathingAttributesFromTFSWithNamespace_ExpectException()
        {
            //------------Setup for test--------------------------

            #region data

            var data = LoadFile("SampleTFS.trx");

            #endregion

            const string xPath = "//UnitTest/@name";

            //------------Execute Test---------------------------
            var result = _xPathParser.ExecuteXPath(data, xPath).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4241, result.Count);
            Assert.AreEqual("ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse", result[0]);
            Assert.AreEqual("Instantiate_Where_MessageBrokerIsNull_Expected_ArgumentNullException", result[4240]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_Parser_WhenXPathingAttributesFromTFSWithoutNamespace_ExpectValidDataWithoutAttributeName()
        {
            //------------Setup for test--------------------------

            #region data

            var data = LoadFile("SampleTFSNoNamespace.trx");
            #endregion

            const string xPath = "//UnitTest/@name";

            //------------Execute Test---------------------------
            var result = _xPathParser.ExecuteXPath(data, xPath).ToList();
            //------------Assert Results-------------------------
            
            Assert.AreEqual(4241, result.Count);
            Assert.AreEqual("ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse", result[0]);
            Assert.AreEqual("Instantiate_Where_MessageBrokerIsNull_Expected_ArgumentNullException", result[4240]);
        }

        [TestMethod]
        public void ExecutePathWhereGivenXMLDocumentExpectXMLReturnedTestXML()
        {
            //------------Setup for test--------------------------
            const string XPath = "//x/a/text()";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath("<x><a>1</a></x>", XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(1,data.Count());
            Assert.AreEqual("1",data[0]);
        }
        
        [TestMethod]
        public void ExecutePathWhereGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        }

        [TestMethod]
        public void ExecutePathWhereRootNodeInXPathGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "/excludelist/type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_ExecutePath_AttrXPathGivenXMLData_XMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method/@name";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6, data.Count());
            Assert.AreEqual("ExtractOutMergeDataFromRequest", data[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathParser_Parser")]
        public void XPathParser_ExecutePath_OrXPathGivenXMLData_XMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method/@name|//type/method/@signature";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(12, data.Count());
            Assert.AreEqual("ExtractInMergeDataFromRequest", data[0]);
            Assert.AreEqual("void(object)", data[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereNullXmlDataExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath(null, "//");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereEmptyXmlDataExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("", "//");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereNullPathExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("//", null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereEmptyPathExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("//", "");
            //------------Assert Results-------------------------
        }

        private string LoadFile(string name)
        {

            

            var resourceName = string.Format("Dev2.Data.Tests.TestingResources.{0}", name);
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var readLen = stream.Length;
                    var byteData = new byte[readLen];

                    stream.Read(byteData, 0, (int)readLen);

                    return Encoding.Default.GetString(byteData);
                }

                return string.Empty;
            }
        }
    }
}
