using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DesignTimeErrorHandlingTests : UIMapBase
    {
        #region Fields

        const string ExplorerTab = "Explorer";

        #endregion

        #region Cleanup

        [TestCleanup]
        public void TestCleanup()
        {
            //close any open dialogs
            var tryFindDialog = StudioWindow.GetChildren()[0];
            if(tryFindDialog.GetType() == typeof(WpfWindow))
            {
                Mouse.Click(tryFindDialog);
                SendKeys.SendWait("{ESCAPE}");
                Assert.Fail("Resource changed dialog hanging after test, might not have rendered properly");
            }
            //close any open tabs
            TabManagerUIMap.CloseAllTabs();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
        }
        
        #endregion

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, the mappings on the service are changed and hitting the fix errors adorner should change the activity instance's mappings")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_DbServiceMappingsFixed()
        // ReSharper restore InconsistentNaming
        {
            const string workflowToUse = "Bug_10011";
            const string serviceToUse = "Bug_10011_DbService";
            Clipboard.Clear();
            // Open the Workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClickServerInServerDDL("localhost");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(workflowToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", workflowToUse);
            var theTab = TabManagerUIMap.GetActiveTab();
            // Edit the DbService
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(serviceToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);
            // Get wizard window

            WizardsUIMap.WaitForWizard();
            // Tab to mappings
            DatabaseServiceWizardUIMap.TabToOutputMappings();
            // Remove column 1+2's mapping
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");
            // Save
            DatabaseServiceWizardUIMap.ClickOK();

            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }

            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);

            // Get wizard window
            WizardsUIMap.WaitForWizard();

            // Tab to mappings
            DatabaseServiceWizardUIMap.TabToOutputMappings();
            // Replace column 1's mapping
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("Column1");
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
                Assert.IsFalse(WorkflowDesignerUIMap.DoesActivityDataMappingContainText(WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)"), "[[get_Rows().Column2]]"), "Mappings not fixed, removed mapping still in use");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, mappings on the service are set to required and hitting the fix errors adorner should prompt the user to add required mappings to the activity instance's mappings")]
        [Owner("Ashley")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        {
            // Open the Workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("PBI_9957_UITEST");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTCATEGORY", "PBI_9957_UITEST");
            var theTab = TabManagerUIMap.GetActiveTab();
            // Edit the DbService
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Bug_10011_DbService");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", "Bug_10011_DbService");

            // Get wizard window
            WizardsUIMap.WaitForWizard();

            // Tab to mappings
            DatabaseServiceWizardUIMap.TabToInputMappings();
            // Set input mapping to required
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            var wizard = StudioWindow.GetChildren()[0];
            Keyboard.SendKeys(wizard, "{TAB}");
            Keyboard.SendKeys(wizard, " ");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            // Save
            DatabaseServiceWizardUIMap.ClickOK();

            Assert.IsTrue(ResourceChangedPopUpUIMap.WaitForDialog(5000), "Resource changed dialog did not show");
            ResourceChangedPopUpUIMap.ClickCancel();

            // Fix Errors
            if(WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "Bug_10011_DbService"))
            {
                //Assert mappings are prompting the user to add required mapping
                var getCloseMappingToggle = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Bug_10011_DbService", "Close Mapping");
                Assert.IsNotNull(getCloseMappingToggle, "Fix Error does not prompt the user to input required mappings");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }
    }
}
