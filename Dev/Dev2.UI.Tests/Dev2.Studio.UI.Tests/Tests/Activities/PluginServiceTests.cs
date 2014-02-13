using System;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class PluginServiceTests : UIMapBase
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
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
        [TestCategory("PluginServiceTests_CodedUI")]
        public void PluginServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            //Drag the service onto the design surface
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DummyService");

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab };

            activityUiMap.ClickEdit();
            //Wizard actions
            PluginServiceWizardUIMap.ClickMappingTab();
            PluginServiceWizardUIMap.EnterDataIntoMappingTextBox(3, newMapping);
            PluginServiceWizardUIMap.ClickSaveButton(1);
            ResourceChangedPopUpUIMap.ClickCancel();
            //Assert the the error button is there
            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing());
            //Click the fix errors button
            activityUiMap.ClickFixErrors();
            activityUiMap.ClickCloseMapping();
            //Assert that the fix errors button isnt there anymore
            Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing());
        }
    }
}
