Feature: StudioUISpecs
	In order to find studio UI bugs
	As a user
	I want to have a good UX

Scenario: Drag toolbox multiassign onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_MultiAssign_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_MultiAssign_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Assign_Tool_Large_View' recorded action is performed
	Then The 'Assert_Assign_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Assign_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Assign_QVI_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox decision onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Decision_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Decision_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox sequence onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sequence_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sequence_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox switch onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Switch_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Switch_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Base_Conversion onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Base_Conversion_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Base_Conversion_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Base_Conversion_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox DotNet Dll Tool onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_DotNet_DLL_Connector_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_DotNet_DLL_Connector_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_DotNet_DLL_Connector_Tool_Small_View' recorded action is performed

Scenario: Drag toolbox MySql Tool onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_MySql_Database_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Mysql_Database_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_MySql_Database_Tool_Small_View' recorded action is performed
	
Scenario: Drag toolbox Sql Tool onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Sql_Server_Tool_small_View' recorded action is performed

Scenario: Drag toolbox Case_Conversion onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Case_Conversion_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Case_Conversion_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Case_Conversion_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Data_Merge onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Data_Merge_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Data_Merge_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Data_Merge_Large_View' recorded action is performed
	Then The 'Assert_Data_Merge_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Data_Merge_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Data_Merge_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Data_Split onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Data_Split_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Data_Split_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Data_Split_Large_View' recorded action is performed
	Then The 'Assert_Data_Split_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Data_Split_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Find_Index onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Find_Index_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Find_Index_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Replace onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Replace_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Replace_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Copy_Path onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Copy_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Copy_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Copy_Tool_Large_View' recorded action is performed
	Then The 'Assert_Copy_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Create_Path onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Create_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Create_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Create_Tool_Large_View' recorded action is performed
	Then The 'Assert_Create_Path_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Delete_Path onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Delete_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Delete_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Read_File onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Read_File_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Read_File_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Read_File_Tool_Large_View' recorded action is performed
	Then The 'Assert_Read_File_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Read_Folder onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Read_Folder_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Read_Folder_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Read_Folder_Tool_Large_View' recorded action is performed
	Then The 'Assert_Read_Folder_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Rename_Folder onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Rename_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Rename_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Rename_Tool_Large_View' recorded action is performed
	Then The 'Assert_Rename_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Unzip onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Unzip_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Unzip_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Unzip_Tool_Large_View' recorded action is performed
	Then The 'Assert_Unzip_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Write_File onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Write_File_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Write_File_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Write_File_Tool_Large_View' recorded action is performed
	Then The 'Assert_Write_File_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Zip onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Zip_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Zip_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Zip_Tool_Large_View' recorded action is performed
	Then The 'Assert_Zip_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox For_Each onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_For_Each_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_For_Each_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Format_Number onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Format_Number_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Format_Number_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Length onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Length_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Length_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Random onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Random_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Random_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Script onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Script_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Script_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Sharepoint_Create onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sharepoint_Create_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Sharepoint_Create_Tool_Large_View' recorded action is performed
	Then The 'Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Sharepoint_Delete onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sharepoint_Delete_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Sharepoint_Delete_Tool_Large_View' recorded action is performed
	Then The 'Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Sharepoint_Read onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sharepoint_Read_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Sharepoint_Read_Tool_Large_View' recorded action is performed
	Then The 'Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Sharepoint_Update onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sharepoint_Update_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Sharepoint_Update_Tool_Large_View' recorded action is performed
	Then The 'Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Sort_Record onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Sort_Record_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sort_Records_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox SQL_Bulk_Insert onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Sql_Bulk_insert_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_SQL_Bulk_Insert_Tool_Large_View' recorded action is performed
	Then The 'Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_SQL_Bulk_Insert_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox System_Information onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_System_Information_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_System_information_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_System_Information_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Unique_Records onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Unique_Records_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Unique_Records_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Web_Request onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Web_Request_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Web_Request_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Web_Request_Tool_Large_View' recorded action is performed
	Then The 'Assert_Web_Request_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox XPath onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_XPath_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_XPath_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Xpath_Tool_Large_View' recorded action is performed
	Then The 'Assert_Xpath_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Xpath_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Xpath_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Calculate onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Calculate_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Calculate_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox CMD_Line onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_CMD_Line_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_CMD_Line_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_CMD_Line_Tool_Large_View' recorded action is performed
	Then The 'Assert_CMD_Line_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Comment onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Comment_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Comment_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Count_Records onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Count_Records_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Count_Records_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox create JSON onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_JSON_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Create_JSON_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Json_Tool_Large_View' recorded action is performed
	Then The 'Assert_Json_Large_View_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Json_Tool_Qvi_Large_View' recorded action is performed
	Then The 'Assert_Json_Qvi_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Delete_Record onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Delete_Record_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Delete_Record_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Date_And_Time onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Date_And_Time_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Date_And_Time_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox DateTime_Difference onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_DateTime_Difference_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Email onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Email_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Email_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Email_Tool_Large_View' recorded action is performed
	Then The 'Assert_Email_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Service Picker onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Service_Picker_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Service_Picker_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Dropbox onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Dropbox_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Dropbox_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Find_Record_Index onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Find_Record_Index_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Find_Record_Index_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Find_Record_Index_Tool_Large_View' recorded action is performed
	Then The 'Assert_Find_Record_index_Large_View_Exists_OnDesignSurface' recorded action is performed

