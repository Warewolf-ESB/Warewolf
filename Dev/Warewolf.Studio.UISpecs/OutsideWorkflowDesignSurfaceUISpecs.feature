Feature: OutsideWorkflowDesignSurfaceUISpecs
	In order to find studio UI bugs
	As a user
	I want to have a good UX

@ignore
@NeedsBlankWorkflow
Scenario: Variable List Exists
	Then I 'Assert_VariableList_Exists'
	Then I 'Assert_VariableList_DeleteButton_Exists'
	Then I 'Assert_VariableList_Recordset_ChildTextBox_Exists'
	Then I 'Assert_VariableList_RecordsetInput_CheckBox_Exists'
	Then I 'Assert_VariableList_RecordsetInput_ChildCheckBox_Exists'
	Then I 'Assert_VariableList_RecordsetItem_Exists'
	Then I 'Assert_VariableList_RecordsetOutput_CheckBox_Exists'
	Then I 'Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists'
	Then I 'Assert_VariableList_RecordsetTextBox_Exists'
	Then I 'Assert_VariableList_SortButton_Exists'
	Then I 'Assert_VariableList_VariableInput_CheckBox_Exists'
	Then I 'Assert_VariableList_VariableItem_Exists'
	Then I 'Assert_VariableList_VariableOutput_CheckBox_Exists'
	Then I 'Assert_VariableList_VariableTextBox_Exists'
	Then I 'Assert_VariableList_DataInputTree_Exists'
	
Scenario: Toolbox Exists
	Then I 'Assert_Toolbox_FilterTextbox_Exists'
	Then I 'Assert_Toolbox_RefreshButton_Exists'

Scenario: Explorer Exists
	Then I 'Assert_Explorer_Exists'
	Then I 'Assert_Explorer_ServerName_Exists'

Scenario: Connect Control Exists
	Then I 'Assert_Connect_Control_Exists_InExplorer'
	Then I 'Assert_Connect_ConnectControl_Button_Exists_InExplorer'
	Then I 'Assert_Explorer_Edit_Connect_Control_Button_Exists'

@ignore	
Scenario: Settings Ribbon Button
	Given I 'Assert_Settings_Button_Exists_OnDesignSurface'
	When I 'Click_Settings_Ribbon_Button'
	#Then 'Assert_Settings_Tab_Exists'
	
#@NeedsNewSettingsTab
#Scenario: Click Close Settings Tab Button
	#Given I 'Assert_Close_Settings_Tab_Button_Exists'
	#When I 'Click_Close_Settings_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_Messagebox_No_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: Scheduler Ribbon Button
	Given I 'Assert_Scheduler_Button_Exists_OnDesignSurface'
	When I 'Click_Scheduler_Ribbon_Button'
	#Then 'Assert_Scheduler_Tab_Exists'
		
#@NeedsNewSchedulerTab
#Scenario: Click Close Scheduler Tab Button
	#Given I 'Assert_Close_Scheduler_Tab_Button_Exists'
	#When I 'Click_Close_Scheduler_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_Messagebox_No_Button_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'
	
@ignore
Scenario: Deploy Ribbon Button
	Given I 'Assert_Deploy_Ribbon_Button_Exists'
	When I 'Click_Deploy_Ribbon_Button'
	Then I 'Assert_Source_Server_Name_Exists'
	Then I 'Assert_Refresh_Button_Source_Server_Exists'
	Then I 'Assert_Filter_Source_Server_Exists'
	Then I 'Assert_Connect_Control_DestinationServer_Exists'
	Then I 'Assert_Override_Count_Exists'
	Then I 'Assert_NewResource_Count_Exists'
	Then I 'Assert_Source_Server_Edit_Exists'
	Then I 'Assert_Connect_Button_Source_Server_Exists'
	Then I 'Assert_Edit_Button_Destination_Server_Exists'
	Then I 'Assert_Connect_button_Destination_Server_Exists'
	Then I 'Assert_Connect_Control_SourceServer_Exists'
	Then I 'Assert_ShowDependencies_Button_DestinationServer_Exists'
	Then I 'Assert_ServiceLabel_DestinationServer_Exists'
	Then I 'Assert_ServicesCount_Label_Exists'
	Then I 'Assert_SourceLabel_DestinationServer_Exists'
	Then I 'Assert_SourceCount_DestinationServer_Exists'
	Then I 'Assert_NewResource_Label_Exists'
	Then I 'Assert_Override_Label_DestinationServer_Exists'
	Then I 'Assert_DeployButton_DestinationServer_Exists'
	Then I 'Assert_SuccessMessage_Label_Exists'

