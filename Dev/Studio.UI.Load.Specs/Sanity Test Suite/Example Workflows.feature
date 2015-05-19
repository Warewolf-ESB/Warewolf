Feature: SpecFloUI_File and Folder - Delete_AutoIDeature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@Examples
Scenario: Testing Example Workflows
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	# Utility - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Comment" to "EXPLORERFILTER"
    And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Comment_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Comment_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Comment(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Date and Time Difference" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Date and Time Difference_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Date and Time Difference_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Date and Time Difference(FlowchartDesigner)" is visible within "10" seconds
    And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Date and Time" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Date and Time_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Date and Time_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Date and Time(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Calculate" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Calculate_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Calculate_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Calculate(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Format Number" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Format Number_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Format Number_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Format Number(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Random" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Random_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Random_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Random(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - System Information" to "EXPLORERFILTER"
    And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - System Information_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Utility - System Information_AutoID"
	Then "WORKFLOWDESIGNER,Utility - System Information(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
   
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Web Request" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Web Request_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Web Request_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Web Request(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
   
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - XPath" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - XPath_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - XPath_AutoID"
	Then "WORKFLOWDESIGNER,Utility - XPath(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Assign" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Assign_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Assign_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Assign(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	##Control Flow - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Control Flow" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID"
	Then "WORKFLOWDESIGNER,Control Flow: Decision(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Sequence_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Sequence_AutoID"
	Then "WORKFLOWDESIGNER,Control Flow - Sequence(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Switch_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Switch_AutoID"
	Then "WORKFLOWDESIGNER,Control Flow - Switch(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to "" 
	And all tabs are closed
	
	##Loop Constructs - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Loop Constructs" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Loop Constructs - For Each_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Loop Constructs - For Each_AutoID"
	Then "WORKFLOWDESIGNER,Loop Constructs - For Each(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
    # Data - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Find Index" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Find Index_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Find Index_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Find Index(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Data - Case Conversion" to "EXPLORERFILTER"
    And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Case Conversion_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Case Conversion_AutoID"
	Then "WORKFLOWDESIGNER,Data - Case Conversion(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Data - Data Split" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Data Split_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Data Split_AutoID"
	Then "WORKFLOWDESIGNER,Data - Data Split(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Data - Data Merge" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Data Merge_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Data - Data Merge_AutoID"
	Then "WORKFLOWDESIGNER,Data - Data Merge(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility - Replace" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Replace_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Utility - Replace_AutoID"
	Then "WORKFLOWDESIGNER,Utility - Replace(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
		
	## Recordset - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Unique Records_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Unique Records_AutoID"
    Then "WORKFLOWDESIGNER,Recordset - Unique Records(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - Count Records" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Count Records_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Count Records_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Count Records(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - Records Length" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - Delete Records" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Delete Records_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Delete Records_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Delete Records(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - Find Records" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Find Records_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Find Records_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Find Records(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - Sort Records" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Sort Records_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Sort Records_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Sort Records(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Recordset - SQL Bulk Insert" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - SQL Bulk Insert_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - SQL Bulk Insert_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - SQL Bulk Insert(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	# File and Folder - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Rename" to "EXPLORERFILTER"
	 And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Rename_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Rename_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Rename(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Unzip" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Unzip_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Unzip_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Unzip(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Write File" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Write File_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Write File_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Write File(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Delete" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Delete_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Delete_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Delete(FlowchartDesigner),Delete(DeleteDesigner),SmallViewContent" is visible within "10" seconds
	And I send "{F6}" to "" 
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Zip" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Zip_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Zip_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Zip(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed

	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Create" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Create_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Create_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Create(FlowchartDesigner),Create(CreateDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Copy" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Copy_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Copy_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Copy(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Move" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Move_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Move_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Move(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Read" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Read File_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Read File_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Read File(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed

	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "File and Folder - Delete" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Delete_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Delete_AutoID"
	Then "WORKFLOWDESIGNER,File and Folder - Delete(FlowchartDesigner),Delete(DeleteDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
	And all tabs are closed

	 
	## Scripting - Tools
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Scripting - CMD Line" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Scripting - CMD Line_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Scripting - CMD Line_AutoID"
	Then "WORKFLOWDESIGNER,Scripting - CMD Line(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to ""
    And all tabs are closed
	
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Scripting - Script" to "EXPLORERFILTER"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Scripting - Script_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Scripting - Script_AutoID"
	Then "WORKFLOWDESIGNER,Scripting - Script(FlowchartDesigner)" is visible within "10" seconds
	And I send "{F6}" to "" 
	And all tabs are closed
	
	 