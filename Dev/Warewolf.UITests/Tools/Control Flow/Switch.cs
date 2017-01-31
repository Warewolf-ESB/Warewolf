using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Switch
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void SwitchTool_DragOnWorkflow_UITest()
        {
            UIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch dialog done button does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.CancelButton.Exists, "Switch dialog cancel button does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.VariableComboBox.Exists, "Varaible Combobox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.DisplayText.Exists, "Display Text Textbox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.OnErrorGroup.Exists, "On Error Pane does not exist after dragging switch tool in from the toolbox.");
            UIMap.Click_Switch_Dialog_Done_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}