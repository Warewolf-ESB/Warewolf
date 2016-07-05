Feature: WorkflowDesignSurface
	One big test covering:
	workflow design surface
	variable list
	debug input
	debug output
	workflow save dialog
	
Scenario: The Workflow Design Surface UI Test
	Given I "Assert_NewWorkFlow_RibbonButton_Exists"
	When I "Click_New_Workflow_Ribbon_Button"
	Then I "Assert_StartNode_Exists"
	And I "Assert_Toolbox_Multiassign_Exists"
	
#Given that the unit before this one passed its post asserts
#	Given I "Assert_StartNode_Exists"
#	And I "Assert_Toolbox_Multiassign_Exists"
	When I "Drag_Toolbox_MultiAssign_Onto_DesignSurface"
	Then I "Assert_Assign_Small_View_Row1_Variable_Textbox_Exists"

#Scenario: Double Clicking Multi Assign Tool Small View on the Design Surface Opens Large View
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
#	And I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	When I "Open_Assign_Tool_Large_View"
	Then I "Assert_Assign_Large_View_Exists_OnDesignSurface"
	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Exists"

#Scenario: Enter Text into Multi Assign Tool Small View Grid Column 1 Row 1 Textbox has text in text property
#	Given I "Assert_Assign_Large_View_Exists_OnDesignSurface"
#	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Exists"
	When I "Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable"
	Then I "Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable"

#Scenario: Validating Multi Assign Tool with a variable entered into the Large View on the Design Surface Passes Validation and Variable is in the Variable list
#	Given I "Assert_Assign_Large_View_Exists_OnDesignSurface"
#	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable"
	When I "Click_Assign_Tool_Large_View_DoneButton"
	Then I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	And I "Assert_VariableList_Scalar_Row1_Textbox_Equals_SomeVariable"
	
#Scenario: Clicking the save ribbon button opens save dialog
	Given I "Assert_Save_Ribbon_Button_Exists"
	When I "Click_Save_Ribbon_Button"
	Then I "Assert_SaveDialog_Exists"
	And I "Assert_SaveDialog_ServiceName_Textbox_Exists"

#Scenario: Entering a valid workflow name into the save dialog does not set the error state of the textbox to true
#	Given I "Assert_Save_Workflow_Dialog_Exists"
#	And I "Assert_Workflow_Name_Textbox_Exists"
	When I "Enter_Workflowname_As_SomeWorkflow"
	Then I "Assert_SaveDialog_SaveButton_Enabled"

#Scenario: Clicking the save button in the save dialog creates a new explorer item
#	Given I "Assert_Save_Workflow_Dialog_Exists"
#	And I "Assert_Workflow_Name_Textbox_Exists"
#	And I "Assert_Workflow_Name_Textbox_Text_Equals_SomeWorkflow"
#	And I "Assert_SaveDialog_SaveButton_Enabled"
	When I "Click_SaveDialog_YesButton"
	And I scroll down in the explorer tree
	Then "localhost\SomeWorkflow" exists in the explorer tree

#Scenario: Clicking Debug Button Shows Debug Input Dialog
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
#	And I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	When I "Click_Debug_Ribbon_Button"
	Then I "Assert_DebugInput_Window_Exists"
	And I "Assert_DebugInput_DebugButton_Exists"

#Scenario: Clicking Debug Button In Debug Input Dialog Generates Debug Output
#	Given I "Assert_Debug_Input_Dialog_Exists"
#	And I "Assert_DebugInput_DebugButton_Exists"
	When I "Click_DebugInput_DebugButton"
	Then I "Assert_DebugOutput_Contains_SomeVariable"

#Scenario: Click Assign Tool QVI Button Opens Qvi
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
	When I "Open_Assign_Tool_Qvi_Large_View"
	Then I "Assert_Assign_QVI_Large_View_Exists_OnDesignSurface"

#Scenario: Clicking the tab close button prompts to save
	Given I "Assert_Close_Tab_Button_Exists"
	When I "Click_Close_Tab_Button"
