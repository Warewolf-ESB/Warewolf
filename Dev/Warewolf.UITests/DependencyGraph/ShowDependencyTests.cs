using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DependencyGraph.DependencyGraphUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

// ReSharper disable CyclomaticComplexity

namespace Warewolf.UITests.DependencyGraph
{
    [CodedUITest]
    public class ShowDependencyTests
    {
        //WOLF-2474
        [TestMethod]
        [TestCategory("Dependency Graph")]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSources_ShouldHaveShowDependencyMenuItem()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            var displayText = DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText.DisplayText;
            Assert.AreEqual("DotnetWorkflowForTesting", displayText);
        }

        [TestMethod]
        [TestCategory("Dependency Graph")]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenDotnetWorkflowAsDependencyIsDoubleClicked_ShouldOpenWorkflowTabWithToolsInside()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText);
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WaitForControlExist(60000);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists, "Workflow tab does not exist after double clicking dependancy graph node and waiting a minute.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.Exists, "Openned workflow does not contain expected DotNet Connector tool on the design surface.");
        }

        [TestMethod]
        [TestCategory("Dependency Graph")]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSourceIsDoubleClicked_ShouldOpenSourceTab()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.DotnetSourceNode.DotNetPluginSourceText);
            Assert.IsTrue(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.Exists);
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading.dll", DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text);
        }

        [TestMethod]
        [TestCategory("Dependency Graph")]
        [TestCategory("Explorer")]
        public void ShowDependencies_ExplorerContextMenuItem_UITest()
        {
            ExplorerUIMap.Select_ShowDependencies_In_ExplorerContextMenu("Hello World");
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies is selected");
            DependencyGraphUIMap.Click_Close_Dependecy_Tab();
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

        DependencyGraphUIMap DependencyGraphUIMap
        {
            get
            {
                if (_DependencyGraphUIMap == null)
                {
                    _DependencyGraphUIMap = new DependencyGraphUIMap();
                }

                return _DependencyGraphUIMap;
            }
        }

        private DependencyGraphUIMap _DependencyGraphUIMap;

        #endregion
    }
}