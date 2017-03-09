using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class ForEach
    {

        [TestMethod]
        [TestCategory("Tools")]
        public void ForEachTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.Exists, "For Each tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ForEachTypeComboBox.Exists, "Type Dropdown does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.FromIntellisenseTextbox.Textbox.Exists, "Start textbox in in range foreach on the design surface does not exist.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHere.Exists, "Activity drop box does not exist on for each.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ToIntellisenseTextbox.Textbox.Exists, "End textbox in in range foreach on the design surface does not exist.");
            //Large View
            LoopConstructToolsUIMap.Open_ForEach_LargeView();
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.TypeCombobox.Exists, "ForEach large view type combobox does not exist after double clicking tool to open large view.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ForEachFromIntellisenseTextbox.Exists, "Foreach from textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ToIntellisenseTextbox.Exists, "For each to textbox does not exist after double click openning large view.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.DropActivityHere.Exists, "For each activity drop box does not exist after openning large view with a double click.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.OnErrorPane.Exists, "For each OnError pane does not exist after double click openning large view.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.DoneButton.Exists, "For each done button does not exist after double click openning large view.");
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Drag_Decision_And_SwitchTools_Into_ForEachActivityDrop_ExpectError_UITest()
        {
            LoopConstructToolsUIMap.Drag_Toolbox_Decision_Onto_Foreach();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists);
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
            LoopConstructToolsUIMap.Drag_Toolbox_Switch_Onto_Foreach();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists);
            DialogsUIMap.Click_DropNotAllowed_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Drag_Assigntool_Into_ForEachActivityDrop_ExpectSuccess_UITest()
        {
            LoopConstructToolsUIMap.Drag_Toolbox_AssignObject_Onto_Foreach();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_For_Each_Onto_DesignSurface();
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

        LoopConstructToolsUIMap LoopConstructToolsUIMap
        {
            get
            {
                if (_LoopConstructToolsUIMap == null)
                {
                    _LoopConstructToolsUIMap = new LoopConstructToolsUIMap();
                }

                return _LoopConstructToolsUIMap;
            }
        }

        private LoopConstructToolsUIMap _LoopConstructToolsUIMap;

        #endregion
    }
}
