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