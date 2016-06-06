Feature: StudioUISpecs
	In order to create good workflows
	As a workflow designer
	I want to see the layout and function of workflows
	
@NeedsBlankWorkflow
Scenario: Drag toolbox decision onto a new workflow opens decision dialog
	When I "Drag_Toolbox_Decision_Onto_DesignSurface"
	Then I "Assert_Decision_Dialog_Done_Button_Exists"

#@NeedsDecisionDialog
#Scenario: Clicking Decision Dialog Done Button Creates a Desision on the Design Surface
	When I "Click_Decision_Dialog_Done_Button"
	Then I "Assert_Decision_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox switch onto a new workflow opens switch dialog
	When I "Drag_Toolbox_Switch_Onto_DesignSurface"
	Then I "Assert_Decision_Dialog_Done_Button_Exists"

#@NeedsSwitchDialog
#Scenario: Clicking Switch Dialog Done Button Creates a Switch on the Design Surface
	When I "Click_Switch_Dialog_Done_Button"
	Then I "Assert_Switch_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox sequence onto a new workflow creates sequence on the design surface
	When I "Drag_Toolbox_Sequence_Onto_DesignSurface"
	Then I "Assert_Sequence_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox multiassign onto a new workflow creates an assign tool with small view on the design surface
	When I "Drag_Toolbox_MultiAssign_Onto_DesignSurface"
	Then I "Assert_MultiAssign_Exists_OnDesignSurface"

#@NeedsMultiAssignSmallViewToolOnTheDesignSurface
#Scenario: Double Clicking Multi Assign Tool Small View on the Design Surface Opens Large View
	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
	When I "Open_Assign_Tool_Large_View"
	Then I "Assert_Assign_Large_View_Exists_OnDesignSurface"

#@NeedsMultiAssignLargeViewOnTheDesignSurface
#Scenario: Click Assign Tool QVI Button Opens Qvi
	When I "Open_Assign_Tool_Qvi_Large_View"
	Then I "Assert_Assign_QVI_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox base conversion onto a new workflow creates base conversion tool with small view on the design surface
	When I "Drag_Toolbox_Base_Conversion_Onto_DesignSurface"
	Then I "Assert_Base_Conversion_Exists_OnDesignSurface"

#@NeedsBaseConversionSmallViewOnTheDesignSurface
#Scenario: Double Clicking Base Conversion Tool Small View on the Design Surface Opens Large View
	When I "Open_Base_Conversion_Tool_Qvi_Large_View"
	Then I "Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DotNet Dll Tool onto a new workflow creates base conversion tool with small view on the design surface
	When I "Drag_DotNet_DLL_Connector_Onto_DesignSurface"
	Then I "Assert_DotNet_DLL_Connector_Exists_OnDesignSurface"

#@NeedsDotNetDllToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking DotNet Dll Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_DotNet_DLL_Connector_Tool_Small_View"
	Then I "Assert_DotNet_DLL_Connector_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox MySql Tool onto a new workflow creates MySql tool with large view on the design surface
	When I "Drag_Toolbox_MySql_Database_Onto_DesignSurface"
	Then I "Assert_Mysql_Database_Large_View_Exists_OnDesignSurface"

#@NeedsMySqlToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking MySql Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_MySql_Database_Tool_Small_View"
	Then I "Assert_Mysql_Database_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sql Server Tool onto a new workflow creates Sql Server tool with large view on the design surface
	When I "Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface"
	Then I "Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface"

#@NeedsSQLServerToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking SQL Server Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_Sql_Server_Tool_small_View"
	Then I "Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Get Web Request Tool onto a new workflow creates Get Web Request tool with large view on the design surface
	When I "Drag_GetWeb_RequestTool_Onto_DesignSurface"
	Then I "Assert_GetWeb_RequestTool_small_View_Exists_OnDesignSurface"

