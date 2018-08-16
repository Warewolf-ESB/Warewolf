Feature: WorkflowResume
	When a workflow execution fails
	I want to Resume
	
Scenario: Resuming a workflow that had failed to connect
	Given I have a workflow "WorkflowWithMysqlToolUsingContainer"
	And "WorkflowWithMysqlToolUsingContainer" contains an Assign "AssignNumber" as
	 | variable   | value |
	 | [[number]] | 1     |
	And "WorkflowWithMysqlToolUsingContainer" contains a mysql database service "ToolUsingContainerAsTheSource"
	And "WorkflowWithMysqlToolUsingContainer" contains an Assign "IncrementNumber" as
	 | variable      | value         |
	 | [[outnumber]] | =[[number]]+1 |
    When "WorkflowWithMysqlToolUsingContainer" is executed
    Then the workflow execution has "AN" error
	And execution stopped on error and did not execute "IncrementNumber"
	And the "ToolUsingContainerAsTheSource" in Workflow "WorkflowWithMysqlToolUsingContainer" has an error
	When I startup the mysql container
	And I select "NewMySqlSource" for "ToolUsingContainerAsTheSource" as Source 
	And I select "Pr_CitiesGetCountries" Action for "ToolUsingContainerAsTheSource" tool
	And "WorkflowWithMysqlToolUsingContainer" is Saved
	And I resume workflow "WorkflowWithMysqlToolUsingContainer" at "ToolUsingContainerAsTheSource" tool
	Then Resume has "NO" error
	And Resume message is "workflow not resumable"
	And the "IncrementNumber" in Workflow "WorkflowWithMysqlToolUsingContainer" debug outputs as
	  | # |                    |
	  | 1 |  [[outnumber]] = 2 |
