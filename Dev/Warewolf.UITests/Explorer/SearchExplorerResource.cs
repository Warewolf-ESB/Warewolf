using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SearchExplorerResource
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void Search_ExplorerResource()
        {
            UIMap.Filter_Explorer("Error WF");
            UIMap.ExplorerItemCountEquals();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Search_ExplorerFolder()
        {
            UIMap.Filter_Explorer("Examples");
            UIMap.ExplorerItemCountEquals();
        }
      

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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