#@NeedsWebRequestSmallViewOnTheDesignSurface
#Scenario: Double Clicking Web Request Tool Small View on the Design Surface Opens Large View
	When I "Open_WebRequest_LargeView"
	Then I "Assert_GetWeb_RequestTool_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Post Web Request Tool onto a new workflow creates Post Web Request tool with large view on the design surface
	When I "Drag_PostWeb_RequestTool_Onto_DesignSurface"
	Then I "Assert_PostWeb_RequestTool_Large_View_Exists_OnDesignSurface"

#@NeedsPostWebRequestToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking Post Web Request Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_PostWeb_RequestTool_small_View"
	Then I "Assert_PostWeb_RequestTool_Small_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Case Conversion onto a new workflow creates Case Conversion tool with small view on the design surface
	When I "Drag_Toolbox_Case_Conversion_Onto_DesignSurface"
	Then I "Assert_Case_Conversion_Exists_OnDesignSurface"

#@NeedsPostWebRequestToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Post Web Request Tool Small View on the Design Surface Opens Large View
	When I "Open_Case_Conversion_Tool_Qvi_Large_View"
	Then I "Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Data Merge onto a new workflow creates Data Merge tool with small view on the design surface
	When I "Drag_Toolbox_Data_Merge_Onto_DesignSurface"
	Then I "Assert_Data_Merge_Exists_OnDesignSurface"

#@NeedsDataMergeToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Data Merge Tool Small View on the Design Surface Opens Large View
	When I "Open_Data_Merge_Large_View"
	Then I "Assert_Data_Merge_Large_View_Exists_OnDesignSurface"

#@NeedsDataMergeLargeViewOnTheDesignSurface
#Scenario: Click Data Merge Tool QVI Button Opens Qvi
	When I "Open_Data_Merge_Tool_Qvi_Large_View"
	Then I "Assert_Data_Merge_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Data_Split onto a new workflow
	When I "Drag_Toolbox_Data_Split_Onto_DesignSurface"
	Then I "Assert_Data_Split_Exists_OnDesignSurface"

#@NeedsDataSplitToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Data Split Tool Small View on the Design Surface Opens Large View
	When I "Open_Data_Split_Large_View"
	Then I "Assert_Data_Split_Large_View_Exists_OnDesignSurface"

#@NeedsDataSplitLargeViewOnTheDesignSurface
#Scenario: Click Data Split Tool QVI Button Opens Qvi
	When I "Open_Data_Split_Tool_Qvi_Large_View"
	Then I "Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Find_Index onto a new workflow
	When I "Drag_Toolbox_Find_Index_Onto_DesignSurface"
	Then I "Assert_Find_Index_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Replace onto a new workflow
	When I "Drag_Toolbox_Replace_Onto_DesignSurface"
	Then I "Assert_Replace_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Copy_Path onto a new workflow
	When I "Drag_Toolbox_Copy_Onto_DesignSurface"
	Then I "Assert_Copy_Exists_OnDesignSurface"

#@NeedsCopy_PathToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Copy_Path Tool Small View on the Design Surface Opens Large View
	When I "Open_Copy_Tool_Large_View"
	Then I "Assert_Copy_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Create_Path onto a new workflow
	When I "Drag_Toolbox_Create_Onto_DesignSurface"
	Then I "Assert_Create_Exists_OnDesignSurface"

#@NeedsCreate_PathToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Create_Path Tool Small View on the Design Surface Opens Large View
	When I "Open_Create_Tool_Large_View"
	Then I "Assert_Create_Path_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Delete_Path onto a new workflow
	When I "Drag_Toolbox_Delete_Onto_DesignSurface"
	Then I "Assert_Delete_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Read_File onto a new workflow
	When I "Drag_Toolbox_Read_File_Onto_DesignSurface"
	Then I "Assert_Read_File_Exists_OnDesignSurface"

#@NeedsRead_FileToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Read_File Tool Small View on the Design Surface Opens Large View
	When I "Open_Read_File_Tool_Large_View"
	Then I "Assert_Read_File_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Read_Folder onto a new workflow
	When I "Drag_Toolbox_Read_Folder_Onto_DesignSurface"
	Then I "Assert_Read_Folder_Exists_OnDesignSurface"

