using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class ODBCTests
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void ODBCToolUITest()
        {
            UIMap.Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface();
            UIMap.Open_ODBC_Tool_Large_View();
            UIMap.Click_NewSource_Button_FromODBC_Tool();
            UIMap.Click_Close_DB_Source_Wizard_Tab_Button();
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
                if ((_UIMap == null))
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
