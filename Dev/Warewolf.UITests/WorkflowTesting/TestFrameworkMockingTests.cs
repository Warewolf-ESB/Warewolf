using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";
        private const string Message = "Hello There World";
                
        [TestMethod]
        public void StepsWithoutOutputsShouldBeMarkedInvalid()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_With_Ribbon_Button_And_Dialog("AssignWorkflow");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
            UIMap.Click_SaveDialog_Save_Button();
        }

        [TestMethod]
        public void CreateTestFromDebugUsingUnsvaceWorkflow()
        {
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
            UIMap.Click_SaveDialog_Save_Button();
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

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
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
