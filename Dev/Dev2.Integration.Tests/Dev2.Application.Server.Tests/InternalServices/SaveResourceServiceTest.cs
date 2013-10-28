using System;
using Dev2.Common.ExtMethods;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for AddResourceServiceTest
    /// </summary>
    [TestClass]
    public class SaveResourceServiceTest
    {
        public SaveResourceServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        const string BlankService = "<Service ID=\"af8990ea-c472-4233-a91e-2ea20c6efe06\" Version=\"1.0\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\" Name=\"BlankWorkflow\" ResourceType=\"WorkflowService\" IsValid=\"false\">" + "<DisplayName>BlankWorkflow</DisplayName>" + "<Category>INTERGRATION TEST SERVICES</Category>" + "<IsNewWorkflow>true</IsNewWorkflow>" + "<AuthorRoles></AuthorRoles>" + "<Comment></Comment>" + "<Tags></Tags>" + "<IconPath></IconPath>" + "<HelpLink></HelpLink>" + "<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>" + "<DataList />" + "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" + "<XamlDefinition><Activity x:Class=\"BlankWorkflow\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.Common\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" " + "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" " + "xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" " + "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" " + "xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" " + "xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" " + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><x:Members><x:Property Name=\"AmbientDataList\" " + "Type=\"InOutArgument(scg:List(x:String))\" /><x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /><x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /></x:Members><sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize><mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings><TextExpression.NamespacesForImplementation><scg:List " + "x:TypeArguments=\"x:String\" Capacity=\"7\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Framework</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco:Collection x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Core</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco:Collection></TextExpression.ReferencesForImplementation><Flowchart DisplayName=\"BlankWorkflow\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /><Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Null /></Flowchart.StartNode></Flowchart></Activity></XamlDefinition></Action><ErrorMessages /></Service>";
        const string UpdatedService = "<Service ID=\"af8990ea-c472-4233-a91e-2ea20c6efe06\" Version=\"1.0\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\" Name=\"BlankWorkflow\" ResourceType=\"WorkflowService\" IsValid=\"true\">" +
                                      "<DisplayName>BlankWorkflow</DisplayName>" +
                                      "<Category>INTERGRATION TEST SERVICES</Category>" +
                                      "<IsNewWorkflow>false</IsNewWorkflow>" +
                                      "<AuthorRoles></AuthorRoles>" +
                                      "<Comment></Comment>" +
                                      "<Tags></Tags>" +
                                      "<IconPath></IconPath>" +
                                      "<HelpLink></HelpLink>" +
                                      "<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>" +
                                      "<DataList><Test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                      "</DataList>" +
                                      "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">"+
    "<XamlDefinition><Activity x:Class=\"BlankWorkflow\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                                      "xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.Common\" " +
                                      "xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" " +
                                      "xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" " +
                                      "xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" " +
                                      "xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" " +
                                      "xmlns:dpe=\"clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure\" " +
                                      "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" " +
                                      "xmlns:s=\"clr-namespace:System;assembly=mscorlib\" " +
                                      "xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" " +
                                      "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" " +
                                      "xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=System\" " +
                                      "xmlns:sco1=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" " +
                                      "xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" " +
                                      "xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" " +
                                      "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><x:Members><x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /><x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /><x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /></x:Members><sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize><mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings><TextExpression.NamespacesForImplementation><scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Framework</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco1:Collection " +
                                      "x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Core</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco1:Collection></TextExpression.ReferencesForImplementation><Flowchart " +
                                      "DisplayName=\"BlankWorkflow\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable " +
                                      "x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable " +
                                      "x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" " +
                                      "Name=\"IsValid\" /><Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable " +
                                      "x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size><av:PointCollection " +
                                      "x:Key=\"ConnectorLocation\">300,77.5 300,127.5</av:PointCollection></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Reference>__ReferenceID0</x:Reference></Flowchart.StartNode><FlowStep x:Name=\"__ReferenceID0\"><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><av:Point x:Key=\"ShapeLocation\">185,127.5</av:Point><av:Size x:Key=\"ShapeSize\">230,78</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><uaba:DsfMultiAssignActivity Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" " +
                                      "InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" " +
                                      "OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" " +
                                      "ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" " +
                                      "ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                                      "ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" " +
                                      "CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (1)\" HasError=\"[HasError]\" " +
                                      "sap:VirtualizedContainerService.HintSize=\"230,78\" " +
                                      "InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" " +
                                      "IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" " +
                                      "OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" " +
                                      "SimulationMode=\"OnDemand\" UniqueID=\"8db15e73-95ec-4df3-b735-ed74a26a30b7\" UpdateAllOccurrences=\"False\"><uaba:DsfMultiAssignActivity.AmbientDataList><InOutArgument x:TypeArguments=\"scg:List(x:String)\" /></uaba:DsfMultiAssignActivity.AmbientDataList><uaba:DsfMultiAssignActivity.FieldsCollection><sco:ObservableCollection " +
                                      "x:TypeArguments=\"uaba:ActivityDTO\"><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" FieldName=\"[[Test]]\" FieldValue=\"PASS\" " +
                                      "IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" WatermarkTextValue=\"Value\" WatermarkTextVariable=\"[[Variable1]]\"><uaba:ActivityDTO.Errors><scg:List x:TypeArguments=\"dpe:IActionableErrorInfo\" x:Key=\"FieldName\" Capacity=\"0\" /><scg:List x:TypeArguments=\"dpe:IActionableErrorInfo\" x:Key=\"FieldValue\" Capacity=\"0\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" " +
                                      "Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" WatermarkTextValue=\"Value\" WatermarkTextVariable=\"[[Variable2]]\"><uaba:ActivityDTO.Errors><scg:List " +
                                      "x:TypeArguments=\"dpe:IActionableErrorInfo\" x:Key=\"FieldName\" Capacity=\"0\" /><scg:List x:TypeArguments=\"dpe:IActionableErrorInfo\" x:Key=\"FieldValue\" Capacity=\"0\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO></sco:ObservableCollection></uaba:DsfMultiAssignActivity.FieldsCollection><uaba:DsfMultiAssignActivity.ParentInstanceID><InOutArgument x:TypeArguments=\"x:String\" /></uaba:DsfMultiAssignActivity.ParentInstanceID><sap:WorkflowViewStateService.ViewState><scg:Dictionary " +
                                      "x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">True</x:Boolean></scg:Dictionary></sap:WorkflowViewStateService.ViewState></uaba:DsfMultiAssignActivity></FlowStep></Flowchart></Activity></XamlDefinition>" +
                                      "</Action>" +
                                      "<ErrorMessages />" +
                                      "</Service>";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get; set;
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        // ReSharper disable once InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveResource_SaveResource")]
        public void SaveResource_WhenHasNoIDPassed_SavesToWorkspace()
        {
            //------------Setup for test--------------------------
            string service = "<Payload><Roles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,DEV2 Limited Internet Access</Roles><ResourceXml>"+BlankService+"</ResourceXml></Payload>";
            var webserverURI = ServerSettings.WebserverURI + "SaveResourceService?" + service;
            //------------Execute Test---------------------------
            string actual = TestHelper.PostDataToWebserver(webserverURI).Unescape();
            //------------Assert Results-------------------------
            var expected = string.Format("<Dev2System.ManagmentServicePayload>Updated WorkflowService 'BlankWorkflow'</Dev2System.ManagmentServicePayload>");
            StringAssert.Contains(actual, expected, "Got [ " + actual + " ]");
            webserverURI = ServerSettings.WebserverURI + @"FindResourceService?ResourceName=BlankWorkflow&ResourceType=WorkflowService";
            string fromServer = TestHelper.PostDataToWebserver(webserverURI);
            Assert.IsFalse(string.IsNullOrEmpty(fromServer));
        }
    }
}