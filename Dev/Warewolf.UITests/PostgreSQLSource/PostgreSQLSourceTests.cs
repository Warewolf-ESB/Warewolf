using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class PostgreSQLSourceTests
    {
        const string SourceName = "CodedUITestMyPostgreSQLSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void PostgreSQLSource_CreateSourceUITests()
        {
            UIMap.Select_NewPostgreSQLSource_FromExplorerContextMenu();
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