
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class PluginServiceTests : UIMapBase
    {

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
        [TestCategory("PluginServiceTests_CodedUI")]
        public void PluginServiceTests_CodedUI_EditService_ExpectErrorButton()
        {
            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            //Drag the service onto the design surface
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DummyService");

            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab })
            {

                activityUiMap.ClickEdit();
                //Wizard actions
                PluginServiceWizardUIMap.ClickMappingTab();
                PluginServiceWizardUIMap.EnterDataIntoMappingTextBox(3, newMapping);
                PluginServiceWizardUIMap.ClickSaveButton(1);
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceTests_CodedUI")]
        public void PluginServiceTests_CodedUI_EditSource_FromEditService_ExceptNoNameError()
        {
            //------------Setup for test--------------------------

            //Drag the service onto the design surface
            UITestControl theTab = ExplorerUIMap.DoubleClickWorkflow("ErrorFrameworkTestWorkflow", "UI TEST");

            UITestControl service = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DummyService");

            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { Activity = service, TheTab = theTab })
            {

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
}
