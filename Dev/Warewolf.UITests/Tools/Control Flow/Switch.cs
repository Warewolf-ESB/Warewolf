using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Switch
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void SwitchTool_DragOnWorkflow_UITest()
        {
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.VariableComboBox.Exists, "Varaible Combobox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DisplayText.Exists, "Display Text Textbox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.OnErrorGroup.Exists, "On Error Pane does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch dialog done button does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.CancelButton.Exists, "Switch dialog cancel button does not exist after dragging switch tool in from the toolbox.");
            DialogsUIMap.Click_Switch_Dialog_Done_Button();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            ToolsUIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

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