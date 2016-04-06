Feature: StudioUISpecs
	In order to find studio UI bugs
	As a user
	I want to have a good UX

Scenario: Drag toolbox multiassign onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox decision onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Decision_Onto_DesignSurface'
	Then I 'Assert_Decision_Exists_OnDesignSurface'

Scenario: Drag toolbox sequence onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sequence_Onto_DesignSurface'
	Then I 'Assert_Sequence_Exists_OnDesignSurface'

Scenario: Drag toolbox switch onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Switch_Onto_DesignSurface'
	Then I 'Assert_Switch_Exists_OnDesignSurface'

Scenario: Drag toolbox Base_Conversion onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Base_Conversion_Onto_DesignSurface'
	Then I 'Assert_Base_Conversion_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Base_Conversion_Tool_Qvi_Large_View'
	Then I 'Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox DotNet Dll Tool onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_DotNet_DLL_Connector_Onto_DesignSurface'
	Then I 'Assert_DotNet_DLL_Connector_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_DotNet_DLL_Connector_Tool_Small_View'

Scenario: Drag toolbox MySql Tool onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_MySql_Database_Onto_DesignSurface'
	Then I 'Assert_Mysql_Database_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_MySql_Database_Tool_Small_View'
	
Scenario: Drag toolbox Sql Tool onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface'
	Then I 'Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sql_Server_Tool_small_View'


Scenario: Drag toolbox Get Web Request Tool onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_GetWeb_RequestTool_Onto_DesignSurface'
	Then I 'Assert_GetWeb_RequestTool_small_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_WebRequest_LargeView'
	Then I 'Assert_GetWeb_RequestTool_Large_View_Exists_OnDesignSurface'
	
@ignore
Scenario: Drag toolbox Post Web Request Tool onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_PostWeb_RequestTool_Onto_DesignSurface'
	Then I 'Assert_PostWeb_RequestTool_Large_View_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_PostWeb_RequestTool_small_View'
	Then I 'Assert_PostWeb_RequestTool_Small_View_Exists_OnDesignSurface'


Scenario: Drag toolbox Case_Conversion onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Case_Conversion_Onto_DesignSurface'
	Then I 'Assert_Case_Conversion_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Case_Conversion_Tool_Qvi_Large_View'
	Then I 'Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Data_Merge onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox Data_Split onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox Find_Index onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Find_Index_Onto_DesignSurface'
	Then I 'Assert_Find_Index_Exists_OnDesignSurface'

Scenario: Drag toolbox Replace onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Replace_Onto_DesignSurface'
	Then I 'Assert_Replace_Exists_OnDesignSurface'

Scenario: Drag toolbox Copy_Path onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Copy_Onto_DesignSurface'
	Then I 'Assert_Copy_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Copy_Tool_Large_View'
	Then I 'Assert_Copy_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Create_Path onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Create_Onto_DesignSurface'
	Then I 'Assert_Create_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Create_Tool_Large_View'
	Then I 'Assert_Create_Path_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Delete_Path onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Delete_Onto_DesignSurface'
	Then I 'Assert_Delete_Exists_OnDesignSurface'

Scenario: Drag toolbox Read_File onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Read_File_Onto_DesignSurface'
	Then I 'Assert_Read_File_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Read_File_Tool_Large_View'
	Then I 'Assert_Read_File_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Read_Folder onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Read_Folder_Onto_DesignSurface'
	Then I 'Assert_Read_Folder_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Read_Folder_Tool_Large_View'
	Then I 'Assert_Read_Folder_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Rename_Folder onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Rename_Onto_DesignSurface'
	Then I 'Assert_Rename_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Rename_Tool_Large_View'
	Then I 'Assert_Rename_Large_View_Exists_OnDesignSurface'
	
@ignore
Scenario: Drag toolbox Unzip onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Unzip_Onto_DesignSurface'
	Then I 'Assert_Unzip_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Unzip_Tool_Large_View'
	Then I 'Assert_Unzip_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Write_File onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Write_File_Onto_DesignSurface'
	Then I 'Assert_Write_File_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Write_File_Tool_Large_View'
	Then I 'Assert_Write_File_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Zip onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Zip_Onto_DesignSurface'
	Then I 'Assert_Zip_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Zip_Tool_Large_View'
	Then I 'Assert_Zip_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox For_Each onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_For_Each_Onto_DesignSurface'
	Then I 'Assert_For_Each_Exists_OnDesignSurface'

Scenario: Drag toolbox Format_Number onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Format_Number_Onto_DesignSurface'
	Then I 'Assert_Format_Number_Exists_OnDesignSurface'

Scenario: Drag toolbox Length onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Length_Onto_DesignSurface'
	Then I 'Assert_Length_Exists_OnDesignSurface'

Scenario: Drag toolbox Random onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Random_Onto_DesignSurface'
	Then I 'Assert_Random_Exists_OnDesignSurface'

Scenario: Drag toolbox Script onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Script_Onto_DesignSurface'
	Then I 'Assert_Script_Exists_OnDesignSurface'

Scenario: Drag toolbox Sharepoint_Create onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Create_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Create_Tool_Large_View'
	Then I 'Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Sharepoint_Delete onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Delete_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Delete_Tool_Large_View'
	Then I 'Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Sharepoint_Read onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Read_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Read_Tool_Large_View'
	Then I 'Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Sharepoint_Update onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface'
	Then I 'Assert_Sharepoint_Update_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Sharepoint_Update_Tool_Large_View'
	Then I 'Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Sort_Record onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Sort_Record_Onto_DesignSurface'
	Then I 'Assert_Sort_Records_Exists_OnDesignSurface'
	
