using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests.DebugOutputWindow
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DebugOutputWindowTests
    {

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Cliick_AddNewTest_From_Debug()
        {

            Uimap.Open_Service_From_Explorer("Hello World");
            Uimap.WaitForControlVisible(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart);
            Uimap.Press_F6();
            Uimap.Click_AddNewTestFromDebug();
            //Uimap.WaitForControlVisible(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList);
            //Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.TestNameDisplay.DisplayText.Contains("*"));            
            //Uimap.Click_Run_Test_Button(TestResultEnum.Pass);
            //Uimap.Click_AddNewTestFromDebug();
            //Assert.IsTrue(Uimap.GetCurrentTest(2).DisplayText.Contains("*"));
            //Uimap.Click_TestMock_Radio_Button();

        }

       

        #region Additional test attributes


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
