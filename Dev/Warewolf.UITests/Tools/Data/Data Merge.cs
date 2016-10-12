using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Merge
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void DataMergeUITest()
        {
            UIMap.Drag_Toolbox_Data_Merge_Onto_DesignSurface();
            UIMap.Open_Data_Merge_Large_View();
            //UIMap.Enter_Values_Into_Data_Merge_Tool_Large_View();
            //UIMap.Click_Data_Merge_Tool_Large_View_Done_Button();
            UIMap.Open_Data_Merge_Tool_Qvi_Large_View();
            //UIMap.Click_Debug_Bibbon_Button();
            //UIMap.Click_Debug_Input_Dialog_Debug_ButtonParams.DataMergeToolDebugOutputExists = true;
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

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
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
