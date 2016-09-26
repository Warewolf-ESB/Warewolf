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
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            File.Delete(resourcesFolder + @"\" + WorkflowName + ".xml");
            Uimap.Filter_Explorer(WorkflowName);
            Uimap.Click_Explorer_Refresh_Button();
            Point point;
            Assert.IsFalse(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.TryGetClickablePoint(out point));
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }

        UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
