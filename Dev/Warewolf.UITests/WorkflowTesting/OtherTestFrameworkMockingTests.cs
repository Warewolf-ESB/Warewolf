using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class OtherTestFrameworkMockingTests
    {
        private const string Resource = "Resource For MockRadioButton";
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void SettingTestStepToMockDoesNotAffectTestOutput()
        {
            UIMap.Filter_Explorer(Resource);
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists);
            UIMap.Press_F6();            
            UIMap.Click_Create_Test_From_Debug();

            UIMap.Click_MockRadioButton_On_TestStep();
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Passing status icon is still visible on test after running test with mocking enabled.");
            UIMap.Click_Close_Tests_Tab();
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

        #endregion
    }
}
