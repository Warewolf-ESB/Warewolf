using TechTalk.SpecFlow;
using Warewolf.UITests.DialogsUIMapClasses;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses
{
    [Binding]
    public partial class LoopConstructToolsUIMap
    {
        public void Drag_Toolbox_AssignObject_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_Switch_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_Decision_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        [When(@"I Open ForEach Large View")]
        public void Open_ForEach_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, new Point(131, 14));
        }

        [When(@"I Open SelectAndApply Large View")]
        public void Open_SelectAndApply_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply, new Point(129, 10));
        }

        [When(@"I RightClick ForEach OnDesignSurface")]
        public void RightClick_ForEach_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, MouseButtons.Right, ModifierKeys.None, new Point(137, 9));
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

        DotNetPluginSourceUIMap DotNetPluginSourceUIMap
        {
            get
            {
                if (_DotNetPluginSourceUIMap == null)
                {
                    _DotNetPluginSourceUIMap = new DotNetPluginSourceUIMap();
                }

                return _DotNetPluginSourceUIMap;
            }
        }

        private DotNetPluginSourceUIMap _DotNetPluginSourceUIMap;

        ComPluginSourceUIMap ComPluginSourceUIMap
        {
            get
            {
                if (_ComPluginSourceUIMap == null)
                {
                    _ComPluginSourceUIMap = new ComPluginSourceUIMap();
                }

                return _ComPluginSourceUIMap;
            }
        }

        private ComPluginSourceUIMap _ComPluginSourceUIMap;

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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

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
    }
}
