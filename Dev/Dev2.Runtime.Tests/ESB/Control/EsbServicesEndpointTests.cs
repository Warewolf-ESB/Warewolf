/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Security.Principal;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    [TestCategory("Runtime ESB")]
    public class EsbServicesEndpointTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var pCounter = new Mock<IWarewolfPerformanceCounterLocater>();
            using (var emptyCounter = new EmptyCounter())
            {
                pCounter.Setup(locater => locater.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>())).Returns(emptyCounter);
            }
            using (var emptyCounter = new EmptyCounter())
            {
                pCounter.Setup(locater => locater.GetCounter(It.IsAny<WarewolfPerfCounterType>())).Returns(emptyCounter);
            }
            using (var emptyCounter = new EmptyCounter())
            {
                pCounter.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(emptyCounter);
            }
            CustomContainer.Register(pCounter.Object);
            CustomContainer.Register<IActivityParser>(new Dev2.Activities.ActivityParser());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void EsbServicesEndpoint_ExecuteWorkflow_GivenHelloWorldWorkflow_RunWorkflowAsyncFalse_ExpectSuccess()
        {
            AssertHelloWorldExists();
            var esbServicesEndpoint = new EsbServicesEndpoint();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1"),
                ExecutingUser = mockPrincipal.Object,
                IsDebug = true,
                RunWorkflowAsync = false,
            };
            dataObject.Environment.Assign("[[Name]]", "somename", 0);

            var request = new EsbExecuteRequest();
            var workspaceId = Guid.NewGuid();

            esbServicesEndpoint.ExecuteRequest(dataObject, request, workspaceId, out var errors);

            const string expectedJson = "{\"Environment\":{\"scalars\":{\"Message\":\"Hello somename.\",\"Name\":\"somename\"},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[\"\"],\"AllErrors\":[]}";
            Assert.AreEqual("Hello somename.", dataObject.Environment.EvalAsListOfStrings("[[Message]]", 0)[0]);
            Assert.AreEqual(expectedJson, dataObject.Environment.ToJson());
        }

        void AssertHelloWorldExists()
        {
            const string hellowWorldResourcePath = "C:\\programdata\\Warewolf\\Resources\\Hello World.bite";
            if (!File.Exists(hellowWorldResourcePath))
            {
                File.WriteAllText(hellowWorldResourcePath,
@"
<Service ID=""acb75027-ddeb-47d7-814e-a54c37247ec1"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""Hello World"" ResourceType=""WorkflowService"" IsValid=""false"" ServerVersion=""0.0.6213.18869"">
  <DisplayName>Hello World</DisplayName>
  <Category>Hello World</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>
  </AuthorRoles>
  <Comment>
  </Comment>
  <Tags>
  </Tags>
  <HelpLink>
  </HelpLink>
  <UnitTestTargetWorkflowService>
  </UnitTestTargetWorkflowService>
  <DataList>
    <Name Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    <Message Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""Hello World"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:dciipe=""clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,679&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""6""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""Hello World"" sap:VirtualizedContainerService.HintSize=""614,639""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;160,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;190,77.5 190,107.5 188,107.5 188,127.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID2&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;65,394&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Set the output variable (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""670132e7-80d4-4e41-94af-ba4a71b28118"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Message]]"" FieldValue=""Hello [[Name]]."" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;FlowDecision x:Name=""__ReferenceID2"" DisplayName=""If [[Name]] &amp;lt;&amp;gt; (Not Equal) "" sap:VirtualizedContainerService.HintSize=""149,87""&gt;&lt;FlowDecision.Condition&gt;&lt;uaba:DsfFlowDecisionActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Decision"" ExpressionText=""Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&amp;quot;{!TheStack!:[{!Col1!:![[Name]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNotEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!Name Input!,!FalseArmText!:!Blank Input!,!DisplayText!:!If [[Name]] &amp;lt;&amp;gt; (Not Equal) !}&amp;quot;,AmbientDataList)"" HasError=""[HasError]"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a03172cf-7f8f-417e-be86-8821d696ca40""&gt;&lt;uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;/uaba:DsfFlowDecisionActivity&gt;&lt;/FlowDecision.Condition&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:String x:Key=""TrueLabel""&gt;Name Input&lt;/x:String&gt;&lt;x:String x:Key=""FalseLabel""&gt;Blank Input&lt;/x:String&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;113.5,127.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;149,87&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""TrueConnector""&gt;113.5,171 90,171 90,364 180,364 180,394&lt;/av:PointCollection&gt;&lt;av:PointCollection x:Key=""FalseConnector""&gt;262.5,171 310,171 310,244&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;FlowDecision.True&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowDecision.True&gt;&lt;FlowDecision.False&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;195,244&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,88&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;310,332 310,362 180,362 180,394&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign a value to Name if blank (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""bd557ca7-113b-4197-afc3-de5d086dfc69"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Name]]"" FieldValue=""World"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowDecision.False&gt;&lt;/FlowDecision&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;325,23&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;250,114&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Execute this workflow:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,114"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""F5 - Debug and set Input variables&amp;#xA;F6 - Quick debug with previous inputs&amp;#xA;F7 - Call as web service from browser or click the link above (uses previous inputs)."" UniqueID=""c4346776-0475-402d-bfb8-11816da71b95""&gt;&lt;uaba:DsfCommentActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfCommentActivity.AmbientDataList&gt;&lt;uaba:DsfCommentActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfCommentActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfCommentActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;345,370&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Learn the Warewolf Syntax:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,100"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""Visit the knowledge base:&amp;#xA;https://warewolf.io/knowledge-base/the-warewolf-esb-syntax/"" UniqueID=""30e3974c-2ff0-482b-a41a-7dddf3cdef2e""&gt;&lt;uaba:DsfCommentActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfCommentActivity.AmbientDataList&gt;&lt;uaba:DsfCommentActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfCommentActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfCommentActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <VersionInfo DateTimeStamp=""2016-12-02T12:58:41.0354952+02:00"" Reason=""Save"" User=""DEV2\Barney.Buchan"" VersionNumber=""2"" ResourceId=""acb75027-ddeb-47d7-814e-a54c37247ec1"" VersionId=""3cd26c72-35fa-4285-8ae2-136af5413908"" />
</Service>
");
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void EsbServicesEndpoint_ExecuteWorkflow_ResourceIsNull_ExpectNothing()
        {
            var esbServicesEndpoint = new EsbServicesEndpoint();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = Guid.Parse("2311e5fb-3eaa-4986-b946-5a687f33fd51"),
                ExecutingUser = mockPrincipal.Object,
                IsDebug = true,
                RunWorkflowAsync = true,
            };
            dataObject.Environment.Assign("[[Name]]", "somename", 0);

            var request = new EsbExecuteRequest();
            var workspaceId = Guid.NewGuid();

            var resultId = esbServicesEndpoint.ExecuteRequest(dataObject, request, workspaceId, out var errors);

            Assert.AreEqual(Guid.Empty, resultId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EsbServicesEndpoint_CreateNewEnvironmentFromInputMappings_GivenInputsDefs_ShouldCreateNewEnvWithMappings()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            IExecutionEnvironment executionEnvironment = new ExecutionEnvironment();
            dataObj.Setup(o => o.Environment).Returns(executionEnvironment).Verifiable();
            dataObj.Setup(o => o.PushEnvironment(It.IsAny<IExecutionEnvironment>())).Verifiable();
            const string inputMappings = @"<Inputs><Input Name=""f1"" Source=""[[recset1(*).f1a]]"" Recordset=""recset1"" /><Input Name=""f2"" Source=""[[recset2(*).f2a]]"" Recordset=""recset2"" /></Inputs>";
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            esbServicesEndpoint.CreateNewEnvironmentFromInputMappings(dataObj.Object, inputMappings, 0);
            //---------------Test Result -----------------------
            dataObj.Verify(o => o.PushEnvironment(It.IsAny<IExecutionEnvironment>()), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EsbServicesEndpoint_ExecuteSubRequest_GivenValidArgs_ShouldCheckIsRemoteWorkflow()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var rCat = new Mock<IResourceCatalog>();
            var mock = new Mock<IResource>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mock.Object);
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dataObj.Setup(o => o.IsRemoteWorkflow());
            dataObj.Setup(o => o.ServiceName).Returns("SomeName");
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------
            esbServicesEndpoint.ExecuteSubRequest(dataObj.Object, Guid.NewGuid(), "", "", out var err, 1, true);

            //---------------Test Result -----------------------
            dataObj.Verify(o => o.IsRemoteWorkflow(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EsbServicesEndpoint_ExecuteSubRequest_GivenExecuteWorkflowAsync_ShouldCheckIsRemoteWorkflow()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var dataObjClon = new Mock<IDSFDataObject>();
            dataObjClon.Setup(o => o.ServiceName).Returns("Service Name");
            var rCat = new Mock<IResourceCatalog>();
            var mock = new Mock<IResource>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mock.Object);
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dataObj.Setup(o => o.IsRemoteWorkflow());
            dataObj.Setup(o => o.RunWorkflowAsync).Returns(true);
            dataObj.Setup(o => o.Clone()).Returns(dataObjClon.Object);
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------

            esbServicesEndpoint.ExecuteSubRequest(dataObj.Object, Guid.NewGuid(), "", "", out var err, 1, true);

            //---------------Test Result -----------------------
            dataObj.Verify(o => o.IsRemoteWorkflow(), Times.Once);
            var contains = err.FetchErrors().Contains(ErrorResource.ResourceNotFound);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EsbServicesEndpoint_ExecuteLogErrorRequest_GivenCorrectUri_ShouldNoThrowException()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var rCat = new Mock<IResourceCatalog>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                esbServicesEndpoint.ExecuteLogErrorRequest(dataObj.Object, It.IsAny<Guid>(), "http://example.com/", out var err, 1);
            }
            catch (Exception ex)
            {
                //---------------Test Result -----------------------
                Assert.Fail(ex.Message);
            }
        }
    }
}
