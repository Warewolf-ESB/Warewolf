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
	And I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	And I double click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "WORKSURFACE,A database must be selected." is visible
	Then I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__Database_AutoID,GetCities"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__TableName_AutoID,dbo.City"
	And I click "WORKSURFACE,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "WORKSURFACE,A database must be selected." is not visible

Scenario: Debug GatherSystemInformation same variables in two activites
	Given I have Warewolf running
	And I send "11330_Integration tests" to "EXPLORER,FilterTextBox,UI_DataListSearchtxt_AutoID"
	And I double click "EXPLORER,Navigation,UI_localhost,UI_SPINT 7_AutoID,UI_11330_Integration tests_AutoID"
	And I send "{F6}" to ""
	And I wait
	Then "DEBUGOUTPUT,Gather System Info 1 (2),Date & Time" is visible
	Then "DEBUGOUTPUT,Gather System Info 1 (2),CPU Available" is visible
	Then "DEBUGOUTPUT,Gather System Info 2 (2),Date & Time" is visible
	Then "DEBUGOUTPUT,Gather System Info 2 (2),CPU Available" is visible
