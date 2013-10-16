using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    [Ignore]//Problem with the code base
    public class DesignTimeErrorHandlingTests
    {
        #region Fields

        const string ExplorerTab = "Explorer";
        DocManagerUIMap _docManagerMap;
        ExplorerUIMap _explorerUiMap;
        TabManagerUIMap _tabManagerDesignerUiMap;
        WorkflowDesignerUIMap _workflowDesignerUiMap;
        DatabaseServiceWizardUIMap _databaseServiceWizardUiMap;
        ResourceChangedPopUpUIMap _resourceChangedPopUpUIMapWizardUiMap;

        #endregion

        #region Properties

        ExplorerUIMap ExplorerUiMap { get { return _explorerUiMap ?? (_explorerUiMap = new ExplorerUIMap()); } }
        public TabManagerUIMap TabManagerUiMap { get { return _tabManagerDesignerUiMap ?? (_tabManagerDesignerUiMap = new TabManagerUIMap()); } }
        public DocManagerUIMap DocManagerUiMap { get { return _docManagerMap ?? (_docManagerMap = new DocManagerUIMap()); } }
        public WorkflowDesignerUIMap WorkflowDesignerUiMap { get { return _workflowDesignerUiMap ?? (_workflowDesignerUiMap = new WorkflowDesignerUIMap()); } }
        public DatabaseServiceWizardUIMap DatabaseServiceWizardUiMap { get { return _databaseServiceWizardUiMap ?? (_databaseServiceWizardUiMap = new DatabaseServiceWizardUIMap()); } }
        public ResourceChangedPopUpUIMap ResourceChangedPopUpUiMap { get { return _resourceChangedPopUpUIMapWizardUiMap ?? (_resourceChangedPopUpUIMapWizardUiMap = new ResourceChangedPopUpUIMap()); } }

        #endregion

        #region Cleanup

        [TestCleanup]
        public void TestCleanup()
        {
            var window = new UIBusinessDesignStudioWindow();
            //close any open dialogs
            var tryFindDialog = window.GetChildren()[0];
            if(tryFindDialog.GetType() == typeof(WpfWindow))
            {
                Mouse.Click(tryFindDialog);
                SendKeys.SendWait("{ESCAPE}");
                Assert.Fail("Resource changed dialog hanging after test, might not have rendered properly");
            }
            //close any open tabs
            TabManagerUiMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUiMap.ClearExplorerSearchText();
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
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText(workflowToUse);
            ExplorerUiMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", workflowToUse);
            var theTab = TabManagerUiMap.GetActiveTab();
            // Edit the DbService
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText(serviceToUse);
            ExplorerUiMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);
            // Get wizard window
            var wizardWindow = DatabaseServiceWizardUiMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            WizardsUIMap.WaitForWizard();
            // Tab to mappings
            DatabaseServiceWizardUiMap.TabToOutputMappings(wizardWindow);
            // Remove column 1+2's mapping
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");
            // Save
            DatabaseServiceWizardUiMap.ClickOK();

            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUiMap.ClickCancel();
            }

            ExplorerUiMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);
            // Get wizard window
            wizardWindow = DatabaseServiceWizardUiMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            WizardsUIMap.WaitForWizard();
            // Tab to mappings
            DatabaseServiceWizardUiMap.TabToOutputMappings(wizardWindow);
            // Replace column 1's mapping
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("Column1");
            // Save
            DatabaseServiceWizardUiMap.ClickOK();
            SendKeys.SendWait("{TAB}utility");
            DatabaseServiceWizardUiMap.SaveDialogClickFirstFolder();
            SendKeys.SendWait("{TAB}{ENTER}");
            if (ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUiMap.ClickCancel();
            }

            // Fix Errors
            if(WorkflowDesignerUiMap.Adorner_ClickFixErrors(theTab, serviceToUse + "(DsfActivityDesigner)"))
            {
                // Assert mapping does not exist
                Assert.IsFalse(WorkflowDesignerUiMap.DoesActivityDataMappingContainText(WorkflowDesignerUiMap.FindControlByAutomationId(theTab, serviceToUse + "(DsfActivityDesigner)"), "[[get_Rows().Column2]]"), "Mappings not fixed, removed mapping still in use");
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
        // ReSharper disable InconsistentNaming
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        // ReSharper restore InconsistentNaming
        {
            // Open the Workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText("PBI_9957_UITEST");
            ExplorerUiMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "PBI_9957_UITEST");
            var theTab = TabManagerUiMap.GetActiveTab();
            // Edit the DbService
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText("Bug_10011_DbService");
            ExplorerUiMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", "Bug_10011_DbService");
            // Get wizard window
            WizardsUIMap.WaitForWizard();
            var wizardWindow = DatabaseServiceWizardUiMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];

            // Tab to mappings
            DatabaseServiceWizardUiMap.TabToInputMappings(wizardWindow);
            // Set input mapping to required
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait(" ");
            // Save
            DatabaseServiceWizardUiMap.ClickOK();

            if (ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUiMap.ClickCancel();
            }

            // Fix Errors
            if(WorkflowDesignerUiMap.Adorner_ClickFixErrors(theTab, "Bug_10011_DbService(DsfActivityDesigner)"))
            {
                //Assert mappings are prompting the user to add required mapping
                var getOpenMappingToggle = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "Bug_10011_DbService(DsfActivityDesigner)", "OpenMappingsToggle");
                var getCloseMappingButton = getOpenMappingToggle.GetChildren()[1];
                Assert.IsTrue(getCloseMappingButton.Height != -1, "Fix Error does not prompt the user to input required mappings");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }

        public UIMap UIMap
        {
            get
            {
                if((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
