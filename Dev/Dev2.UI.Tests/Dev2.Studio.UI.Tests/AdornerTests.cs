using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests : UIMapBase
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

        #region Large View Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_RenameLargeView")]
        public void ToolDesigners_RenameLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "Rename";
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolName, theTab);

            //Get all the textboxes off the large view
            List<UITestControl> allTextBoxesFromLargeView = LargeViewUtilMethods.GetAllTextBoxesFromLargeView(ToolName, theTab);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get the first error control
            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");

            //Get the second error control
            var desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls arnt null
            Assert.IsNotNull(errorControl, "The error didnt show up");
            Assert.IsNotNull(desErrorControl, "The error didnt show up");

            //Enter data into the password boxes
            LargeViewUtilMethods.EnterDataIntoPasswordBoxes(allTextBoxesFromLargeView);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, ToolName,
                                                                           "Open Large View");
            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            //Try get the error controls
            errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");
            desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls are null
            Assert.IsNull(errorControl, "The error showed up");
            Assert.IsNull(desErrorControl, "The error didnt showed up");


            //Check each textbox contains the right text
            int counter = 0;
            foreach(var uiTestControl in allTextBoxesFromLargeView)
            {
                WpfEdit textbox = uiTestControl as WpfEdit;
                if(textbox != null && !textbox.IsPassword)
                {
                    Assert.AreEqual("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]", textbox.Text, "the wrong text was in the textbox");
                }

                counter++;
            }

            #endregion

            #region Test tabbing

            //Set the focus into the first textbox
            allTextBoxesFromLargeView[0].SetFocus();

            //Tab through the controlls
            int numberOfTabsToLastTextbox = 7;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_CopyLargeView")]
        public void ToolDesigners_CopyLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "Copy";
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolName, theTab);

            //Get all the textboxes off the large view
            List<UITestControl> allTextBoxesFromLargeView = LargeViewUtilMethods.GetAllTextBoxesFromLargeView(ToolName, theTab);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get the first error control
            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");

            //Get the second error control
            var desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls arnt null
            Assert.IsNotNull(errorControl, "The error didnt show up");
            Assert.IsNotNull(desErrorControl, "The error didnt show up");

            //Enter data into the password boxes
            LargeViewUtilMethods.EnterDataIntoPasswordBoxes(allTextBoxesFromLargeView);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, ToolName,
                                                                           "Open Large View");
            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            //Try get the error controls
            errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");
            desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls are null
            Assert.IsNull(errorControl, "The error showed up");
            Assert.IsNull(desErrorControl, "The error didnt showed up");


            //Check each textbox contains the right text
            int counter = 0;
            foreach(var uiTestControl in allTextBoxesFromLargeView)
            {
                WpfEdit textbox = uiTestControl as WpfEdit;
                if(textbox != null && !textbox.IsPassword)
                {
                    Assert.AreEqual("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]", textbox.Text, "the wrong text was in the textbox");
                }

                counter++;
            }

            #endregion

            #region Test tabbing

            //Set the focus into the first textbox
            allTextBoxesFromLargeView[0].SetFocus();

            //Tab through the controlls
            int numberOfTabsToLastTextbox = 7;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_UnzipLargeView")]
        public void ToolDesigners_UnzipLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "UnZip";
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolName, theTab);

            //Get all the textboxes off the large view
            List<UITestControl> allTextBoxesFromLargeView = LargeViewUtilMethods.GetAllTextBoxesFromLargeView(ToolName, theTab);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get the first error control
            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");

            //Get the second error control
            var desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls arnt null
            Assert.IsNotNull(errorControl, "The error didnt show up");
            Assert.IsNotNull(desErrorControl, "The error didnt show up");

            //Enter data into the password boxes
            LargeViewUtilMethods.EnterDataIntoPasswordBoxes(allTextBoxesFromLargeView);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, ToolName,
                                                                           "Open Large View");
            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            //Try get the error controls
            errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");
            desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls are null
            Assert.IsNull(errorControl, "The error showed up");
            Assert.IsNull(desErrorControl, "The error didnt showed up");


            //Check each textbox contains the right text
            int counter = 0;
            foreach(var uiTestControl in allTextBoxesFromLargeView)
            {
                WpfEdit textbox = uiTestControl as WpfEdit;
                if(textbox != null && !textbox.IsPassword)
                {
                    Assert.AreEqual("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]", textbox.Text, "the wrong text was in the textbox");
                }

                counter++;
            }

            #endregion

            #region Test tabbing

            //Set the focus into the first textbox
            allTextBoxesFromLargeView[0].SetFocus();

            //Tab through the controlls
            int numberOfTabsToLastTextbox = 8;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_MoveLargeView")]
        public void ToolDesigners_MoveLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "Move";
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolName, theTab);

            //Get all the textboxes off the large view
            List<UITestControl> allTextBoxesFromLargeView = LargeViewUtilMethods.GetAllTextBoxesFromLargeView(ToolName, theTab);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get the first error control
            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Password must have a value");

            //Get the second error control
            var desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Destination Password must have a value");

            //Make sure that the error controls arnt null
            Assert.IsNotNull(errorControl, "The error didnt show up");
            Assert.IsNotNull(desErrorControl, "The error didnt show up");

            //Enter data into the password boxes
            LargeViewUtilMethods.EnterDataIntoPasswordBoxes(allTextBoxesFromLargeView);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, ToolName,
                                                                           "Open Large View");
            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            //Try get the error controls
            errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");
            desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls are null
            Assert.IsNull(errorControl, "The error showed up");
            Assert.IsNull(desErrorControl, "The error didnt showed up");


            //Check each textbox contains the right text
            int counter = 0;
            foreach(var uiTestControl in allTextBoxesFromLargeView)
            {
                WpfEdit textbox = uiTestControl as WpfEdit;
                if(textbox != null && !textbox.IsPassword)
                {
                    Assert.AreEqual("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]", textbox.Text, "the wrong text was in the textbox");
                }

                counter++;
            }

            #endregion

            #region Test tabbing

            //Set the focus into the first textbox
            allTextBoxesFromLargeView[0].SetFocus();

            //Tab through the controlls
            int numberOfTabsToLastTextbox = 7;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_ZipLargeView")]
        public void ToolDesigners_ZipLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "Zip";
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolName, theTab);

            //Get all the textboxes off the large view
            List<UITestControl> allTextBoxesFromLargeView = LargeViewUtilMethods.GetAllTextBoxesFromLargeView(ToolName, theTab);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get the first error control
            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");

            //Get the second error control
            var desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls arnt null
            Assert.IsNotNull(errorControl, "The error didnt show up");
            Assert.IsNotNull(desErrorControl, "The error didnt show up");

            //Enter data into the password boxes
            LargeViewUtilMethods.EnterDataIntoPasswordBoxes(allTextBoxesFromLargeView);

            //Click the done button
            LargeViewUtilMethods.ClickDoneButton(theTab, ToolName);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, ToolName,
                                                                           "Open Large View");
            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            //Try get the error controls
            errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Password must have a value");
            desErrorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                  "Destination Password must have a value");

            //Make sure that the error controls are null
            Assert.IsNull(errorControl, "The error showed up");
            Assert.IsNull(desErrorControl, "The error didnt showed up");


            //Check each textbox contains the right text
            int counter = 0;
            foreach(var uiTestControl in allTextBoxesFromLargeView)
            {
                WpfEdit textbox = uiTestControl as WpfEdit;
                if(textbox != null && !textbox.IsPassword)
                {
                    Assert.AreEqual("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]", textbox.Text, "the wrong text was in the textbox");
                }

                counter++;
            }

            #endregion

            #region Test tabbing

            //Set the focus into the first textbox
            allTextBoxesFromLargeView[0].SetFocus();

            //Tab through the controlls
            int numberOfTabsToLastTextbox = 8;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExternalService_EditService")]
        [Ignore]
        // - Test has problems finding the tab of the dialog. Sometimes its the second dialog others its the third.
        // ClickMappingTab() has the problem 
        // Manually verified
        public void ExternalService_EditService_EditWithNoSecondSaveDialog_ExpectOneDialog()
        {
            //------------Setup for test--------------------------
            ExplorerUIMap.EnterExplorerSearchText("Edit Service Workflow");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", "Edit Service Workflow");

            var newMapping = "ZZZ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);

            //------------Execute Test---------------------------

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Get DB service
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "TravsTestService");
            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(control.BoundingRectangle);

            //Get Adorner buttons
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TravsTestService", "Edit");

            // -- DO DB Services --
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Mouse.Click(button);
            button.WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;

            DatabaseServiceWizardUIMap.ClickMappingTab();
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait(newMapping);
            // -- wizard closed
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");

            // -- DO Web Services --

            //Get DB service
            control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FetchCities");
            // move to show adorner buttons ;)
            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(control.BoundingRectangle);

            //Get Adorner buttons
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "FetchCities", "Edit");

            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Mouse.Click(button);
            button.WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;

            DatabaseServiceWizardUIMap.ClickMappingTab();

            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait(newMapping);
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}{TAB}{ENTER}{TAB}{ENTER}");
            Playback.Wait(1200);

            ResourceChangedPopUpUIMap.ClickCancel();

            // -- DO Plugin Services --

            control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DummyService");
            // move to show adorner buttons ;)
            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(control.BoundingRectangle);

            //Get Adorner buttons
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DummyService", "Edit");

            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Mouse.Click(button);
            button.WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;

            DatabaseServiceWizardUIMap.ClickMappingTab(); // click on the second tab ;)
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait(newMapping);
            Playback.Wait(500);
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");

            //------------Assert Results-------------------------

            // check services for warning icon to incidate mappings out of date ;)

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "TravsTestService"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            WorkflowDesignerUIMap.TryCloseMappings("TravsTestService");

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "FetchCities"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            WorkflowDesignerUIMap.TryCloseMappings("FetchCities");

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "DummyService"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            WorkflowDesignerUIMap.TryCloseMappings("DummyService");
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test that clicking on the help button does indeed open an example workflow")]
        [Owner("Tshepo")]
        public void AdornerHelpButtonOpenAnExampleWorlkflowTest()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            //Get a point
            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", requiredPoint);
            //Get Adorner buttons
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "Open Help");
            Mouse.Click(button);
            //Get 'View Sample' link button
            var findViewSampleLink = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "View Sample Workflow");
            Mouse.Click(findViewSampleLink.GetChildren()[0]);

            //Wait for sample workflow
            UITestControl waitForTabToOpen = null;
            var count = 10;
            while(waitForTabToOpen == null && count > 0)
            {
                waitForTabToOpen = TabManagerUIMap.FindTabByName("Utility - Assign");
                Playback.Wait(500);
                count--;
            }

            //Assert workflow opened after a time out.
            Assert.IsNotNull(waitForTabToOpen);
        }

        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {
            const string resourceToUse = "Bug_10528";
            const string innerResource = "Bug_10528_InnerWorkFlow";

            // Open the Explorer
            ExplorerUIMap.EnterExplorerSearchText(resourceToUse);

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", resourceToUse);
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, innerResource);
            Mouse.Move(controlOnWorkflow, new Point(5, 5));
            var mappingsBtn =
                WorkflowDesignerUIMap.Adorner_GetButton(theTab, innerResource, "Open Mapping") as WpfToggleButton;

            if(mappingsBtn == null)
            {
                Assert.Fail("Could not find mapping button");
            }

            Mouse.Click(mappingsBtn);

            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();
            Point initialResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            var resizeThumb = controlCollection[controlCollection.Count - 1];
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                if(resizeThumb.BoundingRectangle.X == -1)
                {
                    Assert.Fail("Resize indicator is not visible");
                }

                initialResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                initialResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }
            else
            {
                Assert.Fail("Cannot find resize indicator");
            }

            // Drag
            Mouse.Move(new Point(resizeThumb.Left + 5, resizeThumb.Top + 5));
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y - 50));

            // Check position to see it dragged
            Point newResizerPoint = new Point();
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                newResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                newResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }

            if(!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }
        }

        [TestMethod]
        [TestCategory("DsfActivityTests")]
        [Description("Testing when a DsfActivity is dropped onto the design surface that the mapping auto expands and the resize control is visible")]
        [Owner("Travis Frisinger")]
        public void ResizeAdornerMappingsOnDrop_Expected_AdornerMappingIsResized()
        {
            const string resourceToUse = "NewForeachUpgradeDifferentExecutionTests";
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X,
                                                theStartButton.BoundingRectangle.Y + 200);

            // Get a sample workflow
            ExplorerUIMap.EnterExplorerSearchText(resourceToUse);
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", resourceToUse, workflowPoint1);

            Playback.Wait(100);

            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
            Mouse.Click(controlOnWorkflow, new Point(5, 5));

            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();
            Point initialResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            var resizeThumb = controlCollection[controlCollection.Count - 1];
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                if(resizeThumb.BoundingRectangle.X == -1)
                {
                    Assert.Fail("Resize indicator is not visible");
                }

                initialResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                initialResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }
            else
            {
                Assert.Fail("Cannot find resize indicator");
            }

            // Drag
            Mouse.Move(new Point(resizeThumb.Left + 5, resizeThumb.Top + 5));
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y + 50));

            // Check position to see it dragged
            Point newResizerPoint = new Point();
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                newResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                newResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }

            if(!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y > initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }
        }

        // PBI 8601 (Task 8855)
        [TestMethod]
        public void QuickVariableInputFromListTest()
        {
            Clipboard.Clear();
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            Point startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            Point point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);

            //Get Mappings button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "Open Quick Variable Input");

            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            // Enter some invalid data
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",",
                                                                                    "some(<).", "_suf",
                                                                                    "varOne,varTwo,varThree");

            // Click done
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                "Prefix contains invalid characters");
            Assert.IsNotNull(errorControl, "No error displayed for incorrect QVI input");

            // Assert clicking an error focusses the correct textbox
            Mouse.Click(errorControl.GetChildren()[0]);

            // enter some correct data
            SendKeys.SendWait("^a^xpre_");

            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            // Check the data
            string varName = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(varName, "[[pre_varOne_suf]]");
        }
    }
}
