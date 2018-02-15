using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.MergeParser;

namespace Dev2.Core.Tests.Merge.Utils
{
    public struct WorkflowData
    {
        public string Current;
        public string Different;
    }

    public static class WorkflowTestResources
    {
        public readonly static WorkflowData MergePositionChange = new WorkflowData
        {
            Current = "<Activity x:Class=\"MergePositionChange\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><TextExpression.NamespacesForImplementation><scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco:Collection x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco:Collection></TextExpression.ReferencesForImplementation><Flowchart DisplayName=\"MergePositionChange\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size><av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,139</av:PointCollection></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Reference>__ReferenceID1</x:Reference></Flowchart.StartNode><FlowStep x:Name=\"__ReferenceID0\"><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><av:Point x:Key=\"ShapeLocation\">185,307.5</av:Point><av:Size x:Key=\"ShapeSize\">230,88</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (0)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"caf369e9-88fe-47bd-a6d8-97d709e6a052\" UpdateAllOccurrences=\"False\"><uaba:DsfDotNetMultiAssignActivity.AmbientDataList><InOutArgument x:TypeArguments=\"scg:List(x:String)\" /></uaba:DsfDotNetMultiAssignActivity.AmbientDataList><uaba:DsfDotNetMultiAssignActivity.FieldsCollection><scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\" WatermarkTextValue=\"Value\"><uaba:ActivityDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\" WatermarkTextValue=\"\"><uaba:ActivityDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO></scg:List></uaba:DsfDotNetMultiAssignActivity.FieldsCollection><uaba:DsfDotNetMultiAssignActivity.ParentInstanceID><InOutArgument x:TypeArguments=\"x:String\" /></uaba:DsfDotNetMultiAssignActivity.ParentInstanceID><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">True</x:Boolean></scg:Dictionary></sap:WorkflowViewStateService.ViewState></uaba:DsfDotNetMultiAssignActivity></FlowStep><FlowStep x:Name=\"__ReferenceID1\"><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><av:Point x:Key=\"ShapeLocation\">160,139</av:Point><av:Size x:Key=\"ShapeSize\">280,122</av:Size><av:PointCollection x:Key=\"ConnectorLocation\">300,261 300,307.5</av:PointCollection></scg:Dictionary></sap:WorkflowViewStateService.ViewState><uaba:DsfDataMergeActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" Result=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DisplayName=\"Data Merge (0)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"280,122\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"a4d2f848-2223-4f95-880a-49781b89e38a\"><uaba:DsfDataMergeActivity.AmbientDataList><InOutArgument x:TypeArguments=\"scg:List(x:String)\" /></uaba:DsfDataMergeActivity.AmbientDataList><uaba:DsfDataMergeActivity.MergeCollection><scg:List x:TypeArguments=\"uaba:DataMergeDTO\" Capacity=\"4\"><uaba:DataMergeDTO Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" Alignment=\"Left\" At=\"\" EnableAt=\"True\" EnablePadding=\"True\" IndexNumber=\"1\" InputVariable=\"\" Inserted=\"False\" IsAtFocused=\"False\" IsFieldNameFocused=\"False\" IsPaddingFocused=\"False\" MergeType=\"Index\" Padding=\"\"><uaba:DataMergeDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:DataMergeDTO.Errors></uaba:DataMergeDTO><uaba:DataMergeDTO Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" Alignment=\"Left\" At=\"\" EnableAt=\"True\" EnablePadding=\"True\" IndexNumber=\"2\" InputVariable=\"\" Inserted=\"False\" IsAtFocused=\"False\" IsFieldNameFocused=\"False\" IsPaddingFocused=\"False\" MergeType=\"Index\" Padding=\"\"><uaba:DataMergeDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:DataMergeDTO.Errors></uaba:DataMergeDTO></scg:List></uaba:DsfDataMergeActivity.MergeCollection><uaba:DsfDataMergeActivity.ParentInstanceID><InOutArgument x:TypeArguments=\"x:String\" /></uaba:DsfDataMergeActivity.ParentInstanceID><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">True</x:Boolean></scg:Dictionary></sap:WorkflowViewStateService.ViewState></uaba:DsfDataMergeActivity><FlowStep.Next><x:Reference>__ReferenceID0</x:Reference></FlowStep.Next></FlowStep></Flowchart></Activity>",
            Different = "<Activity x:Class=\"MergePositionChange\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><TextExpression.NamespacesForImplementation><scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco:Collection x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco:Collection></TextExpression.ReferencesForImplementation><Flowchart DisplayName=\"MergePositionChange\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size><av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,127.5</av:PointCollection></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Reference>__ReferenceID1</x:Reference></Flowchart.StartNode><FlowStep x:Name=\"__ReferenceID1\"><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><av:Point x:Key=\"ShapeLocation\">185,127.5</av:Point><av:Size x:Key=\"ShapeSize\">230,88</av:Size><av:PointCollection x:Key=\"ConnectorLocation\">300,215.5 300,265.5</av:PointCollection></scg:Dictionary></sap:WorkflowViewStateService.ViewState><uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (0)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"caf369e9-88fe-47bd-a6d8-97d709e6a052\" UpdateAllOccurrences=\"False\"><uaba:DsfDotNetMultiAssignActivity.AmbientDataList><InOutArgument x:TypeArguments=\"scg:List(x:String)\" /></uaba:DsfDotNetMultiAssignActivity.AmbientDataList><uaba:DsfDotNetMultiAssignActivity.FieldsCollection><scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\" WatermarkTextValue=\"Value\"><uaba:ActivityDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO><uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\" WatermarkTextValue=\"\"><uaba:ActivityDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:ActivityDTO.Errors><uaba:ActivityDTO.OutList><scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /></uaba:ActivityDTO.OutList></uaba:ActivityDTO></scg:List></uaba:DsfDotNetMultiAssignActivity.FieldsCollection><uaba:DsfDotNetMultiAssignActivity.ParentInstanceID><InOutArgument x:TypeArguments=\"x:String\" /></uaba:DsfDotNetMultiAssignActivity.ParentInstanceID><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">True</x:Boolean></scg:Dictionary></sap:WorkflowViewStateService.ViewState></uaba:DsfDotNetMultiAssignActivity><FlowStep.Next><FlowStep x:Name=\"__ReferenceID0\"><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><av:Point x:Key=\"ShapeLocation\">160,265.5</av:Point><av:Size x:Key=\"ShapeSize\">280,122</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><uaba:DsfDataMergeActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" Result=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DisplayName=\"Data Merge (0)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"280,122\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"a4d2f848-2223-4f95-880a-49781b89e38a\"><uaba:DsfDataMergeActivity.AmbientDataList><InOutArgument x:TypeArguments=\"scg:List(x:String)\" /></uaba:DsfDataMergeActivity.AmbientDataList><uaba:DsfDataMergeActivity.MergeCollection><scg:List x:TypeArguments=\"uaba:DataMergeDTO\" Capacity=\"4\"><uaba:DataMergeDTO Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" Alignment=\"Left\" At=\"\" EnableAt=\"True\" EnablePadding=\"True\" IndexNumber=\"1\" InputVariable=\"\" Inserted=\"False\" IsAtFocused=\"False\" IsFieldNameFocused=\"False\" IsPaddingFocused=\"False\" MergeType=\"Index\" Padding=\"\"><uaba:DataMergeDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:DataMergeDTO.Errors></uaba:DataMergeDTO><uaba:DataMergeDTO Path=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" Alignment=\"Left\" At=\"\" EnableAt=\"True\" EnablePadding=\"True\" IndexNumber=\"2\" InputVariable=\"\" Inserted=\"False\" IsAtFocused=\"False\" IsFieldNameFocused=\"False\" IsPaddingFocused=\"False\" MergeType=\"Index\" Padding=\"\"><uaba:DataMergeDTO.Errors><scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /></uaba:DataMergeDTO.Errors></uaba:DataMergeDTO></scg:List></uaba:DsfDataMergeActivity.MergeCollection><uaba:DsfDataMergeActivity.ParentInstanceID><InOutArgument x:TypeArguments=\"x:String\" /></uaba:DsfDataMergeActivity.ParentInstanceID><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">True</x:Boolean></scg:Dictionary></sap:WorkflowViewStateService.ViewState></uaba:DsfDataMergeActivity></FlowStep></FlowStep.Next></FlowStep><x:Reference>__ReferenceID0</x:Reference></Flowchart></Activity>"
        };
    }

