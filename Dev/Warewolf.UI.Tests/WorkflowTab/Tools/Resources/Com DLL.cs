using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Resources
{
    [CodedUITest]
    public class Com_DLL
    {        
        [TestMethod]
        [TestCategory("Resource Tools")]
        public void ComDLLTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            //Large View
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.Exists,
                "Com DLL tool does not exist on the design surface after dragging in from the toolbox.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.SourcesCombobox.Exists,
                "Sources Combobox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
               ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.EditSourceButton.Exists,
                "EditSources Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.NewSourceButton.Exists,
                "NewSource Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.NamespaceCombobox.Exists,
                "Namespace Combobox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
               ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.RefreshNamespaceButton.Exists,
                "RefeshNamespace Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
               ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.ActionsCombobox.Exists,
                "Actions Combobox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.ActionRefreshButton.Exists,
                "ActionRefresh Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.InputsTable.Exists,
                "Inputs Table does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.GenerateOutputsButton.Exists,
                "Generate Outputs Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.OutputToObjectCheckbox.Exists,
                "OutputToObjective Checkbox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
               ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.OutputsTable.Exists,
                "Outputs Table does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.RecordsetNameTextbox.Exists,
                "RecrodsetName Textbox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
               ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.LargeView.OnErrorPanel.Exists,
                "OnError Pane does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.DoneButton.Exists,
                "Done button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            //Small View
            ResourcesToolsUIMap.ComDLLTool_Collapse_Large_View_To_Small_View_With_DoubleClick();
            Assert.IsTrue(
                ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                    .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter
                    .Flowchart.ComDll.SmallView.Exists,
                "Com DLL tool small view does not exist after double clicking tool large view.");
            
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void COMDLLTool_EditSource_UITest()
        {
            ResourcesToolsUIMap.Select_Source_From_ComDLLTool();
            var countBefore = ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.SourcesCombobox.Items.Count;
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            ResourcesToolsUIMap.Click_EditSourceButton_On_ComDLLTool();
            MyComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.RefreshSpinner.DrawHighlight();
            UIMap.WaitForSpinner(MyComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Spinner);
            MyComPluginSourceUIMap.Select_AssemblyFile_From_COMPluginDataTree("connection");
            UIMap.Click_Save_RibbonButton();
            MyComPluginSourceUIMap.Click_COMPluginSource_CloseTabButton();
            ResourcesToolsUIMap.ComDLLTool_Open_Large_View_With_DoubleClick();
            var countAfter =ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.SourcesCombobox.Items.Count;
            Assert.AreEqual(countBefore, countAfter);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_ComDLLConnector_Onto_DesignSurface();
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

        ComPluginSourceUIMap MyComPluginSourceUIMap
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

        ResourcesToolsUIMap ResourcesToolsUIMap
        {
            get
            {
                if (_ResourcesToolsUIMap == null)
                {
                    _ResourcesToolsUIMap = new ResourcesToolsUIMap();
                }

                return _ResourcesToolsUIMap;
            }
        }

        private ResourcesToolsUIMap _ResourcesToolsUIMap;

       DialogsUIMap MyDialogsUIMap
        {
            get
            {
                if (_MyDialogsUIMap == null)
                {
                    _MyDialogsUIMap = new DialogsUIMap();
                }

                return _MyDialogsUIMap;
            }
        }

        private DialogsUIMap _MyDialogsUIMap;

        #endregion
    }
}
