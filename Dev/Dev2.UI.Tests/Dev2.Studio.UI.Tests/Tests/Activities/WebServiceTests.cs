using System;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class WebServiceTests : UIMapBase
    {
        #region Fields

        private static DsfActivityUiMap _dbServiceUiMap;

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
            _dbServiceUiMap = new DsfActivityUiMap();
        }
        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            _dbServiceUiMap.Dispose();
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WebServiceTests_CodedUI")]
        public void WebServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            //Drag the service onto the design surface
            _dbServiceUiMap.DragServiceOntoDesigner("FetchCities", "XXX");
            _dbServiceUiMap.ClickEdit();
            //Wizard actions
            WebServiceWizardUIMap.ClickMappingTab();
            WebServiceWizardUIMap.EnterDataIntoMappingTextBox(4, newMapping);
            WebServiceWizardUIMap.ClickSaveButton(3);
            //Close the mappings on the service
            _dbServiceUiMap.ClickCloseMapping();
            //Assert the the error button is there
            Assert.IsTrue(_dbServiceUiMap.IsFixErrorButtonShowing());
            //Click the fix errors button
            _dbServiceUiMap.ClickFixErrors();
            _dbServiceUiMap.ClickCloseMapping();
            //Assert that the fix errors button isnt there anymore
            Assert.IsFalse(_dbServiceUiMap.IsFixErrorButtonShowing());
        }
    }
}
