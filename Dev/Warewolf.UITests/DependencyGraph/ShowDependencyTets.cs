using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

// ReSharper disable CyclomaticComplexity

namespace Warewolf.UITests.DependencyGraph
{
    [CodedUITest]
    public class ShowDependencyTets
    {
        //WOLF-2474
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSources_ShouldHaveShowDependencyMenuItem()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            UIMap.Filter_Explorer(Source);
            UIMap.RightClick_Explorer_Localhost_FirstItem();
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
            UIMap.Filter_Explorer(Source);
            UIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText);
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.Exists);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Explorer_GivenSourceIsDoubleClicked_ShouldOpenSourceTab()
        {
            //---------------Set up test pack-------------------
            const string Source = "DotNetPluginSource";
            UIMap.Filter_Explorer(Source);
            UIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Mouse.DoubleClick(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.DotnetSourceNode.DotNetPluginSourceText);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.Exists);
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading.dll", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text);
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        #endregion
    }
}