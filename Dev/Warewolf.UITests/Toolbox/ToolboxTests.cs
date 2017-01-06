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

        [TestMethod]
        [TestCategory("ToolBox")]
        public void DoubleClickToolboxAssignToolShowsPopup()
        {
            UIMap.DoubleClick_Toolbox();
            Assert.IsTrue(UIMap.MessageBoxWindow.OKButton.Exists);
        }

        [TestMethod]
        [TestCategory("ToolBox")]
        public void SingleClickToolboxAssignToolUpdatesHelpText()
        {
            var initialImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            UIMap.SingleClick_Toolbox();
            var assignImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            Assert.AreNotEqual(initialImage, assignImage);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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
