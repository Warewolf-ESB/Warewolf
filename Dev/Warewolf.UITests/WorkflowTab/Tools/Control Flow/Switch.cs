using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Control_Flow
{
    [CodedUITest]
    public class Switch
    {
        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void SwitchTool_DragOnWorkflow_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.VariableComboBox.Exists, "Varaible Combobox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DisplayText.Exists, "Display Text Textbox does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.OnErrorGroup.Exists, "On Error Pane does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch dialog done button does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.CancelButton.Exists, "Switch dialog cancel button does not exist after dragging switch tool in from the toolbox.");
            DialogsUIMap.Click_Switch_Dialog_Done_Button();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void SwitchTool_Move_Arm_No_Dialog_UITest()
        {
            ExplorerUIMap.Filter_Explorer("MovingSwitchArmsWithoutADialogUITest");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            ControlFlowToolsUIMap.Drag_Switch_Arm_To_Fourth_Comment_Tool();
            Playback.Wait(500);
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector4.Height > 200, "Connector was not moved to fourth comment after drag was attempted.");
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.DecisionOrSwitchDialog), "Dialog exists after moving switch arm.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        ControlFlowToolsUIMap ControlFlowToolsUIMap
        {
            get
            {
                if (_ControlFlowToolsUIMap == null)
                {
                    _ControlFlowToolsUIMap = new ControlFlowToolsUIMap();
                }

                return _ControlFlowToolsUIMap;
            }
        }

        private ControlFlowToolsUIMap _ControlFlowToolsUIMap;

        #endregion
    }
}
