using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Split
    {
        [TestMethod]
        public void DataSplitToolUITest()
        {
            Uimap.Drag_Toolbox_Data_Split_Onto_DesignSurface();
            Uimap.Open_Data_Split_Large_View();
            //Uimap.Enter_Values_Into_Data_Split_Tool_Large_View();
            //Uimap.Click_Data_Split_Tool_Large_View_Done_Button();
            Uimap.Open_Data_Split_Tool_Qvi_Large_View();
            //Uimap.Click_Debug_Bibbon_Button();
            //Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.DataSplitToolDebugOutputExists = true;
            //Uimap.Click_Debug_Input_Dialog_Debug_Button();
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
