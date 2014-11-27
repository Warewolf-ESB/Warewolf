Feature: Recordset-SQL Bulk Insert
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@SQLBulkInsert
Scenario: Correcting errors on sql bulk insert clicking Done shows small view (using ids)
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	Given I double click "TOOLBOX,PART_SearchBox"
    Given I send "{Delete}" to ""
	#Searching SQL Tool in ToolBox
	When I send "Sql" to "TOOLBOX,PART_SearchBox"
	Then I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	#Opening Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	#Checking Validation Messages After click on Done
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Correcting The Error
	Given I click point "151,42" on "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I send "testingDBSrc{ENTER}" to ""
	Given I click point "148,75" on "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I send "dbo.City{ENTER}" to ""
	#This database happens to have alot of mappings to load
	And I wait for "10" seconds
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Given "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),SQL Bulk Insert(SqlBulkInsertDesigner),SmallViewContent,UI__Database_AutoID" is visible within "5" seconds
	