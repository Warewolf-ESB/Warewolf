using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfFindRecordsActivityTests : UIMapBase
    {
        #region Fields


        #endregion

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
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
        [TestCategory("ToolDesigners_FindRecordslargeView")]
        public void ToolDesigners_FindRecordslargeView_TabbingToResultBox_FocusIsSetToResultBox()
        {
            var _dsfActivityUiMap = new DsfFindRecordsUiMap();
            _dsfActivityUiMap.ClickOpenLargeView();
            // Tab to the result box
            for(int j = 0; j < 7; j++)
            {
                KeyboardCommands.SendTab();
            }
            //Check that the focus is in the result box
            Assert.IsTrue(_dsfActivityUiMap.GetResultTextBoxControl(ToolsUiMapBase.ViewType.Large).HasFocus);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_FindRecordsSmallView")]
        public void ToolDesigners_FindRecordsSmallView_SelectingOptionInDopdownWithKeyboard_MatchesBoxEnabled()
        {
            var _dsfActivityUiMap = new DsfFindRecordsUiMap();
            _dsfActivityUiMap.SetFocusToConditionDropDown(1, ToolsUiMapBase.ViewType.Small);
            KeyboardCommands.SendDownArrows(13);
            Assert.IsFalse(_dsfActivityUiMap.IsMatchTextBoxEnabled(1, ToolsUiMapBase.ViewType.Small));
        }
    }
}
