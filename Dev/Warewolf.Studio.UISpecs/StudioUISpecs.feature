Feature: StudioUISpecs
	In order to create good workflows
	As a workflow designer
	I want to see the layout and function of workflows

@ignore	
Scenario: Knowledge Base Ribbon Button
	Given I "Assert_Knowledge_Base_Exists_OnDesignSurface"
	When I "Click_Knowledge_Ribbon_Button"
	#Then "Assert_Knowledge_Base_Tab_Exists"
	
#@NeedsKnowledgeBaseTab
#Scenario: Click Close Knowledgebase Tab Button
	#Given I "Assert_Close_Knowledgebase_Tab_Button_Exists"
	#When I "Click_Close_Knowledgebase_Tab_Button"
	Then I "Assert_MessageBox_No_Button_Exists"

#Scenario: Click MessageBox No
	#Given I "Assert_MessageBox_No_Button_Exists"
	When I "Click_MessageBox_No"
	#Then I "Assert_Tab_Closed"
	
@ignore
Scenario: Lock Menu Ribbon Button
	Given I "Assert_Lock_Button_Exists_OnDesignSurface"
	When I "Click_Unlock_Ribbon_Button"
	#Then "Assert_Lock_Ribbon_Button_Exists"

@ignore	
Scenario: New Database Connector Ribbon Button
	Given I "Assert_Database_Source_Exists"
	When I "Click_NewDatabaseSource_Ribbon_Button"
	#Then "Assert_NewDatabaseSource_Exists"
	
#@NeedsNewDatabaseConnectorTab
#Scenario: Click Close Database Connector Tab Button
	#Given I "Assert_Close_Database_Connector_Tab_Button_Exists"
	#When I "Click_Close_Database_Connector_Tab_Button"
	Then I "Assert_MessageBox_No_Button_Exists"

#Scenario: Click MessageBox No
	#Given I "Assert_Messagebox_No_Exists"
	When I "Click_MessageBox_No"
	#Then I "Assert_Tab_Closed"

@ignore	
Scenario: New Plugin Connector Ribbon Button
	Given I "Assert_Plugin_Source_Exists"
	When I "Click_NewPluginSource_Ribbon_Button"
	#Then "Assert_NewPluginSource_Exists"
	
#@NeedsNewPluginConnectorTab
#Scenario: Click Close Web Connector Tab Button
	#Given I "Assert_Close_Plugin_Connector_Tab_Button_Exists"
	#When I "Click_Close_Plugin_Connector_Tab_Button"
	Then I "Assert_MessageBox_No_Button_Exists"

#Scenario: Click MessageBox No
	#Given I "Assert_Messagebox_No_Exists"
	When I "Click_MessageBox_No"
	#Then I "Assert_Tab_Closed"

@ignore	
Scenario: New Web Connector Ribbon Button
	Given I "Assert_Web_Source_Exists"
	When I "Click_NewWebSource_Ribbon_Button"
	#Then "Assert_New_Web_Connector_Tab_Exists"
	
#@NeedsNewWebConnectorTab
#Scenario: Click Close Web Connector Tab Button
	#Given I "Assert_Close_Web_Connector_Tab_Button_Exists"
	#When I "Click_Close_Web_Connector_Tab_Button"
	Then I "Assert_MessageBox_No_Button_Exists"

#Scenario: Click MessageBox No
	#Given I "Assert_Messagebox_No_Exists"
	When I "Click_MessageBox_No"
	#Then I "Assert_Tab_Closed"

@ignore	
Scenario: Scheduler Ribbon Button
	Given I "Assert_Scheduler_Button_Exists_OnDesignSurface"
	When I "Click_Scheduler_Ribbon_Button"
	Then I "Assert_Scheduler_CreateNewTask_Exists"
	
#@NeedsSchedulerCreateNewTaskRibbonButton
#Scenario: Click Scheduler Create New Task Ribbon Button
	#Given I "Assert_Scheduler_Create_New_Task_Ribbon_Button"
	When I "Click_Scheduler_Create_New_Task_Ribbon_Button"
	Then I "Assert_Scheduler_DisabledRadioButton_Exists"
	