#@NeedsRead_FolderToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Read_Folder Tool Small View on the Design Surface Opens Large View
	When I "Open_Read_Folder_Tool_Large_View"
	Then I "Assert_Read_Folder_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Rename_Folder onto a new workflow
	When I "Drag_Toolbox_Rename_Onto_DesignSurface"
	Then I "Assert_Rename_Exists_OnDesignSurface"

#@NeedsRename_FolderToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Rename_Folder Tool Small View on the Design Surface Opens Large View
	When I "Open_Rename_Tool_Large_View"
	Then I "Assert_Rename_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Unzip onto a new workflow
	When I "Drag_Toolbox_Unzip_Onto_DesignSurface"
	Then I "Assert_Unzip_Exists_OnDesignSurface"

#@NeedsUnzipToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Unzip Tool Small View on the Design Surface Opens Large View
	When I "Open_Unzip_Tool_Large_View"
	Then I "Assert_Unzip_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Write_File onto a new workflow
	When I "Drag_Toolbox_Write_File_Onto_DesignSurface"
	Then I "Assert_Write_File_Exists_OnDesignSurface"

#@NeedsWrite_FileToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Write_File Tool Small View on the Design Surface Opens Large View
	When I "Open_Write_File_Tool_Large_View"
	Then I "Assert_Write_File_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Zip onto a new workflow
	When I "Drag_Toolbox_Zip_Onto_DesignSurface"
	Then I "Assert_Zip_Exists_OnDesignSurface"

#@NeedsZipToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Zip Tool Small View on the Design Surface Opens Large View
	When I "Open_Zip_Tool_Large_View"
	Then I "Assert_Zip_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox For_Each onto a new workflow
	When I "Drag_Toolbox_For_Each_Onto_DesignSurface"
	Then I "Assert_For_Each_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Format_Number onto a new workflow
	When I "Drag_Toolbox_Format_Number_Onto_DesignSurface"
	Then I "Assert_Format_Number_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Length onto a new workflow
	When I "Drag_Toolbox_Length_Onto_DesignSurface"
	Then I "Assert_Length_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Random onto a new workflow
	When I "Drag_Toolbox_Random_Onto_DesignSurface"
	Then I "Assert_Random_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Script onto a new workflow
	When I "Drag_Toolbox_Script_Onto_DesignSurface"
	Then I "Assert_Script_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Create onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Create_Exists_OnDesignSurface"

#@NeedsSharepoint_CreateToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Create Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Create_Tool_Large_View"
	Then I "Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Delete onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Delete_Exists_OnDesignSurface"

#@NeedsSharepoint_DeleteToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Delete Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Delete_Tool_Large_View"
	Then I "Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Read onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Read_Exists_OnDesignSurface"

#@NeedsSharepoint_ReadToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Read Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Read_Tool_Large_View"
	Then I "Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Update onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Update_Exists_OnDesignSurface"

#@NeedsSharepoint_UpdateToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Update Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Update_Tool_Large_View"
	Then I "Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sort_Record onto a new workflow
	When I "Drag_Toolbox_Sort_Record_Onto_DesignSurface"
	Then I "Assert_Sort_Records_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox SQL_Bulk_Insert onto a new workflow
	When I "Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface"
	Then I "Assert_Sql_Bulk_insert_Exists_OnDesignSurface"

#@NeedsSQL_Bulk_InsertToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking SQL_Bulk_Insert Tool Small View on the Design Surface Opens Large View
	When I "Open_SQL_Bulk_Insert_Tool_Large_View"
	Then I "Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface"

