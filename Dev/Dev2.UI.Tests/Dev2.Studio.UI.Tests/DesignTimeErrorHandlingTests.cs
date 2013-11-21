using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
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
        [Owner("Ashley")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_DbServiceMappingsFixed()
        {

            Assert.Fail("This test is impacted by the one below that uses the same resource. Please adjust to use different resources. AND this test does not click the fix errors button. Fix as well.");

            //const string workflowToUse = "Bug_10011";
            //const string serviceToUse = "Bug_10011_DbService";
            //Clipboard.Clear();
            //// Open the Workflow
            //ExplorerUIMap.ClickServerInServerDDL("localhost");

            //ExplorerUIMap.EnterExplorerSearchText(workflowToUse);
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", workflowToUse);
            //var theTab = TabManagerUIMap.GetActiveTab();
            //// Edit the DbService

            //ExplorerUIMap.EnterExplorerSearchText(serviceToUse);
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);
            //// Get wizard window

            //WizardsUIMap.WaitForWizard();
            //// Tab to mappings
            //DatabaseServiceWizardUIMap.TabToOutputMappings();
            //// Remove column 1+2's mapping
            //Playback.Wait(200);
            //SendKeys.SendWait("{TAB}");
            //SendKeys.SendWait("{TAB}");
            //SendKeys.SendWait("{TAB}");
            //SendKeys.SendWait("{TAB}");
            //SendKeys.SendWait("{DEL}");
            //SendKeys.SendWait("{TAB}");
            //SendKeys.SendWait("{DEL}");
            //// Save
            //DatabaseServiceWizardUIMap.ClickOK();

            //if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            //{
            //    ResourceChangedPopUpUIMap.ClickCancel();
            //}

            //// Fix Errors
            //if(WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, serviceToUse + "(ServiceDesigner)"))
            //{
            //    // Assert mapping does not exist
            //    Assert.IsFalse(WorkflowDesignerUIMap.DoesActivityDataMappingContainText(WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)"), "[[get_Rows().Column2]]"), "Mappings not fixed, removed mapping still in use");
            //}
            //else
            //{
            //    Assert.Fail("'Fix Errors' button not visible");
            //}
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, mappings on the service are set to required and hitting the fix errors adorner should prompt the user to add required mappings to the activity instance's mappings")]
        [Owner("Ashley")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        {
            // Open the Workflow

            ExplorerUIMap.EnterExplorerSearchText("PBI_9957_UITEST");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTCATEGORY", "PBI_9957_UITEST");
            var theTab = TabManagerUIMap.GetActiveTab();
            // Edit the DbService

            ExplorerUIMap.EnterExplorerSearchText("Bug_10011_DbService");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", "Bug_10011_DbService");

            // Get wizard window
            WizardsUIMap.WaitForWizard();

            
            DatabaseServiceWizardUIMap.TabToInputMappings();
            // Set input mapping to required
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            var wizard = StudioWindow.GetChildren()[0];
            wizard.WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait(" ");
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
