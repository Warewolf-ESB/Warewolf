using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests.DebugOutputWindow
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DebugOutputWindowTests
    {

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Cliick_AddNewTest_From_Debug()
        {

            Uimap.Open_Service_From_Explorer("Hello World");
            Uimap.Press_F6();
            Uimap.Click_AddNewTestFromDebug();
            Assert.IsTrue(Uimap.GetCurrentTest(1).DisplayText.Contains("*"));
            

        }

        #region Additional test attributes


        UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
