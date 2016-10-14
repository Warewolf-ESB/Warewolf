using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SqlServerTests
    {

        //Todo:Now, generate the Missing drag actions
        //Todo:Put more test steps (Dont forget asserts)        
        
        [TestMethod]
        [TestCategory("Tools")]
        public void SqlServerToolUITest()
        {
            UIMap.Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface();
            UIMap.Open_Sql_Server_Tool_Large_View();
            
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
