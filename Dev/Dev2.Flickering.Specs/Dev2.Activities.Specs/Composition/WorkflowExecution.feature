@WorkflowExecution
Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Background: Setup for workflow execution
			Given Debug events are reset
			And All environments disconnected
			And Debug states are cleared

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the "WorkflowWithAssign" has a start and end duration
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssign' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssign' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |

Scenario: Sharepoint Acceptance Tests
	  Given I have a workflow "Sharepoint Acceptance Tests Outer"
	  And "Sharepoint Acceptance Tests Outer" contains "Sharepoint Connectors Testing" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Sharepoint Acceptance Tests Outer" is executed
	Then the workflow execution has "NO" error
	  And the 'Sharepoint Connectors Testing' in Workflow 'Sharepoint Acceptance Tests Outer' debug outputs as
	  |                   |
	  | [[Result]] = Pass |

Scenario: ForEach using * in CSV executed as a sub execution should maintain data integrity
	  Given I have a workflow "Spec - Test For Each Shared Memory"
	  And "Spec - Test For Each Shared Memory" contains "Test For Each Shared Memory" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Spec - Test For Each Shared Memory" is executed
	  Then the workflow execution has "NO" error	  
	  And the 'Test For Each Shared Memory' in Workflow 'Spec - Test For Each Shared Memory' debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: Workflow with AsyncLogging and ForEach
     Given I have a workflow "WFWithAsyncLoggingForEach"
     And "WFWithAsyncLoggingForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "3000"
	 And "ForEachTest" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[Warewolf]] | bob   |
	 When "WFWithAsyncLoggingForEach" is executed
	 Then the workflow execution has "NO" error
	 And I set logging to "Debug"
	 When "WFWithAsyncLoggingForEach" is executed "first time"
	 Then the workflow execution has "NO" error
	 And I set logging to "OFF"
	 	 When "WFWithAsyncLoggingForEach" is executed "second time"
	 Then the workflow execution has "NO" error
	 And the delta between "first time" and "second time" is less than "1200" milliseconds