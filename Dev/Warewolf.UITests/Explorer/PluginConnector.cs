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
    public class PluginConnector
    {
        const string PluginSourceName = "UITestingPluginSource";

        [TestMethod]
        public void BigPluginConnectorUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Click_NewPluginSource_Ribbon_Button();
            Uimap.Type_dll_into_Plugin_Source_Wizard_Assembly_Textbox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(PluginSourceName);
            Uimap.Click_Close_Plugin_Source_Wizard_Tab_Button();
            Uimap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
            Uimap.TryClearToolboxFilter();
            Uimap.Open_DotNet_DLL_Connector_Tool_Large_View();
            Uimap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Uimap.Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_Combobox();
            Uimap.Select_Next_From_DotNet_DLL_Large_View_Action_Combobox();
            Uimap.Click_DotNet_DLL_Large_View_Generate_Outputs();
            Uimap.Click_DotNet_DLL_Large_View_Test_Inputs_Button();
            Uimap.Click_DotNet_DLL_Large_View_Test_Inputs_Done_Button();
            Uimap.Click_DotNet_DLL_Large_View_Done_Button();
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
            Uimap.TryRemoveFromExplorer(PluginSourceName);
            Uimap.TryClearToolboxFilter();
            Uimap.TryCloseNewPluginSourceWizardTab();
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
