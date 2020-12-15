Feature: WorkflowResume
When a workflow execution fails
I want to Resume

  @ResumeWorkflowExecution
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
    And Resume message is "Execution Completed."
    And the "IncrementNumber" in Workflow "WorkflowWithMysqlToolUsingContainer" debug outputs as
      | # |                   |
      | 1 | [[outnumber]] = 2 |


  Scenario: Resuming a workflow Given No Name And Resume From SetTheOutputVariable tool
    Given I have a server at "localhost" with workflow "Hello World"
    And Workflow "Hello World" has "Set the output variable (1)" activity
    And I resume workflow "Hello World" at "Set the output variable (1)" tool
    Then Resume has "AN" error
    And Resume message is "Scalar value { Name } is NULL"


  Scenario: Resuming a workflow Given Resume From AssignValueToNameIfBlank Tool
    Given I have a server at "localhost" with workflow "Hello World"
    And Workflow "Hello World" has "Assign a value to Name if blank (1)" activity
    And I resume workflow "Hello World" at "Assign a value to Name if blank (1)" tool
    Then Resume has "NO" error
    And Resume message is "Execution Completed."

  @ResumeWorkflowExecution
  Scenario: Resuming Workflow From a specific Version
    Given I have a workflow "ResumeWorkflowFromVersion"
    And "ResumeWorkflowFromVersion" contains an Assign "VarsAssign" as
      | variable    | value |
      | [[rec().a]] | New   |
      | [[rec().a]] | Test  |
    When workflow "ResumeWorkflowFromVersion" is saved "1" time
    Then workflow "ResumeWorkflowFromVersion" has "0" Versions in explorer
    When "WorkflowWithAssignAndCount" is executed
    Then the workflow execution has "NO" error
    And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as
      | # |                     |
      | 1 | [[rec(1).a]] = New  |
      | 2 | [[rec(2).a]] = Test |
    Then I update "ResumeWorkflowFromVersion" by adding "AnotherVarsAssign" as
      | variable     | value              |
      | [[variable]] | NewlyAddedVariable |
    When workflow "ResumeWorkflowFromVersion" is saved "1" time
    Then I update "ResumeWorkflowFromVersion" by adding "ThirVarAssign" as
      | variable                | value               |
      | [[ThirdAssignVariable]] | ThirdAssignVariable |
    When workflow "ResumeWorkflowFromVersion" is saved "1" time
    And I reload Server resources
    And I resume the workflow "ResumeWorkflowFromVersion" at "VarsAssign" from version "2"
    Then Resume has "NO" error
    And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as
      | # |                     |
      | 1 | [[rec(1).a]] = New  |
      | 2 | [[rec(2).a]] = Test |
