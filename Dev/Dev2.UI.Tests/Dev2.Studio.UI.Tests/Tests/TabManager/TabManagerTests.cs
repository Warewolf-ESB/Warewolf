using System;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.TabManager
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class TabManagerTests : UIMapBase
    {
        #region Fields


        #endregion

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TabManagerTests_CodedUI")]
        [Description("For bug 10086 - Switching tabs does not flicker unsaved status")]
        //This test could fail because of the really long time it takes to save a workflow and close the old tab
        public void TabManagerTests_CodedUI_CreateTwoWorkflowsSwitchBetween_ExpectStarNotShowingInName()
        {
            var firstName = "Test" + Guid.NewGuid().ToString().Substring(24);
            var secondName = "Test" + Guid.NewGuid().ToString().Substring(24);

            //Create first workflow
            DsfActivityUiMap dsfActivityUiMap = new DsfActivityUiMap();
            dsfActivityUiMap.DragToolOntoDesigner(ToolType.Assign);
            RibbonUIMap.ClickSave();

            SaveDialogUIMap.ClickAndTypeInNameTextbox(firstName);
            //Create second workflow
            DsfActivityUiMap dsfActivityUiMap2 = new DsfActivityUiMap();
            dsfActivityUiMap2.DragToolOntoDesigner(ToolType.Assign);
            RibbonUIMap.ClickSave();
            SaveDialogUIMap.ClickAndTypeInNameTextbox(secondName);
            //Switch tabs a couple of times 
            TabManagerUIMap.ClickTab(firstName);
            TabManagerUIMap.ClickTab(secondName);
            TabManagerUIMap.ClickTab(firstName);

            //Check that the tabs names dont have stars in them
            Assert.IsTrue(TabManagerUIMap.GetTabCount() >= 2);
            Assert.IsFalse(TabManagerUIMap.GetTabNameAtPosition(0).Contains("*"));
            Assert.IsFalse(TabManagerUIMap.GetTabNameAtPosition(1).Contains("*"));
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceTests_CodedUI")]
        public void PluginServiceTests_CodedUI_EditSource_FromEditService_ExceptNoNameError()
        {
            //------------Setup for test--------------------------

            //Drag the service onto the design surface
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DummyService");

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab };

            //------------Execute Test---------------------------
            activityUiMap.ClickEdit();
            PluginServiceWizardUIMap.EditSource();
            var contents = PluginServiceWizardUIMap.GetWindowContents();
            PluginServiceWizardUIMap.CancelEntireOperation();

            var result = (contents.IndexOf("Name already exists.", StringComparison.Ordinal) >= 0);
            var isEmpty = (contents.Length == 0);

            //------------Assert Results-------------------------
            Assert.IsFalse(isEmpty, "Copy did not copy content of Edit Source Wizard!");
            Assert.IsFalse(result, "Plugin Source Window Contains Save Message?! Check your warewolf-utils.js - updateSaveValidationSpan method");

        }
    }
}
