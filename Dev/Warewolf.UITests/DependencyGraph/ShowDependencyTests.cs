using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

// ReSharper disable CyclomaticComplexity

namespace Warewolf.UITests.DependencyGraph
{
    [CodedUITest]
    public class ShowDependencyTests
    {
        //WOLF-2474
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSources_ShouldHaveShowDependencyMenuItem()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            var displayText = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText.DisplayText;
            Assert.AreEqual("DotnetWorkflowForTesting", displayText);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenDotnetWorkflowAsDependencyIsDoubleClicked_ShouldOpenWorkflowTabWithToolsInside()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.Exists);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSourceIsDoubleClicked_ShouldOpenSourceTab()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            ExplorerUIMap.Filter_Explorer(Source);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.DotnetSourceNode.DotNetPluginSourceText);
            Assert.IsTrue(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.Exists);
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading.dll", DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text);
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

        #endregion
    }
}