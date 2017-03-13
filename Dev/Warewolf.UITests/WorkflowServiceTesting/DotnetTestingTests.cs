using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests.WorkflowTesting
{
    /// <summary>
    /// Summary description for DotnetTestingTests
    /// </summary>
    [CodedUITest]
    public class DotnetTestingTests
    {

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void ClickGenerateTestFromDebugCreatesDotnetTestStepsExpandedFalse()
        {
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsFalse(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked, "Dotnet expander Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void ExpandingDotnetDllShowsChildStepsExpandedTrue()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked,
                "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.DeleteButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.DeleteButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.DeleteButton.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void DeletingConstructorRemovesTheStep()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            WorkflowServiceTestingUIMap.Click_TestViewDotNet_DLL_Constructor_DeleteButton();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void DeletingFavouriteRemovesTheStep()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            WorkflowServiceTestingUIMap.Click_TestViewDotNet_DLL_FavouriteFood_DeleteButton();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void ConstructorValuesAreLoadedCorreclty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            var variableFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.ItemRow.Cell.AssertValue_humanEdit.Text;
            Assert.AreEqual("[[@human]]", variableFromDebug);            ;
            var variableValueFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIInfragisticsControlsTreeItem.UIWarewolfStudioViewMoExpander.UIUI_StepOutputs_ctor_Table.UIItemRow.UIItemWarewolfStudioViCell.UIUI_AssertValue_id1tyComboBox.TextEdit.Text;
            StringAssert.Contains(variableValueFromDebug, "Name");
            StringAssert.Contains(variableValueFromDebug, "PersonFood");
            StringAssert.Contains(variableValueFromDebug, "SurName");
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void ChangeVariableOnTheStepoutPutSetsTheDisplayNameWithAStar()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            WorkflowServiceTestingUIMap.Save_Tets_With_Shortcut();
            WorkflowServiceTestingUIMap.SetConstructorVariable("[[@newVar]]");
            WorkflowServiceTestingUIMap.Assert_Display_Text_ContainStar("Tab", true);
            WorkflowServiceTestingUIMap.Assert_Display_Text_ContainStar("Test", true, 1);
        }


        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void FavouriteFoodsValuesAreLoadedCorreclty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            var variableFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell.AssertValue_foodsEdit.Text;
            Assert.AreEqual("[[@foods]]", variableFromDebug);
            var operand = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell1.AssertOp_foods_AuComboBox.SelectedItem;
            var variableValueFromDebug = WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell2.AssertValue_id1tyComboBox.TextEdit.Text;
            Assert.AreEqual("=", operand);
            StringAssert.Contains(variableValueFromDebug, "$id\": \"1");
            StringAssert.Contains(variableValueFromDebug, "$type\": \"TestingDotnetDllCascading.Food");
            StringAssert.Contains(variableValueFromDebug, "FoodName\": \"Pizza");
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void RunTestsHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void RunTestsWithAssertHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            WorkflowServiceTestingUIMap.SetConstructorAssertValue("");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void RunTestsWithAssertHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void RunTestsWithMockHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            WorkflowServiceTestingUIMap.SetConstructorAssertValue("");
            WorkflowServiceTestingUIMap.ClickConstructorMockRadio(true);
            WorkflowServiceTestingUIMap.ClickFavouriteMockRadio(true);
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void RunTestsWithMockHasTheTestPassing()
        {
            WorkflowServiceTestingUIMap.Expand_DotnetDll_ByClickingCheckbox(true);
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
            ExplorerUIMap.Filter_Explorer(DotnetWorkflowForTesting);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
        }

        public const string DotnetWorkflowForTesting = "DotnetWorkflowForTesting";

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
