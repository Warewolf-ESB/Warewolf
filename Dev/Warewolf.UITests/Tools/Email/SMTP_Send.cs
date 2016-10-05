using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SMTP_Send
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void SMTPSendToolUITest()
        {
            UIMap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
            UIMap.Open_SMTP_Email_Tool_Large_View();
            //UIMap.Enter_Values_Into_SMTP_Email_Tool_Large_View();
            //UIMap.Click_SMTP_Email_Tool_Large_View_Done_Button();
            //UIMap.Click_SMTP_Email_Tool_QVI_Button();
            //UIMap.Click_Debug_Bibbon_Button();
            //UIMap.Click_Debug_Input_Dialog_Debug_ButtonParams.SMTPEmailToolDebugOutputExists = true;
            //UIMap.Click_Debug_Input_Dialog_Debug_Button();
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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
