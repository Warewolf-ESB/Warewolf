using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Warewolf.Studio.UISpecs
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class WorkflowDesignSurface
    {
        public WorkflowDesignSurface()
        {
        }

        [TestMethod]
        public void BigWorkflowDesignSurfaceUITest()
        {
            var uimap = new UIMap();
            var explorerTreeItemActionSteps = new Explorer_Tree_Item_Action_Steps();

            uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            uimap.Click_New_Workflow_Ribbon_Button();
            uimap.Assert_StartNode_Exists();
            uimap.Assert_Toolbox_Multiassign_Exists();

            //Given that the unit before this one passed its post asserts
            //UIMap.Assert_StartNode_Exists();
            //Uimap.Assert_Toolbox_Multiassign_Exists();
            uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Exists();

            //Scenario: Double Clicking Multi Assign Tool Small View on the Design Surface Opens Large View
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            uimap.Open_Assign_Tool_Large_View();
            uimap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Exists();

            //Scenario: Enter Text into Multi Assign Tool Small View Grid Column 1 Row 1 Textbox has text in text property
            //UIMap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Exists();
            uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable();

            //Scenario: Validating Multi Assign Tool with a variable entered into the Large View on the Design Surface Passes Validation and Variable is in the Variable list
            //UIMap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable();
            uimap.Click_Assign_Tool_Large_View_DoneButton();
            uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            uimap.Assert_VariableList_Scalar_Row1_Textbox_Equals_SomeVariable();

            //Scenario: Clicking the save ribbon button opens save dialog
            uimap.Assert_Save_Ribbon_Button_Exists();
            uimap.Click_Save_Ribbon_Button();
            uimap.Assert_SaveDialog_Exists();
            uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Scenario: Entering a valid workflow name into the save dialog does not set the error state of the textbox to true
            //UIMap.Assert_Save_Workflow_Dialog_Exists();
            //Uimap.Assert_Workflow_Name_Textbox_Exists();
            uimap.Enter_Servicename_As_SomeWorkflow();
            uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Scenario: Clicking the save button in the save dialog creates a new explorer item
            //UIMap.Assert_SaveDialog_SaveButton_Enabled();
            uimap.Click_SaveDialog_YesButton();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\SomeWorkflow");

            //Scenario: Clicking Debug Button Shows Debug Input Dialog
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            uimap.Click_Debug_Ribbon_Button();
            uimap.Assert_DebugInput_Window_Exists();
            uimap.Assert_DebugInput_DebugButton_Exists();

            //Scenario: Clicking Debug Button In Debug Input Dialog Generates Debug Output
            //UIMap.Assert_Debug_Input_Dialog_Exists();
            //Uimap.Assert_DebugInput_DebugButton_Exists();
            uimap.Click_DebugInput_DebugButton();
            uimap.Assert_DebugOutput_Contains_SomeVariable();

            //Scenario: Click Assign Tool QVI Button Opens Qvi
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            uimap.Open_Assign_Tool_Qvi_Large_View();
            uimap.Assert_Assign_QVI_Large_View_Exists_OnDesignSurface();

            //Scenario: Explorer context menu delete exists
            //Given "localhost\SomeWorkflow" exists in the explorer tree
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\SomeWorkflow");
            uimap.Assert_ExplorerContextMenu_Delete_Exists();

            //Scenario: Clicking delete in the explorer context menu on SomeWorkflow shows message box
            //UIMap.Assert_ExplorerConextMenu_Delete_Exists();
            uimap.Select_Delete_FromExplorerContextMenu();
            uimap.Assert_MessageBox_Yes_Button_Exists();

            //Scenario: Clicking Yes on the delete prompt dialog removes SomeWorkflow from the explorer tree
            //UIMap.Assert_MessageBox_Yes_Button_Exists();
            uimap.Click_MessageBox_Yes();
            explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\SomeWorkflow");
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;
    }
}
