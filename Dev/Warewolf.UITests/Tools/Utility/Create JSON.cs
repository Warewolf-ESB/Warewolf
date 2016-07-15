using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Create_JSON
    {
        [TestMethod]
        public void CreateJSONToolUITest()
        {
            //Scenario: Drag toolbox create JSON onto a new workflow creates create JSON small view on the dsign surface
            Uimap.Drag_Toolbox_JSON_Onto_DesignSurface();
            Uimap.Assert_Create_JSON_Exists_OnDesignSurface();

            //@NeedsCreateJSONToolSmallViewOnTheDesignSurface
            //Scenario: Double Clicking Create JSON Tool Small View on the Design Surface Opens Large View
            Uimap.Open_Json_Tool_Large_View();
            Uimap.Assert_Json_Large_View_Exists_OnDesignSurface();

            //@NeedsCreateJSONLargeViewOnTheDesignSurface
            //Scenario: Click Create JSON Tool QVI Button Opens Qvi
            Uimap.Open_Json_Tool_Qvi_Large_View();
            Uimap.Assert_Json_Qvi_Large_View_Exists_OnDesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
            Uimap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.CleanupWorkflow();
        }

        #endregion

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

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;
    }
}
