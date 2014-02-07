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

        private static DsfActivityUiMap _dsfActivityUiMap;

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
            _dsfActivityUiMap = new DsfActivityUiMap();
        }
        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            _dsfActivityUiMap.Dispose();
        }

        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("PluginServiceTests_CodedUI")]
        public void PluginServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            //Drag the service onto the design surface
            _dsfActivityUiMap.DragServiceOntoDesigner("DummyService", "TRAV");
            _dsfActivityUiMap.ClickEdit();
            //Wizard actions
            PluginServiceWizardUIMap.ClickMappingTab();
            PluginServiceWizardUIMap.EnterDataIntoMappingTextBox(4, newMapping);
            PluginServiceWizardUIMap.ClickSaveButton(2);
            //Close the mappings on the service
            _dsfActivityUiMap.ClickCloseMapping();
            //Assert the the error button is there
            Assert.IsTrue(_dsfActivityUiMap.IsFixErrorButtonShowing());
            //Click the fix errors button
            _dsfActivityUiMap.ClickFixErrors();
            _dsfActivityUiMap.ClickCloseMapping();
            //Assert that the fix errors button isnt there anymore
            Assert.IsFalse(_dsfActivityUiMap.IsFixErrorButtonShowing());
        }
    }
}
