﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Control_Flow
{
    [CodedUITest]
    public class Sequence
    {
        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void Sequence_DraggingNonDecision_Allowed_LargeView_UITest()
        {
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.Exists, "Sequence on the design surface does not exist");
            ControlFlowToolsUIMap.SequenceTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.UIDoneButton.Exists);
            ControlFlowToolsUIMap.Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool();
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.MultiAssignObject.Exists, "Multi Assign Object Tool does not exist.");
        }

        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void Sequence_DraggingNonDecision_Allowed_SmallView_UITest()
        {
            ControlFlowToolsUIMap.Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool();
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceSmallView.ElementTable.AssignObject.Exists, "Assign Object Tool does not exist.");
        }

        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void Sequence_DraggingSwitch_NotAllowed_BothViews_UITest()
        {
            //Large View
            ControlFlowToolsUIMap.SequenceTool_ChangeView_With_DoubleClick();
            ControlFlowToolsUIMap.Drag_Toolbox_Switch_Onto_Sequence_LargeTool();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
            //Small View
            ControlFlowToolsUIMap.SequenceTool_ChangeView_With_DoubleClick();
            ControlFlowToolsUIMap.Drag_Toolbox_Switch_Onto_Sequence_SmallTool();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void Sequence_DraggingDecision_NotAllowed_BothViews_UITest()
        {
            //Large View 
            ControlFlowToolsUIMap.SequenceTool_ChangeView_With_DoubleClick();
            ControlFlowToolsUIMap.Drag_Toolbox_Decision_Onto_Sequence_LargeTool();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
            //Small View
            ControlFlowToolsUIMap.SequenceTool_ChangeView_With_DoubleClick();
            ControlFlowToolsUIMap.Drag_Toolbox_Decision_Onto_Sequence_SmallTool();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
        }
        const string HelloWorld = "Hello World";
        [TestMethod]
        [TestCategory("Control Flow Tools")]
        public void Sequence_DraggingResourceFromFolder_UITest()
        {
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.Exists, "Sequence on the design surface does not exist");
            ExplorerUIMap.Filter_Explorer(HelloWorld);
            ControlFlowToolsUIMap.Drag_Explorer_First_Item_Onto_Sequence();
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceSmallView.ElementTable.AssignObject.Exists, "Hello World Worflow does not exist.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Sequence_Onto_DesignSurface();
        }
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
