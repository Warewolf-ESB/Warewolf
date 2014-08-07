Feature: Workflow
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to create and execute a workflow

Scenario: Create a new workflow
	Given I have Warewolf running
	When I click new "Workflow"
	Then a new tab is created
	And the tab name contains "Unsaved"
	And start node is visible

Scenario: Create a new Database Service
	Given I have Warewolf running
	When I click new "Database Service"
	And the new Database Service wizard is visible
	And I create a new Database Source "DatabaseSourceCodedUI"
	And I create a database service "DatabaseServiceCodedUI"
	Then "DatabaseSourceCodedUI" is in the explorer
	And "DatabaseServiceCodedUI" is in the explorer

Scenario: Debug a workflow
	Given I have Warewolf running
	When I debug "TravsTestFlow" in "TRAV"
	Then debug contains
	| Label     | Value     |
	| Outputs : | [[a]] = 1 |

Scenario: Drag on Multiassign and enter data
	Given I have Warewolf running
	And I click new "Workflow"
	When I drag on a "Multi Assign"
	And I enter into the "Multi Assign"
	| Variable    | Value |
	| [[theVar1]] | 1     |
	| [[theVar2]] | 2     |
	Then "Multi Assign" contains
	| Variable    | Value |
	| [[theVar1]] | 1     |
	| [[theVar2]] | 2     |

Scenario: Correcting errors on sql bulk insert clicking Done shows small view (using ids)
	Given I have Warewolf running	
	And I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	When I send "Sql Bulk Insert" to "TOOLBOX,PART_SearchBox"
	#And I send "Bug_1096" to "EXPLORER,FilterTextBox,UI_DataListSearchtxt_AutoID"
	And I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	And I double click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "WORKSURFACE,A database must be selected." is visible
	Then I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__Database_AutoID,GetCities"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__TableName_AutoID,dbo.City"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "WORKSURFACE,A database must be selected." is not visible
	#When I drag on a "Sql Bulk Insert" tool as 'SqlBulkIns'
	#And I open the large view of 'SqlBulkIns'
	#When I click 'DoneButton' on 'SqlBulkIns'
	#Then errors are shown for 'SqlBulkIns'
	#Then I select 'GetCities' in 'UI__Database_AutoID' in 'SqlBulkIns'
	#And I select 'City' in 'UI__TableName_AutoID' in 'SqlBulkIns'
	#When I click 'DoneButton' on 'SqlBulkIns'
	#Then Small View for 'SqlBulkIns' is shown