@ignore
Scenario: Drag toolbox SQL_Bulk_Insert onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox System_Information onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_System_Information_Onto_DesignSurface'
	Then I 'Assert_System_information_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_System_Information_Tool_Qvi_Large_View'
	Then I 'Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface'
	
@ignore
Scenario: Drag toolbox Unique_Records onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Unique_Records_Onto_DesignSurface'
	Then I 'Assert_Unique_Records_Exists_OnDesignSurface'

Scenario: Drag toolbox Web_Request onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Web_Request_Onto_DesignSurface'
	Then I 'Assert_Web_Request_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Web_Request_Tool_Large_View'
	Then I 'Assert_Web_Request_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox XPath onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox Calculate onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Calculate_Onto_DesignSurface'
	Then I 'Assert_Calculate_Exists_OnDesignSurface'

Scenario: Drag toolbox CMD_Line onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_CMD_Line_Onto_DesignSurface'
	Then I 'Assert_CMD_Line_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_CMD_Line_Tool_Large_View'
	Then I 'Assert_CMD_Line_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Comment onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Comment_Onto_DesignSurface'
	Then I 'Assert_Comment_Exists_OnDesignSurface'

Scenario: Drag toolbox Count_Records onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Count_Records_Onto_DesignSurface'
	Then I 'Assert_Count_Records_Exists_OnDesignSurface'

Scenario: Drag toolbox create JSON onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
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

Scenario: Drag toolbox Delete_Record onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Delete_Record_Onto_DesignSurface'
	Then I 'Assert_Delete_Record_Exists_OnDesignSurface'

Scenario: Drag toolbox Date_And_Time onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Date_And_Time_Onto_DesignSurface'
	Then I 'Assert_Date_And_Time_Exists_OnDesignSurface'

Scenario: Drag toolbox DateTime_Difference onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_DateTime_Difference_Onto_DesignSurface'
	Then I 'Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface'

Scenario: Drag toolbox Email onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Email_Onto_DesignSurface'
	Then I 'Assert_Email_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Email_Tool_Large_View'
	Then I 'Assert_Email_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Service Picker onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Service_Picker_Onto_DesignSurface'
	Then I 'Assert_Service_Picker_Exists_OnDesignSurface'
	
@ignore
Scenario: Drag toolbox Dropbox onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Dropbox_Onto_DesignSurface'
	Then I 'Assert_Dropbox_Exists_OnDesignSurface'

Scenario: Drag toolbox Find_Record_Index onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Find_Record_Index_Onto_DesignSurface'
	Then I 'Assert_Find_Record_Index_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Find_Record_Index_Tool_Large_View'
	Then I 'Assert_Find_Record_index_Large_View_Exists_OnDesignSurface'

Scenario: Drag toolbox Move onto a new workflow
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Toolbox_Move_Onto_DesignSurface'
	Then I 'Assert_Move_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Open_Move_Tool_Large_View'
	Then I 'Assert_Move_Large_View_Exists_OnDesignSurface'



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
Scenario: Main Menu
	Given I 'Assert_Save_Button_Exists_OnDesignSurface'
	When I 'Click_Save_Ribbon_Button'
	Then I 'Assert_New_Version_Download_Button_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Settings_Button_Exists_OnDesignSurface'
	When I 'Click_Settings_Ribbon_Button'
	#Then 'Assert_Settings_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Debug_Button_Exists_OnDesignSurface'
	When I 'Click_Debug_Ribbon_Button'
	#Then 'Assert_Debug_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Scheduler_Button_Exists_OnDesignSurface'
	When I 'Click_Scheduler_Ribbon_Button'
	#Then 'Assert_Scheduler_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Deploy_Button_Exists_OnDesignSurface'
	When I 'Click_Deploy_Ribbon_Button'
	#Then 'Assert_Deploy_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Knowledge_Base_Exists_OnDesignSurface'
	When I 'Click_Knowledge_Ribbon_Button'
	#Then 'Assert_Knowledge_Base_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Lock_Button_Exists_OnDesignSurface'
	When I 'Click_Unlock_Ribbon_Button'
	#Then 'Assert_Lock_Ribbon_Button_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Database_Source_Exists'
	When I 'Click_NewDatabaseSource_Ribbon_Button'
	#Then 'Assert_NewDatabaseSource_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Plugin_Source_Exists'
	When I 'Click_NewPluginSource_Ribbon_Button'
	#Then 'Assert_NewPluginSource_Exists'
	#Ashley TODO: The test should end here.

	Given I 'Assert_Web_Source_Exists'
	When I 'Click_NewWebSource_Ribbon_Button'
	#Then 'Assert_NewWebSource_Exists'

	
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
	
@ignore
Scenario: Context Menu on Tab
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	Then I 'Tab_Context_Menu'
	
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

Scenario: Get Web Request tool
	Given I 'Assert_NewWorkFlow_RibbonButton_Exists'
	When I 'Click_New_Workflow_Ribbon_Button'
	Then I 'Assert_StartNode_Exists'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	When I 'Drag_Web_Get_Request_Tool_Onto_DesignSurface'
	Then I 'Assert_Web_Get_Request_Tool_Exists_OnDesignSurface'
	#Ashley TODO: The test should end here.

	#Ashley TODO: Use low level binding hooks for this step:
	#Given The test is initialized using low level binding calls
	#When I 'Open_Move_Tool_Large_View'
	Then I 'Assert_Web_Get_Request_Small_View_Exists_OnDesignSurface'