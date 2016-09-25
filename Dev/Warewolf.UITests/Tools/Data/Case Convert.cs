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
            Uimap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
            //Uimap.Open_Case_Conversion_Tool_Large_View();
            //Uimap.Enter_Values_Into_Case_Conversion_Tool_Large_View();
            //Uimap.Click_Case_Conversion_Tool_Large_View_Done_Button();
            Uimap.Open_Case_Conversion_Tool_Qvi_Large_View();
            //Uimap.Click_Debug_Bibbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.CaseConversionToolDebugOutputExists = true;
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
