using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Calculate
    {
        [TestMethod]
        public void CalculateToolUITest()
        {
            Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Assert_StartNode_Exists();

            //Scenario: Drag toolbox Calculate onto a new workflow
            Uimap.Drag_Toolbox_Calculate_Onto_DesignSurface();
            Uimap.Assert_Calculate_Exists_OnDesignSurface();
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
