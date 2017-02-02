@SubworkflowExecution
Feature: SubworkflowExecution
	In order to execute a subworkflow
	As a Warewolf user
	I want to be able to build workflows that contain other workflows from a running server instance
	 
Background: Setup for subworkflow execution
			Given Debug events are reset
			And Debug states are cleared


Scenario: Workflow with an assign and remote workflow
	Given I have a workflow "TestAssignWithRemoteWF"
	 And "TestAssignWithRemoteWF" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	And "TestAssignWithRemoteWF" contains "WorkflowUsedBySpecs" from server "Remote Connection Integration" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable      |
	| inputData        | [[inputData]] | output              | [[output]]       |
	|                  |               | values(*).up        | [[values().up]]  |
	|                  |               | values(*).low       | [[values().low]] |
	  When "TestAssignWithRemoteWF" is executed
	  Then the workflow execution has "NO" error
	   And the "AssignData" in WorkFlow "TestAssignWithRemoteWF" debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the "AssignData" in Workflow "TestAssignWithRemoteWF" debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the "WorkflowUsedBySpecs" in WorkFlow "TestAssignWithRemoteWF" debug inputs as
	  |                       |
	  | [[inputData]] = hello |
	  And the "Setup Assign (1)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                |
	  | 1 | [[in]] = hello |
	  And the "Convert Case (1)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                |
	  | 1 | [[in]] = HELLO |
	  And the "Final Assign (3)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                             |
	  | 1 | [[output]] = HELLO          |
	  | 2 | [[values(1).up]] = HELLO |
	  | 3 | [[values(1).low]] = hello |	  	 
	  And the "WorkflowUsedBySpecs" in Workflow "TestAssignWithRemoteWF" debug outputs as
	  |                           |
	  | [[values(1).up]] = HELLO  |
	  | [[values(1).low]] = hello |
	  | [[output]] = HELLO        |
	
Scenario: Executing Workflow Service and Decision tool expected bubling out error in workflow service
	  Given I have a workflow "Utility - Assign WF"
	  And "Utility - Assign WF" contains "Utility - Assign" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable        |
	  |                  |               | rec(*).set      | [[myrec().set]]      |
	  |                  |               | hero(*).pushups | [[thehero().pushups]] |
	  |                  |               | hero(*).name    | [[thehero().name]]    |
	  When "Utility - Assign WF" is executed
	  Then the workflow execution has "NO" error
	  And the "Utility - Assign" in Workflow "Utility - Assign WF" debug outputs as    
	  |                                                                   |
	  | [[myrec(1).set]] =    Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	  | [[thehero(1).pushups]] = All of them.                                |
	  | [[thehero(1).name]] =   Chuck Norris                                 |

Scenario: Error from workflow service is expected to buble out
	  Given I have a workflow "TestAssignWithRemoteOutputsErrors"
	  And "TestAssignWithRemoteOutputsErrors" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	  And "TestAssignWithRemoteOutputsErrors" contains "WorkflowUsedBySpecs" from server "Remote Connection Integration" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  | inputData        | [[inputData]] | output              | [[output]]       |
	  |                  |               | values(*).up     | [[values().&up]] |
	  |                  |               | values(*).low     | [[values().low]] |
	  When "TestAssignWithRemoteOutputsErrors" is executed
	  Then the "TestAssignWithRemoteOutputsErrors" workflow execution has "AN" error
	  And the "AssignData" in WorkFlow "TestAssignWithRemoteOutputsErrors" debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the "AssignData" in Workflow "TestAssignWithRemoteOutputsErrors" debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the "WorkflowUsedBySpecs" in WorkFlow "TestAssignWithRemoteOutputsErrors" debug inputs as
	  |                       |
	  | [[inputData]] = hello |
	  And the "Setup Assign (1)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                |
	  | 1 | [[in]] = hello |
	  And the "Convert Case (1)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                |
	  | 1 | [[in]] = HELLO |
	  And the "Final Assign (3)" in Workflow "WorkflowUsedBySpecs" debug outputs as
	  | # |                             |
	  | 1 | [[output]] = HELLO          |
	  | 2 | [[values(1).up]] = HELLO |
	  | 3 | [[values(1).low]] = hello |	  	 

Scenario Outline: Workflow to Workflow Mappings 
Given I have a workflow "<Name>"
And "<Name>" contains an Assign "AssignData" as
        | variable         | value         |
        | <AssignVariable> | <AssignValue> |
