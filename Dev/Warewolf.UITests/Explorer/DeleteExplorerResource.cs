using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DeleteExplorerResource
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void Delete_ExplorerResource()
        {
            UIMap.Filter_Explorer("Control Flow - Switch");
            UIMap.Delete_FirstResource_FromContextMenu();
            UIMap.Click_MessageBox_Yes();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Explorer_Refresh_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedResourceIsRemovedFromResources(string resourceName)
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.StartsWith(resourceName));
            Assert.IsNull(firstOrDefault);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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