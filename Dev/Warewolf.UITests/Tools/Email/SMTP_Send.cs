using System;
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
            Uimap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
            Uimap.Open_SMTP_Email_Tool_Large_View();
            //Uimap.Enter_Values_Into_SMTP_Email_Tool_Large_View();
            //Uimap.Click_SMTP_Email_Tool_Large_View_Done_Button();
            //Uimap.Click_SMTP_Email_Tool_QVI_Button();
            //Uimap.Click_Debug_Bibbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.SMTPEmailToolDebugOutputExists = true;
            //Uimap.Click_Debug_Input_Dialog_Debug_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
            Uimap.InitializeABlankWorkflow();
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
