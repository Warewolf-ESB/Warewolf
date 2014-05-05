using System;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DesignTimeErrorHandlingTests : UIMapBase
    {
        #region Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, the mappings on the service are changed and hitting the fix errors adorner should change the activity instance's mappings")]
        [Owner("Ashley Lewis")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_DbServiceMappingsFixed()
        {
            const string workflowToUse = "Bug_10011";
            const string serviceToUse = "Bug_10011_DbService";
            Clipboard.Clear();

            // Open the Workflow
            var theTab = ExplorerUIMap.DoubleClickWorkflow(workflowToUse, "TESTCATEGORY");

            // Edit the DbService
            ExplorerUIMap.DoubleClickService(serviceToUse, "UTILITY");

            //Test the service to get output mappings
            KeyboardCommands.SendTabs(11);
            KeyboardCommands.SendKey("a");
            KeyboardCommands.SendTabs(11);
            KeyboardCommands.SendEnter();
            Playback.Wait(2000);

            // Tab to mappings
            DatabaseServiceWizardUIMap.ClickMappingTab(320);

            // Remove column 1+2's mapping
            KeyboardCommands.SendTabs(4);
            KeyboardCommands.SendDel();
            KeyboardCommands.SendTab();
            KeyboardCommands.SendDel();

            KeyboardCommands.SendTabs(3);
            KeyboardCommands.SendEnter();
            // Save
            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }


            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false);
            activityUiMap.Activity = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)");
            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing(), "Error button should be showing");
            activityUiMap.ClickFixErrors();
            activityUiMap.ClickDoneButton();
            Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing(), "Error button shouldn't be showing");
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is opened, mappings on the service are set to required and hitting the fix errors adorner should prompt the user to add required mappings to the activity instance's mappings")]
        [Owner("Ashley Lewis")]
        // Properly broken functionality
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        {
            const string workflowResourceName = "DesignTimeErrorHandlingRequiredMappingUITest";
            const string dbResourceName = "UserIsPromptedToAddRequiredDbServiceMappingsTest";

            // Open the Workflow
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow(workflowResourceName, "UI TEST");

            // Edit the DbService
            ExplorerUIMap.DoubleClickService(dbResourceName, "INTEGRATION TEST SERVICES");

            // Get wizard window
            DatabaseServiceWizardUIMap.ClickMappingTab(550); // over-ride cuz silly chickens like long names in test ;(

            //set the first input to required
            KeyboardCommands.SendTabs(2, 50);
            KeyboardCommands.SendSpace();

            // Save
            KeyboardCommands.SendTabs(4, 50);
            KeyboardCommands.SendEnter();
            //ResourceChangedPopUpUIMap.ClickCancel();

            UITestControl activity = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, dbResourceName);

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { TheTab = theTab, Activity = activity };

            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button not visible");

            activityUiMap.ClickFixErrors();
            activityUiMap.ClickCloseMapping();
            //Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button is still visible");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DesignTimeErrorHandling_CodedUiTests")]
        public void DesignTimeErrorHandling_CodedUiTests_WhenOpeningMapping_FixButtonIsVisible()
        {
            //------------Setup for test--------------------------
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "TravsTestService");

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab };

            activityUiMap.ClickEdit();
            WizardsUIMap.WaitForWizard();

            //Wizard actions
            DatabaseServiceWizardUIMap.ClickMappingTab();
            DatabaseServiceWizardUIMap.EnterDataIntoMappingTextBox(0, newMapping);
            DatabaseServiceWizardUIMap.ClickSaveButton(3);
            ResourceChangedPopUpUIMap.ClickCancel();
            //Assert the the error button is there
            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing());
            //Click the fix errors button
            activityUiMap.ClickOpenMapping();
            Assert.AreEqual("Fix", activityUiMap.GetDoneButtonDisplayName());
            activityUiMap.ClickDoneButton();
            Assert.AreEqual("Done", activityUiMap.GetDoneButtonDisplayName());
            activityUiMap.ClickDoneButton();
            //Assert that the fix errors button isnt there anymore
            Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing());
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
