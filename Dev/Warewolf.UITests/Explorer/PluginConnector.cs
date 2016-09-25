using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class PluginConnector
    {
        const string PluginSourceName = "UITestingPluginSource";
        string DLLPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll";

        [TestMethod]
        public void BigPluginConnectorUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Click_NewPluginSource_Ribbon_Button();
            if (!File.Exists(DLLPath))
            {
                DLLPath = DLLPath.Replace("Framework64", "Framework");
                if (!File.Exists(DLLPath))
                {
                    throw new Exception("No suitable DLL could be found for this test to use.");
                }
            }
            Uimap.Type_dll_into_Plugin_Source_Wizard_Assembly_Textbox(DLLPath);
            Uimap.Save_With_Ribbon_Button_And_Dialog(PluginSourceName);
            Uimap.Click_Close_Plugin_Source_Wizard_Tab_Button();
            Uimap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
            Uimap.Open_DotNet_DLL_Connector_Tool_Large_View();
            Uimap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Uimap.Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_Combobox();
            Uimap.Select_FirstItem_From_DotNet_DLL_Large_View_Action_Combobox();
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
            Uimap.SetPlaybackSettings();
#if RELEASE
            Uimap.WaitForStudioStart();
#endif
        }

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
