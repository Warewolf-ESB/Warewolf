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


namespace Warewolf.UITests
{
    [CodedUITest]
    public class WebConnector
    {
        const string WebSourceName = "UITestingWebSource";

        [TestMethod]
        public void BigWebConnectorUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Click_New_Web_Source_Ribbon_Button();
            Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            Uimap.Click_New_Web_Source_Test_Connection_Button();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
            Uimap.Click_Close_Web_Source_Wizard_Tab_Button();
            Uimap.Drag_GET_Web_Connector_Onto_DesignSurface();
            Uimap.TryClearToolboxFilter();
            Uimap.Open_GET_Web_Connector_Tool_Large_View();
            Uimap.Select_Last_Source_From_GET_Web_Large_View_Source_Combobox();
            Uimap.Click_GET_Web_Large_View_Generate_Outputs();
            Uimap.Click_GET_Web_Large_View_Test_Inputs_Button();
            Uimap.Click_GET_Web_Large_View_Test_Inputs_Done_Button();
            Uimap.Click_GET_Web_Large_View_Done_Button();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseHangingSaveDialog();
            Uimap.TryRemoveFromExplorer(WebSourceName);
            Uimap.TryClearToolboxFilter();
            Uimap.TryCloseNewWebSourceWizardTab();
            Uimap.TryCloseWorkflowTabs();
        }

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
