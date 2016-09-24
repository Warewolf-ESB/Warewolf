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
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Assert_Display_Text_ContainStar(Tab, false);
            Uimap.Assert_Display_Text_ContainStar(Test, false);
            Uimap.Click_Create_New_Tests(2);
            Uimap.Assert_Display_Text_ContainStar(Tab,true);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Put the Check back
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Tab, false);
            Uimap.Assert_Display_Text_ContainStar(Test, false);
            Uimap.Click_Create_New_Tests(3);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Assert_Display_Text_ContainStar(Test, false, 0);
        }

        [TestMethod]
        public void Create_Save_And_Run_WorkFlowTest()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Run_Test_Button();
            Uimap.Click_Create_New_Tests(2);
            Uimap.Assert_Display_Text_ContainStar(Tab, true, 2);
            Uimap.Assert_Display_Text_ContainStar(Test, true, 2);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Select_Test_From_TestList();
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Assert_Display_Text_ContainStar(Tab, true);
            Uimap.Assert_Display_Text_ContainStar(Test, true);
            Uimap.Click_Delete_Test_Button();
        }

        [TestMethod]
        public void CreateAndSaveWorkFlowTest()
        {
            Uimap.Search_And_Select_HelloWolrd();
            Uimap.Select_Tests_From_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Click_Run_Test_Button();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(DuplicateTestName);
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
            Uimap.Click_RunAll_Button();
        }
        [TestMethod]
        public void RunTestsWithDuplicatedName()
        {
            Uimap.Enter_Text_Into_Explorer_Filter("Hello World");
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Duplicate_Test_Button();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_MessageBox_OK();
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_Yes_On_The_Confirm_Delete();
        }

        [TestMethod]
        public void RunPassingTests()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            Uimap.Enter_Service_Name_Into_Save_Dialog("Testing123");
            Uimap.Click_SaveDialog_Save_Button();
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Enter_Text_Into_Explorer_Filter("Testing123");
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            Uimap.Click_Create_New_Tests();
            Uimap.Update_Test_Name("Testing123_Test");
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Run_Test_Button();
            Uimap.Assert_Test_Result("Passing");
            Uimap.Click_EnableDisable_This_Test_CheckBox();
            Uimap.Click_Run_Test_Button();
            Uimap.Assert_Test_Result("Invalid");
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_MessageBox_Yes();
        }
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
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