#@NeedsSQLBulkInsertLargeViewOnTheDesignSurface
#Scenario: Click SQL Bulk Insert Tool QVI Button Opens Qvi
	When I "Open_SQL_Bulk_Insert_Tool_Qvi_Large_View"
	Then I "Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox System_Information onto a new workflow
	When I "Drag_Toolbox_System_Information_Onto_DesignSurface"
	Then I "Assert_System_information_Exists_OnDesignSurface"

#@NeedsSystem_InformationToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking System_Information Tool Small View on the Design Surface Opens Large View
	When I "Open_System_Information_Tool_Qvi_Large_View"
	Then I "Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Unique_Records onto a new workflow
	When I "Drag_Toolbox_Unique_Records_Onto_DesignSurface"
	Then I "Assert_Unique_Records_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Web_Request onto a new workflow
	When I "Drag_Toolbox_Web_Request_Onto_DesignSurface"
	Then I "Assert_Web_Request_Exists_OnDesignSurface"

#@NeedsWeb_RequestToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Web_Request Tool Small View on the Design Surface Opens Large View
	When I "Open_Web_Request_Tool_Large_View"
	Then I "Assert_Web_Request_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox XPath onto a new workflow
	When I "Drag_Toolbox_XPath_Onto_DesignSurface"
	Then I "Assert_XPath_Exists_OnDesignSurface"

#@NeedsXPathToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking XPath Tool Small View on the Design Surface Opens Large View
	When I "Open_Xpath_Tool_Large_View"
	Then I "Assert_Xpath_Large_View_Exists_OnDesignSurface"

#@NeedsXPathLargeViewOnTheDesignSurface
#Scenario: Click XPath Tool QVI Button Opens Qvi
	When I "Open_Xpath_Tool_Qvi_Large_View"
	Then I "Assert_Xpath_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Calculate onto a new workflow
	When I "Drag_Toolbox_Calculate_Onto_DesignSurface"
	Then I "Assert_Calculate_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox CMD_Line onto a new workflow
	When I "Drag_Toolbox_CMD_Line_Onto_DesignSurface"
	Then I "Assert_CMD_Line_Exists_OnDesignSurface"

#@NeedsCMDLineToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking CMD Line Tool Small View on the Design Surface Opens Large View
	When I "Open_CMD_Line_Tool_Large_View"
	Then I "Assert_CMD_Line_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Comment onto a new workflow
	When I "Drag_Toolbox_Comment_Onto_DesignSurface"
	Then I "Assert_Comment_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Count_Records onto a new workflow
	When I "Drag_Toolbox_Count_Records_Onto_DesignSurface"
	Then I "Assert_Count_Records_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox create JSON onto a new workflow
	When I "Drag_Toolbox_JSON_Onto_DesignSurface"
	Then I "Assert_Create_JSON_Exists_OnDesignSurface"

#@NeedsCreateJSONToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Create JSON Tool Small View on the Design Surface Opens Large View
	When I "Open_Json_Tool_Large_View"
	Then I "Assert_Json_Large_View_Exists_OnDesignSurface"

#@NeedsCreateJSONLargeViewOnTheDesignSurface
#Scenario: Click Create JSON Tool QVI Button Opens Qvi
	When I "Open_Json_Tool_Qvi_Large_View"
	Then I "Assert_Json_Qvi_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Delete_Record onto a new workflow
	When I "Drag_Toolbox_Delete_Record_Onto_DesignSurface"
	Then I "Assert_Delete_Record_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Date_And_Time onto a new workflow
	When I "Drag_Toolbox_Date_And_Time_Onto_DesignSurface"
	Then I "Assert_Date_And_Time_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DateTime_Difference onto a new workflow
	When I "Drag_Toolbox_DateTime_Difference_Onto_DesignSurface"
	Then I "Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Email onto a new workflow
	When I "Drag_Toolbox_Email_Onto_DesignSurface"
	Then I "Assert_Email_Exists_OnDesignSurface"

