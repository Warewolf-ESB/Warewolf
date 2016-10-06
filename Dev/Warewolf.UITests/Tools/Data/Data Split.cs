using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Split
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void DataSplitToolUITest()
        {
            UIMap.Drag_Toolbox_Data_Split_Onto_DesignSurface();
            UIMap.Open_Data_Split_Large_View();
            //UIMap.Enter_Values_Into_Data_Split_Tool_Large_View();
            //UIMap.Click_Data_Split_Tool_Large_View_Done_Button();
            UIMap.Open_Data_Split_Tool_Qvi_Large_View();
            //UIMap.Click_Debug_Bibbon_Button();
            //UIMap.Click_Debug_Input_Dialog_Debug_ButtonParams.DataSplitToolDebugOutputExists = true;
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
