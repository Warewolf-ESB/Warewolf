using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DragAndDropService
    {
        const string Dice = "Dice";
        [TestMethod]
        public void DragAndDropServiceFromExplorerUITest()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Filter_Explorer(Dice);
            UIMap.Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
