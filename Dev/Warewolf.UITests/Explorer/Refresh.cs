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
        public void RefreshExplorerAfterDeletingResourceFromDiskUITest()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            File.Delete(resourcesFolder + @"\" + WorkflowName + ".xml");
            UIMap.Filter_Explorer(WorkflowName);
            UIMap.Click_Explorer_Refresh_Button();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.TryGetClickablePoint(out point));
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
