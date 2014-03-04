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
    public class WebServiceTests : UIMapBase
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
        [TestCategory("WebServiceTests_CodedUI")]
        public void WebServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);

            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FetchCities");

            DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab };

            activityUiMap.ClickEdit();
           
            //Wizard actions
            
            WebServiceWizardUIMap.ClickMappingTab();
            WebServiceWizardUIMap.EnterDataIntoMappingTextBox(6, newMapping);
            WebServiceWizardUIMap.ClickSaveButton(2);
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
