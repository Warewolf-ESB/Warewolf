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
 