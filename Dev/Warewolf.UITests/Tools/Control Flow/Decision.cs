using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Decision
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void DecisionToolUITest()
        {
            Uimap.Drag_Toolbox_Decision_Onto_DesignSurface();
            Uimap.Click_Decision_Dialog_Done_Button();
            Uimap.Open_Decision_Large_View();
            Uimap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed_UITest()
        {
            Uimap.Drag_Toolbox_Decision_Onto_DesignSurface();
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.Exists);
            Uimap.Click_Decision_Dialog_Done_Button();
            Uimap.CopyAndPaste_Decision_Tool_On_The_Designer();
            Assert.IsFalse(Uimap.DecisionOrSwitchDialog.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
        }

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
