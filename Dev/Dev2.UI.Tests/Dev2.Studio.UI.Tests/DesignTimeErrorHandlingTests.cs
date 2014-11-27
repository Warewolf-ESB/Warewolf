
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
            RestartStudioOnFailure();
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is opened, the mappings on the service are changed and hitting the fix errors adorner should change the activity instance's mappings")]
        [Owner("Ashley Lewis")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_DbServiceMappingsFixed()
        {
            const string workflowToUse = "Bug_10011";
            const string serviceToUse = "Bug_10011_DbService";
            var newColumnName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
            Clipboard.Clear();

            // Open the Workflow
            var theTab = ExplorerUIMap.DoubleClickWorkflow(workflowToUse, "TestCategory");

            // Edit the DbService
            ExplorerUIMap.DoubleClickService(serviceToUse, "utility");

            //Test the service to get output mappings
            KeyboardCommands.SendTabs(11);
            KeyboardCommands.SendKey("a");
            KeyboardCommands.SendTabs(11);
            KeyboardCommands.SendEnter();
            Playback.Wait(2000);

            // Tab to mappings
            DatabaseServiceWizardUIMap.ClickMappingTab(320);

            KeyboardCommands.SendTabs(4);
            KeyboardCommands.SendKey(newColumnName);

            KeyboardCommands.SendTabs(4);
            KeyboardCommands.SendEnter();
            // Save
            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }

            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false))
            {
                activityUiMap.Activity = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)");
                Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing(), "Error button should be showing");
                activityUiMap.ClickFixErrors();
                activityUiMap.ClickDoneButton();
                Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing(), "Error button shouldn't be showing");
            }
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
            KeyboardCommands.SendTabs(4, 250);
            KeyboardCommands.SendEnter();
            ResourceChangedPopUpUIMap.ClickCancel();

            UITestControl activity = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, dbResourceName);

            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap(false) { TheTab = theTab, Activity = activity })
            {

                Assert.IsTrue(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button not visible");

                activityUiMap.ClickFixErrors();
                KeyboardCommands.SendKey("[[Name]]", 25);

                activityUiMap.ClickCloseMapping();
                Assert.IsFalse(activityUiMap.IsFixErrorButtonShowing(), "'Fix Errors' button is still visible");
            }
        }
    }
}
