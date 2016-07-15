using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Base_Convert
    {
        [TestMethod]
        public void BaseConvertToolUITest()
        {
            Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Assert_StartNode_Exists();

            //Scenario: Drag toolbox base conversion onto a new workflow creates base conversion tool with small view on the design surface
            Uimap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
            Uimap.Assert_Base_Conversion_Exists_OnDesignSurface();

            //#@NeedsBaseConversionSmallViewOnTheDesignSurface
            //# Scenario: Double Clicking Base Conversion Tool Small View on the Design Surface Opens Large View
            Uimap.Open_Base_Conversion_Tool_Qvi_Large_View();
            Uimap.Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            try
            {
                Uimap.Assert_Close_Tab_Button_Exists();
                Uimap.Click_Close_Tab_Button();
                Uimap.Click_MessageBox_No();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception during test cleanup: " + e.Message);
            }
        }

        #endregion

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
    }
}