Scenario: Drag toolbox Move onto a new workflow
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Toolbox_Move_Onto_DesignSurface' recorded action is performed
	Then The 'Assert_Move_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Open_Move_Tool_Large_View' recorded action is performed
	Then The 'Assert_Move_Large_View_Exists_OnDesignSurface' recorded action is performed



Scenario: Variable List
	Given The 'Assert_VariableList_Exists' recorded action is performed
	Given The 'Assert_VariableList_DeleteButton_Exists' recorded action is performed
	Given The 'Assert_VariableList_Recordset_ChildTextBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetInput_CheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetInput_ChildCheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetItem_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetOutput_CheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_RecordsetTextBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_SortButton_Exists' recorded action is performed
	Given The 'Assert_VariableList_VariableInput_CheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_VariableItem_Exists' recorded action is performed
	Given The 'Assert_VariableList_VariableOutput_CheckBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_VariableTextBox_Exists' recorded action is performed
	Given The 'Assert_VariableList_DataInputTree_Exists' recorded action is performed


Scenario: Toolbox
	Given The 'Assert_Toolbox_FilterTextbox_Exists' recorded action is performed
	Given The 'Assert_Toolbox_RefreshButton_Exists' recorded action is performed


Scenario: Connect Control Exists
	Given The 'Assert_Connect_Control_Exists_InExplorer' recorded action is performed
	Given The 'Assert_Connect_ConnectControl_Button_Exists_InExplorer' recorded action is performed
	Given The 'Assert_Explorer_Edit_Connect_Control_Button_Exists' recorded action is performed


Scenario: Main Menu
	Given The 'Assert_Save_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Save_Ribbon_Button' recorded action is performed
	Then The 'Assert_New_Version_Download_Button_Exists_OnDesignSurface' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Settings_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Settings_Ribbon_Button' recorded action is performed
	#Then 'Assert_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Debug_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Debug_Ribbon_Button' recorded action is performed
	#Then 'Assert_Debug_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Scheduler_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Scheduler_Ribbon_Button' recorded action is performed
	#Then 'Assert_Scheduler_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Deploy_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Deploy_Ribbon_Button' recorded action is performed
	#Then 'Assert_Deploy_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Knowledge_Base_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Knowledge_Ribbon_Button' recorded action is performed
	#Then 'Assert_Knowledge_Base_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Lock_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Unlock_Ribbon_Button' recorded action is performed
	#Then 'Assert_Lock_Ribbon_Button_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Database_Source_Exists' recorded action is performed
	When The 'Click_NewDatabaseSource_Ribbon_Button' recorded action is performed
	#Then 'Assert_NewDatabaseSource_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Plugin_Source_Exists' recorded action is performed
	When The 'Click_NewPluginSource_Ribbon_Button' recorded action is performed
	#Then 'Assert_NewPluginSource_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Given The 'Assert_Web_Source_Exists' recorded action is performed
	When The 'Click_NewWebSource_Ribbon_Button' recorded action is performed
	#Then 'Assert_NewWebSource_Exists' recorded action is performed

	
