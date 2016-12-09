using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ResourceVersion
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void ShowVersionHistory_ForResource()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Move_Assign_Message_Tool_On_The_Design_Surface();
            UIMap.Select_ShowVersionHistory_FromExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);
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
