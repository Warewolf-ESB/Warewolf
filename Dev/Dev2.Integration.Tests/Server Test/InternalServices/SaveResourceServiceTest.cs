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
        const string BlankService = "<Service ID=\"af8990ea-c472-4233-a91e-2ea20c6efe06\" Version=\"1.0\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\" Name=\"BlankWorkflow\" ResourceType=\"WorkflowService\" IsValid=\"false\">" + "<DisplayName>BlankWorkflow</DisplayName>" + "<Category>INTERGRATION TEST SERVICES</Category>" + "<IsNewWorkflow>true</IsNewWorkflow>" + "<AuthorRoles></AuthorRoles>" + "<Comment></Comment>" + "<Tags></Tags>" + "<IconPath></IconPath>" + "<HelpLink></HelpLink>" + "<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>" + "<DataList />" + "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" + "<XamlDefinition><Activity x:Class=\"BlankWorkflow\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.Common\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" " + "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" " + "xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" " + "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" " + "xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" " + "xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" " + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><x:Members><x:Property Name=\"AmbientDataList\" " + "Type=\"InOutArgument(scg:List(x:String))\" /><x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /><x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /></x:Members><sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize><mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings><TextExpression.NamespacesForImplementation><scg:List " + "x:TypeArguments=\"x:String\" Capacity=\"7\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Framework</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco:Collection x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Core</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco:Collection></TextExpression.ReferencesForImplementation><Flowchart DisplayName=\"BlankWorkflow\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /><Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Null /></Flowchart.StartNode></Flowchart></Activity></XamlDefinition></Action><ErrorMessages /></Service>";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveResource_HasNoID")]
        public void SaveResource_WhenHasNoIDPassed_SavesToWorkspace()
        {
            //------------Setup for test--------------------------
            string service = "<Payload><Roles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,DEV2 Limited Internet Access</Roles><ResourceXml>" + BlankService + "</ResourceXml></Payload>";
            var webserverURI = ServerSettings.WebserverURI + "SaveResourceService?" + service;
            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserver(webserverURI).Unescape();
            //------------Assert Results-------------------------
            webserverURI = ServerSettings.WebserverURI + @"FindResourceService?ResourceName=BlankWorkflow&ResourceType=WorkflowService";
            string fromServer = TestHelper.PostDataToWebserver(webserverURI);
            Assert.IsFalse(string.IsNullOrEmpty(fromServer));
        }
    }
}