Feature: OutsideWorkflowDesignSurfaceUISpecs
	In order to find studio UI bugs
	As a user
	I want to have a good UX

@ignore
@NeedsBlankWorkflow
Scenario: Variable List
	Given I 'Assert_VariableList_Exists'
	Given I 'Assert_VariableList_DeleteButton_Exists'
	Given I 'Assert_VariableList_Recordset_ChildTextBox_Exists'
	Given I 'Assert_VariableList_RecordsetInput_CheckBox_Exists'
	Given I 'Assert_VariableList_RecordsetInput_ChildCheckBox_Exists'
	Given I 'Assert_VariableList_RecordsetItem_Exists'
	Given I 'Assert_VariableList_RecordsetOutput_CheckBox_Exists'
	Given I 'Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists'
	Given I 'Assert_VariableList_RecordsetTextBox_Exists'
	Given I 'Assert_VariableList_SortButton_Exists'
	Given I 'Assert_VariableList_VariableInput_CheckBox_Exists'
	Given I 'Assert_VariableList_VariableItem_Exists'
	Given I 'Assert_VariableList_VariableOutput_CheckBox_Exists'
	Given I 'Assert_VariableList_VariableTextBox_Exists'
	Given I 'Assert_VariableList_DataInputTree_Exists'
	
Scenario: Toolbox
	Given I 'Assert_Toolbox_FilterTextbox_Exists'
	Given I 'Assert_Toolbox_RefreshButton_Exists'

Scenario: Connect Control Exists
	Given I 'Assert_Connect_Control_Exists_InExplorer'
	Given I 'Assert_Connect_ConnectControl_Button_Exists_InExplorer'
	Given I 'Assert_Explorer_Edit_Connect_Control_Button_Exists'

@ignore	
Scenario: Settings Ribbon Button
	Given I 'Assert_Settings_Button_Exists_OnDesignSurface'
	When I 'Click_Settings_Ribbon_Button'
	#Then 'Assert_Settings_Tab_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: Scheduler Ribbon Button
	Given I 'Assert_Scheduler_Button_Exists_OnDesignSurface'
	When I 'Click_Scheduler_Ribbon_Button'
	#Then 'Assert_Scheduler_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'
	
@ignore
Scenario: Deploy Ribbon Button
	Given I 'Assert_Deploy_Ribbon_Button_Exists'
	When I 'Click_Deploy_Ribbon_Button'
	#Then 'Assert_Deploy_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: Knowledge Base Ribbon Button
	Given I 'Assert_Knowledge_Base_Exists_OnDesignSurface'
	When I 'Click_Knowledge_Ribbon_Button'
	#Then 'Assert_Knowledge_Base_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'
	
Scenario: Lock Menu Ribbon Button
	Given I 'Assert_Lock_Button_Exists_OnDesignSurface'
	When I 'Click_Unlock_Ribbon_Button'
	#Then 'Assert_Lock_Ribbon_Button_Exists'
	#Ashley TODO: The test should end here.

@ignore	
Scenario: New Database Connector Ribbon Button
	Given I 'Assert_Database_Source_Exists'
	When I 'Click_NewDatabaseSource_Ribbon_Button'
	#Then 'Assert_NewDatabaseSource_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: New Plugin Connector Ribbon Button
	Given I 'Assert_Plugin_Source_Exists'
	When I 'Click_NewPluginSource_Ribbon_Button'
	#Then 'Assert_NewPluginSource_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

@ignore	
Scenario: New Web Connector Ribbon Button
	Given I 'Assert_Web_Source_Exists'
	When I 'Click_NewWebSource_Ribbon_Button'
	#Then 'Assert_NewWebSource_Exists'
	#Ashley TODO: The test should end here.
	
	Given I 'Assert_Close_Tab_Button_Exists'
	When I 'Click_Close_Tab_Button'
	Then I 'Assert_MessageBox_No_Button_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_MessageBox_No'
	#Then I 'Assert_Tab_Closed'

	