@ignore	
Scenario: Knowledge Base Ribbon Button
	Given I 'Assert_Knowledge_Base_Exists_OnDesignSurface'
	When I 'Click_Knowledge_Ribbon_Button'
	#Then 'Assert_Knowledge_Base_Tab_Exists'
	
#@NeedsKnowledgeBaseTab
#Scenario: Click Close Knowledgebase Tab Button
	#Given I 'Assert_Close_Knowledgebase_Tab_Button_Exists'
	#When I 'Click_Close_Knowledgebase_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_MessageBox_No_Button_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'
	
Scenario: Lock Menu Ribbon Button
	Given I 'Assert_Lock_Button_Exists_OnDesignSurface'
	When I 'Click_Unlock_Ribbon_Button'
	#Then 'Assert_Lock_Ribbon_Button_Exists'

@ignore	
Scenario: New Database Connector Ribbon Button
	Given I 'Assert_Database_Source_Exists'
	When I 'Click_NewDatabaseSource_Ribbon_Button'
	#Then 'Assert_NewDatabaseSource_Exists'
	
#@NeedsNewDatabaseConnectorTab
#Scenario: Click Close Database Connector Tab Button
	#Given I 'Assert_Close_Database_Connector_Tab_Button_Exists'
	#When I 'Click_Close_Database_Connector_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_Messagebox_No_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: New Plugin Connector Ribbon Button
	Given I 'Assert_Plugin_Source_Exists'
	When I 'Click_NewPluginSource_Ribbon_Button'
	#Then 'Assert_NewPluginSource_Exists'
	
#@NeedsNewPluginConnectorTab
#Scenario: Click Close Web Connector Tab Button
	#Given I 'Assert_Close_Plugin_Connector_Tab_Button_Exists'
	#When I 'Click_Close_Plugin_Connector_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_Messagebox_No_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: New Web Connector Ribbon Button
	Given I 'Assert_Web_Source_Exists'
	When I 'Click_NewWebSource_Ribbon_Button'
	#Then 'Assert_New_Web_Connector_Tab_Exists'
	
#@NeedsNewWebConnectorTab
#Scenario: Click Close Web Connector Tab Button
	#Given I 'Assert_Close_Web_Connector_Tab_Button_Exists'
	#When I 'Click_Close_Web_Connector_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'

#Scenario: Click MessageBox No
	#Given I 'Assert_Messagebox_No_Exists'
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'
	
@ignore
Scenario: Scheduler Ribbon Button
	Given I 'Assert_Scheduler_Button_Exists_OnDesignSurface'
	When I 'Click_Scheduler_Ribbon_Button'
	Then I 'Assert_Scheduler_CreateNewTask_Exists'
	
#@NeedsSchedulerCreateNewTaskRibbonButton
#Scenario: Click Scheduler Create New Task Ribbon Button
	#Given I 'Assert_Scheduler_Create_New_Task_Ribbon_Button'
	When I 'Click_Scheduler_Create_New_Task_Ribbon_Button'
	Then I 'Assert_Scheduler_DisabledRadioButton_Exists'
	
#@NeedsSchedulerTabOpen
#Scenario: Click Scheduler Create New Task Ribbon Button
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_Disable_Task_Radio_Button'
	Then I 'Assert_Scheduler_EditTrigger_Exists'
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler EditTrigger Button
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_EditTrigger_Button'
	Then I 'Assert_Scheduler_EnabledRadioButton_Exists'
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Enable Task Radio Button
	#Given I 'Assert_Scheduler_Enable_Task_Radio_Button_Exists'
	When I 'Click_Scheduler_Enable_Task_Radio_Button'
	Then I 'Assert_Scheduler_ErrorMessage_Exists'
	Then I 'Assert_Scheduler_HistoryInput_Exists'
	Then I 'Assert_Scheduler_HistoryLabel_Exists'
	Then I 'Assert_Scheduler_HistoryTable_Exists'
	Then I 'Assert_Scheduler_NameInput_Exists'
	Then I 'Assert_Scheduler_NameLabel_Exists'
	Then I 'Assert_Scheduler_PasswordInput_Exists'
	Then I 'Assert_Scheduler_PasswordLabel_Exists'
	Then I 'Assert_Scheduler_ResourcePicker_Exists'
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler ResourcePicker
	#Given The test is initialized 
	When I 'Click_Scheduler_ResourcePicker'
	Then I 'Assert_Scheduler_RunTask_Checkbox_Exists'
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Run Task
	#Given I 'Assert_Scheduler_RunTask_Checkbox_Exists'
	When I 'Click_Scheduler_RunTask'
	Then I 'Assert_Scheduler_DeleteButton_Exists'
	
