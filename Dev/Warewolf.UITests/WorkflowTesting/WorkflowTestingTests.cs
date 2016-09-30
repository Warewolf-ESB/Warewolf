using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WorkflowTestingTests
    {
        const string TestName = "HelloWorld_Test";
        const string DuplicateTestName = "2nd_HelloWorld_Test";
        const string Testing123 = "Testing123";
        const string HelloWorld = "Hello World";

        [TestMethod]
        public void Create_And_Run_New_Failing_Test()
        {
            Uimap.TryCloseWorkflowTab();
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No tests on workflow testing tab after clicking new tests button.");
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Uimap.Click_First_Test_Run_Button();
            Point point;
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Test passing icon is not displayed after running a passing test.");
        }

        [TestMethod]
        public void Create_And_Run_New_Passing_Test()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No tests on workflow testing tab after clicking new tests button.");
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Uimap.Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest();
            Uimap.Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest();
            Uimap.Click_First_Test_Run_Button();
            Point point;
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Test failing icon is not displayed after running a failing test.");
        }

        [TestMethod]
        public void Show_Duplicate_Test_Dialog()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2.Exists, "No second test on workflow testing tab.");
            Uimap.Update_Test_Name("Test 1");
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists, "No duplicate test error dialog when saving a test with the name of an existing test.");
        }

        [TestMethod]
        public void Save_Before_Running_Tests_Dialog()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
            Uimap.Click_Workflow_Testing_Tab_Run_All_Button();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists, "No save before running tests error dialog when clicking run all button while a test is unsaved.");
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
        public void Delete_Test()
        {
            Uimap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Uimap.Click_Create_New_Tests(true);
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
