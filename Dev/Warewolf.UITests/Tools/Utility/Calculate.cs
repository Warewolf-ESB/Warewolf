using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Calculate
    {
        [TestMethod]
		[TestCategory("Utility Tools")]
        public void CalculateToolUITest()
        {
            UIMap.Open_Calculate_Tool_Large_View();
            UIMap.Enter_SomeVariable_Into_Calculate_Large_View_Function_Textbox();
            UIMap.Click_Calculate_Large_View_Done_Button();
            UIMap.Click_Debug_Ribbon_Button();
            UIMap.Click_DebugInput_Debug_Button(); Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug Output does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SettingsButton.Exists, "Output SettingsButton does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Exists, "Output SearchTextBox does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Exists, "DebugOutputTree does not exist after clicking Debug button from Debug Dialog");
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            UIMap.Click_Debug_Output_Calculate_Cell();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Calculate_Onto_DesignSurface();
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

        #endregion
    }
}
