using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SQLServerSourceTests
    {
        const string SourceName = "CodedUITestSQLServerSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void SQLServerSource_CreateSourceUITests()
        {
            UIMap.Click_New_SQLServerSource_From_Explorer_Context_Menu();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }
        
        public UIMap UIMap
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