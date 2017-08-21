using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Scripting
{
    [CodedUITest]
    public class Ruby
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void RubyScriptTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.Exists, "Ruby tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.SmallView.ScriptIntellisenseCombobox.Exists, "Ruby script textbox does not exist after dragging on tool from the toolbox.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.SmallView.ResultIntellisenseCombobox.Exists, "Ruby result textbox does not exist after dragging on tool from the toolbox.");
            //Large View
            ScriptingToolsUIMap.Open_Ruby_LargeView();
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachmentsIntellisenseCombobox.Exists, "Ruby Attachments textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachFileButton.Exists, "Ruby Attach File Button does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.EscapesequencesCheckBox.Exists, "Ruby escape sequences checkbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ScriptIntellisenseCombobox.Exists, "Ruby script textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ResultIntellisenseCombobox.Exists, "Ruby result textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.OnErrorPane.Exists, "Ruby OnError pane does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.DoneButton.Exists, "Ruby Done button does not exist after openning large view with a double click.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Ruby_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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

        ScriptingToolsUIMap ScriptingToolsUIMap
        {
            get
            {
                if (_ScriptingToolsUIMap == null)
                {
                    _ScriptingToolsUIMap = new ScriptingToolsUIMap();
                }

                return _ScriptingToolsUIMap;
            }
        }

        private ScriptingToolsUIMap _ScriptingToolsUIMap;

        #endregion
    }
}