    public class MergeTestUtils
    {
        protected readonly Point location = new Point(10, 10);
        protected DsfMultiAssignActivity multiAssign = new DsfMultiAssignActivity
        {
            UniqueID = Guid.NewGuid().ToString(),
            DisplayName = "MultiAssign"
        };
        protected ModelItem modelItem;
        readonly IServiceDifferenceParser _serviceDifferenceParser = new ServiceDifferenceParser(new ActivityParser(), new ResourceDefinationCleaner());

        protected (List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree) CreateConflictTreeNodeList()
        {
            var resourceModelCurrent = CreateResourceModel(WorkflowTestResources.MergePositionChange.Current);
            var resourceModelDiff = CreateResourceModel(WorkflowTestResources.MergePositionChange.Different);

            var (currentTree, diffTree) = _serviceDifferenceParser.GetDifferences(resourceModelCurrent.Object, resourceModelDiff.Object, false);

            return (currentTree, diffTree);
        }

        protected ToolConflictItem CreateToolConflictItem()
        {
            modelItem = ModelItemUtils.CreateModelItem(multiAssign);
            return ToolConflictItem.NewFromActivity(multiAssign, modelItem, location);
        }

        public static void NewStartConflictItem(out DrawingImage imageSource, out ToolConflictItem startConflictItem)
        {
            imageSource = new DrawingImage();
            startConflictItem = ToolConflictItem.NewStartConflictItem(imageSource);
        }

