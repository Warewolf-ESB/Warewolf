using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.WorkflowTesting
{
    /// <summary>
    /// Summary description for DotnetTestingTests
    /// </summary>
    [CodedUITest]
    public class DotnetTestingTests
    {

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickGenerateTestFromDebugCreatesDotnetTestStepsExpandedFalse()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.UIExpansionIndicatorCheckBox.Checked, "Dotnet expander Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ExpandingDotnetDllShowsChildStepsExpandedTrue()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.UIExpansionIndicatorCheckBox.Checked, 
                "Create New Test Button does not exist on service test tab after openning it by clicking the button in DotnetWorkflowForTesting debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.DeleteButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.DeleteButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.DeleteButton.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void DeletingConstructorRemovesTheStep()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.Click_TestViewDotNet_DLL_Constructor_DeleteButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.UIExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander);
            Assert.IsFalse(controlExistsNow);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void DeletingFavoiriteRemovesTheStep()
        {
            UIMap.Expand_DotnetDll_ByClickingCheckbox(true);
            UIMap.Click_TestViewDotNet_DLL_FavouriteFood_DeleteButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.UIExpansionIndicatorCheckBox.Checked);
            var controlExistsNow = UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander);
            Assert.IsFalse(controlExistsNow);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Filter_Explorer(DotnetWorkflowForTesting);
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
        }

        public const string DotnetWorkflowForTesting = "DotnetWorkflowForTesting";

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
