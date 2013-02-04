using Dev2.Composition;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class WorkflowDesignerUnitTest
    {

        #region Test Variables

        private static object _testGuard = new object();
        WorkflowDesignerViewModel LayoutDesigner;
        Mock<IMediatorRepo> _mockMediatorRepo = new Mock<IMediatorRepo>();

        #endregion Test Variables

        #region Test Initialize

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(_testGuard);

            ImportService.CurrentContext = CompositionInitializer.PopUpProviderForTestsWithMockMainViewModel();

            LayoutDesigner = new WorkflowDesignerViewModel(Dev2MockFactory.ResourceModel.Object);
            Mock<IEnvironmentModel> _moqEnvironment = new Mock<IEnvironmentModel>();
            Mock<IMainViewModel> _mockMainViewModel = new Mock<IMainViewModel>();
            Mock<IFrameworkSecurityContext> _mockSecurityContext = new Mock<IFrameworkSecurityContext>();
            Mock<IResourceRepository> _mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IWebActivity> _test = new Mock<IWebActivity>();
            //5559 Check this test when refactor is finished
            //_mockMainViewModel.Setup(mainVM => mainVM.ActiveEnvironment).Returns(_moqEnvironment.Object);
            _mockMainViewModel.Setup(mainVM => mainVM.OpenWebsiteCommand.Execute(null)).Verifiable();

            _mockMediatorRepo.Setup(c => c.addKey(It.IsAny<Int32>(), It.IsAny<MediatorMessages>(), It.IsAny<String>()));
            _mockMediatorRepo.Setup(c => c.deregisterAllItemMessages(It.IsAny<Int32>()));

            _moqEnvironment.Setup(env => env.WebServerAddress).Returns(new Uri("http://localhost:77/dsf"));
            _moqEnvironment.Setup(env => env.WebServerPort).Returns(1234);
            _moqEnvironment.Setup(env => env.Name).Returns("Test");
            _moqEnvironment.Setup(env => env.Connect()).Verifiable();
            _moqEnvironment.Setup(env => env.IsConnected).Returns(true);
            //_moqEnvironment.Setup(env => env.Resources).Returns(ResourceRepository);

            _test.Setup(c => c.XMLConfiguration).Returns("<WebParts/>").Verifiable();
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion Test Initialize

        #region GetBaseUnlimitedFlowchartActivity Tests

        [TestMethod]
        public void GetBaseUnlimitedFlowchartActivity()
        {
            LayoutDesigner = new WorkflowDesignerViewModel(Dev2MockFactory.ResourceModel.Object);
            ActivityBuilder b = LayoutDesigner.GetBaseUnlimitedFlowchartActivity();
            Assert.IsTrue(b != null);
        }

        #endregion GetBaseUnlimitedFlowchartActivity Tests

        #region Find Missing Tests
        /// <summary>
        /// Tests Find Missing is able to find workflow items not in the datalist
        /// </summary>
        [TestMethod]
        public void FindMissingDataListObject_ItemNotExists_ExpectedListContainingMissingDataListItem()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            // For some reason I was unable to retrieve the value for the below from the string resource file
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(mockResourceModel.Object);

            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> DataListItem = new Mock<IDataListItemModel>();
            DataListItem.Setup(list => list.Name).Returns("testing");
            DataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());
            DataListItems.Add(DataListItem.Object);
            mockDataListViewModel.Setup(dlvm => dlvm.DataList).Returns(DataListItems);
            Mock<IPopUp> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;
            workflowDesigner.AddRemoveDataListItems(mockDataListViewModel.Object);
            Assert.IsTrue(workflowDesigner.WorkflowVerifiedDataParts.FirstOrDefault(c => c.Field == "result") != null);

        }

        /// <summary>
        /// Tests that find missing is able to find items not used in workflow but are in datalist
        /// </summary>
        [TestMethod]
        public void RemoveUnused_ItemExists_ExpectedListOfPartsPopulated()
        {
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(mockResourceModel.Object);

            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> DataListItem = new Mock<IDataListItemModel>();
            Mock<IDataListItemModel> secondDataListItem = new Mock<IDataListItemModel>();
            //19.09.2012: massimo.guerrera - amended to include desciptions
            DataListItem.Setup(list => list.Name).Returns("result");
            DataListItem.Setup(list => list.Description).Returns("result desciption");
            DataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());
            secondDataListItem.Setup(list => list.Name).Returns("testing");
            secondDataListItem.Setup(list => list.Description).Returns("testing description");
            secondDataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListItems.Add(DataListItem.Object);
            DataListItems.Add(secondDataListItem.Object);

            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IPopUp> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            mockDataListViewModel.Setup(dlvm => dlvm.DataList).Returns(DataListItems);
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;
            workflowDesigner.AddRemoveDataListItems(mockDataListViewModel.Object);
            Assert.IsTrue(workflowDesigner.WorkflowVerifiedDataParts.Count == 1);
        }

        /// <summary>
        /// Tests that find missing does not return partially formed datalist parts
        /// </summary>
        [TestMethod]
        public void AddRemoveDataListParts_InvalidDataListpart_Expected_No_Items_To_Add()
        {
            //Mock<IContextualResourceModel> mockResourceModel = new Mock<IContextualResourceModel>();
            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            //Requires Specific XAML
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(@"<Activity mc:Ignorable=""sap"" x:Class=""ServiceToBindFrom"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Unlimited.Applications.BusinessDesignStudio.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Unlimited.Framework"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
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
        <uaba:DsfAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""True"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" FieldName=""[[RECORDSET([[).FIELD]]"" FieldValue=""{}{{&#xA;     var max = &quot;[[result]]&quot;;&#xA;&#xA;     if(max.length &lt; 1){&#xA;&#x9;max = 10;&#xA;     }&#xA;     var i = 0;&#xA;     var id = &quot;&lt;Data&gt;&quot;;&#xA;     var value = new Array();&#xA;     for(var i  = 0; i &lt;= max; i++){&#xA;        id += &quot;&lt;regions&gt;&quot;;&#xA;        id += &quot;&lt;id&gt;&quot;+i+&quot;&lt;/id&gt;&quot;;&#xA;        id += &quot;&lt;name&gt;region&quot;+i + &quot;&lt;/name&gt;&quot;;&#xA;        id += &quot;&lt;/regions&gt;&quot;;&#xA;       }&#xA;  id += &quot;&lt;/Data&gt;&quot;;&#xA;}}"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""259,443"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" UpdateAllOccurrences=""True"" />
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

            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(mockResourceModel.Object);

            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> DataListItems = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> DataListItem = new Mock<IDataListItemModel>();
            Mock<IDataListItemModel> secondDataListItem = new Mock<IDataListItemModel>();
            //19.09.2012: massimo.guerrera - amended to include desciptions
            DataListItem.Setup(list => list.Name).Returns("result");
            DataListItem.Setup(list => list.Description).Returns("result desciption");
            DataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());
            secondDataListItem.Setup(list => list.Name).Returns("testing");
            secondDataListItem.Setup(list => list.Description).Returns("testing description");
            secondDataListItem.Setup(list => list.Children).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListItems.Add(DataListItem.Object);
            DataListItems.Add(secondDataListItem.Object);

            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IPopUp> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            mockDataListViewModel.Setup(dlvm => dlvm.DataList).Returns(DataListItems);
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;
            workflowDesigner.AddRemoveDataListItems(mockDataListViewModel.Object);
            Assert.IsTrue(workflowDesigner.WorkflowVerifiedDataParts.Count == 0);
        }

        #endregion Find Missing Tests

        #region NotifyItemSelected Tests

        [TestMethod]
        public void NotifyItemSelected_WebpagePreviewNullReferenceBugFix()
        {
            Mock<IContextualResourceModel> resource = Dev2MockFactory.SetupResourceModelMock();//new Mock<IContextualResourceModel>();
            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resource.Object);
            DsfWebPageActivity page = new DsfWebPageActivity();
            Assert.IsTrue(wf.NotifyItemSelected(page) == false);
        }

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

        private WorkflowDesignerViewModel InitializeWorkflowDesignerForDataListFunctionality(IContextualResourceModel resourceModel)
        {

            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resourceModel);
            var designerAttributes = new Dictionary<Type, Type>();

            designerAttributes.Add(typeof(DsfActivity), typeof(DsfActivityDesigner));
            designerAttributes.Add(typeof(DsfMultiAssignActivity), typeof(DsfMultiAssignActivityDesigner));
            designerAttributes.Add(typeof(DsfAssignActivity), typeof(DsfAssignActivityDesigner));
            designerAttributes.Add(typeof(TransformActivity), typeof(DsfTransformActivityDesigner));
            designerAttributes.Add(typeof(DsfForEachActivity), typeof(DsfForEachActivityDesigner));
            designerAttributes.Add(typeof(DsfCountRecordsetActivity), typeof(DsfCountRecordsetActivityDesigner));
            wf.MediatorRepo = _mockMediatorRepo.Object;
            wf.InitializeDesigner(designerAttributes);

            return wf;
        }

        private WorkflowDesignerViewModel InitializeWorkflowDesignerForCategoryFunctionality(IContextualResourceModel resourceModel)
        {

            WorkflowDesignerViewModel wf = new WorkflowDesignerViewModel(resourceModel);
            var designerAttributes = new Dictionary<Type, Type>();
            wf.MediatorRepo = _mockMediatorRepo.Object;
            wf.InitializeDesigner(designerAttributes);

            return wf;
        }

        private int GetAddRemoveDataListItemsCount(string xamlInput)
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

            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);

            mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(DataListItems);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IPopUp> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            mockDataListViewModel.Setup(dlvm => dlvm.DataList).Returns(DataListItems);
            WorkflowDesignerViewModel workflowDesigner = InitializeWorkflowDesignerForDataListFunctionality(mockResourceModel.Object);
            workflowDesigner.PopUp = mockPopUp.Object;
            workflowDesigner.MediatorRepo = _mockMediatorRepo.Object;
            workflowDesigner.AddRemoveDataListItems(mockDataListViewModel.Object);
            return workflowDesigner.WorkflowVerifiedDataParts.Count;
        }

        private string WorkflowXAMLForTest()
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

        #endregion Internal Test Methods



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

    }
}