Scenario: Save Dialog
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Cell_Highlights_Workflow_OnDesignSurface'
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Nested_Workflow_Name'
	#Then 'Some Post-assert'
	
@ignore
Scenario: Connectors and Sources cannot be dragged onto the design surface
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Database_Connector_Onto_DesignSurface'
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Plugin_Connector_Onto_DesignSurface'
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Web_Connector_Onto_DesignSurface'
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Sharepoint_Source_Onto_DesignSurface'
	#Then 'Some Post-assert'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Server_Source_Onto_DesignSurface'
	#Then 'Some Post-assert'
	
@ignore
Scenario: Scheduler
	Given I 'Assert_Scheduler_Button_Exists_OnDesignSurface'
	When I 'Click_Scheduler_Ribbon_Button'
	Then I 'Assert_Scheduler_CreateNewTask_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_Create_New_Task_Ribbon_Button'
	Then I 'Assert_Scheduler_DisabledRadioButton_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_Disable_Task_Radio_Button'
	Then I 'Assert_Scheduler_EditTrigger_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_EditTrigger_Button'
	Then I 'Assert_Scheduler_EnabledRadioButton_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_ResourcePicker'
	Then I 'Assert_Scheduler_RunTask_Checkbox_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Scheduler_RunTask'
	Then I 'Assert_Scheduler_DeleteButton_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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
Scenario: Settings
	Given I 'Assert_Settings_Button_Exists_OnDesignSurface'
	When I 'Click_Settings_Ribbon_Button'
	Then I 'Assert_Settings_LoggingTab_Exists'
	Then I 'Assert_Settings_ResourcePermissions_Exists'
	Then I 'Assert_Settings_SecurityTab_Exists'
	Then I 'Assert_Settings_ServerPermissions_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_Admin_ServerPermissions'
	#Then 'Assert_Something_in_Settings_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_Contribute_ResourcePermissions'
	#Then 'Assert_Something_in_Settings_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_Contribute_ServerPermissions'
	#Then 'Assert_Something_in_Settings_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_Execute_ResourcePermissions'
	#Then 'Assert_Something_in_Settings_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_ResourcePermissions_ResourcePicker'
	#Then 'Assert_Something_in_Settings_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Click_Settings_View_ResourcePermissions'
	#Then 'Assert_Something_in_Settings_Exists'

Scenario: Context Menu on design surface
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	Then I 'Open_Context_Menu_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Context Menu on New Workflow Tab
	When I 'RightClick_New_Workflow_Tab'
	Then I 'Assert_New_Workflow_Context_Menu_Exists'
	
@ignore
Scenario: Debug Input window
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
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
	
@ignore
Scenario: Deploy
	Given I 'Assert_Deploy_Button_Exists_OnDesignSurface'
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


Scenario: Explorer
	Given I 'Assert_Explorer_Exists'
	Then I 'Assert_Explorer_ServerName_Exists'
	Then I 'Right_Click_Context_Menu_InExplorer'


Scenario: Pin and unpin Explorer
	Given I 'Click_Toggle_Unpin_Explorer'
	Then I 'Click_Toggle_Pin_Explorer'

Scenario: Pin and unpin Help
	Given I 'Click_Toggle_Unpin_Documentor'
	Then I 'Click_Toggle_Pin_Documentor'


Scenario: Pin and unpin Toolbox
	Given I 'Click_Toggle_Unpiin_Toolbox'
	Then I 'Click_Toggle_Pin_Toolbox'
	
@ignore
Scenario: Pin and unpin Debug Output
	Given I 'Click_Toggle_Unpin_DebugOutput'
	Then I 'Click_Toggle_Pin_DebugOutput'

Scenario: Pin and unpin variable list
	Given I 'Click_Toggle_Unpin_VariableList'
	Then I 'Click_Toggle_Pin_VariableList'