#@NeedsEmailOnDesignSurface
#Scenario: Click Nested Workflow Name Opens Nested Workflow Edit Tab
	#Given I "Assert_Email_Exists_OnDesignSurface"
	When I "Open_Email_Tool_Large_View"
	Then I "Assert_Email_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Service Picker onto a new workflow
	When I "Drag_Toolbox_Service_Picker_Onto_DesignSurface"
	Then I "Assert_Service_Picker_Cancel_Button_Exists"

#@NeedsServicePickerDialog
#Scenario: Click Cancel Service Picker Dialog Creates Service Picker on Design Surface
	#Given I "Assert_Service_Picker_Cancel_Button_Exists"
	When I "Click_Cancel_Service_Picker_Dialog"
	Then I "Assert_Service_Picker_Exists_OnDesignSurface"
	
#Dropbox tool removed
@NeedsBlankWorkflow
Scenario: Drag toolbox Dropbox Download onto a new workflow
	When I "Drag_Toolbox_Dropbox_Download_Onto_DesignSurface"
	Then I "Assert_Dropbox_Download_Exists_OnDesignSurface"
	
#Dropbox tool removed
@NeedsBlankWorkflow
Scenario: Drag toolbox Dropbox Upload onto a new workflow
	When I "Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface"
	Then I "Assert_Dropbox_Upload_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Find_Record_Index onto a new workflow
	When I "Drag_Toolbox_Find_Record_Index_Onto_DesignSurface"
	Then I "Assert_Find_Record_Index_Exists_OnDesignSurface"

#@NeedsFindRecordsToolOnTheDesignSurface
#Scenario: Open Find Record Index Tool Large View
	Given I "Assert_Find_Record_Index_Exists_OnDesignSurface"
	When I "Open_Find_Record_Index_Tool_Large_View"
	Then I "Assert_Find_Record_index_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Move onto a new workflow
	When I "Drag_Toolbox_Move_Onto_DesignSurface"
	Then I "Assert_Move_Exists_OnDesignSurface"

#@NeedsMoveToolOnTheDesignSurface
#Scenario: Open Move Tool Large View
	Given I "Assert_Move_Exists_OnDesignSurface"
	When I "Open_Move_Tool_Large_View"
	Then I "Assert_Move_Large_View_Exists_OnDesignSurface"

@NeedsBlankWorkflow
Scenario: Variable List Exists
	Then I "Assert_VariableList_Exists"
	Then I "Assert_VariableList_DeleteButton_Exists"
	Then I "Assert_VariableList_Recordset_ChildTextBox_Exists"
	Then I "Assert_VariableList_RecordsetInput_CheckBox_Exists"
	Then I "Assert_VariableList_RecordsetInput_ChildCheckBox_Exists"
	Then I "Assert_VariableList_RecordsetItem_Exists"
	Then I "Assert_VariableList_RecordsetOutput_CheckBox_Exists"
	Then I "Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists"
	Then I "Assert_VariableList_RecordsetTextBox_Exists"
	Then I "Assert_VariableList_SortButton_Exists"
	Then I "Assert_VariableList_VariableInput_CheckBox_Exists"
	Then I "Assert_VariableList_VariableItem_Exists"
	Then I "Assert_VariableList_VariableOutput_CheckBox_Exists"
	Then I "Assert_VariableList_VariableTextBox_Exists"
	Then I "Assert_VariableList_DataInputTree_Exists"
	
Scenario: Toolbox Exists
	Then I "Assert_Toolbox_FilterTextbox_Exists"
	Then I "Assert_Toolbox_RefreshButton_Exists"

Scenario: Explorer Exists
	Then I "Assert_Explorer_Exists"
	Then I "Assert_Explorer_ServerName_Exists"

Scenario: Connect Control Exists
	Then I "Assert_Connect_Control_Exists_InExplorer"
	Then I "Assert_Connect_ConnectControl_Button_Exists_InExplorer"
	Then I "Assert_Explorer_Edit_Connect_Control_Button_Exists"