#@NeedsSchedulerTabOpen
#Scenario: Click Scheduler Create New Task Ribbon Button
	#Given The test is initialized using low level binding calls
	When I "Click_Scheduler_Disable_Task_Radio_Button"
	Then I "Assert_Scheduler_EditTrigger_Exists"
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler EditTrigger Button
	#Given The test is initialized using low level binding calls
	When I "Click_Scheduler_EditTrigger_Button"
	Then I "Assert_Scheduler_EnabledRadioButton_Exists"
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Enable Task Radio Button
	#Given I "Assert_Scheduler_Enable_Task_Radio_Button_Exists"
	When I "Click_Scheduler_Enable_Task_Radio_Button"
	Then I "Assert_Scheduler_ErrorMessage_Exists"
	Then I "Assert_Scheduler_HistoryInput_Exists"
	Then I "Assert_Scheduler_HistoryLabel_Exists"
	Then I "Assert_Scheduler_HistoryTable_Exists"
	Then I "Assert_Scheduler_NameInput_Exists"
	Then I "Assert_Scheduler_NameLabel_Exists"
	Then I "Assert_Scheduler_PasswordInput_Exists"
	Then I "Assert_Scheduler_PasswordLabel_Exists"
	Then I "Assert_Scheduler_ResourcePicker_Exists"
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler ResourcePicker
	#Given The test is initialized 
	When I "Click_Scheduler_ResourcePicker"
	Then I "Assert_Scheduler_RunTask_Checkbox_Exists"
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Run Task
	#Given I "Assert_Scheduler_RunTask_Checkbox_Exists"
	When I "Click_Scheduler_RunTask"
	Then I "Assert_Scheduler_DeleteButton_Exists"
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Delete Task
	#Given The test is initialized
	When I "Click_Scheduler_Delete_Task"
	Then I "Assert_Scheduler_Status_RadioButton_Exists"
	Then I "Assert_Scheduler_StatusLabe_Exists"
	Then I "Assert_Scheduler_TriggerLabel_Exists"
	Then I "Assert_Scheduler_TriggerValue_Exists"
	Then I "Assert_Scheduler_UserAccountLabel_Exists"
	Then I "Assert_Scheduler_UsernameInput_Exists"
	Then I "Assert_Scheduler_Usernamelabel_Exists"
	Then I "Assert_Scheduler_WorkflowInput_Exists"
	Then I "Assert_Scheduler_WorkflowLabel_Exists"
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Database Connector Onto Design Surface Should not be droppable
	#Given "Some database connector" exists in the explorer tree
	When I "Drag_Database_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Plugin Connector Onto Design Surface Should not be droppable
	#Given "Some plugin connector" exists in the explorer tree
	When I "Drag_Plugin_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Web Connector Onto Design Surface Should not be droppable
	#Given "Some web connector" exists in the explorer tree
	When I "Drag_Web_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Sharepoint Connector Onto Design Surface Should not be droppable
	#Given "Some Sharepoint Source" exists in the explorer tree
	When I "Drag_Sharepoint_Source_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Server Source Onto Design Surface Should not be droppable
	#Given "Some Server Source" exists in the explorer tree
	When I "Drag_Server_Source_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"