Scenario: Save Dialog
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	Given The 'Assert_Save_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Save_Ribbon_Button' recorded action is performed
	Then The 'Assert_SaveDialog_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_CancelButton_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_ErrorLabel_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_ExplorerTree_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_ExplorerTreeItem_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_ExplorerView_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_FilterTextbox_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_NameLabel_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_RefreshButton_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_SaveButton_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_ServiceName_Textbox_Exists' recorded action is performed
	Then The 'Assert_SaveDialog_WorkspaceName_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_SaveDialog_CancelButton' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed

#Needs to be amended
Scenario: Debug Output Window
	Given The 'DebugOutput_Exists' recorded action is performed
	Given The 'DebugOutput_ExpandCollapseButton_Exists' recorded action is performed
	Given The 'DebugOutput_FilterTextbox_Exists' recorded action is performed
	Given The 'DebugOutput_ResultsTree_Exists' recorded action is performed
	Given The 'DebugOutput_SettingsButton_Exists' recorded action is performed
	When The 'Click_ExpandAndStepIn_NestedWorkflow' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Cell_Highlights_Workflow_OnDesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Nested_Workflow_Name' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed


Scenario: Connectors and Sources cannot be dragged onto the design surface
	When The 'Drag_Database_Connector_Onto_DesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Plugin_Connector_Onto_DesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Web_Connector_Onto_DesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Sharepoint_Source_Onto_DesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Drag_Server_Source_Onto_DesignSurface' recorded action is performed
	#Then 'Some Post-assert' recorded action is performed

Scenario: Scheduler
	Given The 'Assert_Scheduler_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Scheduler_Ribbon_Button' recorded action is performed
	Then The 'Assert_Scheduler_CreateNewTask_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_Create_New_Task_Ribbon_Button' recorded action is performed
	Then The 'Assert_Scheduler_DisabledRadioButton_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_Disable_Task_Radio_Button' recorded action is performed
	Then The 'Assert_Scheduler_EditTrigger_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_EditTrigger_Button' recorded action is performed
	Then The 'Assert_Scheduler_EnabledRadioButton_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_Enable_Task_Radio_Button' recorded action is performed
	Then The 'Assert_Scheduler_ErrorMessage_Exists' recorded action is performed
	Then The 'Assert_Scheduler_HistoryInput_Exists' recorded action is performed
	Then The 'Assert_Scheduler_HistoryLabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_HistoryTable_Exists' recorded action is performed
	Then The 'Assert_Scheduler_NameInput_Exists' recorded action is performed
	Then The 'Assert_Scheduler_NameLabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_PasswordInput_Exists' recorded action is performed
	Then The 'Assert_Scheduler_PasswordLabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_ResourcePicker_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_ResourcePicker' recorded action is performed
	Then The 'Assert_Scheduler_RunTask_Checkbox_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_RunTask' recorded action is performed
	Then The 'Assert_Scheduler_DeleteButton_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Scheduler_Delete_Task' recorded action is performed
	Then The 'Assert_Scheduler_Status_RadioButton_Exists' recorded action is performed
	Then The 'Assert_Scheduler_StatusLabe_Exists' recorded action is performed
	Then The 'Assert_Scheduler_TriggerLabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_TriggerValue_Exists' recorded action is performed
	Then The 'Assert_Scheduler_UserAccountLabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_UsernameInput_Exists' recorded action is performed
	Then The 'Assert_Scheduler_Usernamelabel_Exists' recorded action is performed
	Then The 'Assert_Scheduler_WorkflowInput_Exists' recorded action is performed
	Then The 'Assert_Scheduler_WorkflowLabel_Exists' recorded action is performed

Scenario: Settings
	Given The 'Assert_Settings_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Settings_Ribbon_Button' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	Then The 'Assert_Settings_LoggingTab_Exists' recorded action is performed
	Then The 'Assert_Settings_ResourcePermissions_Exists' recorded action is performed
	Then The 'Assert_Settings_SecurityTab_Exists' recorded action is performed
	Then The 'Assert_Settings_ServerPermissions_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_Admin_ServerPermissions' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_Contribute_ResourcePermissions' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_Contribute_ServerPermissions' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_Execute_ResourcePermissions' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_ResourcePermissions_ResourcePicker' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When The 'Click_Settings_View_ResourcePermissions' recorded action is performed
	#Then 'Assert_Something_in_Settings_Exists' recorded action is performed

