using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Case_Convert
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void CaseConvertUITest()
        {
            UIMap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
            //UIMap.Open_Case_Conversion_Tool_Large_View();
            //UIMap.Enter_Values_Into_Case_Conversion_Tool_Large_View();
            //UIMap.Click_Case_Conversion_Tool_Large_View_Done_Button();
            UIMap.Open_Case_Conversion_Tool_Qvi_Large_View();
            //UIMap.Click_Debug_Bibbon_Button();
            //UIMap.Click_Debug_Input_Dialog_Debug_ButtonParams.CaseConversionToolDebugOutputExists = true;
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
