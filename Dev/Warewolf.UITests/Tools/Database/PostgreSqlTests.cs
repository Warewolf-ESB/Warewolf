using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class PostgreSqlTests
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void PostgreSqlToolUITest()
        {
            UIMap.Drag_Toolbox_PostgreSql_Onto_DesignSurface();
            UIMap.Open_Postgre_Tool_Large_View();
            UIMap.Select_Source_From_PostgreTool();
            UIMap.Select_Action_From_PostgreTool();
            UIMap.Click_Postgre_Done_Button();
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
