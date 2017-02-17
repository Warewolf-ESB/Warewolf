using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;
using Warewolf.UITests.Common;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WorkflowTestingTests
    {
        const string HelloWorld = "Hello World";
        const string xPath = "Utility - XPath";

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Run_Failing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "First 'Hello World' test does not exist as expected.");
            WorkflowServiceTestingUIMap.Click_Create_New_Tests(true, 4);
            WorkflowServiceTestingUIMap.Click_Test_Run_Button(4);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Failing.Exists, "Test failing icon is not displayed after running a failing test.");
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }
       

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Run_Passing_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists,
                "Third 'Hello World' test does not exist as expected.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Invalid.Exists, "Test passing icon is not displayed after running a passing test.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Show_Duplicate_Test_Dialog()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "First 'Hello World' test does not exist as expected.");
            WorkflowServiceTestingUIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            WorkflowServiceTestingUIMap.Update_Test_Name("Blank Input");
            WorkflowServiceTestingUIMap.Save_Tets_With_Shortcut();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No duplicate test error dialog when saving a test with the name of an existing test.");
            UIMap.Duplicate_Test_Name_MessageBox_Ok();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Show_Save_Before_Running_Tests_Dialog()
        {
            UIMap.Show_ExplorerSecondItemTests_With_ExplorerContextMenu(xPath);
            WorkflowServiceTestingUIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            WorkflowServiceTestingUIMap.Click_Workflow_Testing_Tab_Run_All_Button();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "No save before running tests error dialog when clicking run all button while a test is unsaved.");
            UIMap.Click_Save_Before_Continuing_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void RunTestAsSpecificUser()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "This test expects 'Hello World' to have at least 1 existing test.");
            WorkflowServiceTestingUIMap.Select_First_Test();
            WorkflowServiceTestingUIMap.Select_User_From_RunTestAs();
            WorkflowServiceTestingUIMap.Enter_RunAsUser_Username_And_Password();
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Delete_Test()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            WorkflowServiceTestingUIMap.Click_Create_New_Tests(true, 4);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Click_Duplicate_Test_Button()
        {
            UIMap.Click_View_Tests_In_Explorer_Context_Menu(HelloWorld);
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4), "This test expects 'Hello World' to have just 3 existing tests.");
            WorkflowServiceTestingUIMap.Select_First_Test();
            WorkflowServiceTestingUIMap.Click_Duplicate_Test_Button();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No 4th test after starting with 3 tests and duplicating the first.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        #endregion
    }
}
