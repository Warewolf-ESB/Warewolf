using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Control_Flow
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
        public void SwitchTool_DragDialogWindow_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
            Mouse.StartDragging(DialogsUIMap.DecisionOrSwitchDialog);
            Mouse.StopDragging(100, 100);
        }

        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void SwitchTool_EditSwitchCase_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
            DialogsUIMap.Click_Switch_Dialog_Done_Button();

            Mouse.DoubleClick(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch);
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch dialog done button does not exist after dragging switch tool in from the toolbox.");
            DialogsUIMap.Click_Switch_Dialog_Done_Button();

            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Connect_ToDefaultSwitch();
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Connect_SwitchFirstArm();
            Assert.IsTrue(UIMap.ControlExistsNow(DialogsUIMap.ActivityDefaultWindow));
            DialogsUIMap.ActivityDefaultWindow.TextboxHost.SwitchArmCaseTextbox.TextInput.Text = "1";
            Mouse.Click(DialogsUIMap.ActivityDefaultWindow.DoneButton);
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.ActivityDefaultWindow));
            WorkflowTabUIMap.DoubleClickSwitchFirstArm();
            Assert.IsTrue(UIMap.ControlExistsNow(DialogsUIMap.ActivityDefaultWindow));
            DialogsUIMap.ActivityDefaultWindow.TextboxHost.SwitchArmCaseTextbox.TextInput.Text = "2";
            Mouse.Click(DialogsUIMap.ActivityDefaultWindow.DoneButton);
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow));

            WorkflowTabUIMap.DoubleClickSwitchFirstArm();
            Assert.IsTrue(UIMap.ControlExistsNow(DialogsUIMap.ActivityDefaultWindow));
            DialogsUIMap.ActivityDefaultWindow.TextboxHost.SwitchArmCaseTextbox.TextInput.Text = "2";
            Mouse.Click(DialogsUIMap.ActivityDefaultWindow.DoneButton);
            Assert.IsTrue(UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow));
            Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow));
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
