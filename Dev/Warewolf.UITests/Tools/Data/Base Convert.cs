using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Base_Convert
    {
        [TestMethod]
		[TestCategory("Data Tools")]
        public void BaseConvert_OpenLargeViewToolUITest()
        {
            UIMap.Open_Base_Conversion_Tool_Large_View();
            UIMap.Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_Textbox();
            UIMap.Click_Base_Convert_Large_View_Done_Button();
            UIMap.Press_F6();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            UIMap.Click_Debug_Output_BaseConvert_Cell();
            UIMap.Open_Base_Conversion_Tool_Qvi_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
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
