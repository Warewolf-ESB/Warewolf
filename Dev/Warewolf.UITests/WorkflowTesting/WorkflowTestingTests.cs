using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WorkflowTestingTests
    {
        const string DuplicateNameError = "DuplicateNameError";
        const string UnsavedResourceError = "UnsavedResourceError";
        const string TestName = "HelloWorld_Test";
        const string DuplicateTestName = "2nd_HelloWorld_Test";
        const string Testing123 = "Testing123";
        const string Testing123Test = "Testing123_Test";
        const string HelloWorld = "Hello World";

        [TestMethod]
        public void DirtyTest_Should_Set_Star_Next_To_The_Tab_Name_And_Test_Name()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Create_New_Tests(true, 2);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            //Put the Check back
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            //Remove Check
            Uimap.Click_EnableDisable_This_Test_CheckBox(false);
            Uimap.Click_Create_New_Tests(testInstance: 3, nameContainsStar: true);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Assert.IsFalse(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"));
            Assert.IsFalse(Uimap.GetCurrentTest(1).DisplayText.Contains("*"));
        }

        [TestMethod]
        public void Create_Save_And_Run_WorkFlowTest()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Run_Test_Button();
            Uimap.Click_Create_New_Tests(true, 2);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Select_Test_From_TestList();
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_Yes_On_The_Confirm_Delete();
            Uimap.Select_Test(2);
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_Yes_On_The_Confirm_Delete();
        }

        [TestMethod]
        public void CreateAndSaveWorkFlowTest()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest();
            Uimap.Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Run_Test_Button(expectedTestResultEnum: TestResultEnum.Pass);
        }

        [TestMethod]
        public void RunAllTestsBeforeSaving()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_RunAll_Button(UnsavedResourceError);
            Uimap.Click_MessageBox_OK();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Create_New_Tests(true, 2);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_RunAll_Button(DuplicateNameError);
            Uimap.Click_MessageBox_OK();
            Uimap.Select_Test(2);
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
        }

        [TestMethod]
        public void RunTestAsSpecificUser()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
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
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Duplicate_Test_Button();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
        }

        [TestMethod]
        public void SaveAndRunAllTestsWithDuplicatedName()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Duplicate_Test_Button();
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_MessageBox_OK();
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_RunAll_Button();
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_Yes_On_The_Confirm_Delete();
        }

        [TestMethod]
        public void RunTestsWithDuplicatedName()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(TestName);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Duplicate_Test_Button();
            Uimap.Update_Test_Name(TestName);
            Uimap.Select_Test(2);
            Uimap.Click_Run_Test_Button(expectedTestResultEnum: TestResultEnum.Fail, instance: 2);
            Uimap.Click_MessageBox_OK();
            Uimap.Update_Test_Name(DuplicateTestName);
            Uimap.Select_Test(2);
            Uimap.Click_Run_Test_Button(instance: 2);
        }

        [TestMethod]
        public void RunPassingTests()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            Uimap.Enter_Service_Name_Into_Save_Dialog(Testing123);
            Uimap.Click_SaveDialog_Save_Button();
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(Testing123);
            Uimap.Click_Create_New_Tests(true);
            Uimap.Update_Test_Name(Testing123Test);
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.Exists);
            Uimap.Click_EnableDisable_This_Test_CheckBox(true);
            Uimap.Click_Run_Test_Button(TestResultEnum.Pass);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Invalid.Exists);
            Uimap.Click_Delete_Test_Button();
            Uimap.Click_MessageBox_Yes();
        }
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryRemoveTests();
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            File.Delete(resourcesFolder + @"\" + Testing123 + ".xml");
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
