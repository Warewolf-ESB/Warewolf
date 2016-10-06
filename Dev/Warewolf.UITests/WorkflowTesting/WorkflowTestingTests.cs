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
            UIMap.TryCloseWorkflowTab();
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No tests on workflow testing tab after clicking new tests button.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            UIMap.Click_First_Test_Run_Button();
            Point point;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Test passing icon is not displayed after running a passing test.");
        }

        [TestMethod]
        public void Create_And_Run_New_Passing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No tests on workflow testing tab after clicking new tests button.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            UIMap.Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest();
            UIMap.Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest();
            UIMap.Click_First_Test_Run_Button();
            Point point;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Test failing icon is not displayed after running a failing test.");
        }

        [TestMethod]
        public void Show_Duplicate_Test_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2.Exists, "No second test on workflow testing tab.");
            UIMap.Update_Test_Name("Test 1");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No duplicate test error dialog when saving a test with the name of an existing test.");
        }

        [TestMethod]
        public void Save_Before_Running_Tests_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
            UIMap.Click_Workflow_Testing_Tab_Run_All_Button();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No save before running tests error dialog when clicking run all button while a test is unsaved.");
        }

        [TestMethod]
        public void RunTestAsSpecificUser()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Create_New_Tests(true);
            UIMap.Update_Test_Name(TestName);
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Select_User_From_RunTestAs();
            UIMap.Enter_RunAsUser_Username_And_Password();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_RunAll_Button();
        }

        [TestMethod]
        public void Delete_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Create_New_Tests(true);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_Yes_On_The_Confirm_Delete();
        }

        [TestMethod]
        public void RunTestsWithDuplicatedName()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Create_New_Tests(true);
            UIMap.Update_Test_Name(TestName);
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Duplicate_Test_Button();
            UIMap.Update_Test_Name(TestName);
            UIMap.Select_Test(2);
            UIMap.Click_Run_Test_Button(expectedTestResultEnum: TestResultEnum.Fail, instance: 2);
            UIMap.Click_MessageBox_OK();
            UIMap.Update_Test_Name(DuplicateTestName);
            UIMap.Select_Test(2);
            UIMap.Click_Run_Test_Button(instance: 2);
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= UIMap.OnError;
            UIMap.TryRemoveTests();
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            File.Delete(resourcesFolder + @"\" + Testing123 + ".xml");
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
