using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            const string Source = "NewDotNetPluginSource";
            UIMap.Filter_Explorer(Source);
            UIMap.RightClick_Explorer_Localhost_FirstItem();
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            var displayText = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NodesCustom.DotnetWorkflowForTesText.DisplayText;
            Assert.AreEqual("NewDotnetWorkflowForTesting", displayText);
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

        #endregion
    }
}