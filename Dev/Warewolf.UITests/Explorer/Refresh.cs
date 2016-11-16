using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    /// <summary>
    /// Summary description for RemoteServer
    /// </summary>
    [CodedUITest]
    public class Refresh
    {
        const string WorkflowName = "SavedBlank";

        [TestMethod]
        [TestCategory("Explorer")]
        public void RefreshExplorerAfterDeletingResourceFromDiskUITest()
        {
            UIMap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            File.Delete(resourcesFolder + @"\" + WorkflowName + ".xml");
            UIMap.Filter_Explorer(WorkflowName);
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.TryGetClickablePoint(out point));
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
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
