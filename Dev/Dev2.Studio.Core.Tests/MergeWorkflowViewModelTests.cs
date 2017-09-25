using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Interfaces;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.Generic;
using System;
using System.Activities;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation.Model;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MergeWorkflowViewModelTests
    {
        

        [TestInitialize]
        public void InitializeTest()
        {
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            var mockPopupController = new Mock<IPopupController>();
            var mockServer = new Mock<IServer>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockServerRepository = new Mock<IServerRepository>();
            var mockParseServiceForDifferences = new Mock<IServiceDifferenceParser>();

            mockApplicationAdapter.Setup(a => a.Current).Returns(Application.Current);

            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            mockShellViewModel.Setup(a => a.ActiveServer).Returns(mockServer.Object);

            mockServerRepository.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(a => a.IsLoaded).Returns(true);

            CustomContainer.Register(mockApplicationAdapter.Object);
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(mockServerRepository.Object);
            CustomContainer.Register(mockParseServiceForDifferences.Object);
        }

        [TestMethod]
        public void Initialize_GivenIsNewNoEmptyConflicts_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------

            var assignId = Guid.NewGuid();
            var foreachId = Guid.NewGuid();
            List<ModelItem> currentChanges = CreateChanges(ref assignId, ref foreachId);
            List<ModelItem> differenceChanges = CreateChanges(ref assignId, ref foreachId);

            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForCurrent());
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForDifference());
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
        }

        private static List<ModelItem> CreateChanges(ref Guid assignId, ref Guid foreachId)
        {
            var dsfMultiAssignActivity = new DsfMultiAssignActivity()
            {
                UniqueID = assignId.ToString(),
                FieldsCollection = new List<ActivityDTO>()
                {
                    new ActivityDTO("a","a",1),
                    new ActivityDTO("a","a",2)
                }
            };
            var dsfForEachActivity = new DsfForEachActivity()
            {
                UniqueID = foreachId.ToString(),
                DataFunc = new ActivityFunc<string, bool>()
                {
                    Handler = new DsfDateTimeActivity()
                }
            };
            var assignOne = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity);
            var forEach = ModelItemUtils.CreateModelItem(dsfForEachActivity);
            var currentChanges = new List<ModelItem>()
            {
                assignOne,forEach
            };
            return currentChanges;
        }

        [TestMethod]
        public void Initialize_GivenHasConflicts_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            applicationAdapter.Setup(a => a.Current).Returns(Application.Current);
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);

            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForCurrent());
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForDifference());
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(currentResourceModel, differenceResourceModel);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            //---------------Test Result -----------------------
            //var mergeToolModels = mergeWorkflowViewModel.CurrentConflictViewModel.MergeToolModel;
            //var differenceViewModel = mergeWorkflowViewModel.DifferenceConflictViewModel.MergeToolModel;
            //Assert.AreNotSame(mergeToolModels, differenceViewModel);
        }


        [TestMethod]
        public void Initialize_GivenHasConflictsAndForEach_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            applicationAdapter.Setup(a => a.Current).Returns(Application.Current);
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);

            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForCurrent());
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXamlForDifference());
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(currentResourceModel, differenceResourceModel);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            //---------------Test Result -----------------------
        }


        public static StringBuilder WorkflowXamlForCurrent()
        {
            return new StringBuilder(@"<Activity mc:Ignorable=""sap"" 
    x:Class=""Hello World"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" 
    xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
    xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" 
    xmlns:dciipe=""clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces"" 
    xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" 
    xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" 
    xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" 
    xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" 
    xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" 
    xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" 
    xmlns:s=""clr-namespace:System;assembly=mscorlib"" 
    xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" 
    xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" 
    xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" 
    xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" 
    xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Members>
		<x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" />
		<x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" />
		<x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" />
	</x:Members>
	<sap:VirtualizedContainerService.HintSize>654,679</sap:VirtualizedContainerService.HintSize>
	<mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings>
	<TextExpression.NamespacesForImplementation>
		<scg:List x:TypeArguments=""x:String"" Capacity=""6"">
			<x:String>Dev2.Common</x:String>
			<x:String>Dev2.Data.Decisions.Operations</x:String>
			<x:String>Dev2.Data.SystemTemplates.Models</x:String>
			<x:String>Dev2.DataList.Contract</x:String>
			<x:String>Dev2.DataList.Contract.Binary_Objects</x:String>
			<x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String>
		</scg:List>
	</TextExpression.NamespacesForImplementation>
	<TextExpression.ReferencesForImplementation>
		<sco:Collection x:TypeArguments=""AssemblyReference"">
			<AssemblyReference>Dev2.Common</AssemblyReference>
			<AssemblyReference>Dev2.Data</AssemblyReference>
			<AssemblyReference>Dev2.Activities</AssemblyReference>
		</sco:Collection>
	</TextExpression.ReferencesForImplementation>
	<Flowchart DisplayName=""Hello World"" sap:VirtualizedContainerService.HintSize=""614,639"">
		<Flowchart.Variables>
			<Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />
			<Variable x:TypeArguments=""x:String"" Name=""LastResult"" />
			<Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />
			<Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />
			<Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />
			<Variable x:TypeArguments=""uaba:Util"" Name=""t"" />
			<Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" />
		</Flowchart.Variables>
		<sap:WorkflowViewStateService.ViewState>
			<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
				<x:Boolean x:Key=""IsExpanded"">False</x:Boolean>
				<av:Point x:Key=""ShapeLocation"">160,2.5</av:Point>
				<av:Size x:Key=""ShapeSize"">60,75</av:Size>
				<av:PointCollection x:Key=""ConnectorLocation"">190,77.5 190,107.5 188,107.5 188,127.5</av:PointCollection>
			</scg:Dictionary>
		</sap:WorkflowViewStateService.ViewState>
		<Flowchart.StartNode>
			<x:Reference>__ReferenceID2</x:Reference>
		</Flowchart.StartNode>
		<FlowStep x:Name=""__ReferenceID0"">
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">65,394</av:Point>
					<av:Size x:Key=""ShapeSize"">230,88</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Set the output variable (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""670132e7-80d4-4e41-94af-ba4a71b28118"" UpdateAllOccurrences=""False"">
				<uaba:DsfMultiAssignActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfMultiAssignActivity.AmbientDataList>
				<uaba:DsfMultiAssignActivity.FieldsCollection>
					<scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
						<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Message]]"" FieldValue=""Hello [[Name]]."" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
							<uaba:ActivityDTO.Errors>
								<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
							</uaba:ActivityDTO.Errors>
							<uaba:ActivityDTO.OutList>
								<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
							</uaba:ActivityDTO.OutList>
						</uaba:ActivityDTO>
						<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable="""">
							<uaba:ActivityDTO.Errors>
								<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
							</uaba:ActivityDTO.Errors>
							<uaba:ActivityDTO.OutList>
								<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
							</uaba:ActivityDTO.OutList>
						</uaba:ActivityDTO>
					</scg:List>
				</uaba:DsfMultiAssignActivity.FieldsCollection>
				<uaba:DsfMultiAssignActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfMultiAssignActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfMultiAssignActivity>
		</FlowStep>
		<FlowDecision x:Name=""__ReferenceID2"" DisplayName=""If [[Name]] &amp;lt;&amp;gt; (Not Equal) "" sap:VirtualizedContainerService.HintSize=""149,87"">
			<FlowDecision.Condition>
				<uaba:DsfFlowDecisionActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Decision"" ExpressionText=""Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&amp;quot;{!TheStack!:[{!Col1!:![[Name]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNotEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!Name Input!,!FalseArmText!:!Blank Input!,!DisplayText!:!If [[Name]] &amp;lt;&amp;gt; (Not Equal) !}&amp;quot;,AmbientDataList)"" HasError=""[HasError]"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a03172cf-7f8f-417e-be86-8821d696ca40"">
					<uaba:DsfFlowDecisionActivity.AmbientDataList>
						<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
					</uaba:DsfFlowDecisionActivity.AmbientDataList>
					<uaba:DsfFlowDecisionActivity.ParentInstanceID>
						<InOutArgument x:TypeArguments=""x:String"" />
					</uaba:DsfFlowDecisionActivity.ParentInstanceID>
				</uaba:DsfFlowDecisionActivity>
			</FlowDecision.Condition>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<x:String x:Key=""TrueLabel"">Name Input</x:String>
					<x:String x:Key=""FalseLabel"">Blank Input</x:String>
					<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					<av:Point x:Key=""ShapeLocation"">113.5,127.5</av:Point>
					<av:Size x:Key=""ShapeSize"">149,87</av:Size>
					<av:PointCollection x:Key=""TrueConnector"">113.5,171 90,171 90,364 180,364 180,394</av:PointCollection>
					<av:PointCollection x:Key=""FalseConnector"">262.5,171 310,171 310,244</av:PointCollection>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<FlowDecision.True>
				<x:Reference>__ReferenceID0</x:Reference>
			</FlowDecision.True>
			<FlowDecision.False>
				<FlowStep x:Name=""__ReferenceID1"">
					<sap:WorkflowViewStateService.ViewState>
						<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
							<av:Point x:Key=""ShapeLocation"">195,244</av:Point>
							<av:Size x:Key=""ShapeSize"">230,88</av:Size>
							<av:PointCollection x:Key=""ConnectorLocation"">310,332 310,362 180,362 180,394</av:PointCollection>
						</scg:Dictionary>
					</sap:WorkflowViewStateService.ViewState>
					<uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign a value to Name if blank (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""bd557ca7-113b-4197-afc3-de5d086dfc69"" UpdateAllOccurrences=""False"">
						<uaba:DsfMultiAssignActivity.AmbientDataList>
							<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
						</uaba:DsfMultiAssignActivity.AmbientDataList>
						<uaba:DsfMultiAssignActivity.FieldsCollection>
							<scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
								<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Name]]"" FieldValue=""World"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
									<uaba:ActivityDTO.Errors>
										<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
									</uaba:ActivityDTO.Errors>
									<uaba:ActivityDTO.OutList>
										<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
									</uaba:ActivityDTO.OutList>
								</uaba:ActivityDTO>
								<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable="""">
									<uaba:ActivityDTO.Errors>
										<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
									</uaba:ActivityDTO.Errors>
									<uaba:ActivityDTO.OutList>
										<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
									</uaba:ActivityDTO.OutList>
								</uaba:ActivityDTO>
							</scg:List>
						</uaba:DsfMultiAssignActivity.FieldsCollection>
						<uaba:DsfMultiAssignActivity.ParentInstanceID>
							<InOutArgument x:TypeArguments=""x:String"" />
						</uaba:DsfMultiAssignActivity.ParentInstanceID>
						<sap:WorkflowViewStateService.ViewState>
							<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
								<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
							</scg:Dictionary>
						</sap:WorkflowViewStateService.ViewState>
					</uaba:DsfMultiAssignActivity>
					<FlowStep.Next>
						<x:Reference>__ReferenceID0</x:Reference>
					</FlowStep.Next>
				</FlowStep>
			</FlowDecision.False>
		</FlowDecision>
		<x:Reference>__ReferenceID1</x:Reference>
		<FlowStep>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">325,23</av:Point>
					<av:Size x:Key=""ShapeSize"">250,114</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Execute this workflow:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,114"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""F5 - Debug and set Input variables&amp;#xA;F6 - Quick debug with previous inputs&amp;#xA;F7 - Call as web service from browser or click the link above (uses previous inputs)."" UniqueID=""c4346776-0475-402d-bfb8-11816da71b95"">
				<uaba:DsfCommentActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfCommentActivity.AmbientDataList>
				<uaba:DsfCommentActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfCommentActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfCommentActivity>
		</FlowStep>
		<FlowStep>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">345,370</av:Point>
					<av:Size x:Key=""ShapeSize"">250,100</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Learn the Warewolf Syntax:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,100"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""Visit the knowledge base:&amp;#xA;https://warewolf.io/knowledge-base/the-warewolf-esb-syntax/"" UniqueID=""30e3974c-2ff0-482b-a41a-7dddf3cdef2e"">
				<uaba:DsfCommentActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfCommentActivity.AmbientDataList>
				<uaba:DsfCommentActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfCommentActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfCommentActivity>
		</FlowStep>
	</Flowchart>
</Activity>");
        }

        public static StringBuilder WorkflowXamlForDifference()
        {
            return new StringBuilder(@"<Activity mc:Ignorable=""sap"" 
    x:Class=""Hello World"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" 
    xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
    xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" 
    xmlns:dciipe=""clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces"" 
    xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" 
    xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" 
    xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" 
    xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" 
    xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" 
    xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" 
    xmlns:s=""clr-namespace:System;assembly=mscorlib"" 
    xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" 
    xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" 
    xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" 
    xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" 
    xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Members>
		<x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" />
		<x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" />
		<x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" />
	</x:Members>
	<sap:VirtualizedContainerService.HintSize>654,679</sap:VirtualizedContainerService.HintSize>
	<mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings>
	<TextExpression.NamespacesForImplementation>
		<scg:List x:TypeArguments=""x:String"" Capacity=""6"">
			<x:String>Dev2.Common</x:String>
			<x:String>Dev2.Data.Decisions.Operations</x:String>
			<x:String>Dev2.Data.SystemTemplates.Models</x:String>
			<x:String>Dev2.DataList.Contract</x:String>
			<x:String>Dev2.DataList.Contract.Binary_Objects</x:String>
			<x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String>
		</scg:List>
	</TextExpression.NamespacesForImplementation>
	<TextExpression.ReferencesForImplementation>
		<sco:Collection x:TypeArguments=""AssemblyReference"">
			<AssemblyReference>Dev2.Common</AssemblyReference>
			<AssemblyReference>Dev2.Data</AssemblyReference>
			<AssemblyReference>Dev2.Activities</AssemblyReference>
		</sco:Collection>
	</TextExpression.ReferencesForImplementation>
	<Flowchart DisplayName=""Hello World"" sap:VirtualizedContainerService.HintSize=""614,639"">
		<Flowchart.Variables>
			<Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />
			<Variable x:TypeArguments=""x:String"" Name=""LastResult"" />
			<Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />
			<Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />
			<Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />
			<Variable x:TypeArguments=""uaba:Util"" Name=""t"" />
			<Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" />
		</Flowchart.Variables>
		<sap:WorkflowViewStateService.ViewState>
			<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
				<x:Boolean x:Key=""IsExpanded"">False</x:Boolean>
				<av:Point x:Key=""ShapeLocation"">160,2.5</av:Point>
				<av:Size x:Key=""ShapeSize"">60,75</av:Size>
				<av:PointCollection x:Key=""ConnectorLocation"">190,77.5 190,107.5 188,107.5 188,127.5</av:PointCollection>
			</scg:Dictionary>
		</sap:WorkflowViewStateService.ViewState>
		<Flowchart.StartNode>
			<x:Reference>__ReferenceID2</x:Reference>
		</Flowchart.StartNode>
		<FlowStep x:Name=""__ReferenceID0"">
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">295,366</av:Point>
					<av:Size x:Key=""ShapeSize"">230,88</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Set the output variable (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""670132e7-80d4-4e41-94af-ba4a71b28118"" UpdateAllOccurrences=""False"">
				<uaba:DsfMultiAssignActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfMultiAssignActivity.AmbientDataList>
				<uaba:DsfMultiAssignActivity.FieldsCollection>
					<scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
						<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Message]]"" FieldValue=""Hello [[Name]]."" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
							<uaba:ActivityDTO.Errors>
								<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
							</uaba:ActivityDTO.Errors>
							<uaba:ActivityDTO.OutList>
								<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
							</uaba:ActivityDTO.OutList>
						</uaba:ActivityDTO>
						<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable="""">
							<uaba:ActivityDTO.Errors>
								<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
							</uaba:ActivityDTO.Errors>
							<uaba:ActivityDTO.OutList>
								<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
							</uaba:ActivityDTO.OutList>
						</uaba:ActivityDTO>
					</scg:List>
				</uaba:DsfMultiAssignActivity.FieldsCollection>
				<uaba:DsfMultiAssignActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfMultiAssignActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfMultiAssignActivity>
		</FlowStep>
		<FlowDecision x:Name=""__ReferenceID2"" DisplayName=""If [[Name]] &amp;lt;&amp;gt; (Not Equal) "" sap:VirtualizedContainerService.HintSize=""149,87"">
			<FlowDecision.Condition>
				<uaba:DsfFlowDecisionActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Decision"" ExpressionText=""Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&amp;quot;{!TheStack!:[{!Col1!:![[Name]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNotEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!Name Input!,!FalseArmText!:!Blank Input!,!DisplayText!:!If [[Name]] &amp;lt;&amp;gt; (Not Equal) !}&amp;quot;,AmbientDataList)"" HasError=""[HasError]"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a03172cf-7f8f-417e-be86-8821d696ca40"">
					<uaba:DsfFlowDecisionActivity.AmbientDataList>
						<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
					</uaba:DsfFlowDecisionActivity.AmbientDataList>
					<uaba:DsfFlowDecisionActivity.ParentInstanceID>
						<InOutArgument x:TypeArguments=""x:String"" />
					</uaba:DsfFlowDecisionActivity.ParentInstanceID>
				</uaba:DsfFlowDecisionActivity>
			</FlowDecision.Condition>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<x:String x:Key=""TrueLabel"">Name Input</x:String>
					<x:String x:Key=""FalseLabel"">Blank Input</x:String>
					<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					<av:Point x:Key=""ShapeLocation"">113.5,127.5</av:Point>
					<av:Size x:Key=""ShapeSize"">149,87</av:Size>
					<av:PointCollection x:Key=""TrueConnector"">113.5,171 90,171 90,236 120,236 120,266</av:PointCollection>
					<av:PointCollection x:Key=""FalseConnector"">262.5,171 410,171 410,366</av:PointCollection>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<FlowDecision.True>
				<FlowStep x:Name=""__ReferenceID1"">
					<sap:WorkflowViewStateService.ViewState>
						<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
							<av:Point x:Key=""ShapeLocation"">5,266</av:Point>
							<av:Size x:Key=""ShapeSize"">230,88</av:Size>
							<av:PointCollection x:Key=""ConnectorLocation"">120,354 120,410 295,410</av:PointCollection>
						</scg:Dictionary>
					</sap:WorkflowViewStateService.ViewState>
					<uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign a value to Name if blank (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""bd557ca7-113b-4197-afc3-de5d086dfc69"" UpdateAllOccurrences=""False"">
						<uaba:DsfMultiAssignActivity.AmbientDataList>
							<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
						</uaba:DsfMultiAssignActivity.AmbientDataList>
						<uaba:DsfMultiAssignActivity.FieldsCollection>
							<scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
								<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName=""[[Name]]"" FieldValue=""World"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
									<uaba:ActivityDTO.Errors>
										<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
									</uaba:ActivityDTO.Errors>
									<uaba:ActivityDTO.OutList>
										<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
									</uaba:ActivityDTO.OutList>
								</uaba:ActivityDTO>
								<uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable="""">
									<uaba:ActivityDTO.Errors>
										<scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" />
									</uaba:ActivityDTO.Errors>
									<uaba:ActivityDTO.OutList>
										<scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
									</uaba:ActivityDTO.OutList>
								</uaba:ActivityDTO>
							</scg:List>
						</uaba:DsfMultiAssignActivity.FieldsCollection>
						<uaba:DsfMultiAssignActivity.ParentInstanceID>
							<InOutArgument x:TypeArguments=""x:String"" />
						</uaba:DsfMultiAssignActivity.ParentInstanceID>
						<sap:WorkflowViewStateService.ViewState>
							<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
								<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
							</scg:Dictionary>
						</sap:WorkflowViewStateService.ViewState>
					</uaba:DsfMultiAssignActivity>
					<FlowStep.Next>
						<x:Reference>__ReferenceID0</x:Reference>
					</FlowStep.Next>
				</FlowStep>
			</FlowDecision.True>
			<FlowDecision.False>
				<x:Reference>__ReferenceID0</x:Reference>
			</FlowDecision.False>
		</FlowDecision>
		<x:Reference>__ReferenceID1</x:Reference>
		<FlowStep>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">325,23</av:Point>
					<av:Size x:Key=""ShapeSize"">250,114</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Execute this workflow:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,114"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""F5 - Debug and set Input variables&amp;#xA;F6 - Quick debug with previous inputs&amp;#xA;F7 - Call as web service from browser or click the link above (uses previous inputs)."" UniqueID=""c4346776-0475-402d-bfb8-11816da71b95"">
				<uaba:DsfCommentActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfCommentActivity.AmbientDataList>
				<uaba:DsfCommentActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfCommentActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfCommentActivity>
		</FlowStep>
		<FlowStep>
			<sap:WorkflowViewStateService.ViewState>
				<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
					<av:Point x:Key=""ShapeLocation"">325,490</av:Point>
					<av:Size x:Key=""ShapeSize"">250,100</av:Size>
				</scg:Dictionary>
			</sap:WorkflowViewStateService.ViewState>
			<uaba:DsfCommentActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Learn the Warewolf Syntax:"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,100"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" Text=""Visit the knowledge base:&amp;#xA;https://warewolf.io/knowledge-base/the-warewolf-esb-syntax/"" UniqueID=""30e3974c-2ff0-482b-a41a-7dddf3cdef2e"">
				<uaba:DsfCommentActivity.AmbientDataList>
					<InOutArgument x:TypeArguments=""scg:List(x:String)"" />
				</uaba:DsfCommentActivity.AmbientDataList>
				<uaba:DsfCommentActivity.ParentInstanceID>
					<InOutArgument x:TypeArguments=""x:String"" />
				</uaba:DsfCommentActivity.ParentInstanceID>
				<sap:WorkflowViewStateService.ViewState>
					<scg:Dictionary x:TypeArguments=""x:String, x:Object"">
						<x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
					</scg:Dictionary>
				</sap:WorkflowViewStateService.ViewState>
			</uaba:DsfCommentActivity>
		</FlowStep>
	</Flowchart>
</Activity>");
        }
    }
}
