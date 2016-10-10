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
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.AreEqual("Blank Input", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestNameDisplay.DisplayText, "First 'Hello World' test is not 'Blank Input' as expected.");
            UIMap.Click_Test_Run_Button(1);
            Point point;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing.TryGetClickablePoint(out point), "Test failing icon is not displayed after running a failing test.");
        }

        [TestMethod]
        public void Create_And_Run_New_Passing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.AreEqual("Valid Input", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestNameDisplay.DisplayText, "Third 'Hello World' test is not 'Valid Input' as expected.");
            UIMap.Click_Test_Run_Button(3);
            Point point;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Passing.TryGetClickablePoint(out point), "Test passing icon is not displayed after running a passing test.");
        }

        [TestMethod]
        public void Show_Duplicate_Test_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.AreEqual("Blank Input", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestNameDisplay.DisplayText, "First 'Hello World' test is not 'Blank Input' as expected.");
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            UIMap.Update_Test_Name("Blank Input");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No duplicate test error dialog when saving a test with the name of an existing test.");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
        public void Show_Save_Before_Running_Tests_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            UIMap.Click_Workflow_Testing_Tab_Run_All_Button();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No save before running tests error dialog when clicking run all button while a test is unsaved.");
            UIMap.Click_MessageBox_OK();
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
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            UIMap.Click_Create_New_Tests(true);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_Yes_On_The_Confirm_Delete();
        }

        [TestMethod]
        public void Click_Duplicate_Test_Button()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            UIMap.Click_First_Test_Button();
            UIMap.Click_Duplicate_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No 4th test after starting with 3 tests and duplicating the first.");
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
