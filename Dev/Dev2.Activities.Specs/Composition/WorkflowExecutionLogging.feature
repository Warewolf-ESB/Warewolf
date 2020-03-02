Feature: WorkflowExecutionLogging
    In order to get a detailed workflow execution log
    As a warewolf user
    I want to be able to list all entry to exit execution log points

Scenario: Workflow execution entry point detailed log
    Given "Hello World" stop on error is set to "false"
    And an existing workflow "Hello World"
	When a "Hello World" workflow request is received
    Then a detailed entry log is created
    | key           | value                         |
    | DsfDecision   | If [[Name]] <> (Not Equal)    |
    And it has these input parameter values
    | key       | value     |
    | [[Name]]  | World     |
    And execution is complete


Scenario: Workflow execution stops on error detailed logs
    Given "Hello World" stop on error is set to "true"
    And workflow execution entry point detailed logs are created and logged
    When a workflow stops on error
    Then execution is complete

Scenario: Workflow execution completed detailed logs
    Given "Hello World" stop on error is set to "false"
    And workflow execution entry point detailed logs are created and logged
    And a workflow stops on error has no logs
    Then a detailed execution completed log entry is created
    | key                       | value                         |
    | DsfMultiAssignActivity    | Set the output variable (1)   |
    And it has these output parameter values
    | key           | value         |
    | [[Message]]   | Hello World.  |
    And execution is complete

Scenario: Workflow execution failure detailed logs
    Given "Hello World" stop on error is set to "false"
    And workflow execution entry point detailed logs are created and logged
    And a workflow stops on error has no logs
    When a workflow execution has an exception
    Then a detailed execution exception log entry is created
    | one     | two     | three   |
    | value 1 | value 2 | value 3 |
    And a detailed execution completed log entry is has no logs
