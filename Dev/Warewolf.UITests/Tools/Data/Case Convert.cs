using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Case_Convert
    {
        [TestMethod]
        public void CaseConvertUITest()
        {
            //Scenario: Drag toolbox Case Conversion onto a new workflow creates Case Conversion tool with small view on the design surface
            Uimap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
            Uimap.Assert_Case_Conversion_Exists_OnDesignSurface();

            //#@NeedsPostWebRequestToolSmallViewOnTheDesignSurface
            //# Scenario: Double Clicking Post Web Request Tool Small View on the Design Surface Opens Large View
            Uimap.Open_Case_Conversion_Tool_Qvi_Large_View();
            Uimap.Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface();
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
