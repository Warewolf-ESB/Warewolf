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
            Uimap.Drag_Toolbox_Data_Merge_Onto_DesignSurface();
            Uimap.Open_Data_Merge_Large_View();
            //Uimap.Enter_Values_Into_Data_Merge_Tool_Large_View();
            //Uimap.Click_Data_Merge_Tool_Large_View_Done_Button();
            Uimap.Open_Data_Merge_Tool_Qvi_Large_View();
            //Uimap.Click_Debug_Bibbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.DataMergeToolDebugOutputExists = true;
            //Uimap.Click_Debug_Input_Dialog_Debug_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if RELEASE
            Uimap.WaitForStudioStart();
#endif
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
