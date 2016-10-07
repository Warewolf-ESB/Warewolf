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
            Uimap.Enter_Values_Into_Case_Conversion_Tool();
            Uimap.Press_F6();

            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
            Uimap.Click_Clear_Toolbox_Filter_Clear_Button();
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
