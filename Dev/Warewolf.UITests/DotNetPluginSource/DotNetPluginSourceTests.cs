using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DotNetPluginSourceTests
    {
        const string SourceName = "CodedUITestDotNetPluginSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void DotNetPluginSource_CreateSourceUITests()
        {
            UIMap.Click_NewDotNetPluginSource_From_Explorer_Context_Menu();
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