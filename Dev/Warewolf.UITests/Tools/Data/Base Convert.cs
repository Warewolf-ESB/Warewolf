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
            Uimap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
            Uimap.Open_Base_Conversion_Tool_Large_View();
            Uimap.Enter_SomeVariable_Into_Base_Convert_Large_View_Row1_Value_Textbox();
            Uimap.Click_Base_Convert_Large_View_Done_Button();
            Uimap.Open_Base_Conversion_Tool_Qvi_Large_View();
            //TODO: Re-introduce these units before WOLF-1923 can be moved to done.
            //Uimap.Click_Debug_Ribbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.BaseConversionToolDebugOutputExists = true;
            //Uimap.Click_Debug_Input_Dialog_Debug_Button();
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
            try
            {
                Uimap.CleanupWorkflow();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception during test cleanup: " + e.Message);
            }
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
