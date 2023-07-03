Feature: RabbitMQWorkflowExecution
	In order to execute a workflow
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
			Given Debug events are reset
			And Debug states are cleared
	
@RabbitMQWorkflowExecution
Scenario:WF with DsfRabbitMq Consume timeout 5
	Given I depend on a valid RabbitMQ server
	And I have a workflow "RabbitMqConsume5mintimeout"
	And "RabbitMqConsume5mintimeout" contains DsfRabbitMQPublish and Queue1 "DsfPublishRabbitMQActivity" into "[[result1]]"
	And "RabbitMqConsume5mintimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout 5 seconds into "[[result]]"
	When "RabbitMqConsume5mintimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsume5mintimeout" has a start and end duration
	And "RabbitMqConsume5mintimeout" Duration is greater or equal to 5 seconds

@RabbitMQWorkflowExecution
Scenario:WF with RabbitMq Consume timeout 5
	Given I depend on a valid RabbitMQ server
	And I have a workflow "RabbitMqConsume5mintimeout"
	And "RabbitMqConsume5mintimeout" contains RabbitMQPublish and Queue1 - CorrelationID "PublishRabbitMQActivity" into "[[result1]]"
	And "RabbitMqConsume5mintimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout 5 seconds into "[[result]]"
	When "RabbitMqConsume5mintimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsume5mintimeout" has a start and end duration
	And "RabbitMqConsume5mintimeout" Duration is greater or equal to 5 seconds
	
@RabbitMQWorkflowExecution
Scenario:WF with RabbitMq Consume with no timeout 
	Given I have a workflow "RabbitMqConsumeNotimeout"
	And "RabbitMqConsumeNotimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout -1 seconds into "[[result]]"
	When "RabbitMqConsumeNotimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsumeNotimeout" has a start and end duration
	And "RabbitMqConsumeNotimeout" Duration is less or equal to 60 seconds

@COMIPCSaxonCSandStudioTests
Scenario: COM DLL service execute
	Given I have a server at "localhost" with workflow "Testing COM DLL Activity Execute"
	When "localhost" is the active environment used to execute "Testing COM DLL Activity Execute"
    Then the workflow execution has "No" error
	And the "Com DLL" in Workflow "Testing COM DLL Activity Execute" debug outputs is
	|                                |
	| [[PrimitiveReturnValue]] = <PrimitiveReturnValue>0</PrimitiveReturnValue> |

@NestedForEachExecution
Scenario: Workflow with ForEach and Manual Loop
      Given I have a workflow "WFWithForEachWithManualLoop"
	  And "WFWithForEachWithManualLoop" contains an Assign "Setup Counter" as
	    | variable    | value |
	    | [[counter]] | 0     |	
	  And "WFWithForEachWithManualLoop" contains an Assign "Increment Counter" as
	    | variable    | value          |
	    | [[counter]] | =[[counter]]+1 |
	  And "WFWithForEachWithManualLoop" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	  And "ForEachTest" contains an Assign "MyAssign" as
	    | variable    | value |
	    | [[rec().a]] | Test  |
	  And "WFWithForEachWithManualLoop" contains a Decision "Check Counter" as
		| ItemToCheck | Condition | ValueToCompareTo | TrueArmToolName | FalseArmToolName  |
		| [[counter]] | =         | 3                | End Result      | Increment Counter |	  	 	  
	  And "WFWithForEachWithManualLoop" contains an Assign "End Result" as
	    | variable   | value |
	    | [[result]] | DONE  |	 
      When "WFWithForEachWithManualLoop" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest" number '1' in WorkFlow "WFWithForEachWithManualLoop" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the "ForEachTest" number '1' in WorkFlow "WFWithForEachWithManualLoop" has "2" nested children 
	  And the "MyAssign" in step 1 for "ForEachTest" number '1' debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 1 for "ForEachTest" number '1' debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = Test |
	  And the "MyAssign" in step 2 for "ForEachTest" number '1' debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 2 for "ForEachTest" number '1' debug outputs as
		| # |                     |
		| 1 | [[rec(2).a]] = Test |
	  And the "ForEachTest" number '2' in WorkFlow "WFWithForEachWithManualLoop" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the "ForEachTest" number '2' in WorkFlow "WFWithForEachWithManualLoop" has "2" nested children 
	  And the "MyAssign" in step 1 for "ForEachTest" number '2' debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 1 for "ForEachTest" number '2' debug outputs as
		| # |                     |
		| 1 | [[rec(3).a]] = Test |
	  And the "MyAssign" in step 2 for "ForEachTest" number '2' debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 2 for "ForEachTest" number '2' debug outputs as
		| # |                     |
		| 1 | [[rec(4).a]] = Test |
