using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Calculate
    {
        [TestMethod]
        public void CalculateToolUITest()
        {
            Uimap.Drag_Toolbox_Calculate_Onto_DesignSurface();
            Uimap.Open_Calculate_Tool_Large_View();
            Uimap.Enter_SomeVariable_Into_Calculate_Large_View_Function_Textbox();
            Uimap.Click_Calculate_Large_View_Done_Button();
            //TODO: Re-introduce these units before WOLF-1923 can be moved to done.
            //Uimap.Click_Debug_Ribbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.CalculateToolDebugOutputExists = true;
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
            Uimap.CleanupWorkflow();
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
