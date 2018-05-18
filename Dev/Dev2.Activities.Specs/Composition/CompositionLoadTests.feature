@CompositionLoadTests
Feature: CompositionLoadTests
	In order to execute a workflow
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
	Given Debug events are reset
	And Debug states are cleared

Scenario: Workflow with AsyncLogging and ForEach
    Given I have a workflow "WFWithAsyncLoggingForEach"
    And "WFWithAsyncLoggingForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2000"
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
	And the delta between "first time" and "second time" is less than "2600" milliseconds

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the "WorkflowWithAssign" has a start and end duration
	  And the "Rec To Convert" in WorkFlow "WorkflowWithAssign" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the "Rec To Convert" in Workflow "WorkflowWithAssign" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
 