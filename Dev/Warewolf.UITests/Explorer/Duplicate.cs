using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DuplicateExplorerResource
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateResource_ThenAddsNewItemItem()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            UIMap.Enter_Duplicate_workflow_name("Duplicated_HelloWorld");
            UIMap.Click_Duplicate_From_Duplicate_Dialog();
        }
        
        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_ThenAddsNewFolderItem()
        {
            UIMap.Filter_Explorer("Examples");
            UIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            UIMap.Enter_Duplicate_workflow_name("Duplicated_ExampleFolder");
            UIMap.Click_Duplicate_From_Duplicate_Dialog();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_And_Use_Same_Name_Shows_Error()
        {
            UIMap.Click_Duplicate_From_ExplorerContextMenu("DuplicateFolderNameError");
            Assert.IsTrue(UIMap.SaveDialogWindow.ErrorLabel.Exists);
            UIMap.Click_SaveDialog_CancelButton();
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
