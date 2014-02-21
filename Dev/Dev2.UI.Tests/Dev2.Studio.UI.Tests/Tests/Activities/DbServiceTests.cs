using System;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class DbServiceTests : UIMapBase
    {
        #region Fields


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
        [TestCategory("DbServiceTests_CodedUI")]
        public void DbServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            //Drag the service onto the design surface
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "TravsTestService");

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab };

            activityUiMap.ClickEdit();
            //Wizard actions
            DatabaseServiceWizardUIMap.ClickMappingTab();
            DatabaseServiceWizardUIMap.EnterDataIntoMappingTextBox(0, newMapping);
            DatabaseServiceWizardUIMap.ClickSaveButton(3);
            ResourceChangedPopUpUIMap.ClickCancel();
            //Assert the the error button is there
            Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing());
            //Click the fix errors button
            activityUiMap.ClickFixErrors();
            activityUiMap.ClickCloseMapping();
            //Assert that the fix errors button isnt there anymore
            Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing());
        }
    }
}
