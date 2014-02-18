Feature: WorkflowExecution
	In order to input data into a workflow and receive data from a workflow
	As a Warewolf user
	I want to be able to execute a workflow providing input data and receive data

Scenario Outline: ExcuteWorkflow using different variable notation
	Given I have a "SimpleWorkflow" workflow
	And the input variable "<inputVar>" as "<inValue>"
	And output variable is "<outputVar>"
	When I execute the workflow
	Then the execution has "<anError>" error
	And the output variable be "<outValue>"
	And the debug inputs as
	|                        |
	| <inputVar> = <inValue> |
	And the debug output as
	|                          |
	| <outputVar> = <outValue> |
Examples: 
| Name             | inputVar | inValue | outputVar | outValue | anError |
| Scalar To Scalar | [[a]]    | 1       | [[b]]     | 2        | NO      |