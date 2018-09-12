﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.Tools
{
    [CodedUITest]
    public class SelectAndApply
    {
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [TestCategory("Tools")]
        public void SelectAndApplyTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.Exists, "Select and apply Tool does not exist on design surface after dragging from toolbox.");
            //Small View
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.SelectFromIntellisenseTextbox.Exists, "Datasource Combobox does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.AliasIntellisenseTextbox.Exists, "As Alias Combobox does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.DropActivityHere.Exists, "Drop Activity does not exist on the design surface.");
            //Large View
            LoopConstructToolsUIMap.Open_SelectAndApply_LargeView();
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.SelectFromIntellisenseTextbox.Exists, "Datasource Combobox does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.AliasIntellisenseTextbox.Exists, "As Alias Combobox does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.DropActivityHere.Exists, "Drop Activity does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.OnErrorPane.Exists, "OnError pane does not exist on the design surface.");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.DoneButton.Exists, "Done Button does not exist on the design surface.");
        }


        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [TestCategory("Tools")]
        public void SelectAndApplyTool_Small_To_LargeView_Keeps_Variables_UITest()
        {
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.Exists, "Select and apply Tool does not exist on design surface after dragging from toolbox.");
            //Small View
            LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.SelectFromIntellisenseTextbox.Textbox.Text = "[[Variable]]";
            LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.AliasIntellisenseTextbox.Textbox.Text = "[[AliasVariable]]";
            //Large View
            LoopConstructToolsUIMap.Open_SelectAndApply_LargeView();
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.SelectFromIntellisenseTextbox.Textbox.Text == "[[Variable]]", "Variable from small view was removed when changing to large view");
            Assert.IsTrue(LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.AliasIntellisenseTextbox.UITextEdit.Text == "[[AliasVariable]]", "Alias  from small view was removed when changing to large view");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_SelectAndApply_Onto_DesignSurface();
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
