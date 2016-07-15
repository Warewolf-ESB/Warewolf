using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Scripting
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CMD_Script
    {
        [TestMethod]
        public void CMDScriptToolUITest()
        {
            //Scenario: Drag toolbox CMD_Line onto a new workflow
            Uimap.Drag_Toolbox_CMD_Line_Onto_DesignSurface();
            Uimap.Assert_CMD_Line_Exists_OnDesignSurface();

            //@NeedsCMDLineToolSmallViewOnTheDesignSurface
            //Scenario: Double Clicking CMD Line Tool Small View on the Design Surface Opens Large View
            Uimap.Open_CMD_Line_Tool_Large_View();
            Uimap.Assert_CMD_Line_Large_View_Exists_OnDesignSurface();
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
