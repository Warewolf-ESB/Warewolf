using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SMTP_Send
    {
        [TestMethod]
        public void SMTPSendToolUITest()
        {
            //Scenario: Dragging tool onto design surface creates small view
            Uimap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
            Uimap.Assert_SMTP_Email_Exists_OnDesignSurface();

            //Scenario: Double clicking small view opens large view
            Uimap.Open_SMTP_Email_Tool_Large_View();
            Uimap.Assert_SMTP_Email_Large_View_Exists_OnDesignSurface();

            //Scenario: Enter values into large view
            //Scenario: Clicking the Done button passes validation and all variables are in the variable list
            //Scenario: Clicking QVI button opens QVI
            //Scenario: Clicking debug button shows debug input dialog
            //Scenario: Clicking debug button in debug input dialog generates the proper debug output
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
            Uimap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.CleanupWorkflow();
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