#@NeedsSchedulerTaskOpen
#Scenario: Click Scheduler Delete Task
	#Given The test is initialized
	When I 'Click_Scheduler_Delete_Task'
	Then I 'Assert_Scheduler_Status_RadioButton_Exists'
	Then I 'Assert_Scheduler_StatusLabe_Exists'
	Then I 'Assert_Scheduler_TriggerLabel_Exists'
	Then I 'Assert_Scheduler_TriggerValue_Exists'
	Then I 'Assert_Scheduler_UserAccountLabel_Exists'
	Then I 'Assert_Scheduler_UsernameInput_Exists'
	Then I 'Assert_Scheduler_Usernamelabel_Exists'
	Then I 'Assert_Scheduler_WorkflowInput_Exists'
	Then I 'Assert_Scheduler_WorkflowLabel_Exists'
	
@ignore
Scenario: Settings Ribbon Button
	Given I 'Assert_Settings_Button_Exists_OnDesignSurface'
	When I 'Click_Settings_Ribbon_Button'
	Then I 'Assert_Settings_LoggingTab_Exists'
	Then I 'Assert_Settings_ResourcePermissions_Exists'
	Then I 'Assert_Settings_SecurityTab_Exists'
	Then I 'Assert_Settings_ServerPermissions_Exists'
	
@ignore
@NeedsBlankWorkflow
Scenario: Debug Ribbon Button
	When I 'Click_Debug_Ribbon_Button'
	Then I 'Assert_DebugInput_Window_Exists'
	Then I 'Assert_DebugInput_CancelButton_Exists'
	Then I 'Assert_DebugInput_RememberCheckbox_Exists'
	Then I 'Assert_DebugInput_ViewInBrowser_Button_Exists'
	Then I 'Assert_DebugInput_DebugButton_Exists'
	Then I 'Assert_DebugInput_InputData_Window_Exists'
	Then I 'Assert_DebugInput_InputData_Field_Exists'
	Then I 'Assert_DebugInput_Xml_Tab_Exists'
	Then I 'Assert_DebugInput_Xml_Window_Exists'
	Then I 'Assert_DebugInput_Json_Tab_Exists'
	Then I 'Assert_DebugInput_Json_Window_Exists'
		
Scenario: Save Dialog
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	
#@NeedsSaveDialog
#Scenario: Click Save Ribbon Button
	Given I 'Assert_Save_Button_Exists_OnDesignSurface'
	When I 'Click_Save_Ribbon_Button'
	Then I 'Assert_SaveDialog_Exists'
	Then I 'Assert_SaveDialog_CancelButton_Exists'
	Then I 'Assert_SaveDialog_ErrorLabel_Exists'
	Then I 'Assert_SaveDialog_ExplorerTree_Exists'
	Then I 'Assert_SaveDialog_ExplorerTreeItem_Exists'
	Then I 'Assert_SaveDialog_ExplorerView_Exists'
	Then I 'Assert_SaveDialog_FilterTextbox_Exists'
	Then I 'Assert_SaveDialog_NameLabel_Exists'
	Then I 'Assert_SaveDialog_RefreshButton_Exists'
	Then I 'Assert_SaveDialog_SaveButton_Exists'
	Then I 'Assert_SaveDialog_ServiceName_Textbox_Exists'
	Then I 'Assert_SaveDialog_WorkspaceName_Exists'
	
#@NeedsSaveDialog
#Scenario: Click Save Dialog Cancel Button
	#Given I 'Assert_Save_Dialog_Cancel_Button_Exists'
	When I 'Click_SaveDialog_CancelButton'
	Then I 'Assert_StartNode_Exists'
	
@ignore
Scenario: Debug Output Window
	Given I 'DebugOutput_Exists'
	Given I 'DebugOutput_ExpandCollapseButton_Exists'
	Given I 'DebugOutput_FilterTextbox_Exists'
	Given I 'DebugOutput_ResultsTree_Exists'
	Given I 'DebugOutput_SettingsButton_Exists'
	When I 'Click_ExpandAndStepIn_NestedWorkflow'
	#Then I 'Assert_DebugOutput_Cell_Exists'
	
#@NeedsDebugOutput
#Scenario: Click Cell Highlights Workflow OnDesignSurface
	#Given I 'Assert_DebugOutput_Cell_Exists'
	When I 'Click_Cell_Highlights_Workflow_OnDesignSurface'
	#Then 'Assert_Nested_Workflow_Name_Exists'
	
