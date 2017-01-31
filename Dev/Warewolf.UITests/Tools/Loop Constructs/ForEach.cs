using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class ForEach
    {

        [TestMethod]
        [TestCategory("Tools")]
        public void ForEachTool_Small_And_LargeView_UITest()
        {
            //Small View
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ForEachTypeComboBox.Exists, "Type Dropdown does not exist on the design surface.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.FromIntellisenseTextbox.Textbox.Exists, "Start textbox in in range foreach on the design surface does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHere.Exists, "Activity drop box does not exist on for each.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ToIntellisenseTextbox.Textbox.Exists, "End textbox in in range foreach on the design surface does not exist.");
            //Large View
            UIMap.Open_ForEach_LargeView();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.TypeCombobox.Exists, "ForEach large view type combobox does not exist after double clicking tool to open large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ForEachFromIntellisenseTextbox.Exists, "Foreach from textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ToIntellisenseTextbox.Exists, "For each to textbox does not exist after double click openning large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.DropActivityHere.Exists, "For each activity drop box does not exist after openning large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.OnErrorPane.Exists, "For each OnError pane does not exist after double click openning large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.DoneButton.Exists, "For each done button does not exist after double click openning large view.");
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Drag_Decision_And_SwitchTools_Into_ForEachActivityDrop_ExpectError_UITest()
        {
            UIMap.Drag_Toolbox_Decision_Onto_Foreach();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            UIMap.Click_DropNotAllowed_MessageBox_OK();
            UIMap.Drag_Toolbox_Switch_Onto_Foreach();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            UIMap.Click_DropNotAllowed_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Drag_Assigntool_Into_ForEachActivityDrop_ExpectSuccess_UITest()
        {
            UIMap.Drag_Toolbox_AssignObject_Onto_Foreach();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_For_Each_Onto_DesignSurface();
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
