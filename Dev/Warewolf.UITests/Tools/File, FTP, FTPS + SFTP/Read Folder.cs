using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Read_Folder
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void ReadFolderToolUITest()
        {
            Uimap.Drag_Toolbox_Read_Folder_Onto_DesignSurface();
            Uimap.Open_Read_Folder_Tool_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
            Uimap.InitializeABlankWorkflow();
        }
        
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
