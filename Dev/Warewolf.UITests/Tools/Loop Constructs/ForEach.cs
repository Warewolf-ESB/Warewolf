using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class ForEach
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void ForEachTool_OpenLargeViewUITest()
        {
            UIMap.Open_ForEach_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragADecisionIntoForEachExpectNotAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_Decision_Onto_Foreach_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", UIMap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragASwitchIntoForEachExpectNotAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_ASwitch_Onto_Foreach_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", UIMap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragAnAssignIntoForEachExpectAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_AssignObject_Onto_Foreach_LargeTool();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_For_Each_Onto_DesignSurface();
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
