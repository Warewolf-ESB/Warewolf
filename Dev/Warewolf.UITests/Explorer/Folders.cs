using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Folders
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFoldersUITest()
        {
            var acceptanceResources = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Testing Resources";
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsTrue(Directory.Exists(acceptanceResources));
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            UIMap.Filter_Explorer("Acceptance");
            UIMap.Create_SubFolder_In_Folder1();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateResourceInFolderUITest()
        {
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_Resource_In_Folder1();
            UIMap.Save_With_Ribbon_Button_And_Dialog("Hello World", true);
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Delete_Nested_Hello_World();
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(File.Exists(resourcesFolder + @"\" + "Hello World" + ".xml"));
            UIMap.Click_Explorer_Filter_Clear_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
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