@ignore	
Scenario: Settings Ribbon Button
	Given I "Assert_Settings_Button_Exists_OnDesignSurface"
	When I "Click_Settings_Ribbon_Button"
	Then I "Assert_Settings_LoggingTab_Exists"
	Then I "Assert_Settings_ResourcePermissions_Exists"
	Then I "Assert_Settings_SecurityTab_Exists"
	Then I "Assert_Settings_ServerPermissions_Exists"
	
#@NeedsNewSettingsTab
#Scenario: Click Close Settings Tab Button
	#Given I "Assert_Close_Settings_Tab_Button_Exists"
	#When I "Click_Close_Settings_Tab_Button"
	Then I "Assert_MessageBox_No_Button_Exists"

#Scenario: Click MessageBox No
	#Given I "Assert_Messagebox_No_Exists"
	When I "Click_MessageBox_No"
	#Then I "Assert_Tab_Closed"
	
Scenario: Deploy Ribbon Button
	Given I "Assert_Deploy_Ribbon_Button_Exists"
	When I "Click_Deploy_Ribbon_Button"
	Then I "Assert_Source_Server_Name_Exists"
	Then I "Assert_Refresh_Button_Source_Server_Exists"
	Then I "Assert_Filter_Source_Server_Exists"
	Then I "Assert_Connect_Control_DestinationServer_Exists"
	Then I "Assert_Override_Count_Exists"
	Then I "Assert_NewResource_Count_Exists"
	Then I "Assert_Source_Server_Edit_Exists"
	Then I "Assert_Connect_Button_Source_Server_Exists"
	Then I "Assert_Edit_Button_Destination_Server_Exists"
	Then I "Assert_Connect_button_Destination_Server_Exists"
	Then I "Assert_Connect_Control_SourceServer_Exists"
	Then I "Assert_ShowDependencies_Button_DestinationServer_Exists"
	Then I "Assert_ServiceLabel_DestinationServer_Exists"
	Then I "Assert_ServicesCount_Label_Exists"
	Then I "Assert_SourceLabel_DestinationServer_Exists"
	Then I "Assert_SourceCount_DestinationServer_Exists"
	Then I "Assert_NewResource_Label_Exists"
	Then I "Assert_Override_Label_DestinationServer_Exists"
	Then I "Assert_DeployButton_DestinationServer_Exists"
	Then I "Assert_SuccessMessage_Label_Exists"

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
	
@NeedsBlankWorkflow
Scenario: Debug Ribbon Button
	When I "Click_Debug_Ribbon_Button"
	Then I "Assert_DebugInput_Window_Exists"
	Then I "Assert_DebugInput_CancelButton_Exists"
	Then I "Assert_DebugInput_RememberCheckbox_Exists"
	Then I "Assert_DebugInput_ViewInBrowser_Button_Exists"
	Then I "Assert_DebugInput_DebugButton_Exists"
	Then I "Assert_DebugInput_InputData_Window_Exists"
	Then I "Assert_DebugInput_InputData_Field_Exists"
	Then I "Assert_DebugInput_Xml_Tab_Exists"
	Then I "Assert_DebugInput_Xml_Window_Exists"
	Then I "Assert_DebugInput_Json_Tab_Exists"
	Then I "Assert_DebugInput_Json_Window_Exists"
		
Scenario: Save Dialog
	Given I "Assert_NewWorkFlow_RibbonButton_Exists"
	When I "Click_New_Workflow_Ribbon_Button"
	Then I "Assert_StartNode_Exists"
	
#@NeedsSaveDialog
#Scenario: Click Save Ribbon Button
	Given I "Assert_Save_Button_Exists_OnDesignSurface"
	When I "Click_Save_Ribbon_Button"
	Then I "Assert_SaveDialog_Exists"
	Then I "Assert_SaveDialog_CancelButton_Exists"
	Then I "Assert_SaveDialog_ErrorLabel_Exists"
	Then I "Assert_SaveDialog_ExplorerTree_Exists"
	Then I "Assert_SaveDialog_ExplorerTreeItem_Exists"
	Then I "Assert_SaveDialog_ExplorerView_Exists"
	Then I "Assert_SaveDialog_FilterTextbox_Exists"
	Then I "Assert_SaveDialog_NameLabel_Exists"
	Then I "Assert_SaveDialog_RefreshButton_Exists"
	Then I "Assert_SaveDialog_SaveButton_Exists"
	Then I "Assert_SaveDialog_ServiceName_Textbox_Exists"
	Then I "Assert_SaveDialog_WorkspaceName_Exists"
	
