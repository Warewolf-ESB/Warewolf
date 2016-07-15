using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    /// <summary>
    /// Summary description for WorkflowDesignSurface
    /// </summary>
    [CodedUITest]
    public class Assign
    {
        public Assign()
        {
        }

        [TestMethod]
        [Ignore]//Re-introduce before WOLF-1929 can be moved to done
        public void AssignToolUITest()
        {
            if (!Uimap.MainStudioWindow.SideMenuBar.CollapsedSideMenu.NewWorkflowIcon.Exists)
            {
                Uimap.MainStudioWindow.DrawHighlight();
            }
            Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Assert_StartNode_Exists();

            //Action Unit: Dragging a multiassign tool from the toolbox onto the design surface creates that tool on the workflow
            //Given the start node exists
            Uimap.Assert_Toolbox_FilterTextbox_Exists();
            Uimap.Assert_Toolbox_RefreshButton_Exists();
            Uimap.Assert_Toolbox_Multiassign_Exists();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Exists();

            //Action Unit: Double Clicking Multi Assign Tool Small View on the Design Surface Opens Large View
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Exists();

            //Action Unit: Enter Text into Multi Assign Tool Large View Grid Column 1 Row 1 Textbox has text in text property
            //UIMap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Exists();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable();

            //Action Unit: Validating Multi Assign Tool with a variable entered into the Large View on the Design Surface Passes Validation and Variable is in the Variable list
            //UIMap.Assert_Assign_Large_View_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable();
            Uimap.Click_Assign_Tool_Large_View_DoneButton();
            Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            Uimap.Assert_VariableList_Exists();
            Uimap.Assert_VariableList_Scalar_Row1_Textbox_Equals_SomeVariable();

            //Action Unit: Click Assign Tool QVI Button Opens Qvi
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            Uimap.Open_Assign_Tool_Qvi_Large_View();
            Uimap.Assert_Assign_QVI_Large_View_Exists_OnDesignSurface();

            //Action Unit: Clicking the save ribbon button opens save dialog
            Uimap.Assert_Save_Ribbon_Button_Exists();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Assert_SaveDialog_CancelButton_Exists();
            Uimap.Assert_SaveDialog_ErrorLabel_Exists();
            Uimap.Assert_SaveDialog_ExplorerTree_Exists();
            Uimap.Assert_SaveDialog_ExplorerTreeItem_Exists();
            Uimap.Assert_SaveDialog_ExplorerView_Exists();
            Uimap.Assert_SaveDialog_FilterTextbox_Exists();
            Uimap.Assert_SaveDialog_NameLabel_Exists();
            Uimap.Assert_SaveDialog_RefreshButton_Exists();
            Uimap.Assert_SaveDialog_SaveButton_Exists();
            Uimap.Assert_SaveDialog_WorkspaceName_Exists();
            Uimap.Assert_SaveDialog_Exists();
            Uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();
            Playback.Wait(2000);

            //Action Unit: Entering a valid workflow name into the save dialog does not set the error state of the textbox to true
            //UIMap.Assert_Save_Workflow_Dialog_Exists();
            //Uimap.Assert_Workflow_Name_Textbox_Exists();
            Uimap.Enter_Servicename_As_SomeWorkflow();
            if (!Uimap.SaveDialogWindow.SaveButton.Enabled)
            {
                Uimap.SaveDialogWindow.SaveButton.DrawHighlight();
            }
            Uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Action Unit: Clicking the save button in the save dialog dismisses save dialog
            //UIMap.Assert_SaveDialog_SaveButton_Enabled();
            Uimap.Click_SaveDialog_YesButton();
            Playback.Wait(2000);
            Uimap.Assert_SaveDialog_Does_Not_Exist();

            //Action Unit: Filtering the explorer tree shows only SomeWorkflow on local server
            Uimap.Enter_SomeWorkflow_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Assert_Explorer_Localhost_First_Item_Exists();

            /**TODO: Re-introduce these units before WOLF-1923 can be moved to done.
            //Action Unit: Clicking Debug Button Shows Debug Input Dialog
            //UIMap.Assert_MultiAssign_Exists_OnDesignSurface();
            //Uimap.Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Assert_DebugInput_Window_Exists();
            Uimap.Assert_DebugInput_DebugButton_Exists();
	        Uimap.Assert_DebugInput_CancelButton_Exists();
	        Uimap.Assert_DebugInput_RememberCheckbox_Exists();
	        Uimap.Assert_DebugInput_ViewInBrowser_Button_Exists();
	        Uimap.Assert_DebugInput_DebugButton_Exists();
	        Uimap.Assert_DebugInput_InputData_Window_Exists();
	        Uimap.Assert_DebugInput_InputData_Field_Exists();
	        Uimap.Assert_DebugInput_Xml_Tab_Exists();
	        Uimap.Assert_DebugInput_Xml_Window_Exists();
	        Uimap.Assert_DebugInput_Json_Tab_Exists();
	        Uimap.Assert_DebugInput_Json_Window_Exists();

            //Action Unit: Clicking Debug Button In Debug Input Dialog Generates Debug Output
            //UIMap.Assert_Debug_Input_Dialog_Exists();
            //Uimap.Assert_DebugInput_DebugButton_Exists();
            Uimap.Click_DebugInput_DebugButton();
            Uimap.Assert_DebugOutput_Exists();
            Uimap.Assert_DebugOutput_SettingsButton_Exists();
            Uimap.Assert_DebugOutput_Contains_SomeVariable();
	        Uimap.Assert_DebugOutput_ExpandCollapseButton_Exists();
	        Uimap.Assert_DebugOutput_FilterTextbox_Exists();
	        Uimap.Assert_DebugOutput_ResultsTree_Exists();
	        Uimap.Assert_DebugOutput_SettingsButton_Exists();
            **/
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        //Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            //Action Unit: Try close any hanging save dialogs
            if (Uimap.SaveDialogWindow.CancelButton.Exists)
            {
                Uimap.Click_SaveDialog_CancelButton();
            }

            //Action Unit: Explorer context menu delete exists
            //Given "localhost\SomeWorkflow" exists in the explorer tree
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Assert_ExplorerContextMenu_Delete_Exists();

            //Action Unit: Clicking delete in the explorer context menu on SomeWorkflow shows message box
            //UIMap.Assert_ExplorerConextMenu_Delete_Exists();
            Uimap.Select_Delete_FromExplorerContextMenu();
            Uimap.Assert_MessageBox_Yes_Button_Exists();

            //Action Unit: Clicking Yes on the delete prompt dialog dismisses the dialog
            //UIMap.Assert_MessageBox_Yes_Button_Exists();
            Uimap.Click_MessageBox_Yes();
            Uimap.Assert_MessageBox_Does_Not_Exist();

            //Action Unit: Entering 'SomeWorkflow' and refreshing the explorer filter removes SomeWorkflow from the explorer tree
            Uimap.Enter_SomeWorkflow_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Assert_Explorer_Localhost_First_Item_Does_Not_Exist();
        }

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

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;
    }
}
