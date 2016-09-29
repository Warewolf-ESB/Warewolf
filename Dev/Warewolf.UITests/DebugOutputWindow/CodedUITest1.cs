using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.DebugOutputWindow
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CodedUITest1
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
            //------------Setup for test--------------------------
            Uimap.Open_Service_From_Explorer("Hello World");
            Uimap.Press_F6();

            //------------Execute Test---------------------------
            Uimap.Click_AddNewTestFromDebug();

            //------------Assert Results-------------------------
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