#@NeedsNestedWorkflowDebugOutput
#Scenario: Click Nested Workflow Name Opens Nested Workflow Edit Tab
	#Given I 'Assert_Nested_Workflow_Name_Exists'
	When I 'Click_Nested_Workflow_Name'
	#Then I 'Assert_NestedWorkflow_Tab_Exists'
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Database Connector Onto Design Surface Should not be droppable
	#Given 'Some database connector' exists in the explorer tree
	When I 'Drag_Database_Connector_Onto_DesignSurface'
	#Then I 'Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist'
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Plugin Connector Onto Design Surface Should not be droppable
	#Given 'Some plugin connector' exists in the explorer tree
	When I 'Drag_Plugin_Connector_Onto_DesignSurface'
	#Then I 'Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist'
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Web Connector Onto Design Surface Should not be droppable
	#Given 'Some web connector' exists in the explorer tree
	When I 'Drag_Web_Connector_Onto_DesignSurface'
	#Then I 'Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist'
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Sharepoint Connector Onto Design Surface Should not be droppable
	#Given 'Some Sharepoint Source' exists in the explorer tree
	When I 'Drag_Sharepoint_Source_Onto_DesignSurface'
	#Then I 'Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist'
	
@ignore
@NeedsBlankWorkflow
Scenario: Dragging Server Source Onto Design Surface Should not be droppable
	#Given 'Some Server Source' exists in the explorer tree
	When I 'Drag_Server_Source_Onto_DesignSurface'
	#Then I 'Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist'

#@NeedsDebugInputDialog
#Scenario: Click Debug Input Dialog Cancel
	Given I 'Assert_Debug_Input_Cancel_Button_Exists'
	When I 'Click_Debug_Input_Dialog_Cancel'
	Then I 'Assert_DebugInput_Window_Does_Not_Exist'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Admin ServerPermissions
	#Given I 'Assert_Settings_Tab_Exists'
	When I 'Click_Settings_Admin_ServerPermissions'
	#Then I 'Assert_Settings_Admin_ServerPermissions_Checkbox_Is_Checked'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ResourcePermissions
	#Given The test is initialized
	When I 'Click_Settings_Contribute_ResourcePermissions'
	#Then I 'Assert_Settings_Contribute_ResourcePermissions_Checkbox_Is_Checked'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ServerPermissions
	#Given The test is initialized
	When I 'Click_Settings_Contribute_ServerPermissions'
	#Then I 'Assert_Settings_Contribute_ServerPermissions_Checkbox_Is_Checked'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings Contribute ResourcePermissions
	#Given The test is initialized
	When I 'Click_Settings_Execute_ResourcePermissions'
	#Then I 'Assert_Settings_Execute_ResourcePermissions_Checkbox_Is_Checked'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings ResourcePermissions ResourcePicker
	#Given The test is initialized
	When I 'Click_Settings_ResourcePermissions_ResourcePicker'
	#Then I 'Assert_Settings_ResourcePermissions_ResourcePicker_Checkbox_Is_Checked'
	
#@NeedsSettingsTabOpen
#Scenario: Click Settings View ResourcePermissions
	#Given The test is initialized
	When I 'Click_Settings_View_ResourcePermissions'
	#Then I 'Assert_Settings_View_ResourcePermissions_Checkbox_Is_Checked'
	
@NeedsBlankWorkflow
Scenario: Right Click On Design Surface
	When I 'Open_Context_Menu_OnDesignSurface'
	Then I 'Assert_Generic_Context_Menu_Exists'
	
@NeedsBlankWorkflow
Scenario: Context Menu on New Workflow Tab
	When I 'RightClick_New_Workflow_Tab'
	Then I 'Assert_New_Workflow_Context_Menu_Exists'
	
@ignore
Scenario: Pin and unpin Explorer
	Given I 'Click_Toggle_Unpin_Explorer'
	Then I 'Click_Toggle_Pin_Explorer'
	
@ignore
Scenario: Pin and unpin Help
	Given I 'Click_Toggle_Unpin_Documentor'
	Then I 'Click_Toggle_Pin_Documentor'
	
@ignore
Scenario: Pin and unpin Toolbox
	Given I 'Click_Toggle_Unpiin_Toolbox'
	Then I 'Click_Toggle_Pin_Toolbox'
	
@ignore
Scenario: Pin and unpin Debug Output
	Given I 'Click_Toggle_Unpin_DebugOutput'
	Then I 'Click_Toggle_Pin_DebugOutput'
	
@ignore
Scenario: Pin and unpin variable list
	Given I 'Click_Toggle_Unpin_VariableList'
	Then I 'Click_Toggle_Pin_VariableList'