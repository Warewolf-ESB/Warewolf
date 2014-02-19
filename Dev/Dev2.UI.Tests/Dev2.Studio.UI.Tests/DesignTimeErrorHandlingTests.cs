using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DesignTimeErrorHandlingTests : UIMapBase
    {
        #region Cleanup

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
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
            ExplorerUIMap.EnterExplorerSearchText(workflowToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", workflowToUse);
            var theTab = TabManagerUIMap.GetActiveTab();

            // Edit the DbService
            ExplorerUIMap.EnterExplorerSearchText(serviceToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);

            // Get wizard window
            WizardsUIMap.WaitForWizard();
            // Tab to mappings
            DatabaseServiceWizardUIMap.TabToOutputMappings();
            // Remove column 1+2's mapping
            KeyboardCommands.SendTabs(4);
            KeyboardCommands.SendDel();
            KeyboardCommands.SendTab();
            KeyboardCommands.SendDel();

            // Save
            DatabaseServiceWizardUIMap.ClickOK();
            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }

            // Fix Errors
            if(WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, serviceToUse + "(ServiceDesigner)"))
            {
                // Assert mapping does not exist
                Assert.IsFalse(
                    WorkflowDesignerUIMap.DoesActivityDataMappingContainText(
                        WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)"),
                        "[[get_Rows().Column2]]"), "Mappings not fixed, removed mapping still in use");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, mappings on the service are set to required and hitting the fix errors adorner should prompt the user to add required mappings to the activity instance's mappings")]
        [Owner("Ashley Lewis")]
        // Properly broken functionality
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        {
            const string workflowResourceName = "DesignTimeErrorHandlingRequiredMappingUITest";
            const string dbResourceName = "UserIsPromptedToAddRequiredDbServiceMappingsTest";

            // Open the Workflow
            ExplorerUIMap.EnterExplorerSearchText(workflowResourceName);
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow(workflowResourceName, "UI TEST");

            // Edit the DbService
            ExplorerUIMap.DoubleClickService(dbResourceName, "INTEGRATION TEST SERVICES");

            // Get wizard window
            DatabaseServiceWizardUIMap.ClickMappingTab(450); // over-ride cuz silly chickens like long names in test ;(

            //set the first input to required
            KeyboardCommands.SendTabs(2, 50);
            KeyboardCommands.SendSpace();

            // Save
            DatabaseServiceWizardUIMap.ClickOK();
            ResourceChangedPopUpUIMap.ClickCancel();

            UITestControl activity = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, dbResourceName);

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { TheTab = theTab, Activity = activity };

            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button not visible");

            activityUiMap.ClickFixErrors();
            activityUiMap.ClickCloseMapping();
            Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button is still visible");
        }
    }
}
