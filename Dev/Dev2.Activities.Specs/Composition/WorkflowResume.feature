Feature: WorkflowResume
	When a workflow execution fails
	I want to Resume
	
Scenario: Resuming a workflow that had failed to connect
	Given I have a workflow "WorkflowWtithMysqlToolUsingContainer"
	And "WorkflowWtithMysqlToolUsingContainer" contains an Assign "AssignNumber" as
	 | variable   | value |
	 | [[number]] | 1     |
	And "WorkflowWtithMysqlToolUsingContainer" contains a mysql database service "ToolUsingContainerAsTheSource"
	And "WorkflowWtithMysqlToolUsingContainer" contains an Assign "IncrementNumber" as
	 | variable      | value         |
	 | [[outnumber]] | =[[number]]+1 |
    When "WorkflowWtithMysqlToolUsingContainer" is executed
    Then the workflow execution has "AN" error
	And execution stopped on error and did not execute "IncrementNumber"
	And the "ToolUsingContainerAsTheSource" in Workflow "WorkflowWtithMysqlToolUsingContainer" has an error
	When I startup the mysql container
	And I select "NewMySqlSource" for "ToolUsingContainerAsTheSource" as Source 
	And I select "Pr_CitiesGetCountries" Action for "ToolUsingContainerAsTheSource" tool
	And "WorkflowWtithMysqlToolUsingContainer" is Saved
	And I resume workflow "WorkflowWtithMysqlToolUsingContainer" at "ToolUsingContainerAsTheSource" tool
	Then Resume has "NO" error
	And Resume message is "workflow not resumable"
	And the "IncrementNumber" in Workflow "WorkflowWtithMysqlToolUsingContainer" debug outputs as
	  | # |                    |
	  | 1 |  [[outnumber]] = 2 |
