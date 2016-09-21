using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Base_Convert
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void BaseConvertToolUITest()
        {
            Uimap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
            Uimap.Open_Base_Conversion_Tool_Large_View();
            Uimap.Enter_SomeVariable_Into_Base_Convert_Large_View_Row1_Value_Textbox();
            Uimap.Click_Base_Convert_Large_View_Done_Button();
            Uimap.Press_F6();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            Uimap.Click_Debug_Output_BaseConvert_Cell();
            Uimap.Open_Base_Conversion_Tool_Qvi_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
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
