using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Replace
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void ReplaceToolUITest()
        {
            UIMap.Drag_Toolbox_Replace_Onto_DesignSurface();
            //UIMap.Open_Replace_Tool_Large_View();
            //UIMap.Enter_Values_Into_Replace_Tool_Large_View();
            //UIMap.Click_Replace_Tool_Large_View_Done_Button();
            //UIMap.Click_Replace_Tool_QVI_Button();
            //UIMap.Click_Debug_Bibbon_Button();
            //UIMap.Click_Debug_Input_Dialog_Debug_ButtonParams.ReplaceToolDebugOutputExists = true;
            //UIMap.Click_Debug_Input_Dialog_Debug_Button();
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
        
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