#@NeedsDebugInputDialog
#Scenario: Click Debug Input Dialog Cancel
	Given I "Assert_Debug_Input_Cancel_Button_Exists"
	When I "Click_Debug_Input_Dialog_Cancel"
	Then I "Assert_DebugInput_Window_Does_Not_Exist"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Admin ServerPermissions
	#Given I "Assert_Settings_Tab_Exists"
	When I "Click_Settings_Admin_ServerPermissions"
	#Then I "Assert_Settings_Admin_ServerPermissions_Checkbox_Is_Checked"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ResourcePermissions
	#Given The test is initialized
	When I "Click_Settings_Contribute_ResourcePermissions"
	#Then I "Assert_Settings_Contribute_ResourcePermissions_Checkbox_Is_Checked"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ServerPermissions
	#Given The test is initialized
	When I "Click_Settings_Contribute_ServerPermissions"
	#Then I "Assert_Settings_Contribute_ServerPermissions_Checkbox_Is_Checked"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ResourcePermissions
	#Given The test is initialized
	When I "Click_Settings_Execute_ResourcePermissions"
	#Then I "Assert_Settings_Execute_ResourcePermissions_Checkbox_Is_Checked"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings ResourcePermissions ResourcePicker
	#Given The test is initialized
	When I "Click_Settings_ResourcePermissions_ResourcePicker"
	#Then I "Assert_Settings_ResourcePermissions_ResourcePicker_Checkbox_Is_Checked"
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings View ResourcePermissions
	#Given The test is initialized
	When I "Click_Settings_View_ResourcePermissions"
	#Then I "Assert_Settings_View_ResourcePermissions_Checkbox_Is_Checked"
	
@ignore
@NeedsBlankWorkflow
Scenario: Right Click On Design Surface
	When I "Open_Context_Menu_OnDesignSurface"
	Then I "Assert_Generic_Context_Menu_Exists"
	
@ignore
@NeedsBlankWorkflow
Scenario: Context Menu on New Workflow Tab
	When I "RightClick_New_Workflow_Tab"
	Then I "Assert_New_Workflow_Context_Menu_Exists"
	
@ignore
Scenario: Pin and unpin Explorer
	Given I "Click_Toggle_Unpin_Explorer"
	Then I "Click_Toggle_Pin_Explorer"
	
@ignore
Scenario: Pin and unpin Help
	Given I "Click_Toggle_Unpin_Documentor"
	Then I "Click_Toggle_Pin_Documentor"
	
@ignore
Scenario: Pin and unpin Toolbox
	Given I "Click_Toggle_Unpiin_Toolbox"
	Then I "Click_Toggle_Pin_Toolbox"
	
@ignore
Scenario: Pin and unpin Debug Output
	Given I "Click_Toggle_Unpin_DebugOutput"
	Then I "Click_Toggle_Pin_DebugOutput"
	
@ignore
Scenario: Pin and unpin variable list
	Given I "Click_Toggle_Unpin_VariableList"
	Then I "Click_Toggle_Pin_VariableList"

@ignore
Scenario: Ensure unused variables do not appear in Debug Input window
	Given I have variables as
    | Variable    | Note              | Input | Output | IsUsed |
    | [[rec().a]] | This is recordset |       | YES    | YES    |
    | [[rec().b]] |                   |       |        |        |
    | [[mr()]]    |                   |       |        | YES    |
    | [[Var]]     |                   | YES   |        | YES    |
    | [[a]]       |                   |       |        |        |
    | [[lr().a]]  |                   |       |        |        |
	When I press "F5"
	And the Debug Input window is opened
	Then the variables appear as
	 | Variable | Note | Input | Output | IsUsed |
	 | [[mr()]] |      |       |        | YES    |
	 | [[Var]]  |      | YES   |        | YES    |
	 
@ignore
Scenario Outline: Ensure shorcut keys work
	Given I have variables as
    | Variable | Note | Input | Output | IsUsed |
    | [[var]]  |      |       | YES    | YES    |
	And I press "<Keys>"
	Then cursor focus is "<Focus>"
Examples:
	| Keys  | Focus          |
	| Enter | New blank line |
	| Tab   | Input Checkbox |
	
@ignore
Scenario: versioning and mapping
	Given I have variables as
	 | Variable | Note | Input | Output | IsUsed |
	 | [[a]]    |      |       | YES    |        |
	 | [[b]]    |      | YES   |        |        |
	When I save workflow as "test"
	And create variable "[[c]]" equals "" as ""
	And I save "Mapping"
	And "Mapping" is visible in the explorer
	When I right click "Mapping" and "Show Version History"
	Then version history is visible in the explorer
	And I open "v1" of "Mapping"
	Then the variables appear as
   | Variable | Note | Input | Output | IsUsed |
   | [[a]]    |      |       | YES    |        |
   | [[b]]    |      | YES   |        |        |