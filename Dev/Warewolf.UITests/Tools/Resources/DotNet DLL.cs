using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class DotNet_DLL
    {
        const string DotNetDllSource = "DotNetDllSource";
        const string DotNetPlugin = "DotNetDll";

        [TestMethod]
		[TestCategory("Tools")]
        public void DotNetDLLToolUITest()
        {
            UIMap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
            UIMap.Open_DotNet_DLL_Connector_Tool_Large_View();
            UIMap.Add_Dotnet_Dll_Source(DotNetDllSource);
            UIMap.Save_With_Ribbon_Button_And_Dialog(DotNetPlugin, true);
            UIMap.Click_Workflow_ExpandAll();
            UIMap.Select_Source_From_DotnetTool();
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
            UIMap.TryRemoveFromExplorer(DotNetPlugin);
            UIMap.TryRemoveFromExplorer(DotNetDllSource);
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
