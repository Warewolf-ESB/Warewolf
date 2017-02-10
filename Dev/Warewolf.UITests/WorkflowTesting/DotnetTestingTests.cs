using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests.WorkflowTesting
{
    /// <summary>
    /// Summary description for DotnetTestingTests
    /// </summary>
    [CodedUITest]
    public class DotnetTestingTests
    {

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickGenerateTestFromDebugCreatesDotnetTestStepsExpandedFalse()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked, "Dotnet expander Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ExpandingDotnetDllShowsChildStepsExpandedTrue()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked,
                "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.DeleteButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.DeleteButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.DeleteButton.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void DeletingConstructorRemovesTheStep()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.Click_TestViewDotNet_DLL_Constructor_DeleteButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void DeletingFavouriteRemovesTheStep()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.Click_TestViewDotNet_DLL_FavouriteFood_DeleteButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ConstructorValuesAreLoadedCorreclty()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            var variableFromDebug = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.ItemRow.Cell.AssertValue_humanEdit.Text;
            Assert.AreEqual("[[@human]]", variableFromDebug);            ;
            var variableValueFromDebug = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIInfragisticsControlsTreeItem.UIWarewolfStudioViewMoExpander.UIUI_StepOutputs_ctor_Table.UIItemRow.UIItemWarewolfStudioViCell.UIUI_AssertValue_id1tyComboBox.TextEdit.Text;
            StringAssert.Contains(variableValueFromDebug, "Name");
            StringAssert.Contains(variableValueFromDebug, "PersonFood");
            StringAssert.Contains(variableValueFromDebug, "SurName");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ChangeVariableOnTheStepoutPutSetsTheDisplayNameWithAStar()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.Save_Tets_With_Shortcut();
            UIMap.SetConstructorVariable("[[@newVar]]");
            UIMap.Assert_Display_Text_ContainStar("Tab", true);
            UIMap.Assert_Display_Text_ContainStar("Test", true, 1);
        }


        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void FavouriteFoodsValuesAreLoadedCorreclty()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            var variableFromDebug = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell.AssertValue_foodsEdit.Text;
            Assert.AreEqual("[[@foods]]", variableFromDebug);
            var operand = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell1.AssertOp_foods_AuComboBox.SelectedItem;
            var variableValueFromDebug = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ItemRow.Cell2.AssertValue_id1tyComboBox.TextEdit.Text;
            Assert.AreEqual("=", operand);
            StringAssert.Contains(variableValueFromDebug, "$id\": \"1");
            StringAssert.Contains(variableValueFromDebug, "$type\": \"TestingDotnetDllCascading.Food");
            StringAssert.Contains(variableValueFromDebug, "FoodName\": \"Pizza");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestsHasTheTestPassing()
        {
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestsWithAssertHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.SetConstructorAssertValue("");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            UIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing.Exists);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestsWithAssertHasTheTestPassing()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending.Exists);
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestsWithMockHasTheTestFailingWhenConstructorValueIsSetToEmpty()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.SetConstructorAssertValue("");
            UIMap.ClickConstructorMockRadio(true);
            UIMap.ClickFavouriteMockRadio(true);
            UIMap.Click_Run_Test_Button(TestResultEnum.Fail);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestsWithMockHasTheTestPassing()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.ClickConstructorMockRadio(true);
            UIMap.ClickFavouriteMockRadio(true);
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
        }
        #region Additional test attributes



        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Filter_Explorer(DotnetWorkflowForTesting);
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
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

        #endregion
    }
}
