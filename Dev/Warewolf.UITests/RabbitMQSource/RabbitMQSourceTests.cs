using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.RabbitMQSource
{
    [CodedUITest]
    public class RabbitMQSourceTests
    {
        const string SourceName = "CodedUITestRabbitMQSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void RabbitMQSource_CreateSourceUITests()
        {
            UIMap.Select_NewRabbitMQSource_FromExplorerContextMenu();
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