Feature: Recordset-SQL Bulk Insert
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


Scenario: Correcting errors on sql bulk insert clicking Done shows small view (using ids)
	Given I have Warewolf running	
	And I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	When I send "Sql Bulk Insert" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Given "WORKSURFACE,UI_ErrorsTextBlock_AutoID"" is visible
	Given I click point "20,30" on "WORKSURFACE,UI_ErrorsTextBlock_AutoID"
	#Given I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__Database_AutoID,GetCities"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__TableName_AutoID,dbo.City"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	#Given "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner)" contains text "To set server"