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
        const string TestName = "HelloWorld_Test";
        const string DuplicateTestName = "Second_HelloWorld_Test";
        const string Tab = "Tab";
        const string Test = "Test";
        const string All = "All";


        [TestMethod]
        public void DirtyTest_Should_Set_Star_Next_To_The_Tab_Name_And_Test_Name()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Assert_Display_Text_ContainStar(Test, false);
            Uimap.Assert_Display_Text_ContainStar(Tab, false);
            Uimap.Click_Create_New_Tests(2);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Assert_Display_Text_ContainStar(Tab,true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Put the Check back
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            //Put the Check back
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Test, false);
            Uimap.Assert_Display_Text_ContainStar(Tab, false);
            Uimap.Click_Create_New_Tests(3);
            Uimap.Click_Create_New_Tests(4);
            Uimap.Click_Create_New_Tests(5);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Assert_Display_Text_ContainStar(Test, false, All);
        }



        [TestMethod]
        public void CreateAndSaveWorkFlowTest()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            //Uimap.Update_Test_Name(TestName);
            //Uimap.Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest();
            //Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Uimap.Click_EnableDisable_This_Test_CheckBox();
            //Uimap.Click_Run_Test();
            //Uimap.Click_Create_New_Tests();
            //Uimap.Update_Test_Name(DuplicateTestName);
        }

        [TestMethod]
        public void RunAllTestsBeforeSaving()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_RunAll_Button(UnsavedResourceError);
            Uimap.Click_MessageBox_OK();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_RunAll_Button(DuplicateNameError);
            Uimap.Click_MessageBox_OK();
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }
        
        [TestMethod]
        public void RunTestAsSpecificUser()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Select_User_From_RunTestAs();
            Uimap.Enter_RunAsUser_Username_And_Password();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
        }

        [TestMethod]
        public void RunDuplicatedTest()
        {
            Uimap.Enter_Text_Into_Explorer_Filter("Hello World");
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
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
