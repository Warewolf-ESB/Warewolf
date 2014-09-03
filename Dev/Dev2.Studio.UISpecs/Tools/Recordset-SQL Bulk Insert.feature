Feature: Recordset-SQL Bulk Insert
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


Scenario: Correcting errors on sql bulk insert clicking Done shows small view (using ids)
	Given I have Warewolf running
	And all tabs are closed	
	And I click "RIBBONNEWENDPOINT"
	#Searching SQL Tool in ToolBox
	When I send "Sql Bulk Insert" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	#Checking For Large View Validation Message
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	#Given "WORKSURFACE,UI_ErrorsTextBlock_AutoID"" is visible
	#Correcting The Error
	Given I click point "151,42" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I send "GetCities{ENTER}" to ""
	Given I click point "148,75" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I send "dbo.City{ENTER}" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),SmallViewContent,UI__Database_AutoID" is visible


	 