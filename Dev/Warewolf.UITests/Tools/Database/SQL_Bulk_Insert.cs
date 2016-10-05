using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SQL_Bulk_Insert
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void SQLBulkInsertToolUITest()
        {
            UIMap.Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface();
            UIMap.Open_SQL_Bulk_Insert_Tool_Large_View();
            UIMap.Open_SQL_Bulk_Insert_Tool_Qvi_Large_View();
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