        private ConflictModelFactory CreateConflictModelFactory(string xamlDefinition)
        {
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(a => a.Current).Returns(Application.Current);
            CustomContainer.Register(mockApplicationAdapter.Object);

            var resourceModel = CreateResourceModel(xamlDefinition);

            var conflictModelFactory = new ConflictModelFactory(resourceModel.Object);
            return conflictModelFactory;
        }

        private static Mock<IContextualResourceModel> CreateResourceModel(string xamlDefinition)
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(a => a.Name).Returns("localhost");

            IResourceDefinationCleaner resourceDefination = new ResourceDefinationCleaner();
            var resourceModel = Dev2MockFactory.SetupResourceModelMock();

            var assignExampleBuilder = new StringBuilder(xamlDefinition);
            var resource = resourceDefination.GetResourceDefinition(true, resourceModel.Object.ID, assignExampleBuilder);
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            resourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            resourceModel.Setup(resModel => resModel.ResourceName).Returns("MergePositionChange");
            resourceModel.Setup(resModel => resModel.Environment).Returns(mockServer.Object);
            resourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            return resourceModel;
        }

        protected ConflictModelFactory CreateCurrentConflictModelFactory()
        {
            return CreateConflictModelFactory(WorkflowTestResources.MergePositionChange.Current);
        }

        protected ConflictModelFactory CreateDiffConflictModelFactory()
        {
            return CreateConflictModelFactory(WorkflowTestResources.MergePositionChange.Different);
        }

        protected ToolConflictRow CreateConflictRow()
        {
            var (currentTree, diffTree) = CreateConflictTreeNodeList();

            var currentConflictModelFactory = CreateCurrentConflictModelFactory();
            var diffConflictModelFactory = CreateDiffConflictModelFactory();

            var currentViewModel = currentConflictModelFactory.CreateToolModelConfictItem(currentTree[0]);
            var diffViewModel = diffConflictModelFactory.CreateToolModelConfictItem(diffTree[0]);

            var connectors = new List<IConnectorConflictRow>();

            return ToolConflictRow.CreateConflictRow(currentViewModel, diffViewModel, connectors);
        }

        protected ToolConflictRow CreateStartRow()
        {
            NewStartConflictItem(out DrawingImage imageSourceCurrent, out ToolConflictItem startConflictItemCurrent);
            NewStartConflictItem(out DrawingImage imageSourceDiff, out ToolConflictItem startConflictItemDiff);

            return ToolConflictRow.CreateStartRow(startConflictItemCurrent, startConflictItemDiff);
        }
    }
}
