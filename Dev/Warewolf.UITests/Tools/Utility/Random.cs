using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Random
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void RandomToolUITest()
        {
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Enter_Result_Variable_On_Random_Tool();
            UIMap.Press_F6();
            UIMap.Select_Letters_From_Random_Type_Combobox();
            UIMap.Enter_Text_Into_Random_Length();
            UIMap.Press_F6();
            UIMap.Select_GUID_From_Random_Type_Combobox();
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
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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
