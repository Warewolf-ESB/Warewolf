using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Activities.WcfEndPoint;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Core.Tests;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WcfInputRegionTests
    {
        const string AppLocalhost = "http://localhost:3142";

        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = AppLocalhost;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_Constructor")]
        public void WcfInputRegion_Constructor_Scenerio_Result()
        {
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };

            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_Constructor")]
        public void WcfInputRegion_Constructor_TestInput_ConstructorEmpty()
        {
            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());

            var region = new WcfInputRegion();
            Assert.AreEqual(region.IsEnabled, false);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_TestClone")]
        public void WcfInputRegion_TestClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
            var clone = region.CloneRegion() as WcfInputRegion;
            if (clone != null)
            {
                Assert.AreEqual(clone.IsEnabled, false);
                Assert.AreEqual(clone.Errors.Count, 0);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_Test")]
        public void WcfInputRegion_Test_InputAddHeader_ExpectHeightChanges()
        {
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_Test")]
        public void WcfInputRegion_Test_InputAddHeader_ExpectHeightChangesPastThree()
        {
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_RestoreFromPrevious")]
        public void WcfInputRegion_RestoreFromPrevious_Restore_ExpectValuesChanged()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var src = new Mock<IWcfServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>());
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);
            // ReSharper disable once UseObjectOrCollectionInitializer
            var regionToRestore = new WcfInputRegionClone();
            regionToRestore.IsEnabled = true;
            //------------Execute Test---------------------------
            region.RestoreRegion(regionToRestore);
            //------------Assert Results-------------------------

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WcfInputRegion_SourceChanged")]
        public void WcfInputRegion_SourceChanged_UpdateValues()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var src = new Mock<IWcfServiceModel>();
            var lst = new ObservableCollection<IWcfServerSource>() { new WcfServiceSourceDefinition() { Name = "bravo" }, new WcfServiceSourceDefinition() { Name = "johnny" } };
            src.Setup(a => a.RetrieveSources()).Returns(lst);
            WcfSourceRegion sourceRegion = new WcfSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()));
            WcfActionRegion WcfActionRegion = new WcfActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWcfEndPointActivity()), sourceRegion);

            var region = new WcfInputRegion(ModelItemUtils.CreateModelItem(act), WcfActionRegion);

            sourceRegion.SelectedSource = lst[0];
            Assert.AreEqual(region.Inputs.Count, 0);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateOnActionSelection_GivenHasInputs_ShouldWriteToActiveDatalist()
        {
            //---------------Set up test pack-------------------
            var eventAggregator = new Mock<IEventAggregator>();

            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator.Object);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);

            DataListSingleton.SetDataList(dataListViewModel);




            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var actionRegion = new Mock<IActionToolRegion<IWcfAction>>();
            actionRegion.Setup(region => region.SelectedAction).Returns(ValueFunction);

            //---------------Assert Precondition----------------

            var countBefore = DataListSingleton.ActiveDataList.ScalarCollection.Count;
            Assert.AreEqual(4, countBefore);
            //---------------Execute Test ----------------------
            var inputRegion = new WcfInputRegion(modelItem, actionRegion.Object);

            var methodInfo = typeof(WcfInputRegion).GetMethod("UpdateOnActionSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            methodInfo.Invoke(inputRegion, new object[] { });
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateOnActionSelection_GivenHasInputs_ShouldWriteToActiveDatalistAndPopulatesInputValues()
        {
            //---------------Set up test pack-------------------
            var eventAggregator = new Mock<IEventAggregator>();

            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator.Object);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input, string.Empty);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input, string.Empty);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);

            DataListSingleton.SetDataList(dataListViewModel);




            var id = Guid.NewGuid();
            var act = new DsfWcfEndPointActivity() { SourceId = id };
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var actionRegion = new Mock<IActionToolRegion<IWcfAction>>();
            actionRegion.Setup(region => region.SelectedAction).Returns(ValueFunction);

            //---------------Assert Precondition----------------

            var countBefore = DataListSingleton.ActiveDataList.ScalarCollection.Count;
            Assert.AreEqual(4, countBefore);
            //---------------Execute Test ----------------------
            var inputRegion = new WcfInputRegion(modelItem, actionRegion.Object);

            var methodInfo = typeof(WcfInputRegion).GetMethod("UpdateOnActionSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            methodInfo.Invoke(inputRegion, new object[] { });
            //---------------Test Result -----------------------
            Assert.AreEqual("[[name]]", inputRegion.Inputs.ToList()[0].Value);
            Assert.AreEqual("[[surname]]", inputRegion.Inputs.ToList()[1].Value);

        }

        private IWcfAction ValueFunction()
        {
            return new WcfAction()
            {
                FullName = "PrintName",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput("name",""),
                    new ServiceInput("surname",""),
                },
            };
        }

        static IDataListViewModel CreateDataListViewModel(Mock<IContextualResourceModel> mockResourceModel, IEventAggregator eventAggregator = null)
        {
            var dataListViewModel = new DataListViewModel(eventAggregator ?? new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            return dataListViewModel;
        }

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
        <uaba:DsfAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""True"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" FieldName=""[[result]]"" FieldValue=""{}{{&#xA;     var max = &quot;[[max]]&quot;;&#xA;&#xA;     if(max.length &lt; 1){&#xA;&#x9;max = 10;&#xA;     }&#xA;     var i = 0;&#xA;     var id = &quot;&lt;Data&gt;&quot;;&#xA;     var value = new Array();&#xA;     for(var i  = 0; i &lt;= max; i++){&#xA;        id += &quot;&lt;regions&gt;&quot;;&#xA;        id += &quot;&lt;id&gt;&quot;+i+&quot;&lt;/id&gt;&quot;;&#xA;        id += &quot;&lt;name&gt;region&quot;+i + &quot;&lt;/name&gt;&quot;;&#xA;        id += &quot;&lt;/regions&gt;&quot;;&#xA;       }&#xA;  id += &quot;&lt;/Data&gt;&quot;;&#xA;}}"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""259,443"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" UpdateAllOccurrences=""True"" />
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
    }
}
