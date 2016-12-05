using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WorkflowTestingTests
    {
        const string HelloWorld = "Hello World";

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Run_Failing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "First 'Hello World' test does not exist as expected.");
            UIMap.Click_Create_New_Tests(true,4);
            UIMap.Click_Test_Run_Button(4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Failing.Exists, "Test failing icon is not displayed after running a failing test.");
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Run_Passing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists,
                "Third 'Hello World' test does not exist as expected.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Invalid.Exists, "Test passing icon is not displayed after running a passing test.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Show_Duplicate_Test_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "First 'Hello World' test does not exist as expected.");
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            UIMap.Update_Test_Name("Blank Input");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No duplicate test error dialog when saving a test with the name of an existing test.");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Show_Save_Before_Running_Tests_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            UIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            UIMap.Click_Workflow_Testing_Tab_Run_All_Button();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No save before running tests error dialog when clicking run all button while a test is unsaved.");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestAsSpecificUser()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "This test expects 'Hello World' to have at least 1 existing test.");
            UIMap.Select_First_Test();
            UIMap.Select_User_From_RunTestAs();
            UIMap.Enter_RunAsUser_Username_And_Password();
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Delete_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            UIMap.Click_Create_New_Tests(true, 4);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Click_Duplicate_Test_Button()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            UIMap.Select_First_Test();
            UIMap.Click_Duplicate_Test_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No 4th test after starting with 3 tests and duplicating the first.");
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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
