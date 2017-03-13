using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.DebugInputWindow
{
    [CodedUITest]
    public class SaveDebugInputs
    {
        private const string InputDataText = "Coded UI Test";
        const string HelloWorld = "Hello World";

        [TestMethod]
        [TestCategory("Debug Input")]
        public void Save_DebugInputs_AfterCancel_UITest()
        {
            ExplorerUIMap.Filter_Explorer(HelloWorld);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists, "Hello World workflow does not exist");
            UIMap.Click_Debug_RibbonButton();
            UIMap.Check_Debug_Input_Dialog_Remember_Inputs_Checkbox();
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked);
            UIMap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox(InputDataText);
            UIMap.Click_DebugInput_Cancel_Button();
            UIMap.Click_Debug_RibbonButton();
            Assert.AreEqual(InputDataText, UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Cancelling and re-openning the debug input dialog loses input values.");
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void Save_DebugInputs_AfterDebug_UITest()
        {
            ExplorerUIMap.Filter_Explorer(HelloWorld);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Click_Debug_RibbonButton();
            UIMap.Check_Debug_Input_Dialog_Remember_Inputs_Checkbox();
            UIMap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox(InputDataText);
            UIMap.Click_DebugInput_Debug_Button();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug Output does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SettingsButton.Exists, "Output SettingsButton does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Exists, "Output SearchTextBox does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Exists, "DebugOutputTree does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Exists, "Create Test Button does not exist after clicking Debug button from Debug Dialog");
            UIMap.Click_Debug_RibbonButton();
            Assert.AreEqual(InputDataText, UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debugging Hello World workflow and then re-openning the debug input dialog loses input values.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

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

        #endregion
    }
}
