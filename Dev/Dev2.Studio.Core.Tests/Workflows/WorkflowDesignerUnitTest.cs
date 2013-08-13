using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers.DsfMultiAssign;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.ViewModelTests;
using Dev2.Core.Tests.Workflows;
using Dev2.Data.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class WorkflowDesignerUnitTest
    {

        #region Test Variables

        static object _testGuard = new object();

        #endregion Test Variables

        #region Test Initialize

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(_testGuard);

            ImportService.CurrentContext = CompositionInitializer.PopUpProviderForTestsWithMockMainViewModel();

            Mock<IEnvironmentModel> _moqEnvironment = new Mock<IEnvironmentModel>();

            _moqEnvironment.Setup(env => env.Connection.WebServerUri).Returns(new Uri("http://localhost:1234"));
            _moqEnvironment.Setup(env => env.Connection.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _moqEnvironment.Setup(env => env.Name).Returns("Test");
            _moqEnvironment.Setup(env => env.Connect()).Verifiable();
            _moqEnvironment.Setup(env => env.IsConnected).Returns(true);
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion Test Initialize

        #region Remove Unused Tests

        /// <summary>
        /// Tests Remove All UnusedDataListItems is able remove all the unused data list items from the data list
        /// </summary>
        [TestMethod]
        public void RemoveAllUnusedDataListObjectsWithItemsNotUsedExpectedItemsRemoved()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItem.Validator = new DataListValidator();
            secondDataListItem.Validator = new DataListValidator();

            DataListItems.Add(dataListItem);
            DataListItems.Add(secondDataListItem);

            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            //  Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            //Juries 8810 TODO
            //mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            DataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            // workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;
            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.RemoveAllUnusedDataListItems(dataListViewModel);
            Assert.IsTrue(dataListViewModel.ScalarCollection.Count == 0);

        }


        [TestMethod]
        public void MissingPartsMessageOnlySentWhenThereWorkToDoExpect1Call()
        {
            // Set up event agg
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            Mock<IEventAggregator> evtAg = new Mock<IEventAggregator>();

            ImportService.AddExportedValueToContainer(evtAg.Object);

            evtAg.Setup(ea => ea.Publish(It.IsAny<AddMissingDataListItems>()));

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            DataListSingleton.SetDataList(dataListViewModel);

            DataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();

            evtAg.Verify(a => a.Publish(It.IsAny<AddMissingDataListItems>()), Times.Exactly(1));

        }

        [TestMethod]
        public void MissingPartsMessageOnlySentWhenThereWorkToDoExpectNoCalls()
        {
            // Set up event agg
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            Mock<IEventAggregator> evtAg = new Mock<IEventAggregator>();

            ImportService.AddExportedValueToContainer(evtAg.Object);

            evtAg.Setup(ea => ea.Publish(It.IsAny<AddMissingDataListItems>()));

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            DataListSingleton.SetDataList(dataListViewModel);

            DataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();

            evtAg.Verify(a => a.Publish(It.IsAny<AddMissingDataListItems>()), Times.Exactly(0));

        }

        #endregion

        #region Add Missing DataList Items

        /// <summary>
        /// Test the AddMissingDataListItems method that it adds all missing data list items to the datalist
        /// </summary>
        [TestMethod]
        public void AddMissingDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsed()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItem.Validator = new DataListValidator();
            secondDataListItem.Validator = new DataListValidator();

            DataListItems.Add(dataListItem);
            DataListItems.Add(secondDataListItem);

            // Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            //  Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            //Juries 8810 TODO
            //mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel.RecsetCollection.Clear();
            DataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            // workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;

            workflowDesigner.AddMissingOnlyWithNoPopUp(null);
            Assert.IsTrue(76 == dataListViewModel.ScalarCollection.Count);
            Assert.IsTrue(2 == dataListViewModel.RecsetCollection.Count);
        }

        //2013.06.24: Ashley Lewis for bug 9698 - test for get decision elements
        [TestMethod]
        public void GetDecisionElementsWithMissmatchedBracketsInADecisionFieldExpectedCorrectVariableGottenFromDecision()
        {
            //Execute
            var model = new WorkflowDesignerViewModel(Dev2MockFactory.ResourceModel.Object);
            var actual = model.GetDecisionElements("Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:!]]!,!Col2!:![[scalar]]!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)", new DataListViewModel());
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("scalar", actual[0], "Find missing found an invalid variable in a decision");
        }

        #endregion

        #region Set Unused DataList Items

        /// <summary>
        /// Test the FindUnusedDataListItems method that it sets all the unused data list items to unused
        /// </summary>
        [TestMethod]
        public void FindUnusedDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsed()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            IDataListItemModel dataListItem = new DataListItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            IDataListItemModel secondDataListItem = new DataListItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItem.Validator = new DataListValidator();
            secondDataListItem.Validator = new DataListValidator();

            DataListItems.Add(dataListItem);
            DataListItems.Add(secondDataListItem);

            // Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            //  Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            //Juries 8810 TODO
            // mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            DataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            //  workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;

            Assert.IsTrue(dataListViewModel.ScalarCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ScalarCollection[1].IsUsed);

            workflowDesigner.FindUnusedDataListItems();

            Assert.IsTrue(!dataListViewModel.ScalarCollection[0].IsUsed);
            Assert.IsTrue(!dataListViewModel.ScalarCollection[1].IsUsed);

        }

        #endregion

        #region NotifyItemSelected Tests

        [TestMethod]
        public void NotifyItemSelected_WebpagePreviewNullReferenceBugFix()
        {
            Mock<IContextualResourceModel> resource = Dev2MockFactory.SetupResourceModelMock(); //new Mock<IContextualResourceModel>();
            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resource.Object);
            DsfWebPageActivity page = new DsfWebPageActivity();
            Assert.IsTrue(wf.NotifyItemSelected(page) == false);
        }

        //2013.02.11: Ashley Lewis - Bug 6413
        [TestMethod]
        public void ParserCorrectlyIdentifyingDatalistRegions()
        {
            // Create a blank workflow
            // Left side: [[leftVal]]
            // Right side: [[rightVal1]][[rightVal2]]
            // AddRemove should end off with 3 items

            string b = @"<Activity mc:Ignorable=""sads sap"" x:Class=""yayacake""
 xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities""
 xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
 xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data""
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
  <sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>
  <Flowchart DisplayName=""yayacake"" sap:VirtualizedContainerService.HintSize=""614,636"">
    <Flowchart.Variables>
      <Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />
      <Variable x:TypeArguments=""x:String"" Name=""LastResult"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />
      <Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />
      <Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />
      <Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />
      <Variable x:TypeArguments=""uaba:Util"" Name=""t"" />
      <Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" />
    </Flowchart.Variables>
    <sap:WorkflowViewStateService.ViewState>
      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
        <x:Boolean x:Key=""IsExpanded"">False</x:Boolean>
        <av:Point x:Key=""ShapeLocation"">270,2.5</av:Point>
        <av:Size x:Key=""ShapeSize"">60,75</av:Size>
        <av:PointCollection x:Key=""ConnectorLocation"">300,77.5 300,107.5 310,107.5 310,174</av:PointCollection>
      </scg:Dictionary>
    </sap:WorkflowViewStateService.ViewState>
    <Flowchart.StartNode>
      <FlowStep x:Name=""__ReferenceID0"">
        <sap:WorkflowViewStateService.ViewState>
          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
            <av:Point x:Key=""ShapeLocation"">182,174</av:Point>
            <av:Size x:Key=""ShapeSize"">256,92</av:Size>
          </scg:Dictionary>
        </sap:WorkflowViewStateService.ViewState>
        <uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""256,92"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""951ec686-05a2-4549-8b15-c613614d1a12"" UpdateAllOccurrences=""False"">
          <uaba:DsfMultiAssignActivity.AmbientDataList>
            <InOutArgument x:TypeArguments=""scg:List(x:String)"" />
          </uaba:DsfMultiAssignActivity.AmbientDataList>
          <uaba:DsfMultiAssignActivity.FieldsCollection>
            <scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4"">
              <uaba:ActivityDTO FieldName=""[[leftVal]]"" FieldValue=""[[rightVal1]][[rightVal2]]"" IndexNumber=""1"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">
                <uaba:ActivityDTO.OutList>
                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                </uaba:ActivityDTO.OutList>
              </uaba:ActivityDTO>
              <uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""2"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]"">
                <uaba:ActivityDTO.OutList>
                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
                </uaba:ActivityDTO.OutList>
              </uaba:ActivityDTO>
            </scg:List>
          </uaba:DsfMultiAssignActivity.FieldsCollection>
          <sap:WorkflowViewStateService.ViewState>
            <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
              <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
            </scg:Dictionary>
          </sap:WorkflowViewStateService.ViewState>
        </uaba:DsfMultiAssignActivity>
      </FlowStep>
    </Flowchart.StartNode>
    <x:Reference>__ReferenceID0</x:Reference>
  </Flowchart>
</Activity>";

            int itemCount = GetAddRemoveDataListItemsCount(b);

            Assert.AreEqual(3, itemCount);
        }


        #endregion NotifyItemSelected Tests

        #region Internal Test Methods

        WorkflowDesignerViewModel InitializeWorkflowDesignerForDataListFunctionality(IContextualResourceModel resourceModel)
        {

            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resourceModel);
            var designerAttributes = new Dictionary<Type, Type>();

            designerAttributes.Add(typeof(DsfActivity), typeof(DsfActivityDesigner));
            designerAttributes.Add(typeof(DsfMultiAssignActivity), typeof(Dev2.Activities.Designers.DsfMultiAssign.DsfMultiAssignActivityDesigner));
            designerAttributes.Add(typeof(DsfAssignActivity), typeof(DsfAssignActivityDesigner));
            designerAttributes.Add(typeof(TransformActivity), typeof(DsfTransformActivityDesigner));
            designerAttributes.Add(typeof(DsfForEachActivity), typeof(DsfForEachActivityDesigner));
            designerAttributes.Add(typeof(DsfCountRecordsetActivity), typeof(DsfCountRecordsetActivityDesigner));
            // wf.MediatorRepo = _mockMediatorRepo.Object;
            wf.InitializeDesigner(designerAttributes);

            return wf;
        }

        WorkflowDesignerViewModel InitializeWorkflowDesignerForCategoryFunctionality(IContextualResourceModel resourceModel)
        {

            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resourceModel);
            var designerAttributes = new Dictionary<Type, Type>();
            //wf.MediatorRepo = _mockMediatorRepo.Object;
            wf.InitializeDesigner(designerAttributes);

            return wf;
        }

        int GetAddRemoveDataListItemsCount(string xamlInput)
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            //Requires Specific XAML
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(xamlInput);
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(mockResourceModel.Object);

            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> DataListItem = new Mock<IDataListItemModel>();
            Mock<IDataListItemModel> secondDataListItem = new Mock<IDataListItemModel>();
            DataListItem.Setup(list => list.Name).Returns("result");
            DataListItem.Setup(list => list.Description).Returns("result desciption");
            DataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());
            secondDataListItem.Setup(list => list.Name).Returns("testing");
            secondDataListItem.Setup(list => list.Description).Returns("testing description");
            secondDataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListItems.Add(DataListItem.Object);
            DataListItems.Add(secondDataListItem.Object);

            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            // Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            //Juries 8810 TODO
            //mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);

            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            mockDataListViewModel.Setup(dlvm => dlvm.DataList).Returns(DataListItems);
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            return workflowDesigner.WorkflowVerifiedDataParts.Count;
        }

        string WorkflowXAMLForTest()
        {
            return @"<Activity mc:Ignorable=""sap"" x:Class=""ServiceToBindFrom"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
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
</Activity>";
        }

        public string GetAddMissingWorkflowXml()
        {
            return @"<Activity mc:Ignorable=""sads sap"" x:Class=""AllTools""
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
</Activity>";
        }

        #endregion Internal Test Methods

        #region Update Resource Message Handler

        //2013.02.11: Ashley Lewis - Bug 8553
        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForCategoryFunctionality(mockResourceModel.Object);

            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            workflowDesigner.Handle(updatemsg);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");
            updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            workflowDesigner.Handle(updatemsg);

            Assert.AreEqual("Testing2", workflowDesigner.ResourceModel.Category);
        }

        #endregion

        #region InitializeDesigner

        // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInitializesFramework45Properties()
        {
            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wfd = new WorkflowDesignerViewModel(crm.Object);
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
        }

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInvokesWorkflowHelper()
        {
            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() }).Verifiable();
            wh.Setup(h => h.EnsureImplementation(It.IsAny<ModelService>())).Verifiable();
            wh.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Verifiable();

            var wfd = new WorkflowDesignerViewModel(crm.Object, wh.Object);

            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            wh.Verify(h => h.CreateWorkflow(It.IsAny<string>()));
            wh.Verify(h => h.EnsureImplementation(It.IsAny<ModelService>()));
        }

        #endregion

        #region CTOR

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowDesignerViewModelConstructorWithNullWorkflowHelperExpectedThrowsArgumentNullException()
        {
            var wfd = new WorkflowDesignerViewModel(null, null);
        }

        #endregion

        #region ServiceDefinition

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelServiceDefinitionExpectedInvokesWorkflowHelperSerializeWorkflow()
        {
            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() }).Verifiable();
            wh.Setup(h => h.EnsureImplementation(It.IsAny<ModelService>())).Verifiable();
            wh.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Verifiable();

            var wfd = new WorkflowDesignerViewModel(crm.Object, wh.Object);

            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            var serviceDef = wfd.ServiceDefinition;
            wh.Verify(h => h.SerializeWorkflow(It.IsAny<ModelService>()));
        }

        #endregion

        #region CheckIfRemoteWorkflowTests


        [TestMethod]
        public void CheckIfRemoteWorkflowAndSetPropertiesExpectedServiceUriToBeNull()
        {
            Guid envId = Guid.NewGuid();
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IWorkflowHelper> mockWorkflowHelper = new Mock<IWorkflowHelper>();
            DsfActivity testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true);
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.ID).Returns(envId);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            WorkflowDesignerViewModelTestClass testClass = new WorkflowDesignerViewModelTestClass(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv.Object);
            Assert.IsTrue(testAct.ServiceUri == null);
            Assert.IsTrue(testAct.ServiceServer == Guid.Empty);

        }

        [TestMethod]
        public void CheckIfRemoteWorkflowAndSetPropertiesExpectedServiceUriToBeLocalHost()
        {
            Guid envId = Guid.NewGuid();
            Guid envId2 = Guid.NewGuid();
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IWorkflowHelper> mockWorkflowHelper = new Mock<IWorkflowHelper>();
            DsfActivity testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true);
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.ID).Returns(envId);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);

            Mock<IEnvironmentModel> mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.ID).Returns(envId2);
            WorkflowDesignerViewModelTestClass testClass = new WorkflowDesignerViewModelTestClass(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv2.Object);
            Assert.IsTrue(testAct.ServiceUri == "http://localhost:1234/");
            Assert.IsTrue(testAct.ServiceServer == envId2);

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
            crm.Setup(res => res.ServiceDefinition).Returns(StringResourcesTest.xmlServiceDefinition);

            var treeVM = new ResourceTreeViewModel(new Mock<IDesignValidationService>().Object, null, crm.Object);

            var wh = new Mock<IWorkflowHelper>();

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(crm.Object, new DsfActivity(), true);

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
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });

            #endregion

            //Execute
            var wfd = new TestWorkflowDesignerViewModel(crm.Object, wh.Object, false);
            wfd.SetDataObject(treeVM);
            wfd.TestModelServiceModelChanged(args.Object);

            //Verify
            prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
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

            var wfd = new TestWorkflowDesignerViewModel(crm.Object, wh.Object, false);

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
        }

        [TestMethod]
        [Description("When the model changes we mark the resource as unsaved")]
        [Ignore]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectMarksResourceIsWorkflowSavedFalse()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            resourceModel.Setup(r => r.WorkflowXaml).Returns("TestXaml");
            resourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new TestWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion


            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true);

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
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });

            #endregion

            //Execute
            viewModel.TestWorkflowDesignerModelChangedWithNullSender();

            //Verify
            prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
        }

        [TestMethod]
        [Description("When the xaml changes after undo changes we mark the resource as unsaved")]
        [Ignore]
        public void WorkflowDesignerViewModel_UnitTest_UndoWithXAMLSame_ExpectMarksResourceIsWorkflowSavedTrue()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            resourceModel.Setup(r => r.WorkflowXaml).Returns("TestXaml");
            resourceModel.SetupProperty(model => model.IsWorkflowSaved);
            resourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new TestWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true);

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
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });

            #endregion

            //Execute
            resourceModel.Setup(r => r.WorkflowXaml).Returns((string)null);
            viewModel.TestWorkflowDesignerModelChanged();

            //Verify
            Assert.IsTrue(resourceModel.Object.IsWorkflowSaved);
        }

        [TestMethod]
        [Description("When the xaml changes after a redo we mark the resource as unsaved")]
        [Ignore]
        public void WorkflowDesignerViewModel_UnitTest_RedoWithXAMLDifferent_ExpectMarksResourceIsWorkflowSavedFalse()
        {

            // user this .... 
            var workflow = new ActivityBuilder();

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            resourceModel.Setup(r => r.WorkflowXaml).Returns("TestXaml");
            resourceModel.SetupProperty(model => model.IsWorkflowSaved);
            resourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new TestWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion


            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true);

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
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });

            #endregion

            //Execute

            //Verify
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
        }

        #endregion


        #endregion

        #region EditActivity

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_EditActivity")]
        [Description("WorkflowDesignerViewModel EditActivity must connect and load the resources of a disconnected environment.")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_UnitTest_EditActivityWithDisconnectedEnvironment_ConnectsAndLoadsResources()
        {
            const string ServiceName = "Test Service";
            var environmentID = Guid.NewGuid();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(false);
            environment.Setup(e => e.Connect()).Verifiable();
            environment.Setup(e => e.LoadResources()).Verifiable();
            environment.Setup(e => e.ResourceRepository).Returns(new Mock<IResourceRepository>().Object);

            // Setup environment repository to return our environment
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(environment.Object);
    
            #region Setup viewModel

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(new Mock<IResourceRepository>().Object);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            var viewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var modelItem = DsfActivityViewModelTests.CreateModelItem(Guid.NewGuid(), ServiceName, environmentID);
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
            const string ServiceName = "Test Service";
            var environmentID = Guid.Empty;

            #region Setup parentEnvironment

            // If the view model is able to resolve the remote server ID it will search it for the resource
            var resourceRepository = new Mock<IResourceRepository>();
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns((IResourceModel)null);

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
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(new Mock<IResourceRepository>().Object);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            var viewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var modelService = viewModel.Designer.Context.Services.GetService<ModelService>();
            var modelItem = DsfActivityViewModelTests.CreateModelItem(Guid.NewGuid(), ServiceName, environmentID);
            modelItem.Setup(mi => mi.Root).Returns(modelService.Root);

            viewModel.Handle(new EditActivityMessage(modelItem.Object, parentEnvironment.Object.ID, envRepository.Object));

            Assert.AreSame(parentEnvironment.Object, actualEnvironment);
        }

        #endregion

        #region DebugSelectionChanged

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Description("WorkflowDesignerViewModel selects the model item when the selection is changed in the debug window.")]
        [Owner("Trevor Williams-Ros")]
        [Ignore]
        public void WorkflowDesignerViewModel_UnitTest_DebugSelectionChangedFound_SelectsModelItem()
        {
            WorkflowDesignerViewModel_UnitTest_Run(true);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Description("WorkflowDesignerViewModel selects the root flow chart when the selection is changed in the debug window and it is not found.")]
        [Owner("Trevor Williams-Ros")]
        [Ignore]
        public void WorkflowDesignerViewModel_UnitTest_DebugSelectionChangedNotFound_SelectsFlowchart()
        {
            WorkflowDesignerViewModel_UnitTest_Run(false);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Description("WorkflowDesignerViewModel selects the decision when the selection is changed in the debug window and it is not found.")]
        [Owner("Trevor Williams-Ros")]
        [Ignore] // Cannot inject test decision with UniqueID property!
        public void WorkflowDesignerViewModel_UnitTest_DebugSelectionChangedNotFound_SelectsDecision()
        {
            WorkflowDesignerViewModel_UnitTest_Run(true);
        }

        static void WorkflowDesignerViewModel_UnitTest_Run(bool selectsModelItem)
        {
            var debugState = new DebugState { DisplayName = "SelectionChangedTest", ID = Guid.NewGuid() };

            #region Setup workflow

            var activity = new TestActivity
            {
                DisplayName = debugState.DisplayName,
                UniqueID = selectsModelItem ? debugState.ID.ToString() : Guid.NewGuid().ToString()
            };

            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = new FlowStep
                    {
                        Action = activity
                    }
                }
            };

            #endregion

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new TestWorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState });

            Assert.AreEqual(1, viewModel.BringIntoViewHitCount, "WorkflowDesignerViewModel did not bring selection into view.");

            if(selectsModelItem)
            {
                Assert.AreEqual(1, viewModel.SelectModelItemHitCount, "WorkflowDesignerViewModel did not select model item.");
                Assert.AreEqual(typeof(TestActivity), viewModel.SelectModelItemValue.ItemType, "WorkflowDesignerViewModel did not select model item.");
            }
            else
            {
                Assert.AreEqual(1, viewModel.SelectModelItemHitCount, "WorkflowDesignerViewModel did not select root flow chart.");
                Assert.AreEqual(typeof(Flowchart), viewModel.SelectModelItemValue.ItemType, "WorkflowDesignerViewModel did not select root flow chart.");
            }
        }

        #endregion
    }
}
