using System;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.TabManager
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class TabManagerTests : UIMapBase
    {
        #region Fields


        #endregion

        #region Setup
        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TabManagerTests_CodedUI")]
        [Description("For bug 10086 - Switching tabs does not flicker unsaved status")]
        //This test could fail because of the really long time it takes to save a workflow and close the old tab
        public void TabManagerTests_CodedUI_CreateTwoWorkflowsSwitchBetween_ExpectStarNotShowingInName()
        {
            var firstName = "Test" + Guid.NewGuid().ToString().Substring(24);
            var secondName = "Test" + Guid.NewGuid().ToString().Substring(24);

            //Create first workflow
            DsfActivityUiMap dsfActivityUiMap = new DsfActivityUiMap();
            dsfActivityUiMap.DragToolOntoDesigner(ToolType.Assign);
            RibbonUIMap.ClickSave();
            SaveDialogUIMap.ClickAndTypeInNameTextbox(firstName);
            //Create second workflow
            DsfActivityUiMap dsfActivityUiMap2 = new DsfActivityUiMap();
            dsfActivityUiMap2.DragToolOntoDesigner(ToolType.Assign);
            RibbonUIMap.ClickSave();
            SaveDialogUIMap.ClickAndTypeInNameTextbox(secondName);
            //Switch tabs a couple of times 
            TabManagerUIMap.ClickTab(firstName);
            TabManagerUIMap.ClickTab(secondName);
            TabManagerUIMap.ClickTab(firstName);

            //Check that the tabs names dont have stars in them
            Assert.AreEqual(2, TabManagerUIMap.GetTabCount());
            Assert.IsFalse(TabManagerUIMap.GetTabNameAtPosition(0).Contains("*"));
            Assert.IsFalse(TabManagerUIMap.GetTabNameAtPosition(1).Contains("*"));
        }
    }
}
