Feature: StudioUISpecs
	In order to create good workflows
	As a workflow designer
	I want to see the layout and function of workflows

@mytag
Scenario: Add two numbers
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
	
@NeedsBlankWorkflow
Scenario: Drag toolbox multiassign onto a new workflow
	When I 'Drag_Toolbox_MultiAssign_Onto_DesignSurface'
	Then I 'Assert_MultiAssign_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Assign_Tool_Large_View'
	Then I 'Assert_Assign_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Assign_Tool_Qvi_Large_View'
	Then I 'Assert_Assign_QVI_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox decision onto a new workflow
	When I 'Drag_Toolbox_Decision_Onto_DesignSurface'
	Then I 'Assert_Decision_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox sequence onto a new workflow
	When I 'Drag_Toolbox_Sequence_Onto_DesignSurface'
	Then I 'Assert_Sequence_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox switch onto a new workflow
	When I 'Drag_Toolbox_Switch_Onto_DesignSurface'
	Then I 'Assert_Switch_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Base_Conversion onto a new workflow
	When I 'Drag_Toolbox_Base_Conversion_Onto_DesignSurface'
	Then I 'Assert_Base_Conversion_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Base_Conversion_Tool_Qvi_Large_View'
	Then I 'Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DotNet Dll Tool onto a new workflow
	When I 'Drag_DotNet_DLL_Connector_Onto_DesignSurface'
	Then I 'Assert_DotNet_DLL_Connector_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_DotNet_DLL_Connector_Tool_Small_View'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox MySql Tool onto a new workflow
	When I 'Drag_Toolbox_MySql_Database_Onto_DesignSurface'
	Then I 'Assert_Mysql_Database_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_MySql_Database_Tool_Small_View'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sql Tool onto a new workflow
	When I 'Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface'
	Then I 'Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sql_Server_Tool_small_View'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Get Web Request Tool onto a new workflow
	When I 'Drag_GetWeb_RequestTool_Onto_DesignSurface'
	Then I 'Assert_GetWeb_RequestTool_small_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_WebRequest_LargeView'
	Then I 'Assert_GetWeb_RequestTool_Large_View_Exists_OnDesignSurface'
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox Post Web Request Tool onto a new workflow
	When I 'Drag_PostWeb_RequestTool_Onto_DesignSurface'
	Then I 'Assert_PostWeb_RequestTool_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_PostWeb_RequestTool_small_View'
	Then I 'Assert_PostWeb_RequestTool_Small_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Case_Conversion onto a new workflow
	When I 'Drag_Toolbox_Case_Conversion_Onto_DesignSurface'
	Then I 'Assert_Case_Conversion_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Case_Conversion_Tool_Qvi_Large_View'
	Then I 'Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Data_Merge onto a new workflow
	When I 'Drag_Toolbox_Data_Merge_Onto_DesignSurface'
	Then I 'Assert_Data_Merge_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Data_Merge_Large_View'
	Then I 'Assert_Data_Merge_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Data_Merge_Tool_Qvi_Large_View'
	Then I 'Assert_Data_Merge_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Data_Split onto a new workflow
	When I 'Drag_Toolbox_Data_Split_Onto_DesignSurface'
	Then I 'Assert_Data_Split_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Data_Split_Large_View'
	Then I 'Assert_Data_Split_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Data_Split_Tool_Qvi_Large_View'
	Then I 'Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Find_Index onto a new workflow
	When I 'Drag_Toolbox_Find_Index_Onto_DesignSurface'
	Then I 'Assert_Find_Index_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Replace onto a new workflow
	When I 'Drag_Toolbox_Replace_Onto_DesignSurface'
	Then I 'Assert_Replace_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Copy_Path onto a new workflow
	When I 'Drag_Toolbox_Copy_Onto_DesignSurface'
	Then I 'Assert_Copy_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Copy_Tool_Large_View'
	Then I 'Assert_Copy_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Create_Path onto a new workflow
	When I 'Drag_Toolbox_Create_Onto_DesignSurface'
	Then I 'Assert_Create_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Create_Tool_Large_View'
	Then I 'Assert_Create_Path_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Delete_Path onto a new workflow
	When I 'Drag_Toolbox_Delete_Onto_DesignSurface'
	Then I 'Assert_Delete_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Read_File onto a new workflow
	When I 'Drag_Toolbox_Read_File_Onto_DesignSurface'
	Then I 'Assert_Read_File_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Read_File_Tool_Large_View'
	Then I 'Assert_Read_File_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Read_Folder onto a new workflow
	When I 'Drag_Toolbox_Read_Folder_Onto_DesignSurface'
	Then I 'Assert_Read_Folder_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Read_Folder_Tool_Large_View'
	Then I 'Assert_Read_Folder_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Rename_Folder onto a new workflow
	When I 'Drag_Toolbox_Rename_Onto_DesignSurface'
	Then I 'Assert_Rename_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Rename_Tool_Large_View'
	Then I 'Assert_Rename_Large_View_Exists_OnDesignSurface'
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox Unzip onto a new workflow
	When I 'Drag_Toolbox_Unzip_Onto_DesignSurface'
	Then I 'Assert_Unzip_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Unzip_Tool_Large_View'
	Then I 'Assert_Unzip_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Write_File onto a new workflow
	When I 'Drag_Toolbox_Write_File_Onto_DesignSurface'
	Then I 'Assert_Write_File_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Write_File_Tool_Large_View'
	Then I 'Assert_Write_File_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Zip onto a new workflow
	When I 'Drag_Toolbox_Zip_Onto_DesignSurface'
	Then I 'Assert_Zip_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Zip_Tool_Large_View'
	Then I 'Assert_Zip_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox For_Each onto a new workflow
	When I 'Drag_Toolbox_For_Each_Onto_DesignSurface'
	Then I 'Assert_For_Each_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Format_Number onto a new workflow
	When I 'Drag_Toolbox_Format_Number_Onto_DesignSurface'
	Then I 'Assert_Format_Number_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Length onto a new workflow
	When I 'Drag_Toolbox_Length_Onto_DesignSurface'
	Then I 'Assert_Length_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Random onto a new workflow
	When I 'Drag_Toolbox_Random_Onto_DesignSurface'
	Then I 'Assert_Random_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Script onto a new workflow
	When I 'Drag_Toolbox_Script_Onto_DesignSurface'
	Then I 'Assert_Script_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Create onto a new workflow
	When I 'Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Create_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Create_Tool_Large_View'
	Then I 'Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Delete onto a new workflow
	When I 'Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Delete_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Delete_Tool_Large_View'
	Then I 'Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Read onto a new workflow
	When I 'Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Read_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Read_Tool_Large_View'
	Then I 'Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Update onto a new workflow
	When I 'Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Update_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Update_Tool_Large_View'
	Then I 'Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sort_Record onto a new workflow
	When I 'Drag_Toolbox_Sort_Record_Onto_DesignSurface'
	Then I 'Assert_Sort_Records_Exists_OnDesignSurface'
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox SQL_Bulk_Insert onto a new workflow
	When I 'Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface'
	Then I 'Assert_Sql_Bulk_insert_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_SQL_Bulk_Insert_Tool_Large_View'
	Then I 'Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_SQL_Bulk_Insert_Tool_Qvi_Large_View'
	Then I 'Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox System_Information onto a new workflow
	When I 'Drag_Toolbox_System_Information_Onto_DesignSurface'
	Then I 'Assert_System_information_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_System_Information_Tool_Qvi_Large_View'
	Then I 'Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface'
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox Unique_Records onto a new workflow
	When I 'Drag_Toolbox_Unique_Records_Onto_DesignSurface'
	Then I 'Assert_Unique_Records_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Web_Request onto a new workflow
	When I 'Drag_Toolbox_Web_Request_Onto_DesignSurface'
	Then I 'Assert_Web_Request_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Web_Request_Tool_Large_View'
	Then I 'Assert_Web_Request_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox XPath onto a new workflow
	When I 'Drag_Toolbox_XPath_Onto_DesignSurface'
	Then I 'Assert_XPath_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Xpath_Tool_Large_View'
	Then I 'Assert_Xpath_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Xpath_Tool_Qvi_Large_View'
	Then I 'Assert_Xpath_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Calculate onto a new workflow
	When I 'Drag_Toolbox_Calculate_Onto_DesignSurface'
	Then I 'Assert_Calculate_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox CMD_Line onto a new workflow
	When I 'Drag_Toolbox_CMD_Line_Onto_DesignSurface'
	Then I 'Assert_CMD_Line_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_CMD_Line_Tool_Large_View'
	Then I 'Assert_CMD_Line_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Comment onto a new workflow
	When I 'Drag_Toolbox_Comment_Onto_DesignSurface'
	Then I 'Assert_Comment_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Count_Records onto a new workflow
	When I 'Drag_Toolbox_Count_Records_Onto_DesignSurface'
	Then I 'Assert_Count_Records_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox create JSON onto a new workflow
	When I 'Drag_Toolbox_JSON_Onto_DesignSurface'
	Then I 'Assert_Create_JSON_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Json_Tool_Large_View'
	Then I 'Assert_Json_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Json_Tool_Qvi_Large_View'
	Then I 'Assert_Json_Qvi_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Delete_Record onto a new workflow
	When I 'Drag_Toolbox_Delete_Record_Onto_DesignSurface'
	Then I 'Assert_Delete_Record_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Date_And_Time onto a new workflow
	When I 'Drag_Toolbox_Date_And_Time_Onto_DesignSurface'
	Then I 'Assert_Date_And_Time_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DateTime_Difference onto a new workflow
	When I 'Drag_Toolbox_DateTime_Difference_Onto_DesignSurface'
	Then I 'Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Email onto a new workflow
	When I 'Drag_Toolbox_Email_Onto_DesignSurface'
	Then I 'Assert_Email_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Email_Tool_Large_View'
	Then I 'Assert_Email_Large_View_Exists_OnDesignSurface'
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox Service Picker onto a new workflow
	When I 'Drag_Toolbox_Service_Picker_Onto_DesignSurface'
	Then I 'Assert_Service_Picker_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	
@ignore
@NeedsBlankWorkflow
Scenario: Drag toolbox Dropbox onto a new workflow
	When I 'Drag_Toolbox_Dropbox_Onto_DesignSurface'
	Then I 'Assert_Dropbox_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Find_Record_Index onto a new workflow
	When I 'Drag_Toolbox_Find_Record_Index_Onto_DesignSurface'
	Then I 'Assert_Find_Record_Index_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Find_Record_Index_Tool_Large_View'
	Then I 'Assert_Find_Record_index_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Move onto a new workflow
	When I 'Drag_Toolbox_Move_Onto_DesignSurface'
	Then I 'Assert_Move_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Move_Tool_Large_View'
	Then I 'Assert_Move_Large_View_Exists_OnDesignSurface'
	
@NeedsBlankWorkflow
Scenario: Get Web Request tool
	When I 'Drag_Web_Get_Request_Tool_Onto_DesignSurface'
	Then I 'Assert_Web_Get_Request_Tool_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	#When I 'Open_Move_Tool_Large_View'
	Then I 'Assert_Web_Get_Request_Small_View_Exists_OnDesignSurface'