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
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch dialog done button does not exist after dragging switch tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.CancelButton.Exists, "Switch dialog cancel button does not exist after dragging switch tool in from the toolbox.");
            UIMap.Click_Switch_Dialog_Done_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
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