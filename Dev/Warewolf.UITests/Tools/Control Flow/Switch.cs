using System;
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
            Uimap.Drag_Toolbox_Switch_Onto_DesignSurface();
            Uimap.Click_Switch_Dialog_Done_Button();
            Uimap.First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface();
            Uimap.Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
            Uimap.InitializeABlankWorkflow();
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
