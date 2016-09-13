using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Create_WEB_Server_Source_From_Tool
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void WEBServerSourceFromTool()
        {
            Uimap.Drag_Toolbox_Web_Request_Onto_DesignSurface();
            Uimap.Open_Large_View_FromContextMenu();
            Uimap.Select_NewDatabaseSource_FromSqlServerTool();

            //Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            //Uimap.Click_New_Web_Source_Test_Connection_Button();
            //Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
            //Uimap.Click_Close_Web_Source_Wizard_Tab_Button();
            //Uimap.Drag_GET_Web_Connector_Onto_DesignSurface();
            //Uimap.TryClearToolboxFilter();
            //Uimap.Open_GET_Web_Connector_Tool_Large_View();
            //Uimap.Select_Last_Source_From_GET_Web_Large_View_Source_Combobox();
            //Uimap.Click_GET_Web_Large_View_Generate_Outputs();
            //Uimap.Click_GET_Web_Large_View_Test_Inputs_Button();
            //Uimap.Click_GET_Web_Large_View_Test_Inputs_Done_Button();
            //Uimap.Click_GET_Web_Large_View_Done_Button();
            //Uimap.Click_Debug_Ribbon_Button();
            //Uimap.Click_DebugInput_Debug_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
            Uimap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.CleanupABlankWorkflow();
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
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
