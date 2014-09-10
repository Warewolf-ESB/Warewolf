using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.Designers2.Foreach;
using Dev2.AppResources.Repositories;
using Dev2.Collections;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Core.Tests.ViewModelTests;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Diagnostics.Debug;
using Dev2.Factory;
using Dev2.Messages;
using Dev2.Models;
using Dev2.Providers.Errors;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;
using Dev2.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Workflows
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkflowDesignerUnitTest
    {

        private static readonly object TestLock = new object();
        private static bool _isDesignerInited;

        //Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(TestLock);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(TestLock);
        }

        #region Remove Unused Tests

        /// <summary>
        /// Tests Remove All UnusedDataListItems is able remove all the unused data list items from the data list
        /// </summary>
        [TestMethod]
        public void RemoveAllUnusedDataListObjectsWithItemsNotUsedExpectedItemsRemoved()
        {
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);


            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            dataListViewModel.RemoveUnusedDataListItems();
            workflowDesigner.Dispose();
            Assert.AreEqual(0, dataListViewModel.ScalarCollection.Count);

        }

        [TestMethod]
        public void MissingPartsMessageOnlySentWhenThereWorkToDoExpect1Call()
        {
            // Set up event agg

            var evtAg = new Mock<IEventAggregator>();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.ScalarCollection).Returns(new OptomizedObservableCollection<IDataListItemModel>());
            mockDataListViewModel.Setup(model => model.UpdateDataListItems(It.IsAny<IResourceModel>(), It.IsAny<IList<IDataListVerifyPart>>())).Verifiable();
            var dataListViewModel = mockDataListViewModel.Object;
            var dataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            WorkflowDesignerViewModel workflowDesigner = CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(mockResourceModel.Object, evtAg.Object);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.Dispose();
            mockDataListViewModel.Verify(model => model.UpdateDataListItems(It.IsAny<IResourceModel>(), It.IsAny<IList<IDataListVerifyPart>>()), Times.Once());
        }

        #endregion

        #region Add Missing DataList Items

        /// <summary>
        /// Test the AddMissingDataListItems method that it adds all missing data list items to the datalist
        /// </summary>
        [TestMethod]
        public void AddMissingDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsed()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);

            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel.RecsetCollection.Clear();
            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            WorkflowDesignerViewModel workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            workflowDesigner.PopUp = mockPopUp.Object;

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            Assert.AreEqual(2, dataListViewModel.ScalarCollection.Count);
            Assert.AreEqual(0, dataListViewModel.RecsetCollection.Count);
            workflowDesigner.Dispose();
        }


        [TestMethod]
        public void AddMissingDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsedBob()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("rec1().a", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);

            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel.RecsetCollection.Clear();
            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            WorkflowDesignerViewModel workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            workflowDesigner.PopUp = mockPopUp.Object;

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            Assert.AreEqual(2, dataListViewModel.ScalarCollection.Count);
            Assert.AreEqual(0, dataListViewModel.RecsetCollection.Count);
            workflowDesigner.Dispose();
        }

        //2013.06.24: Ashley Lewis for bug 9698 - test for get decision elements
        [TestMethod]
        public void GetDecisionElementsWithMissmatchedBracketsInADecisionFieldExpectedCorrectVariableGottenFromDecision()
        {
            //Execute
            var model = CreateWorkflowDesignerViewModel(Dev2MockFactory.ResourceModel.Object, null, false);

            var mockResourceModel = new Mock<IContextualResourceModel>();
            var dataListViewModel = CreateDataListViewModel(mockResourceModel);
            var actual = model.GetDecisionElements("Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:!]]!,!Col2!:![[scalar]]!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)", dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("scalar", actual[0], "Find missing found an invalid variable in a decision");

        }

        [TestMethod]
        public void GetDecisionElementsWhenItemAlreadyInDataListShouldStillReturnInList()
        {
            //Execute
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.ResourceModel;
            var eventAggregator = new EventAggregator();


            var model = CreateWorkflowDesignerViewModel(eventAggregator, resourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();

            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);

            var recsetModel = new DataListItemModel("RecSet");
            dataListViewModel.RecsetCollection.Add(recsetModel);
            dataListViewModel.RecsetCollection[2].Children.Add(new DataListItemModel("f1", parent: recsetModel));
            const string expression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[RecSet().f1]]!,!Col2!:!Is Equal!,!Col3!:!0!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)";
            var actual = model.GetDecisionElements(expression, dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("RecSet().f1", actual[0], "Find missing found an invalid variable in a decision");
        }

        #endregion

        #region Set Unused DataList Items

        /// <summary>
        /// Test the FindUnusedDataListItems method that it sets all the unused data list items to unused
        /// </summary>
        [TestMethod]
        public void FindUnusedDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsed()
        {
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(mockResourceModel.Object, eventAggregator);
            workflowDesigner.PopUp = mockPopUp.Object;
            //  workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;

            Assert.IsTrue(dataListViewModel.ScalarCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ScalarCollection[1].IsUsed);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.Dispose();
            Assert.IsTrue(!dataListViewModel.ScalarCollection[0].IsUsed);
        }

        #endregion

        #region NotifyItemSelected Tests

        [TestMethod]
        public void NotifyItemSelected_WebpagePreviewNullReferenceBugFix()
        {
            Mock<IContextualResourceModel> resource = Dev2MockFactory.SetupResourceModelMock(); //new Mock<IContextualResourceModel>();
            WorkflowDesignerViewModel wf = CreateWorkflowDesignerViewModel(resource.Object, null, false);
            var page = new DsfWebPageActivity();
            Assert.IsTrue(wf.NotifyItemSelected(page) == false);
            wf.Dispose();
        }

        #endregion NotifyItemSelected Tests

        #region Internal Test Methods

        StringBuilder WorkflowXAMLForTest()
        {
            return new StringBuilder(@"<Activity mc:Ignorable=""sap"" x:Class=""ServiceToBindFrom"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Members>
    <x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" />
    <x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" />
    <x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" />
  </x:Members>
  <sap:VirtualizedContainerService.HintSize>778,1014</sap:VirtualizedContainerService.HintSize>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings>
  <Flowchart sap:VirtualizedContainerService.HintSize=""738,974"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces"">
    <Flowchart.Variables>
      <Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />
      <Variable x:TypeArguments=""x:String"" Name=""LastResult"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />
      <Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />
      <Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />
      <Variable x:TypeArguments=""uaba:Util"" Name=""t"" />
    </Flowchart.Variables>
    <sap:WorkflowViewStateService.ViewState>
      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
        <x:Boolean x:Key=""IsExpanded"">False</x:Boolean>
        <av:Point x:Key=""ShapeLocation"">270,2.5</av:Point>
        <av:Size x:Key=""ShapeSize"">60,75</av:Size>
        <av:PointCollection x:Key=""ConnectorLocation"">300,77.5 300,107.5 150,107.5 150,276</av:PointCollection>
        <x:Double x:Key=""Height"">938.5</x:Double>
        <x:Double x:Key=""Width"">724</x:Double>
      </scg:Dictionary>
    </sap:WorkflowViewStateService.ViewState>
    <Flowchart.StartNode>
      <FlowStep x:Name=""__ReferenceID0"">
        <sap:WorkflowViewStateService.ViewState>
          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
            <av:Point x:Key=""ShapeLocation"">20.5,276</av:Point>
            <av:Size x:Key=""ShapeSize"">259,443</av:Size>
            <av:PointCollection x:Key=""ConnectorLocation"">271.5,280 301.5,280 301.5,342 450,342 450,372</av:PointCollection>
          </scg:Dictionary>
        </sap:WorkflowViewStateService.ViewState>
        <uaba:DsfAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""True"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" FieldName=""[[result]]"" FieldValue=""{}{{&#xA;     var max = &quot;[[max]]&quot;;&#xA;&#xA;     if(max.length &lt; 1){&#xA;&#x9;max = 10;&#xA;     }&#xA;     var i = 0;&#xA;     var id = &quot;&lt;Data&gt;&quot;;&#xA;     var value = new Array();&#xA;     for(var i  = 0; i &lt;= max; i++){&#xA;        id += &quot;&lt;regions&gt;&quot;;&#xA;        id += &quot;&lt;id&gt;&quot;+i+&quot;&lt;/id&gt;&quot;;&#xA;        id += &quot;&lt;name&gt;region&quot;+i + &quot;&lt;/name&gt;&quot;;&#xA;        id += &quot;&lt;/regions&gt;&quot;;&#xA;       }&#xA;  id += &quot;&lt;/Data&gt;&quot;;&#xA;}}"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""259,443"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" UpdateAllOccurrences=""True"" />
      </FlowStep>
    </Flowchart.StartNode>
    <x:Reference>__ReferenceID0</x:Reference>
    <FlowStep>
      <sap:WorkflowViewStateService.ViewState>
        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
          <av:Point x:Key=""ShapeLocation"">393.5,33</av:Point>
          <av:Size x:Key=""ShapeSize"">200,22</av:Size>
        </scg:Dictionary>
      </sap:WorkflowViewStateService.ViewState>
      <uaba:DsfCommentActivity DisplayName=""DEF"" sap:VirtualizedContainerService.HintSize=""200,22"" Text=""01-02-2012 : Travis.Frisinger@dev2.co.za&#xA;&#xA;A testing service used to ensure&#xA;databinding works for both datagrid&#xA;and radio button, checkbox, and drop down."" />
    </FlowStep>
  </Flowchart>
</Activity>");
        }

        public StringBuilder GetAddMissingWorkflowXml()
        {
            return new StringBuilder(@"<Activity mc:Ignorable=""sads sap"" x:Class=""AllTools""
 xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities""
 xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
 xmlns:d=""clr-namespace:Dev2;assembly=Dev2.Core""
 xmlns:di=""clr-namespace:Dev2.Interfaces;assembly=Dev2.Core""
 xmlns:dsca=""clr-namespace:Dev2.Studio.Core.Activities;assembly=Dev2.Studio.Core.Activities""
 xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
 xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities""
 xmlns:s=""clr-namespace:System;assembly=mscorlib""
 xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger""
 xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation""
 xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib""
 xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities""
 xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core""
 xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Members>
    <x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" />
    <x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" />
    <x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" />
  </x:Members>
  <sap:VirtualizedContainerService.HintSize>1350,1096</sap:VirtualizedContainerService.HintSize>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings>
  <Flowchart DisplayName=""AllTools"" sap:VirtualizedContainerService.HintSize=""1310,1056"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces"">
    <Flowchart.Variables>
      <Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />
      <Variable x:TypeArguments=""x:String"" Name=""LastResult"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />
      <Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />
      <Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />
      <Variable x:TypeArguments=""uaba:Util"" Name=""t"" />
    </Flowchart.Variables>
    <sap:WorkflowViewStateService.ViewState>
      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
        <x:Boolean x:Key=""IsExpanded"">False</x:Boolean>
        <av:Point x:Key=""ShapeLocation"">270,2.5</av:Point>
        <av:Size x:Key=""ShapeSize"">60,75</av:Size>
        <x:Double x:Key=""Height"">1020.5</x:Double>
        <x:Double x:Key=""Width"">1295.92</x:Double>
        <av:PointCollection x:Key=""ConnectorLocation"">270,40 170,40 170,83.5</av:PointCollection>
      </scg:Dictionary>
    </sap:WorkflowViewStateService.ViewState>
    <Flowchart.StartNode>
      <FlowStep x:Name=""__ReferenceID4"">
        <sap:WorkflowViewStateService.ViewState>
          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
            <av:Point x:Key=""ShapeLocation"">41,83.5</av:Point>
            <av:Size x:Key=""ShapeSize"">258,113</av:Size>
            <av:PointCollection x:Key=""ConnectorLocation"">299,140 329,140 329,92 350,92</av:PointCollection>
          </scg:Dictionary>
        </sap:WorkflowViewStateService.ViewState>
        <uaba:DsfDataSplitActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Data Split (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""258,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" ReverseOrder=""False"" SimulationMode=""OnDemand"" SourceString=""[[test1]]"" UniqueID=""97f4088a-a5c6-4e34-bfc9-6f10a0a44cd3"">
          <uaba:DsfDataSplitActivity.ParentInstanceID>
            <InOutArgument x:TypeArguments=""x:String"" />
          </uaba:DsfDataSplitActivity.ParentInstanceID>
          <uaba:DsfDataSplitActivity.ResultsCollection>
            <scg:List x:TypeArguments=""uaba:DataSplitDTO"" Capacity=""4"">
              <uaba:DataSplitDTO At="""" EnableAt=""True"" Include=""False"" IndexNumber=""1"" OutputVariable=""[[test2]]"" SplitType=""Index"" WatermarkTextVariable=""[[Recordset().F1]]"">
                <uaba:DataSplitDTO.OutList>
                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                </uaba:DataSplitDTO.OutList>
              </uaba:DataSplitDTO>
              <uaba:DataSplitDTO At="""" EnableAt=""True"" Include=""False"" IndexNumber=""2"" OutputVariable=""[[test3]]"" SplitType=""Index"" WatermarkTextVariable=""[[Recordset().F2]]"">
                <uaba:DataSplitDTO.OutList>
                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                </uaba:DataSplitDTO.OutList>
              </uaba:DataSplitDTO>
              <uaba:DataSplitDTO At="""" EnableAt=""True"" Include=""False"" IndexNumber=""3"" OutputVariable="""" SplitType=""Index"" WatermarkTextVariable="""">
                <uaba:DataSplitDTO.OutList>
                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                </uaba:DataSplitDTO.OutList>
              </uaba:DataSplitDTO>
            </scg:List>
          </uaba:DsfDataSplitActivity.ResultsCollection>
        </uaba:DsfDataSplitActivity>
        <FlowStep.Next>
          <FlowStep x:Name=""__ReferenceID1"">
            <sap:WorkflowViewStateService.ViewState>
              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                <av:Point x:Key=""ShapeLocation"">350,49</av:Point>
                <av:Size x:Key=""ShapeSize"">200,86</av:Size>
                <av:PointCollection x:Key=""ConnectorLocation"">450,135 450,157 460,157</av:PointCollection>
              </scg:Dictionary>
            </sap:WorkflowViewStateService.ViewState>
            <uaba:DsfCountRecordsetActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" CountNumber=""[[test5]]"" DatabindRecursive=""False"" DisplayName=""Count Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,86"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[test4()]]"" SimulationMode=""OnDemand"" UniqueID=""6ffbfe4f-2425-4fa3-805d-4815652f4236"">
              <uaba:DsfCountRecordsetActivity.ParentInstanceID>
                <InOutArgument x:TypeArguments=""x:String"" />
              </uaba:DsfCountRecordsetActivity.ParentInstanceID>
            </uaba:DsfCountRecordsetActivity>
            <FlowStep.Next>
              <FlowStep x:Name=""__ReferenceID12"">
                <sap:WorkflowViewStateService.ViewState>
                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                    <av:Point x:Key=""ShapeLocation"">360,157</av:Point>
                    <av:Size x:Key=""ShapeSize"">200,86</av:Size>
                    <av:PointCollection x:Key=""ConnectorLocation"">360,200 330,200 330,280 269,280</av:PointCollection>
                  </scg:Dictionary>
                </sap:WorkflowViewStateService.ViewState>
                <uaba:DsfFolderRead Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Read Folder"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,86"" InputPath=""[[test5]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test6]]"" SimulationMode=""OnDemand"" UniqueID=""f09a9a50-26b2-4d42-b630-a70a1edb43d8"" Username="""">
                  <uaba:DsfFolderRead.ParentInstanceID>
                    <InOutArgument x:TypeArguments=""x:String"" />
                  </uaba:DsfFolderRead.ParentInstanceID>
                </uaba:DsfFolderRead>
                <FlowStep.Next>
                  <FlowStep x:Name=""__ReferenceID0"">
                    <sap:WorkflowViewStateService.ViewState>
                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                        <av:Point x:Key=""ShapeLocation"">51,223.5</av:Point>
                        <av:Size x:Key=""ShapeSize"">218,113</av:Size>
                        <av:PointCollection x:Key=""ConnectorLocation"">160,336.5 160,367</av:PointCollection>
                      </scg:Dictionary>
                    </sap:WorkflowViewStateService.ViewState>
                    <uaba:DsfForEachActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" test=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""For Each"" FailOnFirstError=""False"" ForEachElementName=""[[test7]]"" FromDisplayName=""[[test7]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""218,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""d1014511-2b7d-4270-8454-a80a36ab821f"">
                      <uaba:DsfForEachActivity.DataFunc>
                        <ActivityFunc x:TypeArguments=""x:String, x:Boolean"" DisplayName=""Data Action"">
                          <ActivityFunc.Argument>
                            <DelegateInArgument x:TypeArguments=""x:String"" Name=""explicitData_20120904020509"" />
                          </ActivityFunc.Argument>
                        </ActivityFunc>
                      </uaba:DsfForEachActivity.DataFunc>
                      <uaba:DsfForEachActivity.ParentInstanceID>
                        <InOutArgument x:TypeArguments=""x:String"" />
                      </uaba:DsfForEachActivity.ParentInstanceID>
                    </uaba:DsfForEachActivity>
                    <FlowStep.Next>
                      <FlowStep x:Name=""__ReferenceID3"">
                        <sap:WorkflowViewStateService.ViewState>
                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                            <av:Point x:Key=""ShapeLocation"">45,367</av:Point>
                            <av:Size x:Key=""ShapeSize"">230,106</av:Size>
                            <av:PointCollection x:Key=""ConnectorLocation"">275,420 305,420 305,380 340,380</av:PointCollection>
                          </scg:Dictionary>
                        </sap:WorkflowViewStateService.ViewState>
                        <uaba:DsfCalculateActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Calculate"" Expression=""sum([[test8]])"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,106"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test9]]"" SimulationMode=""OnDemand"" UniqueID=""99232732-3010-4cfb-8b4a-077a963734a2"">
                          <uaba:DsfCalculateActivity.ParentInstanceID>
                            <InOutArgument x:TypeArguments=""x:String"" />
                          </uaba:DsfCalculateActivity.ParentInstanceID>
                        </uaba:DsfCalculateActivity>
                        <FlowStep.Next>
                          <FlowStep x:Name=""__ReferenceID2"">
                            <sap:WorkflowViewStateService.ViewState>
                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                <av:Point x:Key=""ShapeLocation"">340,337</av:Point>
                                <av:Size x:Key=""ShapeSize"">220,86</av:Size>
                                <av:PointCollection x:Key=""ConnectorLocation"">450,423 450,434.5</av:PointCollection>
                              </scg:Dictionary>
                            </sap:WorkflowViewStateService.ViewState>
                            <uaba:DsfSortRecordsActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Sort Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""220,86"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SelectedSort=""Forward"" SimulationMode=""OnDemand"" SortField=""[[test10]]"" UniqueID=""8a8591df-00bb-48d4-89e6-f19ea4127bca"">
                              <uaba:DsfSortRecordsActivity.ParentInstanceID>
                                <InOutArgument x:TypeArguments=""x:String"" />
                              </uaba:DsfSortRecordsActivity.ParentInstanceID>
                            </uaba:DsfSortRecordsActivity>
                            <FlowStep.Next>
                              <FlowStep x:Name=""__ReferenceID15"">
                                <sap:WorkflowViewStateService.ViewState>
                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                    <av:Point x:Key=""ShapeLocation"">345,434.5</av:Point>
                                    <av:Size x:Key=""ShapeSize"">210,111</av:Size>
                                    <av:PointCollection x:Key=""ConnectorLocation"">345,490 315,490 315,530 304,530</av:PointCollection>
                                  </scg:Dictionary>
                                </sap:WorkflowViewStateService.ViewState>
                                <uaba:DsfFileWrite Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" Append=""False"" DatabindRecursive=""False"" DisplayName=""Write"" FileContents=""[[test12]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""210,111"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test11]]"" Overwrite=""False"" Password="""" Result=""[[test13]]"" SimulationMode=""OnDemand"" UniqueID=""f0ed679b-6dba-41b5-b51b-a5134c109d27"" Username="""">
                                  <uaba:DsfFileWrite.ParentInstanceID>
                                    <InOutArgument x:TypeArguments=""x:String"" />
                                  </uaba:DsfFileWrite.ParentInstanceID>
                                </uaba:DsfFileWrite>
                                <FlowStep.Next>
                                  <FlowStep x:Name=""__ReferenceID17"">
                                    <sap:WorkflowViewStateService.ViewState>
                                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                        <av:Point x:Key=""ShapeLocation"">16,486</av:Point>
                                        <av:Size x:Key=""ShapeSize"">288,88</av:Size>
                                        <av:PointCollection x:Key=""ConnectorLocation"">160,574 150,574 150,583.5</av:PointCollection>
                                      </scg:Dictionary>
                                    </sap:WorkflowViewStateService.ViewState>
                                    <uaba:DsfBaseConvertActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Base Conversion (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""288,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""ba500b52-4940-4f8a-8371-ddaf67b7d5f7"">
                                      <uaba:DsfBaseConvertActivity.AmbientDataList>
                                        <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                      </uaba:DsfBaseConvertActivity.AmbientDataList>
                                      <uaba:DsfBaseConvertActivity.ConvertCollection>
                                        <scg:List x:TypeArguments=""d:BaseConvertTO"" Capacity=""4"">
                                          <d:BaseConvertTO Expressions=""{x:Null}"" WatermarkText=""{x:Null}"" FromExpression=""[[test14]]"" FromType=""Text"" IndexNumber=""1"" ToExpression="""" ToType=""Base 64"" WatermarkTextVariable=""Any data"" />
                                          <d:BaseConvertTO Expressions=""{x:Null}"" WatermarkText=""{x:Null}"" FromExpression=""[[test15]]"" FromType=""Text"" IndexNumber=""2"" ToExpression="""" ToType=""Base 64"" WatermarkTextVariable="""" />
                                          <d:BaseConvertTO Expressions=""{x:Null}"" WatermarkText=""{x:Null}"" FromExpression="""" FromType=""Text"" IndexNumber=""3"" ToExpression="""" ToType=""Base 64"" WatermarkTextVariable="""" />
                                        </scg:List>
                                      </uaba:DsfBaseConvertActivity.ConvertCollection>
                                      <uaba:DsfBaseConvertActivity.ParentInstanceID>
                                        <InOutArgument x:TypeArguments=""x:String"" />
                                      </uaba:DsfBaseConvertActivity.ParentInstanceID>
                                      <sap:WorkflowViewStateService.ViewState>
                                        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                          <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                        </scg:Dictionary>
                                      </sap:WorkflowViewStateService.ViewState>
                                    </uaba:DsfBaseConvertActivity>
                                    <FlowStep.Next>
                                      <FlowStep x:Name=""__ReferenceID19"">
                                        <sap:WorkflowViewStateService.ViewState>
                                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                            <av:Point x:Key=""ShapeLocation"">18.5,583.5</av:Point>
                                            <av:Size x:Key=""ShapeSize"">263,113</av:Size>
                                            <av:PointCollection x:Key=""ConnectorLocation"">281.5,640 310.5,640 310.5,600</av:PointCollection>
                                          </scg:Dictionary>
                                        </sap:WorkflowViewStateService.ViewState>
                                        <uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""263,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test18]]"" SimulationMode=""OnDemand"" UniqueID=""9d4ed269-7c02-415d-ab0a-9b7211f80e91"">
                                          <uaba:DsfDataMergeActivity.AmbientDataList>
                                            <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                          </uaba:DsfDataMergeActivity.AmbientDataList>
                                          <uaba:DsfDataMergeActivity.MergeCollection>
                                            <scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4"">
                                              <uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""True"" IndexNumber=""1"" InputVariable=""[[test16]]"" MergeType=""Chars"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" />
                                              <uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable=""[[test17]]"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" />
                                              <uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""3"" InputVariable="""" MergeType=""None"" Padding="""" WatermarkTextVariable="""" />
                                            </scg:List>
                                          </uaba:DsfDataMergeActivity.MergeCollection>
                                          <uaba:DsfDataMergeActivity.ParentInstanceID>
                                            <InOutArgument x:TypeArguments=""x:String"" />
                                          </uaba:DsfDataMergeActivity.ParentInstanceID>
                                          <sap:WorkflowViewStateService.ViewState>
                                            <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                              <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                            </scg:Dictionary>
                                          </sap:WorkflowViewStateService.ViewState>
                                        </uaba:DsfDataMergeActivity>
                                        <FlowStep.Next>
                                          <FlowStep x:Name=""__ReferenceID20"">
                                            <sap:WorkflowViewStateService.ViewState>
                                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                <av:Point x:Key=""ShapeLocation"">310.5,556</av:Point>
                                                <av:Size x:Key=""ShapeSize"">239,88</av:Size>
                                                <av:PointCollection x:Key=""ConnectorLocation"">430,644 430,652</av:PointCollection>
                                              </scg:Dictionary>
                                            </sap:WorkflowViewStateService.ViewState>
                                            <uaba:DsfCaseConvertActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Case Conversion (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""239,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""1cb10777-e1c4-4246-8263-f28bd06d482c"">
                                              <uaba:DsfCaseConvertActivity.AmbientDataList>
                                                <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                              </uaba:DsfCaseConvertActivity.AmbientDataList>
                                              <uaba:DsfCaseConvertActivity.ConvertCollection>
                                                <scg:List x:TypeArguments=""di:ICaseConvertTO"" Capacity=""4"">
                                                  <d:CaseConvertTO ExpressionToConvert=""{x:Null}"" Expressions=""{x:Null}"" ConvertType=""UPPER"" IndexNumber=""1"" Result=""[[test19]]"" StringToConvert=""[[test19]]"" WatermarkTextVariable=""[[Text]]"" />
                                                  <d:CaseConvertTO ExpressionToConvert=""{x:Null}"" Expressions=""{x:Null}"" ConvertType=""UPPER"" IndexNumber=""2"" Result=""[[test20]]"" StringToConvert=""[[test20]]"" WatermarkTextVariable="""" />
                                                  <d:CaseConvertTO ExpressionToConvert=""{x:Null}"" Expressions=""{x:Null}"" ConvertType=""UPPER"" IndexNumber=""3"" Result="""" StringToConvert="""" WatermarkTextVariable="""" />
                                                </scg:List>
                                              </uaba:DsfCaseConvertActivity.ConvertCollection>
                                              <uaba:DsfCaseConvertActivity.ParentInstanceID>
                                                <InOutArgument x:TypeArguments=""x:String"" />
                                              </uaba:DsfCaseConvertActivity.ParentInstanceID>
                                              <sap:WorkflowViewStateService.ViewState>
                                                <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                  <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                </scg:Dictionary>
                                              </sap:WorkflowViewStateService.ViewState>
                                            </uaba:DsfCaseConvertActivity>
                                            <FlowStep.Next>
                                              <FlowStep x:Name=""__ReferenceID23"">
                                                <sap:WorkflowViewStateService.ViewState>
                                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                    <av:Point x:Key=""ShapeLocation"">313,652</av:Point>
                                                    <av:Size x:Key=""ShapeSize"">234,136</av:Size>
                                                    <av:PointCollection x:Key=""ConnectorLocation"">313,720 283,720 283,770 250,770</av:PointCollection>
                                                  </scg:Dictionary>
                                                </sap:WorkflowViewStateService.ViewState>
                                                <uaba:DsfReplaceActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CaseMatch=""False"" DatabindRecursive=""False"" DisplayName=""Replace"" FieldsToSearch=""[[test21]]"" Find=""[[test22]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""234,136"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" ReplaceWith=""[[test23]]"" Result=""[[test24]]"" SimulationMode=""OnDemand"" UniqueID=""652252f7-ed9f-43aa-896c-0806e105115d"">
                                                  <uaba:DsfReplaceActivity.AmbientDataList>
                                                    <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                  </uaba:DsfReplaceActivity.AmbientDataList>
                                                  <uaba:DsfReplaceActivity.ParentInstanceID>
                                                    <InOutArgument x:TypeArguments=""x:String"" />
                                                  </uaba:DsfReplaceActivity.ParentInstanceID>
                                                  <sap:WorkflowViewStateService.ViewState>
                                                    <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                      <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                    </scg:Dictionary>
                                                  </sap:WorkflowViewStateService.ViewState>
                                                </uaba:DsfReplaceActivity>
                                                <FlowStep.Next>
                                                  <FlowStep x:Name=""__ReferenceID24"">
                                                    <sap:WorkflowViewStateService.ViewState>
                                                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                        <av:Point x:Key=""ShapeLocation"">30,714.5</av:Point>
                                                        <av:Size x:Key=""ShapeSize"">220,111</av:Size>
                                                        <av:PointCollection x:Key=""ConnectorLocation"">140,825.5 140,850 565,850</av:PointCollection>
                                                      </scg:Dictionary>
                                                    </sap:WorkflowViewStateService.ViewState>
                                                    <uaba:DsfFindRecordsActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Find Record Index"" FieldsToSearch=""[[test25]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""220,111"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MatchCase=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test27]]"" SearchCriteria=""[[test26]]"" SearchType=""&lt;"" SimulationMode=""OnDemand"" StartIndex="""" UniqueID=""7caa5d01-a8cb-4881-aa5b-965e48e29e36"">
                                                      <uaba:DsfFindRecordsActivity.AmbientDataList>
                                                        <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                      </uaba:DsfFindRecordsActivity.AmbientDataList>
                                                      <uaba:DsfFindRecordsActivity.ParentInstanceID>
                                                        <InOutArgument x:TypeArguments=""x:String"" />
                                                      </uaba:DsfFindRecordsActivity.ParentInstanceID>
                                                      <sap:WorkflowViewStateService.ViewState>
                                                        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                          <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                        </scg:Dictionary>
                                                      </sap:WorkflowViewStateService.ViewState>
                                                    </uaba:DsfFindRecordsActivity>
                                                    <FlowStep.Next>
                                                      <FlowStep x:Name=""__ReferenceID25"">
                                                        <sap:WorkflowViewStateService.ViewState>
                                                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                            <av:Point x:Key=""ShapeLocation"">565,769.5</av:Point>
                                                            <av:Size x:Key=""ShapeSize"">250,161</av:Size>
                                                            <av:PointCollection x:Key=""ConnectorLocation"">690,769.5 710,769.5 710,758</av:PointCollection>
                                                          </scg:Dictionary>
                                                        </sap:WorkflowViewStateService.ViewState>
                                                        <uaba:DsfDateTimeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DateTime=""[[test28]]"" DisplayName=""Date and Time"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,161"" InputFormat=""[[test29]]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputFormat=""[[test31]]"" Result=""[[test32]]"" SimulationMode=""OnDemand"" TimeModifierAmount=""0"" TimeModifierAmountDisplay=""[[test30]]"" TimeModifierType="""" UniqueID=""4af25a0a-0e74-4aa1-a1ee-8ed5044fb7fe"">
                                                          <uaba:DsfDateTimeActivity.AmbientDataList>
                                                            <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                          </uaba:DsfDateTimeActivity.AmbientDataList>
                                                          <uaba:DsfDateTimeActivity.ParentInstanceID>
                                                            <InOutArgument x:TypeArguments=""x:String"" />
                                                          </uaba:DsfDateTimeActivity.ParentInstanceID>
                                                          <sap:WorkflowViewStateService.ViewState>
                                                            <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                              <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                            </scg:Dictionary>
                                                          </sap:WorkflowViewStateService.ViewState>
                                                        </uaba:DsfDateTimeActivity>
                                                        <FlowStep.Next>
                                                          <FlowStep x:Name=""__ReferenceID22"">
                                                            <sap:WorkflowViewStateService.ViewState>
                                                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                <av:Point x:Key=""ShapeLocation"">610,622</av:Point>
                                                                <av:Size x:Key=""ShapeSize"">200,136</av:Size>
                                                                <av:PointCollection x:Key=""ConnectorLocation"">710,622 690,622 690,613.5</av:PointCollection>
                                                              </scg:Dictionary>
                                                            </sap:WorkflowViewStateService.ViewState>
                                                            <uaba:DsfNumberFormatActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DecimalPlacesToShow=""[[test35]]"" DisplayName=""Format Number"" Expression=""[[test33]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,136"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test36]]"" RoundingDecimalPlaces=""[[test34]]"" RoundingType=""Up"" SimulationMode=""OnDemand"" UniqueID=""af9c603a-5abc-4fb4-a214-1ab4ecce85d4"">
                                                              <uaba:DsfNumberFormatActivity.AmbientDataList>
                                                                <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                              </uaba:DsfNumberFormatActivity.AmbientDataList>
                                                              <uaba:DsfNumberFormatActivity.ParentInstanceID>
                                                                <InOutArgument x:TypeArguments=""x:String"" />
                                                              </uaba:DsfNumberFormatActivity.ParentInstanceID>
                                                              <sap:WorkflowViewStateService.ViewState>
                                                                <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                  <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                                </scg:Dictionary>
                                                              </sap:WorkflowViewStateService.ViewState>
                                                            </uaba:DsfNumberFormatActivity>
                                                            <FlowStep.Next>
                                                              <FlowStep x:Name=""__ReferenceID18"">
                                                                <sap:WorkflowViewStateService.ViewState>
                                                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                    <av:Point x:Key=""ShapeLocation"">565,526.5</av:Point>
                                                                    <av:Size x:Key=""ShapeSize"">250,87</av:Size>
                                                                    <av:PointCollection x:Key=""ConnectorLocation"">690,526.5 710,526.5 710,505.5</av:PointCollection>
                                                                  </scg:Dictionary>
                                                                </sap:WorkflowViewStateService.ViewState>
                                                                <uaba:DsfActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""CalculateTaxReturns"" FriendlySourceName=""localhost"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,87"" IconPath=""pack://application:,,,/Warewolf Studio;component/images/workflowservice2.png"" InputMapping=""&lt;Inputs&gt;&lt;Input Name=&quot;EmpNo&quot; Source=&quot;[[test37]]&quot; /&gt;&lt;Input Name=&quot;TaxNumber&quot; Source=&quot;[[test38]]&quot; /&gt;&lt;Input Name=&quot;AnualIncome&quot; Source=&quot;[[test39]]&quot; /&gt;&lt;Input Name=&quot;IncomeAfterTax&quot; Source=&quot;[[test40]]&quot; /&gt;&lt;Input Name=&quot;AmountOfTaxReturned&quot; Source=&quot;[[test41]]&quot; /&gt;&lt;/Inputs&gt;"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""True"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&lt;Outputs&gt;&lt;Output Name=&quot;EmpNo&quot; MapsTo=&quot;EmpNo&quot; Value=&quot;[[test42]]&quot; /&gt;&lt;Output Name=&quot;TaxNumber&quot; MapsTo=&quot;TaxNumber&quot; Value=&quot;[[test43]]&quot; /&gt;&lt;Output Name=&quot;AnualIncome&quot; MapsTo=&quot;AnualIncome&quot; Value=&quot;[[test44]]&quot; /&gt;&lt;Output Name=&quot;IncomeAfterTax&quot; MapsTo=&quot;IncomeAfterTax&quot; Value=&quot;[[test45]]&quot; /&gt;&lt;Output Name=&quot;AmountOfTaxReturned&quot; MapsTo=&quot;AmountOfTaxReturned&quot; Value=&quot;[[test46]]&quot; /&gt;&lt;/Outputs&gt;"" RemoveInputFromOutput=""False"" ServiceName=""CalculateTaxReturns"" SimulationMode=""OnDemand"" ToolboxFriendlyName=""CalculateTaxReturns"" Type=""Workflow"" UniqueID=""4f7e8a6b-632e-48d1-83af-857cac4e7676"">
                                                                  <uaba:DsfActivity.AmbientDataList>
                                                                    <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                                  </uaba:DsfActivity.AmbientDataList>
                                                                  <uaba:DsfActivity.HelpLink>
                                                                    <InArgument x:TypeArguments=""x:String"">
                                                                      <Literal x:TypeArguments=""x:String"" Value="""" />
                                                                    </InArgument>
                                                                  </uaba:DsfActivity.HelpLink>
                                                                  <uaba:DsfActivity.ParentInstanceID>
                                                                    <InOutArgument x:TypeArguments=""x:String"" />
                                                                  </uaba:DsfActivity.ParentInstanceID>
                                                                  <sap:WorkflowViewStateService.ViewState>
                                                                    <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                      <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                                    </scg:Dictionary>
                                                                  </sap:WorkflowViewStateService.ViewState>
                                                                </uaba:DsfActivity>
                                                                <FlowStep.Next>
                                                                  <FlowStep x:Name=""__ReferenceID16"">
                                                                    <sap:WorkflowViewStateService.ViewState>
                                                                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                        <av:Point x:Key=""ShapeLocation"">585,394.5</av:Point>
                                                                        <av:Size x:Key=""ShapeSize"">250,111</av:Size>
                                                                        <av:PointCollection x:Key=""ConnectorLocation"">710,394.5 710,383</av:PointCollection>
                                                                      </scg:Dictionary>
                                                                    </sap:WorkflowViewStateService.ViewState>
                                                                    <uaba:DsfZip Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" ArchiveName="""" ArchivePassword="""" CompressionRatio="""" DatabindRecursive=""False"" DisplayName=""Zip"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test47]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test48]]"" Password="""" Result=""[[test49]]"" SimulationMode=""OnDemand"" UniqueID=""df13d832-15cd-41b6-8139-a0036a656061"" Username="""">
                                                                      <uaba:DsfZip.ParentInstanceID>
                                                                        <InOutArgument x:TypeArguments=""x:String"" />
                                                                      </uaba:DsfZip.ParentInstanceID>
                                                                    </uaba:DsfZip>
                                                                    <FlowStep.Next>
                                                                      <FlowStep x:Name=""__ReferenceID9"">
                                                                        <sap:WorkflowViewStateService.ViewState>
                                                                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                            <av:Point x:Key=""ShapeLocation"">585,297</av:Point>
                                                                            <av:Size x:Key=""ShapeSize"">250,86</av:Size>
                                                                            <av:PointCollection x:Key=""ConnectorLocation"">710,297 710,295.5</av:PointCollection>
                                                                          </scg:Dictionary>
                                                                        </sap:WorkflowViewStateService.ViewState>
                                                                        <uaba:DsfPathDelete Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Delete"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,86"" InputPath=""[[test50]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test51]]"" SimulationMode=""OnDemand"" UniqueID=""dc0af9b7-a9b9-46fb-8a91-1636be6aeeb5"" Username="""">
                                                                          <uaba:DsfPathDelete.ParentInstanceID>
                                                                            <InOutArgument x:TypeArguments=""x:String"" />
                                                                          </uaba:DsfPathDelete.ParentInstanceID>
                                                                        </uaba:DsfPathDelete>
                                                                        <FlowStep.Next>
                                                                          <FlowStep x:Name=""__ReferenceID10"">
                                                                            <sap:WorkflowViewStateService.ViewState>
                                                                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                <av:Point x:Key=""ShapeLocation"">585,184.5</av:Point>
                                                                                <av:Size x:Key=""ShapeSize"">250,111</av:Size>
                                                                                <av:PointCollection x:Key=""ConnectorLocation"">710,184.5 700,184.5 700,175.5</av:PointCollection>
                                                                              </scg:Dictionary>
                                                                            </sap:WorkflowViewStateService.ViewState>
                                                                            <uaba:DsfPathMove Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Move"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test52]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test53]]"" Overwrite=""False"" Password="""" Result=""[[test54]]"" SimulationMode=""OnDemand"" UniqueID=""0eab6512-5425-45e4-845c-0851f1c337eb"" Username="""">
                                                                              <uaba:DsfPathMove.ParentInstanceID>
                                                                                <InOutArgument x:TypeArguments=""x:String"" />
                                                                              </uaba:DsfPathMove.ParentInstanceID>
                                                                            </uaba:DsfPathMove>
                                                                            <FlowStep.Next>
                                                                              <FlowStep x:Name=""__ReferenceID14"">
                                                                                <sap:WorkflowViewStateService.ViewState>
                                                                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                    <av:Point x:Key=""ShapeLocation"">585,64.5</av:Point>
                                                                                    <av:Size x:Key=""ShapeSize"">230,111</av:Size>
                                                                                    <av:PointCollection x:Key=""ConnectorLocation"">700,64.5 700,53</av:PointCollection>
                                                                                  </scg:Dictionary>
                                                                                </sap:WorkflowViewStateService.ViewState>
                                                                                <uaba:DsfUnZip Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" ArchivePassword="""" DatabindRecursive=""False"" DisplayName=""UnZip"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,111"" InputPath=""[[test55]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test56]]"" Overwrite=""False"" Password="""" Result=""[[test57]]"" SimulationMode=""OnDemand"" UniqueID=""18133af1-9540-47f4-9c13-c144dbf78529"" Username="""">
                                                                                  <uaba:DsfUnZip.ParentInstanceID>
                                                                                    <InOutArgument x:TypeArguments=""x:String"" />
                                                                                  </uaba:DsfUnZip.ParentInstanceID>
                                                                                </uaba:DsfUnZip>
                                                                                <FlowStep.Next>
                                                                                  <FlowStep x:Name=""__ReferenceID5"">
                                                                                    <sap:WorkflowViewStateService.ViewState>
                                                                                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                        <av:Point x:Key=""ShapeLocation"">600,7</av:Point>
                                                                                        <av:Size x:Key=""ShapeSize"">200,46</av:Size>
                                                                                        <av:PointCollection x:Key=""ConnectorLocation"">800,30 830,30 830,74.5 860,74.5</av:PointCollection>
                                                                                      </scg:Dictionary>
                                                                                    </sap:WorkflowViewStateService.ViewState>
                                                                                    <uaba:DsfWebPageActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" FriendlySourceName=""{x:Null}"" HelpLink=""{x:Null}"" IconPath=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" ToolboxFriendlyName=""{x:Null}"" Type=""{x:Null}"" WebsiteRegionName=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""Webpage 1"" FormEncodingType=""application/x-www-form-urlencoded"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,46"" InputMapping=""&lt;Inputs&gt;&lt;Input Name=&quot;asdas&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;Variable&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sadag&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sdfsdfsdf&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;asdasd&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;asda&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sczxvxcv&quot; Source=&quot;&quot; /&gt;&lt;/Inputs&gt;"" InstructionList=""[InstructionList]"" IsPreview=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MetaTags="""" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&lt;Outputs&gt;&lt;Output Name=&quot;asdas&quot; MapsTo=&quot;asdas&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;Variable&quot; MapsTo=&quot;Variable&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sadag&quot; MapsTo=&quot;sadag&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sdfsdfsdf&quot; MapsTo=&quot;sdfsdfsdf&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;asdasd&quot; MapsTo=&quot;asdasd&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;asda&quot; MapsTo=&quot;asda&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sczxvxcv&quot; MapsTo=&quot;sczxvxcv&quot; Value=&quot;&quot; /&gt;&lt;/Outputs&gt;"" RemoveInputFromOutput=""False"" ServiceName=""Webpage 1"" SimulationMode=""OnDemand"" UniqueID=""f1aaab83-dbf2-41d3-8122-4cb838339213"" WebsiteServiceName=""Default Master Page"" XMLConfiguration=""&lt;WebParts/&gt;"">
                                                                                      <uaba:DsfWebPageActivity.ParentInstanceID>
                                                                                        <InOutArgument x:TypeArguments=""x:String"" />
                                                                                      </uaba:DsfWebPageActivity.ParentInstanceID>
                                                                                    </uaba:DsfWebPageActivity>
                                                                                    <FlowStep.Next>
                                                                                      <FlowStep x:Name=""__ReferenceID7"">
                                                                                        <sap:WorkflowViewStateService.ViewState>
                                                                                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                            <av:Point x:Key=""ShapeLocation"">860,19</av:Point>
                                                                                            <av:Size x:Key=""ShapeSize"">250,111</av:Size>
                                                                                            <av:PointCollection x:Key=""ConnectorLocation"">985,130 985,156 1005,156</av:PointCollection>
                                                                                          </scg:Dictionary>
                                                                                        </sap:WorkflowViewStateService.ViewState>
                                                                                        <uaba:DsfPathCopy Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Copy"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test58]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test59]]"" Overwrite=""False"" Password="""" Result=""[[test60]]"" SimulationMode=""OnDemand"" UniqueID=""6044030a-6fec-4bc9-a20b-7ec97c8e148c"" Username="""">
                                                                                          <uaba:DsfPathCopy.ParentInstanceID>
                                                                                            <InOutArgument x:TypeArguments=""x:String"" />
                                                                                          </uaba:DsfPathCopy.ParentInstanceID>
                                                                                        </uaba:DsfPathCopy>
                                                                                        <FlowStep.Next>
                                                                                          <FlowStep x:Name=""__ReferenceID8"">
                                                                                            <sap:WorkflowViewStateService.ViewState>
                                                                                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                <av:Point x:Key=""ShapeLocation"">880,156</av:Point>
                                                                                                <av:Size x:Key=""ShapeSize"">250,86</av:Size>
                                                                                                <av:PointCollection x:Key=""ConnectorLocation"">1005,242 1005,259 1015,259</av:PointCollection>
                                                                                              </scg:Dictionary>
                                                                                            </sap:WorkflowViewStateService.ViewState>
                                                                                            <uaba:DsfPathCreate Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Create"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,86"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test61]]"" Overwrite=""False"" Password="""" Result=""[[test62]]"" SimulationMode=""OnDemand"" UniqueID=""11e7b5b4-d821-4a0b-8b64-d5a37a44349a"" Username="""">
                                                                                              <uaba:DsfPathCreate.ParentInstanceID>
                                                                                                <InOutArgument x:TypeArguments=""x:String"" />
                                                                                              </uaba:DsfPathCreate.ParentInstanceID>
                                                                                            </uaba:DsfPathCreate>
                                                                                            <FlowStep.Next>
                                                                                              <FlowStep x:Name=""__ReferenceID11"">
                                                                                                <sap:WorkflowViewStateService.ViewState>
                                                                                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                    <av:Point x:Key=""ShapeLocation"">910,259</av:Point>
                                                                                                    <av:Size x:Key=""ShapeSize"">210,86</av:Size>
                                                                                                    <av:PointCollection x:Key=""ConnectorLocation"">1015,345 1015,354.5 1020,354.5</av:PointCollection>
                                                                                                  </scg:Dictionary>
                                                                                                </sap:WorkflowViewStateService.ViewState>
                                                                                                <uaba:DsfFileRead Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Read File"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""210,86"" InputPath=""[[test63]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test64]]"" SimulationMode=""OnDemand"" UniqueID=""bf8f7e76-8567-40ae-9afa-c989781579f0"" Username="""">
                                                                                                  <uaba:DsfFileRead.ParentInstanceID>
                                                                                                    <InOutArgument x:TypeArguments=""x:String"" />
                                                                                                  </uaba:DsfFileRead.ParentInstanceID>
                                                                                                </uaba:DsfFileRead>
                                                                                                <FlowStep.Next>
                                                                                                  <FlowStep x:Name=""__ReferenceID13"">
                                                                                                    <sap:WorkflowViewStateService.ViewState>
                                                                                                      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                        <av:Point x:Key=""ShapeLocation"">895,354.5</av:Point>
                                                                                                        <av:Size x:Key=""ShapeSize"">250,111</av:Size>
                                                                                                        <av:PointCollection x:Key=""ConnectorLocation"">1020,465.5 1000,465.5 1000,486</av:PointCollection>
                                                                                                      </scg:Dictionary>
                                                                                                    </sap:WorkflowViewStateService.ViewState>
                                                                                                    <uaba:DsfPathRename Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Rename"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test65]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test66]]"" Overwrite=""False"" Password="""" Result=""[[test67]]"" SimulationMode=""OnDemand"" UniqueID=""127da2c5-6741-4418-94f3-43831521cc87"" Username="""">
                                                                                                      <uaba:DsfPathRename.ParentInstanceID>
                                                                                                        <InOutArgument x:TypeArguments=""x:String"" />
                                                                                                      </uaba:DsfPathRename.ParentInstanceID>
                                                                                                    </uaba:DsfPathRename>
                                                                                                    <FlowStep.Next>
                                                                                                      <FlowStep x:Name=""__ReferenceID6"">
                                                                                                        <sap:WorkflowViewStateService.ViewState>
                                                                                                          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                            <av:Point x:Key=""ShapeLocation"">861,486</av:Point>
                                                                                                            <av:Size x:Key=""ShapeSize"">278,88</av:Size>
                                                                                                            <av:PointCollection x:Key=""ConnectorLocation"">1000,574 980,574 980,589.5</av:PointCollection>
                                                                                                          </scg:Dictionary>
                                                                                                        </sap:WorkflowViewStateService.ViewState>
                                                                                                        <uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""278,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""62c410de-109f-4f95-a262-0ba8ebfbae64"" UpdateAllOccurrences=""False"">
                                                                                                          <uaba:DsfMultiAssignActivity.FieldsCollection>
                                                                                                            <scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
                                                                                                              <uaba:ActivityDTO FieldName=""[[test68]]"" FieldValue=""[[test69]]"" IndexNumber=""1"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
                                                                                                                <uaba:ActivityDTO.OutList>
                                                                                                                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                                                                                                                </uaba:ActivityDTO.OutList>
                                                                                                              </uaba:ActivityDTO>
                                                                                                              <uaba:ActivityDTO FieldName=""[[test70]]"" FieldValue=""[[test71]]"" IndexNumber=""2"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]"">
                                                                                                                <uaba:ActivityDTO.OutList>
                                                                                                                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                                                                                                                </uaba:ActivityDTO.OutList>
                                                                                                              </uaba:ActivityDTO>
                                                                                                              <uaba:ActivityDTO WatermarkTextValue=""{x:Null}"" WatermarkTextVariable=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""3"">
                                                                                                                <uaba:ActivityDTO.OutList>
                                                                                                                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                                                                                                                </uaba:ActivityDTO.OutList>
                                                                                                              </uaba:ActivityDTO>
                                                                                                            </scg:List>
                                                                                                          </uaba:DsfMultiAssignActivity.FieldsCollection>
                                                                                                          <uaba:DsfMultiAssignActivity.ParentInstanceID>
                                                                                                            <InOutArgument x:TypeArguments=""x:String"" />
                                                                                                          </uaba:DsfMultiAssignActivity.ParentInstanceID>
                                                                                                        </uaba:DsfMultiAssignActivity>
                                                                                                        <FlowStep.Next>
                                                                                                          <FlowStep x:Name=""__ReferenceID21"">
                                                                                                            <sap:WorkflowViewStateService.ViewState>
                                                                                                              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                                <av:Point x:Key=""ShapeLocation"">867.5,589.5</av:Point>
                                                                                                                <av:Size x:Key=""ShapeSize"">225,161</av:Size>
                                                                                                                <av:PointCollection x:Key=""ConnectorLocation"">980,750.5 970,750.5 970,759.5</av:PointCollection>
                                                                                                              </scg:Dictionary>
                                                                                                            </sap:WorkflowViewStateService.ViewState>
                                                                                                            <uaba:DsfIndexActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" Characters=""[[test73]]"" DatabindRecursive=""False"" Direction=""Left to Right"" DisplayName=""Find Index"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""225,161"" InField=""[[test72]]"" Index=""First Occurrence"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MatchCase=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test74]]"" SimulationMode=""OnDemand"" StartIndex=""0"" UniqueID=""3e62d9e0-44ee-44cf-8cb4-16b6cc797aab"">
                                                                                                              <uaba:DsfIndexActivity.AmbientDataList>
                                                                                                                <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                                                                              </uaba:DsfIndexActivity.AmbientDataList>
                                                                                                              <uaba:DsfIndexActivity.ParentInstanceID>
                                                                                                                <InOutArgument x:TypeArguments=""x:String"" />
                                                                                                              </uaba:DsfIndexActivity.ParentInstanceID>
                                                                                                              <sap:WorkflowViewStateService.ViewState>
                                                                                                                <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                                  <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                                                                                </scg:Dictionary>
                                                                                                              </sap:WorkflowViewStateService.ViewState>
                                                                                                            </uaba:DsfIndexActivity>
                                                                                                            <FlowStep.Next>
                                                                                                              <FlowStep x:Name=""__ReferenceID26"">
                                                                                                                <sap:WorkflowViewStateService.ViewState>
                                                                                                                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                                    <av:Point x:Key=""ShapeLocation"">845,759.5</av:Point>
                                                                                                                    <av:Size x:Key=""ShapeSize"">250,161</av:Size>
                                                                                                                  </scg:Dictionary>
                                                                                                                </sap:WorkflowViewStateService.ViewState>
                                                                                                                <uaba:DsfDateTimeDifferenceActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" Explicit=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Date and Time Difference"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,161"" Input1=""[[test75]]"" Input2=""[[test76]]"" InputFormat=""[[test77]]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputType=""Years"" Result=""[[test78]]"" SimulationMode=""OnDemand"" UniqueID=""33b638ef-4afb-421a-b2ba-e20b02cc4930"">
                                                                                                                  <uaba:DsfDateTimeDifferenceActivity.AmbientDataList>
                                                                                                                    <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
                                                                                                                  </uaba:DsfDateTimeDifferenceActivity.AmbientDataList>
                                                                                                                  <uaba:DsfDateTimeDifferenceActivity.ParentInstanceID>
                                                                                                                    <InOutArgument x:TypeArguments=""x:String"" />
                                                                                                                  </uaba:DsfDateTimeDifferenceActivity.ParentInstanceID>
                                                                                                                  <sap:WorkflowViewStateService.ViewState>
                                                                                                                    <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                                                                                                                      <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                                                                                                                    </scg:Dictionary>
                                                                                                                  </sap:WorkflowViewStateService.ViewState>
                                                                                                                </uaba:DsfDateTimeDifferenceActivity>
                                                                                                              </FlowStep>
                                                                                                            </FlowStep.Next>
                                                                                                          </FlowStep>
                                                                                                        </FlowStep.Next>
                                                                                                      </FlowStep>
                                                                                                    </FlowStep.Next>
                                                                                                  </FlowStep>
                                                                                                </FlowStep.Next>
                                                                                              </FlowStep>
                                                                                            </FlowStep.Next>
                                                                                          </FlowStep>
                                                                                        </FlowStep.Next>
                                                                                      </FlowStep>
                                                                                    </FlowStep.Next>
                                                                                  </FlowStep>
                                                                                </FlowStep.Next>
                                                                              </FlowStep>
                                                                            </FlowStep.Next>
                                                                          </FlowStep>
                                                                        </FlowStep.Next>
                                                                      </FlowStep>
                                                                    </FlowStep.Next>
                                                                  </FlowStep>
                                                                </FlowStep.Next>
                                                              </FlowStep>
                                                            </FlowStep.Next>
                                                          </FlowStep>
                                                        </FlowStep.Next>
                                                      </FlowStep>
                                                    </FlowStep.Next>
                                                  </FlowStep>
                                                </FlowStep.Next>
                                              </FlowStep>
                                            </FlowStep.Next>
                                          </FlowStep>
                                        </FlowStep.Next>
                                      </FlowStep>
                                    </FlowStep.Next>
                                  </FlowStep>
                                </FlowStep.Next>
                              </FlowStep>
                            </FlowStep.Next>
                          </FlowStep>
                        </FlowStep.Next>
                      </FlowStep>
                    </FlowStep.Next>
                  </FlowStep>
                </FlowStep.Next>
              </FlowStep>
            </FlowStep.Next>
          </FlowStep>
        </FlowStep.Next>
      </FlowStep>
    </Flowchart.StartNode>
    <x:Reference>__ReferenceID0</x:Reference>
    <x:Reference>__ReferenceID1</x:Reference>
    <x:Reference>__ReferenceID2</x:Reference>
    <x:Reference>__ReferenceID3</x:Reference>
    <FlowStep>
      <sap:WorkflowViewStateService.ViewState>
        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
          <av:Point x:Key=""ShapeLocation"">359,253</av:Point>
          <av:Size x:Key=""ShapeSize"">202,74</av:Size>
        </scg:Dictionary>
      </sap:WorkflowViewStateService.ViewState>
      <uaba:DsfCommentActivity Text=""{x:Null}"" DisplayName=""Comment"" sap:VirtualizedContainerService.HintSize=""202,74"" />
    </FlowStep>
    <x:Reference>__ReferenceID4</x:Reference>
    <x:Reference>__ReferenceID5</x:Reference>
    <x:Reference>__ReferenceID6</x:Reference>
    <x:Reference>__ReferenceID7</x:Reference>
    <x:Reference>__ReferenceID8</x:Reference>
    <x:Reference>__ReferenceID9</x:Reference>
    <x:Reference>__ReferenceID10</x:Reference>
    <x:Reference>__ReferenceID11</x:Reference>
    <x:Reference>__ReferenceID12</x:Reference>
    <x:Reference>__ReferenceID13</x:Reference>
    <x:Reference>__ReferenceID14</x:Reference>
    <x:Reference>__ReferenceID15</x:Reference>
    <x:Reference>__ReferenceID16</x:Reference>
    <x:Reference>__ReferenceID17</x:Reference>
    <x:Reference>__ReferenceID18</x:Reference>
    <x:Reference>__ReferenceID19</x:Reference>
    <x:Reference>__ReferenceID20</x:Reference>
    <x:Reference>__ReferenceID21</x:Reference>
    <x:Reference>__ReferenceID22</x:Reference>
    <x:Reference>__ReferenceID23</x:Reference>
    <x:Reference>__ReferenceID24</x:Reference>
    <x:Reference>__ReferenceID25</x:Reference>
    <x:Reference>__ReferenceID26</x:Reference>
  </Flowchart>
</Activity>");
        }

        #endregion Internal Test Methods

        #region Update Resource Message Handler

        //2013.02.11: Ashley Lewis - Bug 8553
        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            //var designerAttributes = new Dictionary<Type, Type>();
            //workflowDesigner.InitializeDesigner(designerAttributes);

            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            workflowDesigner.Handle(updatemsg);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");
            updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            workflowDesigner.Handle(updatemsg);

            Assert.AreEqual("Testing2", workflowDesigner.ResourceModel.Category);
            workflowDesigner.Dispose();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_BuildDataPart")]
        public void WorkflowDesignerViewModel_BuildDataPart_ValidItem_ShouldAddItemToDataList()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();
            DataListSingleton.SetDataList(dataListViewModel);
            dataListViewModel.AddBlankRow(null);
            //------------Execute Test---------------------------
            workflowDesigner.Handle(new AddStringListToDataListMessage(new List<string> { "[[rec().set]]", "[[test()]]", "[[scalar]]" }));
            //------------Assert Results-------------------------
            var dataListItemModels = DataListSingleton.ActiveDataList.DataList;
            workflowDesigner.Dispose();
            Assert.AreEqual(5, dataListItemModels.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_BuildDataPart")]
        public void WorkflowDesignerViewModel_BuildDataPart_InValidItems_ShouldNotAddItemToDataList()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();
            DataListSingleton.SetDataList(dataListViewModel);
            dataListViewModel.AddBlankRow(null);
            //------------Execute Test---------------------------
            workflowDesigner.Handle(new AddStringListToDataListMessage(new List<string> { "[[rec().s*et]]", "[[test**()]]", "[[1scalar]]" }));
            workflowDesigner.Dispose();
            //------------Assert Results--------------------------
            var dataListItemModels = DataListSingleton.ActiveDataList.DataList;
            Assert.AreEqual(3, dataListItemModels.Count);
            Assert.AreEqual("", dataListItemModels[0].DisplayName);
            Assert.AreEqual("rec()", dataListItemModels[1].DisplayName);
            Assert.AreEqual("", dataListItemModels[1].Children[0].DisplayName);
            Assert.AreEqual("", dataListItemModels[2].DisplayName);
        }

        #endregion

        #region InitializeDesigner

        // - start

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowDesigner_Initialize")]
        public void WorkflowDesigner_Initialize_WhenWorkflowXamlNull_ExpectWorkflowXamlFetch()
        {

            //------------Setup for test--------------------------
            var repo = new Mock<IResourceRepository>();
            repo.Setup(c => c.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage { Message = null });
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() }).Verifiable();
            wh.Setup(h => h.EnsureImplementation(It.IsAny<ModelService>())).Verifiable();
            wh.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Verifiable();

            //------------Execute Test---------------------------
            var wfd = CreateWorkflowDesignerViewModel(crm.Object, wh.Object);

            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            wfd.Dispose();

            //------------Assert Results-------------------------
            wh.Verify(h => h.CreateWorkflow(It.IsAny<string>()));
            wh.Verify(h => h.EnsureImplementation(It.IsAny<ModelService>()));

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowDesigner_Initialize")]
        public void WorkflowDesigner_Initialize_WhenWorkflowXamlNullAndFetchFails_ExpectNewWorkflow()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {

                    #region Setup viewModel

                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    ExecuteMessage exeMsg = null;
                    // ReSharper disable ExpressionIsAlwaysNull
                    resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(exeMsg);
                    // ReSharper restore ExpressionIsAlwaysNull

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    resourceModel.Setup(r => r.ResourceName).Returns("Test");
                    StringBuilder xamlBuilder = new StringBuilder("abc");

                    var workflowHelper = new Mock<IWorkflowHelper>();

                    var ok2 = false;
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() =>
                    {
                        ok2 = true;
                        return workflow;
                    });
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);

                    #endregion

                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment

                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolote functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    viewModel.LoadXaml();

                    // verify CreateWorkflow called
                    Assert.IsTrue(ok2);
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);

        }

        // - end

        // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInitializesFramework45Properties()
        {
            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wfd = CreateWorkflowDesignerViewModel(crm.Object);
            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            var designerConfigService = wfd.Designer.Context.Services.GetService<DesignerConfigurationService>();
            Assert.AreEqual(new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5)), designerConfigService.TargetFrameworkName);
            Assert.IsTrue(designerConfigService.AutoConnectEnabled);
            Assert.IsTrue(designerConfigService.AutoSplitEnabled);
            Assert.IsTrue(designerConfigService.BackgroundValidationEnabled);
            Assert.IsTrue(designerConfigService.PanModeEnabled);
            Assert.IsTrue(designerConfigService.RubberBandSelectionEnabled);

            // Disabled for now
            Assert.IsFalse(designerConfigService.AnnotationEnabled);
            Assert.IsFalse(designerConfigService.AutoSurroundWithSequenceEnabled);

            var designerView = wfd.Designer.Context.Services.GetService<DesignerView>();
            Assert.AreEqual(ShellHeaderItemsVisibility.Breadcrumb, designerView.WorkflowShellHeaderItemsVisibility & ShellHeaderItemsVisibility.Breadcrumb);
            Assert.AreEqual(ShellHeaderItemsVisibility.ExpandAll, designerView.WorkflowShellHeaderItemsVisibility & ShellHeaderItemsVisibility.ExpandAll);
            Assert.AreEqual(ShellHeaderItemsVisibility.CollapseAll, designerView.WorkflowShellHeaderItemsVisibility & ShellHeaderItemsVisibility.CollapseAll);

            Assert.AreEqual(ShellBarItemVisibility.Zoom, designerView.WorkflowShellBarItemVisibility & ShellBarItemVisibility.Zoom);
            Assert.AreEqual(ShellBarItemVisibility.PanMode, designerView.WorkflowShellBarItemVisibility & ShellBarItemVisibility.PanMode);
            Assert.AreEqual(ShellBarItemVisibility.MiniMap, designerView.WorkflowShellBarItemVisibility & ShellBarItemVisibility.MiniMap);

            Assert.IsNotNull(wfd.OutlineView);

            wfd.Dispose();
        }

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInvokesWorkflowHelper()
        {
            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() }).Verifiable();
            wh.Setup(h => h.EnsureImplementation(It.IsAny<ModelService>())).Verifiable();
            wh.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Verifiable();

            var wfd = CreateWorkflowDesignerViewModel(crm.Object, wh.Object);

            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            wh.Verify(h => h.CreateWorkflow(It.IsAny<string>()));
            wh.Verify(h => h.EnsureImplementation(It.IsAny<ModelService>()));

            wfd.Dispose();
        }

        #endregion

        #region CTOR

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowDesignerViewModel_UnitTest_ConstructorWithNullWorkflowHelper_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object,
                // ReSharper restore ObjectCreationAsStatement
                null, null,
                new Mock<IPopupController>().Object, false);

        }

        #endregion

        #region ServiceDefinition

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelServiceDefinitionExpectedInvokesWorkflowHelperSerializeWorkflow()
        {
            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() }).Verifiable();
            wh.Setup(h => h.EnsureImplementation(It.IsAny<ModelService>())).Verifiable();
            wh.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Verifiable();

            var wfd = CreateWorkflowDesignerViewModel(crm.Object, wh.Object);

            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);
            wfd.Dispose();
            wh.Verify(h => h.SerializeWorkflow(It.IsAny<ModelService>()));
        }

        #endregion

        #region CheckIfRemoteWorkflowTests

        [TestMethod]
        public void CheckIfRemoteWorkflowAndSetPropertiesExpectedServiceUriToBeNull()
        {
            const string ServiceUri = "http://localhost:1234/";
            var resourceEnvironmentID = Guid.NewGuid();
            var envId = Guid.NewGuid();
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true, environmentRepository, true);
            var mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.ID).Returns(envId);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv.Object);
            Assert.IsTrue(testAct.ServiceUri == null);
            Assert.IsTrue(testAct.ServiceServer == Guid.Empty);

            var contextEnvironment = new Mock<IEnvironmentModel>();
            contextEnvironment.Setup(e => e.ID).Returns(resourceEnvironmentID);

            var activity = new DsfActivity();
            var workflow = new ActivityBuilder { Implementation = activity };

            #region Setup resourceModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.Setup(m => m.Environment.ID).Returns(resourceEnvironmentID);
            resourceModel.Setup(m => m.Environment.Connection.WebServerUri).Returns(new Uri(ServiceUri));
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 1");
            #endregion

            #region Setup viewModel

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            // not necessary to invoke:  viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            viewModel.TestCheckIfRemoteWorkflowAndSetProperties(activity, resourceModel.Object, contextEnvironment.Object);

            viewModel.Dispose();

            Assert.IsNull(activity.ServiceUri);
            Assert.AreEqual(Guid.Empty, activity.ServiceServer);

        }

        [TestMethod]
        public void CheckIfRemoteWorkflowAndSetPropertiesExpectedServiceUriToBeLocalHost()
        {
            const string ServiceUri = "http://localhost:1234/";
            var resourceEnvironmentID = Guid.NewGuid();
            var contextEnvironment = new Mock<IEnvironmentModel>();
            contextEnvironment.Setup(e => e.ID).Returns(Guid.NewGuid());
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IWorkflowHelper> mockWorkflowHelper = new Mock<IWorkflowHelper>();
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            Guid envId2 = Guid.NewGuid();
            Mock<IEnvironmentModel> mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.ID).Returns(envId2);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            DsfActivity testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true, environmentRepository, true);
            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv2.Object);
            Assert.IsTrue(testAct.ServiceUri == "https://localhost:3143/" || testAct.ServiceUri == "http://localhost:3142/" || testAct.ServiceUri == "http://127.0.0.1:3142/", "Expected https://localhost:3143/ or http://localhost:3142/ or http://127.0.0.1:3142/ but got: " + testAct.ServiceUri);
            Assert.IsTrue(testAct.ServiceServer == envId2);

            var activity = new DsfActivity();
            var workflow = new ActivityBuilder { Implementation = activity };

            #region Setup resourceModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name");
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.Setup(m => m.Environment.ID).Returns(resourceEnvironmentID);
            resourceModel.Setup(m => m.Environment.Connection.WebServerUri).Returns(new Uri(ServiceUri));

            #endregion

            #region Setup viewModel

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            // not necessary to invoke:  viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            viewModel.TestCheckIfRemoteWorkflowAndSetProperties(activity, resourceModel.Object, contextEnvironment.Object);

            viewModel.Dispose();

            Assert.AreEqual("http://localhost:1234/", activity.ServiceUri);
            Assert.AreEqual(resourceEnvironmentID, activity.ServiceServer);
        }

        #endregion

        #region ModelServiceModelChanged

        // BUG 9143 - 2013.07.03 - TWR - added
        [TestMethod]
        public void WorkflowDesignerViewModelModelServiceModelChangedWithNextReferencingSelfExpectedClearsNext()
        {
            TestModelServiceModelChangedSelfReference(true);
        }

        // BUG 9143 - 2013.07.03 - TWR - added
        [TestMethod]
        public void WorkflowDesignerViewModelModelServiceModelChangedWithNextReferencingOtherExpectedDoesNotClearNext()
        {
            TestModelServiceModelChangedSelfReference(false);
        }

        //Bug 
        [TestMethod]
        public void WorkflowDesignerViewModelTestStartNodeNotDoubleConnect()
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));
            //, new Mock<IWizardEngine>().Object
            var explorerItem = new ExplorerItemModel();
            IContextualResourceModel contextualResourceModel = crm.Object;
            explorerItem.DisplayName = contextualResourceModel.ResourceName;
            explorerItem.EnvironmentId = contextualResourceModel.Environment.ID;


            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(contextualResourceModel, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            //Execute
            var wfd = new WorkflowDesignerViewModelMock(contextualResourceModel, wh.Object);
            wfd.SetDataObject(explorerItem);
            wfd.TestModelServiceModelChanged(args.Object);

            wfd.Dispose();

            //Verify
            prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_UnitTest")]
        [Description("Dropping a decision onto an auto connect node; expects an edit decision message with isnew equal to true to be published")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_PerformAddItems_DecisionActivity_EditDecisionMessageWithIsNewTruePublished()
        // ReSharper restore InconsistentNaming
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowDecision));

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup mock ModelChangedEventArgs

            var eventArgs = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            eventArgs.Setup(c => c.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var eventAggregator = new Mock<IEventAggregator>();
            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);
            var expectedMessage = new ConfigureDecisionExpressionMessage
            {
                ModelItem = source.Object,
                EnvironmentModel = crm.Object.Environment,
                IsNew = true
            };

            eventAggregator.Setup(c => c.Publish(It.IsAny<ConfigureDecisionExpressionMessage>()))
                            .Callback<Object>(actualMessage => Assert.AreEqual(expectedMessage.IsNew, ((ConfigureDecisionExpressionMessage)actualMessage).IsNew, "Wrong message published"))
                             .Verifiable();

            // Execute unit
            wd.TestModelServiceModelChanged(eventArgs.Object);

            wd.Dispose();

            eventAggregator.Verify(c => c.Publish(It.IsAny<ConfigureDecisionExpressionMessage>()), Times.Once(), "Dropping a decision onto an auto connect node did not publish configure decision message");
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DragOnToForEach")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_PerformAddItems_ForEachActivity_DragOnRemoteWorkflow()
        // ReSharper restore InconsistentNaming
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(Guid.NewGuid());
            var mockRemoteEnvironment = new Mock<IEnvironmentModel>();
            var remoteServerID = Guid.NewGuid();
            mockRemoteEnvironment.Setup(model => model.ID).Returns(remoteServerID);
            var mockRemoteConnection = new Mock<IEnvironmentConnection>();
            mockRemoteConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://remoteserver:3142/"));
            mockRemoteEnvironment.Setup(model => model.Connection).Returns(mockRemoteConnection.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockRemoteEnvironment.Object);
            repo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), It.IsAny<bool>())).Returns(mockResourceModel.Object);
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(model => model.ID).Returns(Guid.Empty);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            GetEnvironmentRepository(env); // Set the active environment



            #region setup mock ModelChangedEventArgs

            var eventArgs = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            eventArgs.Setup(c => c.ItemsAdded).Returns((IEnumerable<ModelItem>)null);
            var mock = new Mock<ModelProperty>();
            DsfActivity setValue = null;
            mock.Setup(property => property.SetValue(It.IsAny<object>())).Callback<object>(o =>
            {
                setValue = o as DsfActivity;
            });
            mock.Setup(property => property.Name).Returns("Handler");
            eventArgs.Setup(c => c.PropertiesChanged).Returns(new List<ModelProperty> { mock.Object });
#pragma warning restore 618

            #endregion

            var eventAggregator = new Mock<IEventAggregator>();
            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);
            wd.SetActiveEnvironment(env.Object);
            wd.SetDataObject(new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IConnectControlSingleton>().Object));

            // Execute unit
            wd.TestModelServiceModelChanged(eventArgs.Object);

            wd.Dispose();
            Assert.IsNotNull(setValue);
            Assert.AreEqual("http://remoteserver:3142/", setValue.ServiceUri);
            Assert.AreEqual(remoteServerID, setValue.ServiceServer);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_UnitTest")]
        [Description("Dropping a switch onto an auto connect node; expects an edit switch message with isnew equal to true to be published")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_PerformAddItems_SwitchActivity_SwitchMessageWithIsNewTruePublished()
        // ReSharper restore InconsistentNaming
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup mock ModelChangedEventArgs

            var eventArgs = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            eventArgs.Setup(c => c.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup event aggregator


            #endregion
            var eventAggregator = new Mock<IEventAggregator>();
            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);

            var expectedMessage = new ConfigureSwitchExpressionMessage
            {
                ModelItem = source.Object,
                EnvironmentModel = crm.Object.Environment,
                IsNew = true
            };

            eventAggregator.Setup(c => c.Publish(It.IsAny<ConfigureSwitchExpressionMessage>()))
                            .Callback<Object>(actualMessage => Assert.AreEqual(expectedMessage.IsNew, ((ConfigureSwitchExpressionMessage)actualMessage).IsNew, "Wrong message published"))
                             .Verifiable();


            // Execute unit
            wd.TestModelServiceModelChanged(eventArgs.Object);

            wd.Dispose();

            eventAggregator.Verify(c => c.Publish(It.IsAny<ConfigureSwitchExpressionMessage>()), Times.Once(), "Dropping a switch onto an auto connect node did not publish configure switch message");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow")]
        public void WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow_MessageWithArgs_Saves()
        {
            //------------Setup for test--------------------------
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.SaveToServer(It.IsAny<IResourceModel>())).Verifiable();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup mock ModelChangedEventArgs

            var eventArgs = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            eventArgs.Setup(c => c.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup event aggregator

            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateDeployMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);

            var unsavedResourceModel = new Mock<IContextualResourceModel>();
            unsavedResourceModel.Setup(model => model.ResourceName).Returns("Unsaved 1");
            unsavedResourceModel.Setup(model => model.WorkflowXaml).Returns(new StringBuilder("workflow xaml"));
            unsavedResourceModel.Setup(r => r.Environment).Returns(env.Object);
            var saveUnsavedWorkflowMessage = new SaveUnsavedWorkflowMessage(unsavedResourceModel.Object, "new name", "new category", false);
            //------------Execute Test---------------------------
            wd.Handle(saveUnsavedWorkflowMessage);
            //------------Assert Results-------------------------
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateDeployMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>()), Times.Never());
            repo.Verify(repository => repository.SaveToServer(It.IsAny<IResourceModel>()), Times.Once());
            repo.Verify(repository => repository.Save(It.IsAny<IResourceModel>()), Times.Once());
            repo.Verify(repository => repository.DeleteResource(It.IsAny<IResourceModel>()), Times.Once());


        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow")]
        public void WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow_MessageWithArgs_OpenTab_Saves()
        {
            //------------Setup for test--------------------------
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.SaveToServer(It.IsAny<IResourceModel>())).Verifiable();
            repo.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup mock ModelChangedEventArgs

            var eventArgs = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            eventArgs.Setup(c => c.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            #region setup event aggregator

            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateDeployMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);

            var unsavedResourceModel = new Mock<IContextualResourceModel>();
            unsavedResourceModel.Setup(model => model.ResourceName).Returns("Unsaved 1");
            unsavedResourceModel.Setup(model => model.WorkflowXaml).Returns(new StringBuilder("workflow xaml"));
            unsavedResourceModel.Setup(r => r.Environment).Returns(env.Object);
            var saveUnsavedWorkflowMessage = new SaveUnsavedWorkflowMessage(unsavedResourceModel.Object, "new name", "new category", true);
            //------------Execute Test---------------------------
            wd.Handle(saveUnsavedWorkflowMessage);
            //------------Assert Results-------------------------
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateDeployMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>()), Times.Once());
            repo.Verify(repository => repository.SaveToServer(It.IsAny<IResourceModel>()), Times.Once());
            repo.Verify(repository => repository.Save(It.IsAny<IResourceModel>()), Times.Once());
            repo.Verify(repository => repository.DeleteResource(It.IsAny<IResourceModel>()), Times.Once());


        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Description("WorkflowDesigner assigns new unique id on copy paste of an activity/tool")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithUniqueID_NewIDAssigned()
        // ReSharper restore InconsistentNaming
        {
            var notExpected = Guid.NewGuid().ToString();

            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            #endregion

            #region setup Mock ModelItem
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);
            (testAct as IDev2Activity).UniqueID = notExpected;

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            var source = new Mock<ModelItem>();
            source.Setup(c => c.Content).Returns(prop.Object);

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object);

            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Content);
            if(actual.Content != null)
            {
                IDev2Activity dev2Activity = actual.Content.ComputedValue as IDev2Activity;
                Assert.IsNotNull(dev2Activity);
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if(dev2Activity != null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    Assert.AreNotEqual(notExpected, dev2Activity.UniqueID, "Activity ID not changed");
                }
            }
        }

        #region TestModelServiceModelChangedNextReference

        // BUG 9143 - 2013.07.03 - TWR - added
        static void TestModelServiceModelChangedSelfReference(bool isSelfReference)
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            foreach(var propertyName in WorkflowDesignerViewModel.SelfConnectProperties)
            {
                var prop = new Mock<ModelProperty>();
                prop.Setup(p => p.ClearValue()).Verifiable();
                properties.Add(propertyName, prop);

                propertyCollection.Protected().Setup<ModelProperty>("Find", propertyName, true).Returns(prop.Object);
            }

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var target = isSelfReference ? source : new Mock<ModelItem>();

            var info = new Mock<ModelChangeInfo>();
            info.Setup(i => i.ModelChangeType).Returns(ModelChangeType.PropertyChanged);
            info.Setup(i => i.Subject).Returns(source.Object);
            info.Setup(i => i.Value).Returns(target.Object);

            var args = new Mock<ModelChangedEventArgs>();
            args.Setup(m => m.ModelChangeInfo).Returns(info.Object);

            var wfd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object);

            foreach(var propertyName in WorkflowDesignerViewModel.SelfConnectProperties)
            {
                info.Setup(i => i.PropertyName).Returns(propertyName);
                wfd.TestModelServiceModelChanged(args.Object);

                var prop = properties[propertyName];
                if(isSelfReference)
                {
                    prop.Verify(p => p.ClearValue(), Times.Once());
                }
                else
                {
                    prop.Verify(p => p.ClearValue(), Times.Never());
                }
            }

            wfd.Dispose();
        }

        [TestMethod]
        [Description("When the model changes we mark the resource as unsaved")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectMarksResourceIsWorkflowSavedFalse()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {


                    #region Setup viewModel

                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    resourceModel.Setup(r => r.ResourceName).Returns("Test");
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Setup(res => res.WorkflowXaml).Returns(xamlBuilder);

                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
                    //viewModel.InitializeDesigner(new Dictionary<Type, Type>());

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolote functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    viewModel.TestWorkflowDesignerModelChangedWithNullSender();

                    viewModel.Dispose();

                    //Verify
                    prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
                    Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);
        }



        [TestMethod]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectLoadFromServerDoesNotReflectEdit()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {

                    #region Setup viewModel
                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    resourceModel.Setup(r => r.ResourceName).Returns("Test");
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Setup(res => res.WorkflowXaml).Returns(xamlBuilder);

                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x/>"));
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object) { ServiceDefinition = new StringBuilder("<x/>") };

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolete functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel.Object);
                    OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);

                    viewModel.TestWorkflowDesignerModelChanged();

                    viewModel.Dispose();

                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForDesignerLoad(workSurfaceKey);

                    //Verify
                    StringAssert.Contains(StringResources.xmlServiceDefinition, resourceModel.Object.WorkflowXaml.ToString());
                    Assert.AreEqual(StringResources.xmlServiceDefinition, resourceModel.Object.WorkflowXaml.ToString());
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectFirstFocusDoesNotReflectEdit()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {
                    #region Setup viewModel
                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.SetupAllProperties();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
                    resourceModel.Object.ResourceName = "Test";
                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object) { ServiceDefinition = xamlBuilder };

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolete functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel.Object);
                    OpeningWorkflowsHelper.AddWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                    resourceModel.Object.IsWorkflowSaved = true;
                    viewModel.TestWorkflowDesignerModelChanged();

                    viewModel.Dispose();
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

                    //Verify
                    Assert.IsTrue(resourceModel.Object.IsWorkflowSaved);
                    Assert.IsTrue(resourceModel.Object.ResourceName.IndexOf("*", StringComparison.Ordinal) < 0);
                    StringAssert.Contains("<x></x>", resourceModel.Object.WorkflowXaml.ToString());
                    Assert.AreEqual("<x></x>", resourceModel.Object.WorkflowXaml.ToString());
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_DataListNotNull_ExpectFirstFocusDoesNotReflectEdit()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {
                    #region Setup viewModel
                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.SetupAllProperties();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
                    resourceModel.Object.ResourceName = "Test";
                    resourceModel.Object.DataList = "<DataList><a></a></DataList>";
                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object) { ServiceDefinition = xamlBuilder };

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolete functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel.Object);
                    OpeningWorkflowsHelper.AddWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                    resourceModel.Object.IsWorkflowSaved = true;
                    viewModel.TestWorkflowDesignerModelChanged();

                    viewModel.Dispose();
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

                    //Verify
                    Assert.IsTrue(resourceModel.Object.IsWorkflowSaved);
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_DataListDifferent_ExpectFirstFocusDoesNotReflectEdit()
        {
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {
                    #region Setup viewModel
                    var workflow = new ActivityBuilder();
                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.SetupAllProperties();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
                    resourceModel.Object.ResourceName = "Test";
                    resourceModel.Object.DataList = "<DataList><a></a></DataList>";
                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
                    workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));
                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object) { ServiceDefinition = xamlBuilder };

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolete functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    //Execute
                    var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel.Object);
                    OpeningWorkflowsHelper.AddWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                    resourceModel.Object.IsWorkflowSaved = true;
                    resourceModel.Object.DataList = "<DataList><b></b></DataList>";
                    viewModel.TestWorkflowDesignerModelChanged();

                    viewModel.Dispose();
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

                    //Verify
                    Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);
        }


        [TestMethod]
        [Description("When the xaml changes after a redo we mark the resource as unsaved")]
        public void WorkflowDesignerViewModel_UnitTest_RedoWithXAMLDifferent_ExpectMarksResourceIsWorkflowSavedFalse()
        {

            //------------Setup for test--------------------------
            var ok = true;
            var msg = string.Empty;
            var t = new Thread(() =>
            {
                try
                {

                    var workflow = new ActivityBuilder();

                    #region Setup viewModel

                    var resourceRep = new Mock<IResourceRepository>();
                    resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

                    var resourceModel = new Mock<IContextualResourceModel>();
                    resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
                    resourceModel.Setup(r => r.ResourceName).Returns("Test");
                    resourceModel.SetupProperty(model => model.IsWorkflowSaved);
                    StringBuilder xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
                    resourceModel.Setup(res => res.WorkflowXaml).Returns(xamlBuilder);

                    var workflowHelper = new Mock<IWorkflowHelper>();
                    workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
                    workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);

                    var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);

                    #endregion


                    #region setup Mock ModelItem

                    var properties = new Dictionary<string, Mock<ModelProperty>>();
                    var propertyCollection = new Mock<ModelPropertyCollection>();
                    var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
                    var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

                    var prop = new Mock<ModelProperty>();
                    prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
                    prop.Setup(p => p.ComputedValue).Returns(testAct);
                    properties.Add("Action", prop);

                    propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

                    var source = new Mock<ModelItem>();
                    source.Setup(s => s.Properties).Returns(propertyCollection.Object);
                    source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

                    #endregion

                    #region setup mock to change properties

                    //mock item adding - this is obsolote functionality but not refactored due to overhead
                    var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
                    args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

                    #endregion

                    viewModel.Dispose();

                    //Verify
                    Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
                }
                catch(Exception e)
                {
                    ok = false;
                    msg = e.Message + " -> " + e.StackTrace;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.IsTrue(ok, msg);

        }

        #endregion

        #endregion

        #region EditActivity

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_EditActivity")]
        [Description("WorkflowDesignerViewModel EditActivity must load the workflow when then enviromentID is null")]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_EditActivityWithNullEnviromentID_ConnectsAndLoadsResources()
        {
            Guid ServiceID = Guid.NewGuid();
            var environmentID = Guid.NewGuid();

            Mock<IResourceRepository> mockResRepo = new Mock<IResourceRepository>();
            mockResRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(null);
            environment.Setup(e => e.IsConnected).Returns(false);
            environment.Setup(e => e.Connect()).Verifiable();
            environment.Setup(e => e.LoadResources()).Verifiable();
            environment.Setup(e => e.ResourceRepository).Returns(mockResRepo.Object);

            // Setup environment repository to return our environment
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(environment.Object);

            #region Setup viewModel

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(mockResRepo.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 67");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            var viewModel = CreateWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var modelItem = TestUtils.CreateModelItem(Guid.NewGuid(), ServiceID, environmentID);
            var modelService = viewModel.Designer.Context.Services.GetService<ModelService>();
            modelItem.Setup(mi => mi.Root).Returns(modelService.Root);

            viewModel.Handle(new EditActivityMessage(modelItem.Object, Guid.NewGuid(), envRepository.Object));

            environment.Verify(e => e.Connect());
            environment.Verify(e => e.LoadResources());
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_EditActivity")]
        [Description("WorkflowDesignerViewModel EditActivity must connect and load the resources of a disconnected environment.")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_UnitTest_EditActivityWithDisconnectedEnvironment_ConnectsAndLoadsResources()
        {
            Guid ServiceID = Guid.NewGuid();
            var environmentID = Guid.NewGuid();

            Mock<IResourceRepository> mockResourceRepo = new Mock<IResourceRepository>();

            mockResourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(false);
            environment.Setup(e => e.Connect()).Verifiable();
            environment.Setup(e => e.LoadResources()).Verifiable();
            environment.Setup(e => e.ResourceRepository).Returns(mockResourceRepo.Object);

            // Setup environment repository to return our environment
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(environment.Object);

            #region Setup viewModel

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(mockResourceRepo.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 13");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            var viewModel = CreateWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var modelItem = TestUtils.CreateModelItem(Guid.NewGuid(), ServiceID, environmentID);
            var modelService = viewModel.Designer.Context.Services.GetService<ModelService>();
            modelItem.Setup(mi => mi.Root).Returns(modelService.Root);

            viewModel.Handle(new EditActivityMessage(modelItem.Object, Guid.NewGuid(), envRepository.Object));

            environment.Verify(e => e.Connect());
            environment.Verify(e => e.LoadResources());
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_EditActivity")]
        [Description("WorkflowDesignerViewModel EditActivity must rehydrate its environment ID with its parent id if empty.")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_UnitTest_EditActivityWithEmptyEnvironmentID_UsesParentEnvironmentID()
        {
            Guid ServiceID = Guid.NewGuid();
            var environmentID = Guid.Empty;

            #region Setup parentEnvironment

            // If the view model is able to resolve the remote server ID it will search it for the resource
            var resourceRepository = new Mock<IResourceRepository>();
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            resourceRepository.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var parentEnvironment = new Mock<IEnvironmentModel>();
            parentEnvironment.Setup(c => c.ID).Returns(Guid.NewGuid());
            parentEnvironment.Setup(c => c.IsConnected).Returns(true);
            parentEnvironment.Setup(c => c.ResourceRepository).Returns(resourceRepository.Object);

            #endregion

            IEnvironmentModel actualEnvironment = null;

            // Setup environment repository to look for our environment
            var environments = new List<IEnvironmentModel> { parentEnvironment.Object };
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                         .Callback((Expression<Func<IEnvironmentModel, bool>> filter) => { actualEnvironment = environments.AsQueryable().FirstOrDefault(filter); });

            #region Setup viewModel

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRepository.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 56");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            var viewModel = CreateWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var modelService = viewModel.Designer.Context.Services.GetService<ModelService>();
            var modelItem = TestUtils.CreateModelItem(Guid.NewGuid(), ServiceID, environmentID);
            modelItem.Setup(mi => mi.Root).Returns(modelService.Root);

            viewModel.Handle(new EditActivityMessage(modelItem.Object, parentEnvironment.Object.ID, envRepository.Object));

            Assert.AreSame(parentEnvironment.Object, actualEnvironment);
        }

        #endregion

        #region DebugSelectionChanged

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_NullDebugState_DoesNothing()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "SelectionChangedTest1", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 7");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            //----------------------- Execute -----------------------//
            EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = null, SelectionType = ActivitySelectionType.Single });

            var result = viewModel.BringIntoViewHitCount;

            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.AreEqual(0, result);
        }


        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_DesignerViewHidden_DoesNothing()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "SelectionChangedTest1", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 6");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            viewModel.SetIsDesignerViewVisible(false);

            //----------------------- Execute -----------------------//
            EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = new DebugState(), SelectionType = ActivitySelectionType.Single });

            var result = viewModel.GetSelectedModelItemHitCount;

            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionItemNotFound_SelectsFlowchart()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(Flowchart), false);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionItemFound_SelectsModelItem()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionDecisionOrSwitchItemFound_SelectsDecisionOrSwitch()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(FlowDecision));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_AddSelection_SelectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Add, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_RemoveSelection_SelectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Remove, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_ClearSelection_DeselectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.None, typeof(TestActivity));
        }

        static void Verify_DebugSelectionChanged(ActivitySelectionType selectionType, Type selectedActivityType, bool selectsModelItem = true)
        {
            //----------------------- Setup -----------------------//
            var ID = Guid.NewGuid();
            var states = new List<IDebugState> { new DebugState { DisplayName = "SelectionChangedTest1", ID = ID, WorkSurfaceMappingId = ID } };
            ID = Guid.NewGuid();
            if(selectionType == ActivitySelectionType.Add || selectionType == ActivitySelectionType.Remove)
            {

                states.Add(new DebugState { DisplayName = "SelectionChangedTest2", ID = ID, WorkSurfaceMappingId = ID });
            }

            #region Setup workflow

            FlowNode prevNode = null;

            var nodes = new List<FlowNode>();
            foreach(var node in states.Select(state => CreateFlowNode(state.ID, state.DisplayName, selectsModelItem, selectedActivityType)))
            {
                if(prevNode != null)
                {
                    var flowStep = prevNode as FlowStep;
                    if(flowStep != null)
                    {
                        flowStep.Next = node;
                    }
                }
                nodes.Add(node);
                prevNode = node;
            }

            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = nodes[0]
                }
            };

            #endregion

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            //----------------------- Execute -----------------------//
            var i = 0;
            foreach(var debugState in states)
            {
                if(selectionType == ActivitySelectionType.None || selectionType == ActivitySelectionType.Remove)
                {
                    // Ensure we have something to clear/remove
                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = ActivitySelectionType.Add });

                    // Only issue change event after all have been added
                    if(++i == states.Count)
                    {
                        var selectionBefore = viewModel.Designer.Context.Items.GetValue<Selection>();
                        Assert.AreEqual(states.Count, selectionBefore.SelectionCount);

                        EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = selectionType });
                    }
                }
                else
                {
                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = selectionType });
                }
            }

            //----------------------- Assert -----------------------//

            var selection = viewModel.Designer.Context.Items.GetValue<Selection>();

            switch(selectionType)
            {
                case ActivitySelectionType.None:
                    Assert.AreEqual(0, selection.SelectionCount);
                    Assert.AreEqual(1, viewModel.BringIntoViewHitCount); // 1 because we had to add something first!
                    Assert.AreEqual(0, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Single:
                    Assert.AreEqual(1, selection.SelectionCount);
                    Assert.AreEqual(1, viewModel.BringIntoViewHitCount);
                    Assert.AreEqual(1, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Add:
                    Assert.AreEqual(2, selection.SelectionCount);
                    Assert.AreEqual(2, viewModel.BringIntoViewHitCount);
                    Assert.AreEqual(2, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Remove:
                    Assert.AreEqual(2, selection.SelectionCount);
                    Assert.AreEqual(2, viewModel.BringIntoViewHitCount); // 2 because we had to add something first!
                    Assert.AreEqual(1, viewModel.SelectedDebugModelItems.Count);
                    break;
            }

            foreach(var modelItem in selection.SelectedObjects)
            {
                Assert.AreEqual(selectedActivityType, modelItem.ItemType);
                if(selectsModelItem)
                {
                    var actualID = selectedActivityType == typeof(FlowDecision)
                        ? Guid.Parse(((TestDecisionActivity)modelItem.GetProperty("Condition")).UniqueID)
                        : ModelItemUtils.GetUniqueID(modelItem);

                    var actualState = states.FirstOrDefault(s => s.ID == actualID);
                    Assert.IsNotNull(actualState);
                }
            }

            viewModel.Dispose();
        }

        static FlowNode CreateFlowNode(Guid id, string displayName, bool selectsModelItem, Type activityType)
        {
            if(activityType == typeof(FlowDecision))
            {
                return new FlowDecision(new TestDecisionActivity
                {
                    DisplayName = displayName,
                    UniqueID = selectsModelItem ? id.ToString() : Guid.NewGuid().ToString()
                });
            }

            return new FlowStep
            {
                Action = new TestActivity
            {
                DisplayName = displayName,
                UniqueID = selectsModelItem ? id.ToString() : Guid.NewGuid().ToString()
            }
            };
        }

        #endregion

        #region ViewPreviewMouseDown

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_UnitTest")]
        [Description("Clicking a decision publishes configure decision message with isnew equal to false")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_ViewPreviewMouseDown_Decision_ConfigureDecisionPublishedWithIsNewFalse()
        // ReSharper restore InconsistentNaming
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowDecision));

            #endregion

            #region setup event aggregator

            var eventAggregator = new Mock<IEventAggregator>();

            #endregion

            var wd = new Mock<WorkflowDesignerViewModelMock>(crm.Object, wh.Object, eventAggregator.Object, false);

            wd.Setup(c => c.SelectedModelItem).Returns(source.Object);

            var expectedMessage = new ConfigureDecisionExpressionMessage
            {
                ModelItem = source.Object,
                EnvironmentModel = crm.Object.Environment,
                IsNew = false
            };

            eventAggregator.Setup(c => c.Publish(It.IsAny<ConfigureDecisionExpressionMessage>()))
                            .Callback<Object>(actualMessage => Assert.AreEqual(expectedMessage.IsNew, ((ConfigureDecisionExpressionMessage)actualMessage).IsNew, "Wrong message published"))
                             .Verifiable();

            wd.Object.TestHandleMouseClick(new Mock<DependencyObject>().Object, null);

            wd.Object.Dispose();

            eventAggregator.Verify(c => c.Publish(It.IsAny<ConfigureDecisionExpressionMessage>()), Times.Once(), "Dropping a decision onto an auto connect node did not publish configure decision message");
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_UnitTest")]
        [Description("Clicking a switch publishes configure switch message with isnew equal to false")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void WorkflowDesignerViewModel_ViewPreviewMouseDown_Switch_ConfigureDecisionPublishedWithIsNewFalse()
        // ReSharper restore InconsistentNaming
        {
            #region Setup view model constructor parameters

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            #endregion

            #region setup event aggregator

            var eventAggregator = new Mock<IEventAggregator>();

            #endregion

            var wd = new Mock<WorkflowDesignerViewModelMock>(crm.Object, wh.Object, eventAggregator.Object, false);

            wd.Setup(c => c.SelectedModelItem).Returns(source.Object);

            var expectedMessage = new ConfigureSwitchExpressionMessage
            {
                ModelItem = source.Object,
                EnvironmentModel = crm.Object.Environment,
                IsNew = false
            };

            eventAggregator.Setup(c => c.Publish(It.IsAny<ConfigureSwitchExpressionMessage>()))
                            .Callback<Object>(actualMessage => Assert.AreEqual(expectedMessage.IsNew, ((ConfigureSwitchExpressionMessage)actualMessage).IsNew, "Wrong message published"))
                             .Verifiable();

            wd.Object.TestHandleMouseClick(new Mock<DependencyObject>().Object, null);

            wd.Object.Dispose();

            eventAggregator.Verify(c => c.Publish(It.IsAny<ConfigureSwitchExpressionMessage>()), Times.Once(), "Dropping a switch onto an auto connect node did not publish configure switch message");
        }

        #endregion


        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_CanSave_InvokesResourceModelIsAuthorizedForContribute()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 66");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            //----------------------- Execute -----------------------//
            var result = viewModel.CanSave;

            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.AreEqual(ExpectedCanSave, result);

            resourceModel.Verify(m => m.IsAuthorized(AuthorizationContext.Contribute));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_GetsCommand()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 44");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            var expandAllCommand = viewModel.ExpandAllCommand;
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsNotNull(expandAllCommand);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_True_RequestExpandAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 332");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestExapandAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedExpandAll);
            viewModel.ExpandAllCommand.Execute(true);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedExpandAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_False_RequestRestoreAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 34");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestExapandAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedRestoreAll);
            viewModel.ExpandAllCommand.Execute(false);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedRestoreAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_CollapseAllCommand_True_RequestCollapseAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 22");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestCollapseAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedCollapseAll);
            viewModel.CollapseAllCommand.Execute(true);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedCollapseAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_CollapseAllCommand_False_RequestRestoreAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestCollapseAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedRestoreAll);
            viewModel.CollapseAllCommand.Execute(false);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedRestoreAll);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_DoWorkspaceSave_IsNewResourceModel_ShouldCallSave()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 57");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DoWorkspaceSave();
            //------------Assert Results-------------------------
            resourceRep.Verify(repository => repository.Save(It.IsAny<IResourceModel>()), Times.Once());
            Assert.IsNotNull(resourceModel.Object.WorkflowXaml);
            Assert.AreEqual("my workflow", resourceModel.Object.WorkflowXaml.ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_DataListNull_ShouldReturnUrlEmptyDataListPortion()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns("<DataList></DataList>");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?<DataList></DataList>&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?<DataList></DataList>", displayWorkflowLink);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_HasDataListNoInputs_ShouldReturnUrlEmptyDataListPortion()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            viewModel.OpenWorkflowLinkCommand.Execute("Do not perform action");
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?<DataList></DataList>&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?<DataList></DataList>", displayWorkflowLink);
            mockPopController.Verify(controller => controller.ShowNoInputsSelectedWhenClickLink(), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_HasDataListHasInputs_ShouldReturnUrlWithDataListPortion()
        {
            //------------Setup for test--------------------------
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "");
            var settingPath = Path.Combine(appData, @"Local\Warewolf\DebugData\PersistSettings.dat");

            if(File.Exists(settingPath))
            {
                File.Delete(settingPath);
            }

            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            viewModel.OpenWorkflowLinkCommand.Execute("Do not perform action");
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?scalar1=&scalar2=&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?scalar1=&scalar2=", displayWorkflowLink);
            mockPopController.Verify(controller => controller.ShowNoInputsSelectedWhenClickLink(), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_SavedDebugData_ShouldReturnUrlWithDataListUsingSavedData()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var workflowInputDataViewModel = WorkflowInputDataViewModel.Create(resourceModel.Object);
            workflowInputDataViewModel.LoadWorkflowInputs();
            workflowInputDataViewModel.WorkflowInputs[0].Value = "1";
            workflowInputDataViewModel.WorkflowInputs[1].Value = "2";
            workflowInputDataViewModel.DoSaveActions();
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?scalar1=1&scalar2=2&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/services/myservice.json?scalar1=1&scalar2=2", displayWorkflowLink);
            workflowInputDataViewModel.WorkflowInputs[0].Value = "";
            workflowInputDataViewModel.WorkflowInputs[1].Value = "";
            workflowInputDataViewModel.DoSaveActions();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_DoWorkspaceSave_NotNewResourceModel_ShouldCallSave()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(false);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 9");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);
            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DoWorkspaceSave();
            //------------Assert Results-------------------------
            resourceRep.Verify(repository => repository.Save(It.IsAny<IResourceModel>()), Times.Never());
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_HandleUpdateResourceMessage")]
        public void WorkflowDesignerViewModel_HandleUpdateResourceMessage_WhenMessageHasErrors_ResourceModelShouldHaveErrors()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };


            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(false);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 11");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupGet(model => model.Errors);
            resourceModel.Setup(model => model.Errors).Returns(new ObservableReadOnlyList<IErrorInfo>(new List<IErrorInfo> { new ErrorInfo() }));
            resourceModel.Setup(model => model.AddError(It.IsAny<IErrorInfo>())).Verifiable();
            var updateResourceMessage = new UpdateResourceMessage(resourceModel.Object);

            //------------Execute Test---------------------------
            viewModel.Handle(updateResourceMessage);
            //------------Assert Results-------------------------
            resourceModel.Verify(model => model.AddError(It.IsAny<IErrorInfo>()), Times.Once());
        }

        static IEnvironmentRepository SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            return GetEnvironmentRepository(mockEnvironment);
        }

        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {

            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
            repo.ActiveEnvironment = mockEnvironment.Object;

            return repo;
        }

        static IDataListViewModel CreateDataListViewModel(Mock<IContextualResourceModel> mockResourceModel, IEventAggregator eventAggregator = null)
        {
            var dataListViewModel = new DataListViewModel(eventAggregator ?? new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            return dataListViewModel;
        }

        static WorkflowDesignerViewModel CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(IContextualResourceModel resourceModel, IEventAggregator eventPublisher = null)
        {
            var wf = CreateWorkflowDesignerViewModel(eventPublisher, resourceModel, new WorkflowHelper(), false);

            var designerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfActivity), typeof(Dev2.Activities.Designers2.Service.ServiceDesigner) },
                { typeof(DsfMultiAssignActivity), typeof(Dev2.Activities.Designers2.MultiAssign.MultiAssignDesigner) }, 
                { typeof(DsfForEachActivity), typeof(ForeachDesigner) }, 
                { typeof(DsfCountRecordsetActivity), typeof(Dev2.Activities.Designers2.CountRecords.CountRecordsDesigner) }
            };

            wf.InitializeDesigner(designerAttributes);

            return wf;
        }

        static WorkflowDesignerViewModel CreateWorkflowDesignerViewModel(IContextualResourceModel resourceModel, IWorkflowHelper workflowHelper = null, bool createDesigner = true, string helperText = null)
        {
            return CreateWorkflowDesignerViewModel(null, resourceModel, workflowHelper, createDesigner, helperText);
        }

        static WorkflowDesignerViewModel CreateWorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resourceModel, IWorkflowHelper workflowHelper = null, bool createDesigner = true, string helperText = null)
        {
            eventPublisher = eventPublisher ?? new Mock<IEventAggregator>().Object;

            var popupController = new Mock<IPopupController>();

            if(workflowHelper == null)
            {
                var wh = new Mock<IWorkflowHelper>();
                wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() });
                if(helperText != null)
                {
                    wh.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(new StringBuilder(helperText));
                }
                workflowHelper = wh.Object;
            }

            var viewModel = new WorkflowDesignerViewModel(eventPublisher, resourceModel, workflowHelper, popupController.Object, createDesigner, _isDesignerInited);

            _isDesignerInited = true;


            return viewModel;

        }
    }
}
