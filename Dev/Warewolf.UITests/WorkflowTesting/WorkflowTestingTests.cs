using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WorkflowTestingTests
    {
        const string DuplicateNameError = "DuplicateNameError";
        const string UnsavedResourceError = "UnsavedResourceError";
        const string DuplicateTestName = "Second_Dice_Test";

        [TestMethod]
        public void CreateAndSaveWorkFlowTest()
        {
            //Uimap.CreateAndSave_Dice_Wokflow();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Disable_This_Test();
        }

        [TestMethod]
        public void RunAllTestsBeforeSaving()
        {
            //Uimap.CreateAndSave_Dice_Wokflow();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name();
            Uimap.Click_RunAll_Button(UnsavedResourceError);
            Uimap.Click_MessageBox_OK();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name();
            Uimap.Click_RunAll_Button(DuplicateNameError);
            Uimap.Click_MessageBox_OK();
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }
        
        [TestMethod]
        public void RunTestAsSpecificUser()
        {
            //Uimap.CreateAndSave_Dice_Wokflow();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Select_User_From_RunTestAs();
            Uimap.Enter_RunAsUser_Username_And_Password();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
        }

        [TestMethod]
        public void RunDeuplicatedTest()
        {
            //Uimap.CreateAndSave_Dice_Wokflow();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Duplicate_Test_Button();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();            
            Uimap.Click_RunAll_Button(DuplicateNameError);
            Uimap.Click_MessageBox_OK();            
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
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
