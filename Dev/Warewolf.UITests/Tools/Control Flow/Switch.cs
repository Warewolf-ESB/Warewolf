using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Switch
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void SwitchToolUITest()
        {
            UIMap.Drag_Toolbox_Switch_Onto_DesignSurface();
            UIMap.Click_Switch_Dialog_Done_Button();
            UIMap.First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface();
            UIMap.Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.InitializeABlankWorkflow();
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
