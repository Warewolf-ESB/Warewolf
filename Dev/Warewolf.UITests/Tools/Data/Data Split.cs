using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Split
    {
        [TestMethod]
        public void DataSplitToolUITest()
        {
            //Scenario: Drag toolbox Data_Split onto a new workflow creates small view on design surface
            Uimap.Drag_Toolbox_Data_Split_Onto_DesignSurface();
            Uimap.Assert_Data_Split_Exists_OnDesignSurface();

            //@NeedsDataSplitToolSmallViewOnTheDesignSurface
            // Scenario: Double Clicking Data Split Tool Small View on the Design Surface Opens Large View
            Uimap.Open_Data_Split_Large_View();
            Uimap.Assert_Data_Split_Large_View_Exists_OnDesignSurface();

            //@NeedsDataSplitLargeViewOnTheDesignSurface
            // Scenario: Click Data Split Tool QVI Button Opens Qvi
            Uimap.Open_Data_Split_Tool_Qvi_Large_View();
            Uimap.Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface();
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
