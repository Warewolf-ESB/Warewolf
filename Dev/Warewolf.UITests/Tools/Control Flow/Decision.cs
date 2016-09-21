using System;
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
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
            Uimap.InitializeABlankWorkflow();
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