#@NeedsSaveDialog
#Scenario: Click Save Dialog Cancel Button
	#Given I "Assert_Save_Dialog_Cancel_Button_Exists"
	When I "Click_SaveDialog_CancelButton"
	Then I "Assert_StartNode_Exists"
	
Scenario: Debug Output Window
	Given I "DebugOutput_Exists"
	Given I "DebugOutput_ExpandCollapseButton_Exists"
	Given I "DebugOutput_FilterTextbox_Exists"
	Given I "DebugOutput_ResultsTree_Exists"
	Given I "DebugOutput_SettingsButton_Exists"
	When I "Click_ExpandAndStepIn_NestedWorkflow"
	#Then I "Assert_DebugOutput_Cell_Exists"
	
#@NeedsDebugOutput
#Scenario: Click Cell Highlights Workflow OnDesignSurface
	#Given I "Assert_DebugOutput_Cell_Exists"
	When I "Click_Cell_Highlights_Workflow_OnDesignSurface"
	#Then "Assert_Nested_Workflow_Name_Exists"
	
#@NeedsNestedWorkflowDebugOutput
#Scenario: Click Nested Workflow Name Opens Nested Workflow Edit Tab
	#Given I "Assert_Nested_Workflow_Name_Exists"
	When I "Click_Nested_Workflow_Name"
	#Then I "Assert_NestedWorkflow_Tab_Exists"
	
@NeedsBlankWorkflow
Scenario: Dragging Database Connector Onto Design Surface Should not be droppable
	#Given "Some database connector" exists in the explorer tree
	When I "Drag_Database_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@NeedsBlankWorkflow
Scenario: Dragging Plugin Connector Onto Design Surface Should not be droppable
	#Given "Some plugin connector" exists in the explorer tree
	When I "Drag_Plugin_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@NeedsBlankWorkflow
Scenario: Dragging Web Connector Onto Design Surface Should not be droppable
	#Given "Some web connector" exists in the explorer tree
	When I "Drag_Web_Connector_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
@NeedsBlankWorkflow
Scenario: Dragging Sharepoint Connector Onto Design Surface Should not be droppable
	#Given "Some Sharepoint Source" exists in the explorer tree
	When I "Drag_Sharepoint_Source_Onto_DesignSurface"
	#Then I "Assert_Database_Connector_On_The_Design_Surface_Does_Not_Exist"
	
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
	
@NeedsBlankWorkflow
Scenario: Right Click On Design Surface
	When I "Open_Context_Menu_OnDesignSurface"
	Then I "Assert_Generic_Context_Menu_Exists"
	
@NeedsBlankWorkflow
Scenario: Context Menu on New Workflow Tab
	When I "RightClick_New_Workflow_Tab"
	Then I "Assert_New_Workflow_Context_Menu_Exists"
	
Scenario: Pin and unpin Explorer
	Given I "Click_Toggle_Unpin_Explorer"
	Then I "Click_Toggle_Pin_Explorer"
	
Scenario: Pin and unpin Help
	Given I "Click_Toggle_Unpin_Documentor"
	Then I "Click_Toggle_Pin_Documentor"
	
Scenario: Pin and unpin Toolbox
	Given I "Click_Toggle_Unpiin_Toolbox"
	Then I "Click_Toggle_Pin_Toolbox"
	
Scenario: Pin and unpin Debug Output
	Given I "Click_Toggle_Unpin_DebugOutput"
	Then I "Click_Toggle_Pin_DebugOutput"
	
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