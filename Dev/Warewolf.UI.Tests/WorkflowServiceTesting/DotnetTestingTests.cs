﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Common;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowServiceTesting
{
    /// <summary>
    /// Summary description for DotnetTestingTests
    /// </summary>
    [CodedUITest]
    public class DotnetTestingTests
    {

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void ClickGenerateTestFromDebugCreatesDotnetTestStepsExpandedFalse()
        {
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");            
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void ExpandingDotnetDllShowsChildStepsExpandedTrue()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DefaultCtor.ConstructorExpander.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DefaultCtor.ConstructorExpander.StepOutputs_ctor_Table.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DefaultCtor.ConstructorExpander.UIWarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DefaultCtor.ConstructorExpander.UIWarewolfStudioViewMoButton.DeleteButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DeleteButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.CtorFavouriteFood.FavouriteFoodsExpander.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.CtorFavouriteFood.FavouriteFoodsExpander.StepOutputs_FavouTable.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.CtorFavouriteFood.FavouriteFoodsExpander.WarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.CtorFavouriteFood.FavouriteFoodsExpander.WarewolfStudioViewMoButton.DeleteButton.Exists);
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void DeletingConstructorRemovesTheStep()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.Click_TestViewDotNet_DLL_Constructor_DeleteButton();
            var controlExistsNow = UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.DefaultCtor.ConstructorExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void DeletingFavouriteRemovesTheStep()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.Click_TestViewDotNet_DLL_FavouriteFood_DeleteButton();
            var controlExistsNow = UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DotnetDllTreeItem.CtorFavouriteFood.FavouriteFoodsExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void ConstructorValuesAreLoadedCorrectly()
        {           
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            var variableFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.UIWarewolfStudioViewMoTreeItem.UIItemTreeItem.Step.UIUI_StepOutputs_ctor_Table.UIItemRow.ResultsCell.ResultEdit.Text;
            Assert.AreEqual("[[@human]]", variableFromDebug);
            var variableValueFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.UIWarewolfStudioViewMoTreeItem.UIItemTreeItem.Step.UIUI_StepOutputs_ctor_Table.UIItemRow.AssertValueCell.AssertValueComboBox.AssertionValue.Text;
            StringAssert.Contains(variableValueFromDebug, "Name");
            StringAssert.Contains(variableValueFromDebug, "PersonFood");
            StringAssert.Contains(variableValueFromDebug, "SurName");
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void ChangeVariableOnTheStepoutPutSetsTheDisplayNameWithAStar()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.Save_Tets_With_Shortcut();
            WorkflowServiceTestingUIMap.SetConstructorVariable("[[@newVar]]");
            WorkflowServiceTestingUIMap.Assert_Display_Text_ContainStar("Tab", true);
            WorkflowServiceTestingUIMap.Assert_Display_Text_ContainStar("Test", true, 1);
        }


        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void FavouriteFoodsValuesAreLoadedCorreclty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            var variableFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.UIWarewolfStudioViewMoTreeItem.FavFoodItem.Step.UIUI_StepOutputs_FavouTable.UIItemRow.ResultCell.ResultEdit.Text;
            Assert.AreEqual("[[@foods]]", variableFromDebug);
            var operand = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.UIWarewolfStudioViewMoTreeItem.FavFoodItem.Step.UIUI_StepOutputs_FavouTable.UIItemRow.AssertCell.AssertComboBox.SelectedItem;
            var variableValueFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.UIWarewolfStudioViewMoTreeItem.FavFoodItem.Step.UIUI_StepOutputs_FavouTable.UIItemRow.AssertValueCell.AssertValueComboBox.ValueEdit.Text;
            Assert.AreEqual("=", operand);
            StringAssert.Contains(variableValueFromDebug, "$id\": \"1");
            StringAssert.Contains(variableValueFromDebug, "$type\": \"TestingDotnetDllCascading.Food");
            StringAssert.Contains(variableValueFromDebug, "FoodName\": \"Pizza");
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void RunTestsHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void RunTestsWithAssertHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.SetConstructorAssertValue("");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void RunTestsWithAssertHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void RunTestsWithMockHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.SetConstructorAssertValue("");
            WorkflowServiceTestingUIMap.ClickConstructorMockRadio(true);
            WorkflowServiceTestingUIMap.ClickFavouriteMockRadio(true);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Dotnet Connector Mocking Tests")]
        public void RunTestsWithMockHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox();
            WorkflowServiceTestingUIMap.ClickConstructorMockRadio(true);
            WorkflowServiceTestingUIMap.ClickFavouriteMockRadio(true);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Filter_Explorer("DotnetWorkflowForTesting");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Click_Save_RibbonButton();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        #endregion
    }
}
