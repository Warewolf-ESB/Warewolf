using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class AggregateCalculate
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void AggregateCalculateToolUITest()
        {
            UIMap.Drag_Toolbox_AggregateCalculate_Onto_DesignSurface();
            UIMap.Open_AggregateCalculate_Tool_large_view();
            UIMap.Enter_Text_Into_Aggregate_Calculate_Large_View();
            UIMap.Press_F6();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
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
