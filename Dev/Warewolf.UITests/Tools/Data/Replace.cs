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
            Uimap.Drag_Toolbox_Replace_Onto_DesignSurface();
            //Uimap.Open_Replace_Tool_Large_View();
            //Uimap.Enter_Values_Into_Replace_Tool_Large_View();
            //Uimap.Click_Replace_Tool_Large_View_Done_Button();
            //Uimap.Click_Replace_Tool_QVI_Button();
            //Uimap.Click_Debug_Bibbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.ReplaceToolDebugOutputExists = true;
            //Uimap.Click_Debug_Input_Dialog_Debug_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.InitializeABlankWorkflow();
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
