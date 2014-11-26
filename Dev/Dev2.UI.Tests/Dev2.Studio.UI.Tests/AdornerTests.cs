
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests : UIMapBase
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

        #region Large View Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_RenameLargeView")]
        public void ToolDesigners_RenameLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine()
        {
            const string ToolName = "Rename";
            const ToolType ToolType = ToolType.Rename;
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolType, theTab);

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

            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType, theTab);

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
                KeyboardCommands.SendTab(50);
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
            const ToolType ToolType = ToolType.Copy;
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolType, theTab);

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

            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType, theTab);

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
            const int numberOfTabsToLastTextbox = 7;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                KeyboardCommands.SendTab(50);
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
            const ToolType ToolType = ToolType.Unzip;
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolType, theTab);

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

            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType, theTab);

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
                KeyboardCommands.SendTab(50);
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
            const ToolType ToolType = ToolType.Move;
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolType, theTab);

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

            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType, theTab);

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
                KeyboardCommands.SendTab(50);
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
            const ToolType ToolType = ToolType.Zip;
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            #region Test entering text into the textboxes

            //Enter test data into all the textboxes in the large view
            LargeViewUtilMethods.LargeViewTextboxesEnterTestData(ToolType, theTab);

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

            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType, theTab);

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
            int numberOfTabsToLastTextbox = 9;
            for(int i = 0; i < numberOfTabsToLastTextbox; i++)
            {
                KeyboardCommands.SendTab(50);
            }
            //Assert that the focus is in the last textbox
            Assert.IsTrue(allTextBoxesFromLargeView[allTextBoxesFromLargeView.Count - 1].HasFocus, "The tabbing is out of order");

            #endregion
        }

        #endregion

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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, requiredPoint);
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
                waitForTabToOpen = TabManagerUIMap.FindTabByName("Utility - Assign", 500);
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

            ExplorerUIMap.DoubleClickOpenProject("localhost", "INTEGRATION TEST SERVICES", resourceToUse);
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, innerResource);
            Mouse.Move(controlOnWorkflow, new Point(5, 5));
            Mouse.DoubleClick();

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
            Mouse.Click();
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
            // ReSharper disable ObjectCreationAsStatement
            new Point(theStartButton.BoundingRectangle.X - 100,
                // ReSharper restore ObjectCreationAsStatement
                                                theStartButton.BoundingRectangle.Y + 100);

            // Get a sample workflow
            ExplorerUIMap.DragResourceOntoWorkflowDesigner(theTab, resourceToUse, "INTEGRATION TEST SERVICES");

            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse, 100);
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
            Point startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            Point point = new Point(startPoint.X - 100, startPoint.Y + 100);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, point);

            //Get Mappings button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "Open Quick Variable Input");

            // Click it
            MouseCommands.MoveAndClick(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));

            // Enter some invalid data
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",",
                                                                                    "some(<).", "_suf",
                                                                                    "varOne,varTwo,varThree");

            // Click done
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                "Prefix contains invalid characters");
            Assert.IsNotNull(errorControl, "No error displayed for incorrect QVI input");

            #region Scroll Right

            var scrollBarH = WorkflowDesignerUIMap.ScrollViewer_GetHorizontalScrollBar(theTab);
            WorkflowDesignerUIMap.ScrollViewer_GetHorizontalScrollBar(theTab);

            // Look far right
            Mouse.StartDragging(scrollBarH);
            Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollRight(theTab)); 

            #endregion

            // Assert clicking an error focuses the correct text-box
            MouseCommands.ClickControl(errorControl.GetChildren()[0]);

            // enter some correct data
            KeyboardCommands.SendKey("^a^xpre_", 100);

            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            // Check the data
            string varName = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(varName, "[[pre_varOne_suf]]");
        }
    }
}
