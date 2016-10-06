using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Calculate
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void CalculateToolUITest()
        {
            UIMap.Drag_Toolbox_Calculate_Onto_DesignSurface();
            UIMap.Open_Calculate_Tool_Large_View();
            UIMap.Enter_SomeVariable_Into_Calculate_Large_View_Function_Textbox();
            UIMap.Click_Calculate_Large_View_Done_Button();
            UIMap.Click_Debug_Ribbon_Button();
            UIMap.Click_DebugInput_Debug_Button();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            UIMap.Click_Debug_Output_Calculate_Cell();
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
