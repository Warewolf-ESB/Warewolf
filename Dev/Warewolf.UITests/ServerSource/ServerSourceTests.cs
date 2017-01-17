using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.ServerSource
{
    [CodedUITest]
    public class ServerSourceTests
    {
        const string SourceName = "CodedUITestServerSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ServerSource_CreateSourceUITests()
        {
            UIMap.Select_NewServerSource_FromExplorerContextMenu();
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