Scenario: Context Menu on design surface
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	Then The 'Open_Context_Menu_OnDesignSurface' recorded action is performed

Scenario: Context Menu on Tab
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	Then The 'Tab_Context_Menu' recorded action is performed



Scenario: Debug Input window
	Given The 'Assert_NewWorkFlow_RibbonButton_Exists' recorded action is performed
	When The 'Click_New_Workflow_Ribbon_Button' recorded action is performed
	Then The 'Assert_StartNode_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	Then The 'Assert_DebugInput_Window_Exists' recorded action is performed
	Then The 'Assert_DebugInput_CancelButton_Exists' recorded action is performed
	Then The 'Assert_DebugInput_RememberCheckbox_Exists' recorded action is performed
	Then The 'Assert_DebugInput_ViewInBrowser_Button_Exists' recorded action is performed
	Then The 'Assert_DebugInput_DebugButton_Exists' recorded action is performed
	Then The 'Assert_DebugInput_InputData_Window_Exists' recorded action is performed
	Then The 'Assert_DebugInput_InputData_Field_Exists' recorded action is performed
	Then The 'Assert_DebugInput_Xml_Tab_Exists' recorded action is performed
	Then The 'Assert_DebugInput_Xml_Window_Exists' recorded action is performed
	Then The 'Assert_DebugInput_Json_Tab_Exists' recorded action is performed
	Then The 'Assert_DebugInput_Json_Window_Exists' recorded action is performed


Scenario: Deploy
	Given The 'Assert_Deploy_Button_Exists_OnDesignSurface' recorded action is performed
	When The 'Click_Deploy_Ribbon_Button' recorded action is performed
	#Then 'Assert_Deploy_Exists' recorded action is performed
	#Ashley TODO: The test should end here.

	Then The 'Assert_Source_Server_Name_Exists' recorded action is performed
	Then The 'Assert_Refresh_Button_Source_Server_Exists' recorded action is performed
	Then The 'Assert_Filter_Source_Server_Exists' recorded action is performed
	Then The 'Assert_Connect_Control_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_Override_Count_Exists' recorded action is performed
	Then The 'Assert_NewResource_Count_Exists' recorded action is performed
	Then The 'Assert_Source_Server_Edit_Exists' recorded action is performed
	Then The 'Assert_Connect_Button_Source_Server_Exists' recorded action is performed
	Then The 'Assert_Edit_Button_Destination_Server_Exists' recorded action is performed
	Then The 'Assert_Connect_button_Destination_Server_Exists' recorded action is performed
	Then The 'Assert_Connect_Control_SourceServer_Exists' recorded action is performed
	Then The 'Assert_ShowDependencies_Button_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_ServiceLabel_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_ServicesCount_Label_Exists' recorded action is performed
	Then The 'Assert_SourceLabel_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_SourceCount_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_NewResource_Label_Exists' recorded action is performed
	Then The 'Assert_Override_Label_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_DeployButton_DestinationServer_Exists' recorded action is performed
	Then The 'Assert_SuccessMessage_Label_Exists' recorded action is performed


Scenario: Explorer
	Given The 'Assert_Explorer_Exists' recorded action is performed
	Then The 'Assert_Explorer_ServerName_Exists' recorded action is performed
	Then The 'Right_Click_Context_Menu_InExplorer' recorded action is performed


Scenario: Pin and unpin Explorer
	Given The 'Click_Toggle_Unpin_Explorer' recorded action is performed
	Then The 'Click_Toggle_Pin_Explorer' recorded action is performed

Scenario: Pin and unpin Help
	Given The 'Click_Toggle_Unpin_Documentor' recorded action is performed
	Then The 'Click_Toggle_Pin_Documentor' recorded action is performed


Scenario: Pin and unpin Toolbox
	Given The 'Click_Toggle_Unpiin_Toolbox' recorded action is performed
	Then The 'Click_Toggle_Pin_Toolbox' recorded action is performed

Scenario: Pin and unpin Debug Output
	Given The 'Click_Toggle_Unpin_DebugOutput' recorded action is performed
	Then The 'Click_Toggle_Pin_DebugOutput' recorded action is performed

Scenario: Pin and unpin variable list
	Given The 'Click_Toggle_Unpin_VariableList' recorded action is performed
	Then The 'Click_Toggle_Pin_VariableList' recorded action is performed