And "<Name>" contains "WorkflowMappingsInnerWorkflow" from server "localhost" with mapping as
| From Variable  | Input to Service | Output from Service | To Variable  |
| <FromVariable> | <ToService>      | <FromService>       | <ToVariable> |
When "<Name>" is executed
Then the workflow execution has "NO" error
And the "WorkflowMappingsInnerWorkflow" in WorkFlow "<Name>" debug inputs as
      |                        |
      | <ToServiceAssignValue> |	  
And the "WorkflowMappingsInnerWorkflow" in Workflow "<Name>" debug outputs as
      |                        |
      |  <ToVariableAndResult> |
Examples: 
| Name                       | AssignVariable | AssignValue | FromVariable   | ToService  | FromService | ToVariable     | ToServiceAssignValue   | ToVariableAndResult    |
| ScalToRecInAndScaltoRecOut | [[OuterIn]]    | hello       | [[OuterIn]]    | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| BlnkToRecIn                |                |             |                | in(*).in   | InnerOutput | [[OuterOut]]   |                        | [[OuterOut]] =         |
| BlnkToScalIn               |                |             |                | InnerInput | InnerOutput | [[OuterOut]]   |                        | [[OuterOut]] =         |
| HdCdScalToRecInSclToSclOut | [[OuterIn]]    | ll          | he[[OuterIn]]o | in(*).in   | InnerOutput | [[OuterOut]]   | [[in(1).in]] = hello   | [[OuterOut]] = hello   |
| RecToRecInAndRecOut        | [[rec().out]]  | hello       | [[rec().out]]  | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| ScalToScalIn&ScalOut       | [[OuterIn]]    | hello       | [[OuterIn]]    | InnerInput | InnerOutput | [[OuterOut]]   | [[InnerInput]] = hello | [[OuterOut]] = hello   |
| Static                     |                |             | hello          | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| RecToScalOut               | [[rec().in]]   | hello       | [[rec().in]]   | InnerInput | InnerOutput | [[Output]]     | [[InnerInput]] = hello | [[Output]] = hello     |
| RecToBlank                 | [[rec().in]]   | hello       | [[rec().in]]   | InnerInput | InnerOutput |                | [[InnerInput]] = hello |                        |
| ScalToBlank                | [[var]]        | hello       | [[var]]        | InnerInput | InnerOutput |                | [[InnerInput]] = hello |                        |


 Scenario: Executing Asynchrounous testing workflow base
	  Given I have a workflow "Testing - Async Test Master Testc"
	  And "Testing - Async Test Master Testc" contains "Async Test Master" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Testing - Async Test Master Testc" is executed
	  Then the workflow execution has "NO" error
	  And the "Async Test Master" in Workflow "Async Test Master" debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: MYSQL backward Compatiblity
	Given I have a workflow "MySQLMigration"
	And "MySQLMigration" contains "MySQLDATA" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      |                  |               | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "MySQLMigration" is executed
	Then the workflow execution has "NO" error

Scenario: Data connector backward Compatiblity
	Given I have a workflow "DataMigration"
	And "DataMigration" contains "DataCon" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      | [[ProductId]]    | productId     | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "DataMigration" is executed
	Then the workflow execution has "NO" error

Scenario: Mappings from nested workflow
	Given I have a workflow "OutterWolf1371"
	And "OutterWolf1371" contains "Wolf-1371" from server "localhost" with mapping as
         | Input to Service | From Variable | Output from Service | To Variable |
         | [[b]]            | b             | a                   | [[a]]       |
	When "OutterWolf1371" is executed
	Then the workflow execution has "NO" error

Scenario: Plugin connector backward Compatiblity
	Given I have a workflow "PluginMigration"
	And "PluginMigration" contains "PluginService" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service      | To Variable          |
      | [[s]]            | s             | [[PrimitiveReturnValue]] | PrimitiveReturnValue |
	When "PluginMigration" is executed
	Then the workflow execution has "NO" error

@UsingRemoteResources
Scenario: Executing WF on a remote server 
         Given I have a workflow "Testing - TestRemoteTools"
         And "Testing - TestRemoteTools" contains "TestRemoteTools" from server "Remote Connection Integration" with mapping as
         | Input to Service | From Variable | Output from Service | To Variable      |
         When "Testing - TestRemoteTools" is executed
         Then the workflow execution has "NO" error     

		 
