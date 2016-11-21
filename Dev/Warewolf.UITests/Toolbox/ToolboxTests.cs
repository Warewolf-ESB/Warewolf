using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Toolbox
{
    [CodedUITest]
    public class ToolboxTests
    {
        [TestMethod]
        [TestCategory("ToolBox")]
        public void ClickToolboxClearButtonRemovesText()
        {
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text));
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
        }

        UIMap UIMap
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
