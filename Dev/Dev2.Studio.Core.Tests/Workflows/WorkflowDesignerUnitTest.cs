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
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.CountRecordsNullHandler;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.Service;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Core.Tests.Environments;
using Dev2.Data;
using Dev2.Factory;
using Dev2.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Threading;
using Dev2.Utilities;
using Dev2.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.ViewModels;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Instrumentation;
using Dev2.Activities;

namespace Dev2.Core.Tests.Workflows
{
    [TestClass]
    [TestCategory("Studio Workflows Core")]
    [DoNotParallelize]
    public partial class WorkflowDesignerUnitTest
    {
        static bool _isDesignerInited;

        #region Remove Unused Tests

        [TestMethod]
        public void RemoveAllUnusedDataListObjectsWithItemsNotUsedExpectedItemsRemoved()
        {
            var eventAggregator = new EventAggregator();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);


            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.Add);
            dataListViewModel.RecsetCollection.Clear();
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            dataListViewModel.RemoveUnusedDataListItems();
            workflowDesigner.Dispose();

        }

        [TestMethod]
        public void SetModelToDirtyAndExpectThatItemsWillBeAdded()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            var serverRepository = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepository.Object);
            var eventAggregator = new EventAggregator();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);


            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.Add);
            dataListViewModel.RecsetCollection.Clear();
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            var dataListItem3 = new ScalarItemModel("scalar8", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem3);
            Thread.Sleep(2000);
            workflowDesigner.Dispose();
            Assert.AreEqual(6, dataListViewModel.ScalarCollection.Count);
        }

        [TestMethod]
        public void SetModelToCleanAndExpectThatNoItemsWillBeAdded()
        {
            var eventAggregator = new EventAggregator();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());

            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input);
            var dataListItem3 = new ScalarItemModel("scalar8", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);


            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.Add);
            dataListViewModel.RecsetCollection.Clear();
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);

            dataListItems.Add(dataListItem3);
            Thread.Sleep(3000);

            workflowDesigner.Dispose();
            Assert.AreEqual(6, dataListViewModel.ScalarCollection.Count);

        }
        [TestMethod]
        public void MissingPartsMessageOnlySentWhenThereWorkToDoExpect1Call()
        {
            // Set up event agg

            var evtAg = new Mock<IEventAggregator>();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(GetAddMissingWorkflowXml());

            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.ScalarCollection).Returns(new OptomizedObservableCollection<IScalarItemModel>());
            mockDataListViewModel.Setup(model => model.UpdateDataListItems(It.IsAny<IResourceModel>(), It.IsAny<IList<IDataListVerifyPart>>())).Verifiable();
            var dataListViewModel = mockDataListViewModel.Object;
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            DataListSingleton.SetDataList(dataListViewModel);

            dataListItems.ToList().ForEach(dataListViewModel.Add);
            var workflowDesigner = CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(mockResourceModel.Object, evtAg.Object);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.Dispose();
        }

        #endregion

        #region AddMode Missing DataList Items        

        [TestMethod]
        public void GetDecisionElementsWithMissmatchedBracketsInADecisionFieldExpectedCorrectVariableGottenFromDecision()
        {
            //Execute
            var model = CreateWorkflowDesignerViewModel(Dev2MockFactory.ResourceModel.Object, null, false);
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var dataListViewModel = CreateDataListViewModel(mockResourceModel);
            var actual = WorkflowDesignerViewModel.TryGetDecisionElements("Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:!]]!,!Col2!:![[scalar]]!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)", dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("[[scalar]]", actual[0], "Find missing found an invalid variable in a decision");

        }

        [TestMethod]
        public void GetDecisionElementsWhenItemAlreadyInDataListShouldStillReturnInList()
        {
            //Execute
            var resourceModel = Dev2MockFactory.ResourceModel;
            var eventAggregator = new EventAggregator();


            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var model = CreateWorkflowDesignerViewModel(eventAggregator, resourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();

            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);

            var recsetModel = new RecordSetItemModel("RecSet");
            dataListViewModel.Add(recsetModel);
            dataListViewModel.RecsetCollection[2].Children.Add(new RecordSetFieldItemModel("f1", parent: recsetModel));
            const string expression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[RecSet().f1]]!,!Col2!:!Is Equal!,!Col3!:!0!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)";
            var actual = WorkflowDesignerViewModel.TryGetDecisionElements(expression, dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("[[RecSet().f1]]", actual[0], "Find missing found an invalid variable in a decision");
        }

        [TestMethod]
        public void GetDecisionElementsWhenItemAlreadyInDataListShouldNotReturnRecsetIfScalar()
        {
            //Execute
            var resourceModel = Dev2MockFactory.ResourceModel;
            var eventAggregator = new EventAggregator();

            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var model = CreateWorkflowDesignerViewModel(eventAggregator, resourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();

            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);


            var recsetModel = new RecordSetItemModel("RecSet");
            dataListViewModel.Add(recsetModel);
            dataListViewModel.RecsetCollection[2].Children.Add(new RecordSetFieldItemModel("a", parent: recsetModel));
            const string expression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[a]]!,!Col2!:!Is Equal!,!Col3!:!0!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)";
            var actual = WorkflowDesignerViewModel.TryGetDecisionElements(expression, dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            var expected = "[[a]]";
            var actualResult = actual[0];
            FixBreaks(ref expected, ref actualResult);
            Assert.AreEqual(expected, actualResult, "Find missing found an invalid variable in a decision");
        }
        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }
        [TestMethod]
        public void GetDecisionElementsWhenItemAlreadyInDataListShouldNotReturnRecsetIfScalarNonExistent()
        {
            //Execute
            var resourceModel = Dev2MockFactory.ResourceModel;
            var eventAggregator = new EventAggregator();

            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var model = CreateWorkflowDesignerViewModel(eventAggregator, resourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();

            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);


            var recsetModel = new RecordSetItemModel("RecSet");
            dataListViewModel.Add(recsetModel);
            dataListViewModel.RecsetCollection[2].Children.Add(new RecordSetFieldItemModel("aasszzz", parent: recsetModel));
            const string expression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[a]]!,!Col2!:!Is Equal!,!Col3!:!0!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If ]] Is Equal [[scalar]]!}\",AmbientDataList)";
            var actual = WorkflowDesignerViewModel.TryGetDecisionElements(expression, dataListViewModel);
            model.Dispose();
            //Assert
            Assert.AreEqual(1, actual.Count, "Find missing returned an unexpected number of results when finding variables in a decision");
            Assert.AreEqual("[[a]]", actual[0], "Find missing found an invalid variable in a decision");
        }


        #endregion

        #region Set Unused DataList Items

        [TestMethod]
        public void FindUnusedDataListItemsWithUnusedDataListItemsExpectedItemsToBeSetToNotUsed()
        {
            var eventAggregator = new EventAggregator();

            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var dataListViewModel = CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);
            DataListSingleton.SetDataList(dataListViewModel);
            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListItems.ToList().ForEach(dataListViewModel.Add);
            dataListViewModel.RecsetCollection.Clear();
            var workflowDesigner = CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(mockResourceModel.Object, eventAggregator);
            workflowDesigner.PopUp = mockPopUp.Object;

            Assert.IsTrue(dataListViewModel.ScalarCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ScalarCollection[1].IsUsed);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.Dispose();
        }

        #endregion

        #region Internal Test Methods

        public static StringBuilder WorkflowXAMLForTest()
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

        StringBuilder GetAddMissingWorkflowXml()
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
        <uaba:DsfDataSplitActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Data Split (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""258,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" ReverseOrder=""False"" SimulationMode=""OnDemand"" SourceString=""[[test1]]"" UniqueID=""97f4088a-a5c6-4e34-bfc9-6f10a0a44cd3"">
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
            <uaba:DsfCountRecordsetNullHandlerActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" CountNumber=""[[test5]]"" DatabindRecursive=""False"" DisplayName=""Count Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,86"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[test4()]]"" SimulationMode=""OnDemand"" UniqueID=""6ffbfe4f-2425-4fa3-805d-4815652f4236"">
              <uaba:DsfCountRecordsetNullHandlerActivity.ParentInstanceID>
                <InOutArgument x:TypeArguments=""x:String"" />
              </uaba:DsfCountRecordsetNullHandlerActivity.ParentInstanceID>
            </uaba:DsfCountRecordsetNullHandlerActivity>
            <FlowStep.Next>
              <FlowStep x:Name=""__ReferenceID12"">
                <sap:WorkflowViewStateService.ViewState>
                  <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                    <av:Point x:Key=""ShapeLocation"">360,157</av:Point>
                    <av:Size x:Key=""ShapeSize"">200,86</av:Size>
                    <av:PointCollection x:Key=""ConnectorLocation"">360,200 330,200 330,280 269,280</av:PointCollection>
                  </scg:Dictionary>
                </sap:WorkflowViewStateService.ViewState>
                <uaba:DsfFolderRead Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Read Folder"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,86"" InputPath=""[[test5]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test6]]"" SimulationMode=""OnDemand"" UniqueID=""f09a9a50-26b2-4d42-b630-a70a1edb43d8"" Username="""">
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
                    <uaba:DsfForEachActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" test=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""For Each"" FailOnFirstError=""False"" ForEachElementName=""[[test7]]"" FromDisplayName=""[[test7]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""218,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""d1014511-2b7d-4270-8454-a80a36ab821f"">
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
                        <uaba:DsfCalculateActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Calculate"" Expression=""sum([[test8]])"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,106"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test9]]"" SimulationMode=""OnDemand"" UniqueID=""99232732-3010-4cfb-8b4a-077a963734a2"">
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
                            <uaba:DsfSortRecordsActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Sort Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""220,86"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SelectedSort=""Forward"" SimulationMode=""OnDemand"" SortField=""[[test10]]"" UniqueID=""8a8591df-00bb-48d4-89e6-f19ea4127bca"">
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
                                <uaba:DsfFileWrite Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" Append=""False"" DatabindRecursive=""False"" DisplayName=""Write"" FileContents=""[[test12]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""210,111"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test11]]"" Overwrite=""False"" Password="""" Result=""[[test13]]"" SimulationMode=""OnDemand"" UniqueID=""f0ed679b-6dba-41b5-b51b-a5134c109d27"" Username="""">
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
                                    <uaba:DsfBaseConvertActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DisplayName=""Base Conversion (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""288,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""ba500b52-4940-4f8a-8371-ddaf67b7d5f7"">
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
                                        <uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""263,113"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test18]]"" SimulationMode=""OnDemand"" UniqueID=""9d4ed269-7c02-415d-ab0a-9b7211f80e91"">
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
                                            <uaba:DsfCaseConvertActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DisplayName=""Case Conversion (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""239,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""1cb10777-e1c4-4246-8263-f28bd06d482c"">
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
                                                <uaba:DsfReplaceActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" CaseMatch=""False"" DatabindRecursive=""False"" DisplayName=""Replace"" FieldsToSearch=""[[test21]]"" Find=""[[test22]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""234,136"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" ReplaceWith=""[[test23]]"" Result=""[[test24]]"" SimulationMode=""OnDemand"" UniqueID=""652252f7-ed9f-43aa-896c-0806e105115d"">
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
                                                    <uaba:DsfFindRecordsActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DisplayName=""Find Record Index"" FieldsToSearch=""[[test25]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""220,111"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MatchCase=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test27]]"" SearchCriteria=""[[test26]]"" SearchType=""&lt;"" SimulationMode=""OnDemand"" StartIndex="""" UniqueID=""7caa5d01-a8cb-4881-aa5b-965e48e29e36"">
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
                                                        <uaba:DsfDateTimeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DateTime=""[[test28]]"" DisplayName=""Date and Time"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,161"" InputFormat=""[[test29]]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputFormat=""[[test31]]"" Result=""[[test32]]"" SimulationMode=""OnDemand"" TimeModifierAmount=""0"" TimeModifierAmountDisplay=""[[test30]]"" TimeModifierType="""" UniqueID=""4af25a0a-0e74-4aa1-a1ee-8ed5044fb7fe"">
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
                                                            <uaba:DsfNumberFormatActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DecimalPlacesToShow=""[[test35]]"" DisplayName=""Format Number"" Expression=""[[test33]]"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,136"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test36]]"" RoundingDecimalPlaces=""[[test34]]"" RoundingType=""Up"" SimulationMode=""OnDemand"" UniqueID=""af9c603a-5abc-4fb4-a214-1ab4ecce85d4"">
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
                                                                <uaba:DsfActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""CalculateTaxReturns"" FriendlySourceName=""localhost"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,87"" IconPath=""pack://application:,,,/Warewolf Studio;component/images/workflowservice2.png"" InputMapping=""&lt;Inputs&gt;&lt;Input Name=&quot;EmpNo&quot; Source=&quot;[[test37]]&quot; /&gt;&lt;Input Name=&quot;TaxNumber&quot; Source=&quot;[[test38]]&quot; /&gt;&lt;Input Name=&quot;AnualIncome&quot; Source=&quot;[[test39]]&quot; /&gt;&lt;Input Name=&quot;IncomeAfterTax&quot; Source=&quot;[[test40]]&quot; /&gt;&lt;Input Name=&quot;AmountOfTaxReturned&quot; Source=&quot;[[test41]]&quot; /&gt;&lt;/Inputs&gt;"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""True"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&lt;Outputs&gt;&lt;Output Name=&quot;EmpNo&quot; MapsTo=&quot;EmpNo&quot; Value=&quot;[[test42]]&quot; /&gt;&lt;Output Name=&quot;TaxNumber&quot; MapsTo=&quot;TaxNumber&quot; Value=&quot;[[test43]]&quot; /&gt;&lt;Output Name=&quot;AnualIncome&quot; MapsTo=&quot;AnualIncome&quot; Value=&quot;[[test44]]&quot; /&gt;&lt;Output Name=&quot;IncomeAfterTax&quot; MapsTo=&quot;IncomeAfterTax&quot; Value=&quot;[[test45]]&quot; /&gt;&lt;Output Name=&quot;AmountOfTaxReturned&quot; MapsTo=&quot;AmountOfTaxReturned&quot; Value=&quot;[[test46]]&quot; /&gt;&lt;/Outputs&gt;"" RemoveInputFromOutput=""False"" ServiceName=""CalculateTaxReturns"" SimulationMode=""OnDemand"" ToolboxFriendlyName=""CalculateTaxReturns"" Type=""Workflow"" UniqueID=""4f7e8a6b-632e-48d1-83af-857cac4e7676"">
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
                                                                    <uaba:DsfZip Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" ArchiveName="""" ArchivePassword="""" CompressionRatio="""" DatabindRecursive=""False"" DisplayName=""Zip"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test47]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test48]]"" Password="""" Result=""[[test49]]"" SimulationMode=""OnDemand"" UniqueID=""df13d832-15cd-41b6-8139-a0036a656061"" Username="""">
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
                                                                        <uaba:DsfPathDelete Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Delete"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,86"" InputPath=""[[test50]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test51]]"" SimulationMode=""OnDemand"" UniqueID=""dc0af9b7-a9b9-46fb-8a91-1636be6aeeb5"" Username="""">
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
                                                                            <uaba:DsfPathMove Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Move"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test52]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test53]]"" Overwrite=""False"" Password="""" Result=""[[test54]]"" SimulationMode=""OnDemand"" UniqueID=""0eab6512-5425-45e4-845c-0851f1c337eb"" Username="""">
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
                                                                                <uaba:DsfUnZip Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" ArchivePassword="""" DatabindRecursive=""False"" DisplayName=""UnZip"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,111"" InputPath=""[[test55]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test56]]"" Overwrite=""False"" Password="""" Result=""[[test57]]"" SimulationMode=""OnDemand"" UniqueID=""18133af1-9540-47f4-9c13-c144dbf78529"" Username="""">
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
                                                                                    <uaba:DsfWebPageActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" FriendlySourceName=""{x:Null}"" HelpLink=""{x:Null}"" IconPath=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" ToolboxFriendlyName=""{x:Null}"" Type=""{x:Null}"" WebsiteRegionName=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""Webpage 1"" FormEncodingType=""application/x-www-form-urlencoded"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,46"" InputMapping=""&lt;Inputs&gt;&lt;Input Name=&quot;asdas&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;Variable&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sadag&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sdfsdfsdf&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;asdasd&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;asda&quot; Source=&quot;&quot; /&gt;&lt;Input Name=&quot;sczxvxcv&quot; Source=&quot;&quot; /&gt;&lt;/Inputs&gt;"" InstructionList=""[InstructionList]"" IsPreview=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MetaTags="""" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&lt;Outputs&gt;&lt;Output Name=&quot;asdas&quot; MapsTo=&quot;asdas&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;Variable&quot; MapsTo=&quot;Variable&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sadag&quot; MapsTo=&quot;sadag&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sdfsdfsdf&quot; MapsTo=&quot;sdfsdfsdf&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;asdasd&quot; MapsTo=&quot;asdasd&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;asda&quot; MapsTo=&quot;asda&quot; Value=&quot;&quot; /&gt;&lt;Output Name=&quot;sczxvxcv&quot; MapsTo=&quot;sczxvxcv&quot; Value=&quot;&quot; /&gt;&lt;/Outputs&gt;"" RemoveInputFromOutput=""False"" ServiceName=""Webpage 1"" SimulationMode=""OnDemand"" UniqueID=""f1aaab83-dbf2-41d3-8122-4cb838339213"" WebsiteServiceName=""Default Master Page"" XMLConfiguration=""&lt;WebParts/&gt;"">
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
                                                                                        <uaba:DsfPathCopy Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Copy"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test58]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test59]]"" Overwrite=""False"" Password="""" Result=""[[test60]]"" SimulationMode=""OnDemand"" UniqueID=""6044030a-6fec-4bc9-a20b-7ec97c8e148c"" Username="""">
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
                                                                                            <uaba:DsfPathCreate Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Create"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,86"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test61]]"" Overwrite=""False"" Password="""" Result=""[[test62]]"" SimulationMode=""OnDemand"" UniqueID=""11e7b5b4-d821-4a0b-8b64-d5a37a44349a"" Username="""">
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
                                                                                                <uaba:DsfFileRead Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Read File"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""210,86"" InputPath=""[[test63]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Password="""" Result=""[[test64]]"" SimulationMode=""OnDemand"" UniqueID=""bf8f7e76-8567-40ae-9afa-c989781579f0"" Username="""">
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
                                                                                                    <uaba:DsfPathRename Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""Rename"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,111"" InputPath=""[[test65]]"" InstructionList=""[InstructionList]"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""[[test66]]"" Overwrite=""False"" Password="""" Result=""[[test67]]"" SimulationMode=""OnDemand"" UniqueID=""127da2c5-6741-4418-94f3-43831521cc87"" Username="""">
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
                                                                                                        <uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (2)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""278,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""62c410de-109f-4f95-a262-0ba8ebfbae64"" UpdateAllOccurrences=""False"">
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
                                                                                                            <uaba:DsfIndexActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" Characters=""[[test73]]"" DatabindRecursive=""False"" Direction=""Left to Right"" DisplayName=""Find Index"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""225,161"" InField=""[[test72]]"" Index=""First Occurrence"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" MatchCase=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[test74]]"" SimulationMode=""OnDemand"" StartIndex=""0"" UniqueID=""3e62d9e0-44ee-44cf-8cb4-16b6cc797aab"">
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
                                                                                                                <uaba:DsfDateTimeDifferenceActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" Explicit=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" AddMode=""False"" DatabindRecursive=""False"" DisplayName=""Date and Time Difference"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,161"" Input1=""[[test75]]"" Input2=""[[test76]]"" InputFormat=""[[test77]]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputType=""Years"" Result=""[[test78]]"" SimulationMode=""OnDemand"" UniqueID=""33b638ef-4afb-421a-b2ba-e20b02cc4930"">
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

        #region UpdateMode Resource Message Handler

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_BuildDataPart")]
        public void WorkflowDesignerViewModel_BuildDataPart_ValidItem_ShouldAddItemToDataList()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new EventAggregator();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            DataListSingleton.SetDataList(dataListViewModel);
            dataListViewModel.AddBlankRow(null);
            //------------Execute Test---------------------------
            workflowDesigner.Handle(new AddStringListToDataListMessage(new List<string> { "[[rec().set]]", "[[test()]]", "[[scalar]]" }));
            //------------Assert Results-------------------------
            var dataListItemModels = DataListSingleton.ActiveDataList.DataList;
            workflowDesigner.Dispose();
            Assert.AreEqual(10, dataListItemModels.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_BuildDataPart")]
        public void WorkflowDesignerViewModel_BuildDataPart_InValidItems_ShouldNotAddItemToDataList()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new EventAggregator();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowXAMLForTest());
            var workflowDesigner = CreateWorkflowDesignerViewModel(eventAggregator, mockResourceModel.Object, null, false);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            DataListSingleton.SetDataList(dataListViewModel);
            dataListViewModel.AddBlankRow(null);
            //------------Execute Test---------------------------
            try
            {
                workflowDesigner.Handle(new AddStringListToDataListMessage(new List<string> { "[[rec().s*et]]", "[[test**()]]", "[[1scalar]]" }));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                workflowDesigner.Dispose();
            }
            //------------Assert Results--------------------------
            var dataListItemModels = DataListSingleton.ActiveDataList.RecsetCollection;
            Assert.AreEqual(4, dataListItemModels.Count);
            Assert.AreEqual("", dataListItemModels[3].DisplayName);
            Assert.AreEqual("rec()", dataListItemModels[2].Name);
            Assert.AreEqual("", dataListItemModels[3].Children[0].DisplayName);
        }

        #endregion

        #region InitializeDesigner

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowDesigner_Initialize")]
        public void WorkflowDesigner_Initialize_WhenWorkflowXamlNull_ExpectWorkflowXamlFetch()
        {

            //------------Setup for test--------------------------
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var repo = new Mock<IResourceRepository>();
            repo.Setup(c => c.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage { Message = null });
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
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);

            #region Setup viewModel

            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            ExecuteMessage exeMsg = null;

            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(exeMsg);


            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            var xamlBuilder = new StringBuilder("abc");

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
        
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInitializesFramework45Properties()
        {
            var repo = new Mock<IResourceRepository>();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");

            var wfd = CreateWorkflowDesignerViewModel(crm.Object);
            var attr = new Dictionary<Type, Type>();

            wfd.InitializeDesigner(attr);

            var designerConfigService = wfd.Designer.Context.Services.GetService<DesignerConfigurationService>();
            Assert.AreEqual(new FrameworkName(".NETFramework", new Version(4, 5)), designerConfigService.TargetFrameworkName);
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


            wfd.Dispose();
        }

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WorkflowDesignerViewModelInitializeDesignerExpectedInvokesWorkflowHelper()
        {
            var repo = new Mock<IResourceRepository>();
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowDesignerViewModel_UnitTest_ConstructorWithNullWorkflowHelper_ThrowsArgumentNullException()
        {

            new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object,

                null, null,
                new Mock<IPopupController>().Object, new SynchronousAsyncWorker(), false);

        }

        #endregion

        #region ServiceDefinition

        [TestMethod]
        public void WorkflowDesignerViewModelServiceDefinitionExpectedInvokesWorkflowHelperSerializeWorkflow()
        {
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
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
            mockEnv.Setup(c => c.EnvironmentID).Returns(envId);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv.Object);
            Assert.IsTrue(testAct.ServiceUri == null);
            Assert.IsTrue(testAct.ServiceServer == Guid.Empty);

            var contextEnvironment = new Mock<IServer>();
            contextEnvironment.Setup(e => e.EnvironmentID).Returns(resourceEnvironmentID);

            var activity = new DsfActivity();
            var workflow = new ActivityBuilder { Implementation = activity };

            #region Setup resourceModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            envConn.Setup(m => m.WebServerUri).Returns(new Uri(ServiceUri));
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.Setup(m => m.Environment.EnvironmentID).Returns(resourceEnvironmentID);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 1");
            #endregion

            #region Setup viewModel

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);

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
            var contextEnvironment = new Mock<IServer>();
            contextEnvironment.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            var mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            var envId2 = Guid.NewGuid();
            var mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.EnvironmentID).Returns(envId2);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true, environmentRepository, true);
            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv2.Object);
            Assert.IsTrue(testAct.ServiceUri == "https://localhost:3143/" || testAct.ServiceUri == "http://localhost:3142/" || testAct.ServiceUri == "http://127.0.0.1:3142/", "Expected https://localhost:3143/ or http://localhost:3142/ or http://127.0.0.1:3142/ but got: " + testAct.ServiceUri);
            Assert.IsTrue(testAct.ServiceServer == envId2);
            Assert.AreEqual("Test *", testClass.DisplayName);
            Assert.AreEqual("WorkflowService", testClass.ResourceType.ToString());

            var activity = new DsfActivity();
            var workflow = new ActivityBuilder { Implementation = activity };

            #region Setup resourceModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            envConn.Setup(m => m.WebServerUri).Returns(new Uri(ServiceUri));
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name");
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.Setup(m => m.Environment.EnvironmentID).Returns(resourceEnvironmentID);

            #endregion

            #region Setup viewModel

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);

            #endregion

            viewModel.TestCheckIfRemoteWorkflowAndSetProperties(activity, resourceModel.Object, contextEnvironment.Object);

            viewModel.Dispose();

            Assert.AreEqual("http://localhost:1234/", activity.ServiceUri);
            Assert.AreEqual(resourceEnvironmentID, activity.ServiceServer);
        }

        [TestMethod]
        public void CheckIfRemoteWorkflowAndSetPropertiesExpectedResourceTypeToBeUnknown()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            var contextEnvironment = new Mock<IServer>();
            contextEnvironment.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            var mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            var envId2 = Guid.NewGuid();
            var mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.EnvironmentID).Returns(envId2);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = DsfActivityFactory.CreateDsfActivity(mockResourceModel.Object, new DsfActivity(), true, environmentRepository, true);
            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object);
            testClass.ResourceModel = null;
            testClass.TestCheckIfRemoteWorkflowAndSetProperties(testAct, mockResourceModel.Object, mockEnv2.Object);
            Assert.IsTrue(testAct.ServiceUri == "https://localhost:3143/" || testAct.ServiceUri == "http://localhost:3142/" || testAct.ServiceUri == "http://127.0.0.1:3142/", "Expected https://localhost:3143/ or http://localhost:3142/ or http://127.0.0.1:3142/ but got: " + testAct.ServiceUri);
            Assert.IsTrue(testAct.ServiceServer == envId2);
            Assert.AreEqual("Unknown", testClass.ResourceType.ToString());
        }

        #endregion

        #region ModelServiceModelChanged

        [TestMethod]
        public void WorkflowDesignerViewModelModelServiceModelChangedWithNextReferencingSelfExpectedClearsNext()
        {
            TestModelServiceModelChangedSelfReference(true);
        }

        [TestMethod]
        public void WorkflowDesignerViewModelModelServiceModelChangedWithNextReferencingOtherExpectedDoesNotClearNext()
        {
            TestModelServiceModelChangedSelfReference(false);
        }

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
            var contextualResourceModel = crm.Object;

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
            wfd.TestModelServiceModelChanged(args.Object);

            wfd.Dispose();

            //Verify
            prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DragOnToForEach")]
        [Owner("Hagashen Naidu")]

        public void WorkflowDesignerViewModel_PerformAddItems_ForEachActivity_DragOnRemoteWorkflow()

        {
            #region Setup view model constructor parameters

            if (Application.Current != null)
            {
                try
                {
                    Application.Current.MainWindow = null;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            var repo = new Mock<IResourceRepository>();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(Guid.NewGuid());
            var mockRemoteEnvironment = new Mock<IServer>();
            var remoteServerID = Guid.NewGuid();
            mockRemoteEnvironment.Setup(model => model.EnvironmentID).Returns(remoteServerID);
            var mockRemoteConnection = new Mock<IEnvironmentConnection>();
            mockRemoteConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://remoteserver:3142/"));
            mockRemoteEnvironment.Setup(model => model.Connection).Returns(mockRemoteConnection.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockRemoteEnvironment.Object);
            repo.Setup(repository => repository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(mockResourceModel.Object);
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
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
            var mockModelChangeInfo = new Mock<ModelChangeInfo>();
            mockModelChangeInfo.Setup(info => info.PropertyName).Returns("Handler");
            mockModelChangeInfo.Setup(info => info.ModelChangeType).Returns(ModelChangeType.PropertyChanged);
            var mock = new Mock<ModelProperty>();
            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(item => item.Content).Returns(mock.Object);
            mockModelChangeInfo.Setup(info => info.Subject).Returns(mockModelItem.Object);

            eventArgs.Setup(c => c.ModelChangeInfo).Returns(mockModelChangeInfo.Object);

            DsfActivity setValue = null;
            mock.Setup(property => property.SetValue(It.IsAny<object>())).Callback<object>(o =>
            {
                setValue = o as DsfActivity;
            });

#pragma warning restore 618

            #endregion

            var eventAggregator = new Mock<IEventAggregator>();
            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);
            wd.SetActiveEnvironment(env.Object);
            var activeEnvironment = wd.GetActiveEnvironment();
            Assert.AreEqual(activeEnvironment, env.Object);
            wd.SetDataObject(new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object));

            // Execute unit
            wd.TestModelServiceModelChanged(eventArgs.Object);

            wd.Dispose();
            Assert.IsNotNull(setValue);
            Assert.AreEqual("http://remoteserver:3142/", setValue.ServiceUri);
            Assert.AreEqual(remoteServerID, setValue.ServiceServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow")]
        public void WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow_MessageWithArgs_Saves()
        {
            //------------Setup for test--------------------------
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
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
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);

            var unsavedResourceModel = new Mock<IContextualResourceModel>();
            unsavedResourceModel.Setup(model => model.ResourceName).Returns("Unsaved 1");
            unsavedResourceModel.Setup(model => model.WorkflowXaml).Returns(new StringBuilder("workflow xaml"));
            unsavedResourceModel.Setup(r => r.Environment).Returns(env.Object);
            unsavedResourceModel.Setup(a => a.Environment.ResourceRepository.SaveToServer(It.IsAny<IResourceModel>())).Returns(new ExecuteMessage());
            var saveUnsavedWorkflowMessage = new SaveUnsavedWorkflowMessage(unsavedResourceModel.Object, "new name", "new category", false, false, "");
            //------------Execute Test---------------------------
            wd.Handle(saveUnsavedWorkflowMessage);
            var workflowLink = wd.DisplayWorkflowLink;
            //------------Assert Results-------------------------
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>()), Times.Never());
            unsavedResourceModel.Verify(a => a.Environment.ResourceRepository.SaveToServer(It.IsAny<IResourceModel>()), Times.Once());
            Assert.AreEqual(workflowLink, wd.DisplayWorkflowLink);
            wd.Dispose();


        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow")]
        public void WorkflowDesignerViewModel_HandleSaveUnsavedWorkflow_MessageWithArgs_OpenTab_Saves()
        {
            //------------Setup for test--------------------------
            #region Setup view model constructor parameters
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
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
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Verifiable();
            eventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, wh.Object, eventAggregator.Object);

            var unsavedResourceModel = new Mock<IContextualResourceModel>();
            unsavedResourceModel.Setup(model => model.ResourceName).Returns("Unsaved 1");
            unsavedResourceModel.Setup(r => r.Environment).Returns(env.Object);
            unsavedResourceModel.Setup(a => a.Environment.ResourceRepository.SaveToServer(It.IsAny<IResourceModel>())).Returns(new ExecuteMessage());
            var saveUnsavedWorkflowMessage = new SaveUnsavedWorkflowMessage(unsavedResourceModel.Object, "new name", "new category", true, false, "");
            //------------Execute Test---------------------------
            wd.Handle(saveUnsavedWorkflowMessage);
            //------------Assert Results-------------------------
            eventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
            unsavedResourceModel.Verify(a => a.Environment.ResourceRepository.SaveToServer(It.IsAny<IResourceModel>()));

        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Description("WorkflowDesigner assigns new unique id on copy paste of an activity/tool")]
        [Owner("Ashley Lewis")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithUniqueID_NewIDAssigned()
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
            (testAct as IDev2Activity).UniqueID = string.Empty;

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            var source = new Mock<ModelItem>();
            source.Setup(c => c.Content).Returns(prop.Object);

            var modelService = new Mock<ModelService>();

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object, modelService: modelService.Object);
            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Content);
            var dev2Activity = actual.Content.ComputedValue as IDev2Activity;
            Assert.IsNotNull(dev2Activity);
            Assert.AreNotEqual(notExpected, dev2Activity.UniqueID, "Activity ID not changed");
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Description("WorkflowDesigner assigns new unique id on copy paste of an activity/tool")]
        [Owner("Ashley Lewis")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithUniqueID_NotPaste_ShouldKeepID()
        {
            var expectedId = Guid.NewGuid().ToString();

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
            (testAct as IDev2Activity).UniqueID = expectedId;

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            var source = new Mock<ModelItem>();
            source.Setup(c => c.Content).Returns(prop.Object);

            var modelService = new Mock<ModelService>();

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object, modelService: modelService.Object);
            wd.SetIsPaste(false);
            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Content);
            var dev2Activity = actual.Content.ComputedValue as IDev2Activity;
            Assert.IsNotNull(dev2Activity);
            Assert.AreEqual(expectedId, dev2Activity.UniqueID, "Activity ID not changed");
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Description("WorkflowDesigner assigns new unique id on copy paste of an activity/tool")]
        [Owner("Ashley Lewis")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithUniqueID_Paste_ShouldNotKeepID()
        {
            var expectedId = Guid.NewGuid().ToString();

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
            (testAct as IDev2Activity).UniqueID = expectedId;

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            var source = new Mock<ModelItem>();
            source.Setup(c => c.Content).Returns(prop.Object);

            var modelService = new Mock<ModelService>();

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object, modelService: modelService.Object);
            wd.SetIsPaste(true);
            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Content);
            var dev2Activity = actual.Content.ComputedValue as IDev2Activity;
            Assert.IsNotNull(dev2Activity);
            Assert.AreNotEqual(expectedId, dev2Activity.UniqueID, "Activity ID not changed");
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Owner("Pieter Terblanche")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithDsfDecision_DecisionHandled()

        {
            var notExpected = Guid.NewGuid().ToString();

            #region Setup view model constructor parameters

            var properties = new Dictionary<string, Mock<ModelProperty>>();
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
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new FlowDecision(new TestDecisionActivity
            {
                DisplayName = "Test",
                UniqueID = notExpected
            });

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(c => c.Content).Returns(prop.Object);
            source.Setup(c => c.ItemType).Returns(testAct.GetType());

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
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Owner("Pieter Terblanche")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithDsfSwitch_SwitchHandled()

        {
            #region Setup view model constructor parameters

            var properties = new Dictionary<string, Mock<ModelProperty>>();
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
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new FlowSwitch<string>
            {
                DisplayName = "TestSwitch",
                Expression = Activity<string>.FromValue("True")
            };

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(c => c.Content).Returns(prop.Object);
            source.Setup(c => c.ItemType).Returns(testAct.GetType());

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
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Owner("Pieter Terblanche")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithFlowStepWithServiceName_FlowStepHandled()
        {
            CustomContainer.DeRegister<IApplicationTracker>();
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);

            #region Setup view model constructor parameters

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(r => r.DisplayName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            #endregion

            #region setup Mock ModelItem
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new DsfActivity
            {
                DisplayName = "Test",
                ServiceName = "NewService",
                ResourceID = Guid.NewGuid()
            };

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
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

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object);

            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Owner("Pieter Terblanche")]
        public void WorkflowDesignerViewModel_PerformAddItems_ModelItemWithFlowStepWithoutServiceName_FlowStepHandled()

        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            #region Setup view model constructor parameters
            var resourceId = Guid.NewGuid();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var repo = new Mock<IResourceRepository>();

            var env = EnviromentRepositoryTest.CreateMockEnvironment();


            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            repo.Setup(repository => repository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(crm.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            #endregion

            #region setup Mock ModelItem
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new DsfActivity();
            testAct.DisplayName = "Test";

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
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

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object);
            var explorerItem = new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object,
                a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object);


            explorerItem.ResourceId = resourceId;
            wd.SetDataObject(explorerItem);


            // Execute unit
            var actual = wd.TestPerformAddItems(source.Object);

            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_PerformAddItems")]
        [Owner("Pieter Terblanche")]
        public void WorkflowDesignerViewModel_PerformAddItems_ApplyForDrop_DropNotHandled()

        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            #region Setup view model constructor parameters
            var resourceId = Guid.NewGuid();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var repo = new Mock<IResourceRepository>();

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            repo.Setup(repository => repository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(crm.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());

            #endregion

            #region setup Mock ModelItem
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new DsfActivity();
            testAct.DisplayName = "Test";
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
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

            var wd = new WorkflowDesignerViewModelMock(crm.Object, workflowHelper.Object, new Mock<IEventAggregator>().Object);
            var explorerItem = new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object,
                a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object);

            explorerItem.ResourceId = resourceId;
            wd.SetDataObject(explorerItem);

            var dataObject = new Mock<IDataObject>();

            var handled = wd.SetApplyForDrop(dataObject.Object);
            wd.Dispose();

            //Assert Unique ID has changed
            Assert.IsFalse(handled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowDesignerViewModel))]
        public void WorkflowDesignerViewModel_GetGates()
        {
            #region Setup view model constructor parameters

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var repo = new Mock<IResourceRepository>();
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var serviceDefinition = new StringBuilder();

            var mockModelService = new Mock<ModelService>();

            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            mockWorkflowHelper.Setup(workflowHelper => workflowHelper.CreateWorkflow(It.IsAny<string>())).Returns(new ActivityBuilder());
            mockWorkflowHelper.Setup(workflowHelper => workflowHelper.SerializeWorkflow(mockModelService.Object)).Returns(serviceDefinition);

            #endregion

            #region setup Mock ModelItem
            SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var testAct = new FlowSwitch<string>
            {
                DisplayName = "TestSwitch",
                Expression = Activity<string>.FromValue("True")
            };

            var propertyCollection = new Mock<ModelPropertyCollection>();
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(c => c.Content).Returns(prop.Object);
            source.Setup(c => c.ItemType).Returns(testAct.GetType());

            #endregion

            #region setup mock to change properties

            //mock item adding - this is obsolote functionality but not refactored due to overhead
            var args = new Mock<ModelChangedEventArgs>();
#pragma warning disable 618
            args.Setup(a => a.ItemsAdded).Returns(new List<ModelItem> { source.Object });
#pragma warning restore 618

            #endregion

            var uniqueId1 = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();

            var gate1 = new GateActivity
            {
                UniqueID = uniqueId1,
                DisplayName = "Gate 1",
            };

            var gate2 = new GateActivity
            {
                UniqueID = uniqueId2,
                DisplayName = "Gate 2",
            };

            gate1.NextNodes = new List<IDev2Activity> { gate2 };
            gate2.NextNodes = null;

            var treeNodes = new List<Common.ConflictTreeNode>
            {
                new Common.ConflictTreeNode(gate1, new Point()),
                new Common.ConflictTreeNode(gate2, new Point())
            };

            var mockServiceDifferenceParser = new Mock<IServiceDifferenceParser>();
            mockServiceDifferenceParser.Setup(serviceDifferenceParser => serviceDifferenceParser.BuildWorkflow(serviceDefinition)).Returns(treeNodes);

            CustomContainer.Register(mockServiceDifferenceParser.Object);

            var wd = new WorkflowDesignerViewModelMock(crm.Object, mockWorkflowHelper.Object, new Mock<IEventAggregator>().Object, mockModelService.Object);

            var list = wd.GetSelectableGates(uniqueId2);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(Guid.Empty.ToString(), list[0].Value);
            Assert.AreEqual("End", list[0].Name);
            Assert.AreEqual(uniqueId1, list[1].Value);
            Assert.AreEqual("Gate 1", list[1].Name);
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

            foreach (var propertyName in WorkflowDesignerViewModel.SelfConnectProperties)
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

            foreach (var propertyName in WorkflowDesignerViewModel.SelfConnectProperties)
            {
                info.Setup(i => i.PropertyName).Returns(propertyName);
                wfd.TestModelServiceModelChanged(args.Object);

                var prop = properties[propertyName];
                if (isSelfReference)
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
        [Timeout(60000)]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectMarksResourceIsWorkflowSavedFalse()
        {
            #region Setup viewModel

            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
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

            //Execute
            viewModel.TestWorkflowDesignerModelChangedWithNullSender();

            viewModel.Dispose();

            //Verify
            prop.Verify(p => p.SetValue(It.IsAny<DsfActivity>()), Times.Never());
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [Timeout(60000)]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectLoadFromServerDoesNotReflectEdit()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
            resourceModel.Setup(res => res.WorkflowXaml).Returns(xamlBuilder);

            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
            workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x/>"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);

            Assert.AreEqual(viewModel.DesignerText, viewModel.ServiceDefinition);
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [Timeout(60000)]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_ExpectFirstFocusDoesNotReflectEdit()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.SetupAllProperties();
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
            resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
            resourceModel.Object.ResourceName = "Test";
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
            workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
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
            resourceModel.Object.IsWorkflowSaved = true;
            viewModel.TestWorkflowDesignerModelChanged();

            viewModel.Dispose();
            OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

            //Verify
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
            Assert.IsTrue(resourceModel.Object.ResourceName.IndexOf("*", StringComparison.Ordinal) < 0);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_DataListNotNull_ExpectFirstFocusDoesNotReflectEdit()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.SetupAllProperties();
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
            resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
            resourceModel.Object.ResourceName = "Test";
            resourceModel.Object.DataList = "<DataList><a></a></DataList>";
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
            workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
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
            resourceModel.Object.IsWorkflowSaved = true;
            viewModel.TestWorkflowDesignerModelChanged();

            viewModel.Dispose();
            OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

            //Verify
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        public void WorkflowDesignerViewModel_UnitTest_ViewModelModelChanged_DataListDifferent_ExpectFirstFocusDoesNotReflectEdit()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.SetupAllProperties();
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
            resourceModel.Object.WorkflowXaml = new StringBuilder("<a/>");
            resourceModel.Object.ResourceName = "Test";
            resourceModel.Object.DataList = "<DataList><a></a></DataList>";
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(xamlBuilder);
            workflowHelper.Setup(h => h.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("<x></x>"));

            #endregion

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object);
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
            resourceModel.Object.IsWorkflowSaved = true;
            resourceModel.Object.DataList = "<DataList><b></b></DataList>";
            viewModel.TestWorkflowDesignerModelChanged();

            viewModel.Dispose();
            OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);

            //Verify
            Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
        }

        [TestMethod]
        [Description("When the xaml changes after a redo we mark the resource as unsaved")]
        public void WorkflowDesignerViewModel_UnitTest_RedoWithXAMLDifferent_ExpectMarksResourceIsWorkflowSavedFalse()
        {
            #region Setup viewModel
            var workflow = new ActivityBuilder();
            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(r => r.ResourceName).Returns("Test");
            resourceModel.SetupProperty(model => model.IsWorkflowSaved);
            var xamlBuilder = new StringBuilder(StringResources.xmlServiceDefinition);
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

        #endregion

        #endregion

        public static IServerRepository SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.EnvironmentID).Returns(environmentId);
            return GetEnvironmentRepository(mockEnvironment);
        }

        static IServerRepository GetEnvironmentRepository(Mock<IServer> mockEnvironment)
        {
            var repo = new TestLoadServerRespository(mockEnvironment.Object) { IsLoaded = true };
            CustomContainer.Register<IServerRepository>(repo);

            new ServerRepository(repo);

            repo.ActiveServer = mockEnvironment.Object;

            return repo;
        }

        public static IDataListViewModel CreateDataListViewModel(Mock<IContextualResourceModel> mockResourceModel) => CreateDataListViewModel(mockResourceModel, null);

        public static IDataListViewModel CreateDataListViewModel(Mock<IContextualResourceModel> mockResourceModel, IEventAggregator eventAggregator)
        {
            var dataListViewModel = new DataListViewModel(eventAggregator ?? new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            return dataListViewModel;
        }

        public static WorkflowDesignerViewModel CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(IContextualResourceModel resourceModel) => CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(resourceModel, null);

        public static WorkflowDesignerViewModel CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(IContextualResourceModel resourceModel, IEventAggregator eventPublisher)
        {
            var wf = CreateWorkflowDesignerViewModel(eventPublisher, resourceModel, new WorkflowHelper(), false);

            var designerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfActivity), typeof(ServiceDesigner) },
                { typeof(DsfMultiAssignActivity), typeof(MultiAssignDesigner) },
                { typeof(DsfForEachActivity), typeof(ForeachDesigner) },
                { typeof(DsfCountRecordsetNullHandlerActivity), typeof(CountRecordsDesigner) }
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

            if (workflowHelper == null)
            {
                var wh = new Mock<IWorkflowHelper>();
                wh.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(() => new ActivityBuilder { Implementation = new DynamicActivity() });
                if (helperText != null)
                {
                    wh.Setup(h => h.SanitizeXaml(It.IsAny<StringBuilder>())).Returns(new StringBuilder(helperText));
                }
                workflowHelper = wh.Object;
            }

            var viewModel = new WorkflowDesignerViewModel(eventPublisher, resourceModel, workflowHelper, popupController.Object, new SynchronousAsyncWorker(), createDesigner, _isDesignerInited);

            _isDesignerInited = true;


            return viewModel;

        }
    }
}