Scenario: ForEach with NestedStarTest and Inner WF
	  Given I have a workflow "ForEach Output2"
	  And "ForEach Output2" contains "TestInnerWFForEachOutputs" from server "localhost" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "ForEach Output2" is executed
	Then the workflow execution has "NO" error
	And the "TestInnerWFForEachOutputs" in Workflow "ForEach Output2" debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: Workflow with Performance counters
	  Given I have a workflow "PerfCounterTest"
	  And I have reset local perfromance Counters
	  And "PerfCounterTest" contains "PerfCounter" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "PerfCounterTest" is executed
	Then the perfcounter raw values are
	| Name                                              | Value |
	| Average workflow execution time                   | x     |
	| Concurrent requests currently executing           | 0     |
	| Count of Not Authorised errors                    | 0     |
	| Total Errors                                      | 4     |
	| Request Per Second                                | x     |
	| Count of requests for workflows which don’t exist | 9     |

Scenario: Sharepoint Acceptance Tests
	  Given I have a workflow "Sharepoint Acceptance Tests Outer"
	  And "Sharepoint Acceptance Tests Outer" contains "Sharepoint Connectors Testing" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Sharepoint Acceptance Tests Outer" is executed
	Then the workflow execution has "NO" error
	  And the "Sharepoint Connectors Testing" in Workflow "Sharepoint Acceptance Tests Outer" debug outputs as
	  |                   |
	  | [[Result]] = Pass |

Scenario: ForEach using * in CSV executed as a sub execution passes out an ordered recordset
	  Given I have a workflow "Spec - Test For Each Shared Memory"
	  And "Spec - Test For Each Shared Memory" contains "Test For Each Shared Memory" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Spec - Test For Each Shared Memory" is executed
	  Then the workflow execution has "NO" error	  
	  And the "Test For Each Shared Memory" in Workflow "Spec - Test For Each Shared Memory" debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: Ensure that End this Workflow is working 
	  Given I have a workflow "EndNestedWorkflows"
	  And "EndNestedWorkflows" contains "Testing/Bugs/wolf-402" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "EndNestedWorkflows" is executed
	Then the workflow execution has "NO" error

Scenario: Xml Serialisation bug when returning xml
	Given I have a workflow "XmlSerialisation"
	And "XmlSerialisation" contains "Testing/Bugs/wolf-829" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "XmlSerialisation" is executed
	Then the workflow execution has "NO" error

Scenario: Mixing Scalar And Recordset bug 
	Given I have a workflow "MixingScalarAndRecordset"
	And "MixingScalarAndRecordset" contains "Testing/Bugs/Wolf-860" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "MixingScalarAndRecordset" is executed
	Then the workflow execution has "NO" error

Scenario: ForEach using * and web get request with error
	  Given I have a workflow "Spec - Test For Each  Get"
	  And "Spec - Test For Each  Get" contains "GetRequestErrorHandling" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Spec - Test For Each  Get" is executed
	  Then the workflow execution has "AN" error	  
	  And the "GetRequestErrorHandling" in Workflow "Spec - Test For Each  Get" debug outputs as
	  |                   |
	  | [[Result]] = Pass |


Scenario: Error not bubbling up
	Given I have a workflow "Wolf-1212_Test"
	And "Wolf-1212_Test" contains "ErrorHandled" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  |                  |               | Error              | [[Error]]  |
	When "Wolf-1212_Test" is executed
	Then the workflow execution has "NO" error
	And the "ErrorHandled" in Workflow "Wolf-1212_Test" debug outputs as
	  |                                                                                                                  |
	  | [[Result]] = Fail                                                                                                |
	  | [[Error]] = Could not parse input datetime with given input format (if you left the input format blank then even |


Scenario: Error not bubbling up error message
	Given I have a workflow "Wolf-1212_2"
	And "Wolf-1212_2" contains "ErrorBubbleUp" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "Wolf-1212_2" is executed
	Then the workflow execution has "NO" error
	And the "ErrorBubbleUp" in Workflow "Wolf-1212_2" debug outputs as
	  |                   |
	  | [[Result]] = Pass |

Scenario: Rabbit MQ Test
	  Given I have a workflow "RabbitMQ Tester WF"
	  And "RabbitMQ Tester WF" contains "RabbitMQTest" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | result              | [[result]]  |
	  When "RabbitMQ Tester WF" is executed
	Then the workflow execution has "NO" error
	  And the "RabbitMQTest" in Workflow "RabbitMQ Tester WF" debug outputs as
	  |                   |
	  | [[result]] = Pass |