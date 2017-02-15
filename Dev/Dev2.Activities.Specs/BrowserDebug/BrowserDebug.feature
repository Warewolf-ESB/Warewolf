Feature: BrowserDebug
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Executing an empty workflow
		Given I have a workflow "BlankWorkflow"
		When workflow "BlankWorkflow" is saved "1" time
		And I Debug "http://localhost:3142/secure/BlankWorkflow.debug?" in Browser
		Then The Debug in Browser content contains "The workflow must have at least one service or activity connected to the Start Node."

Scenario: Executing a workflow with no inputs and outputs
		Given I have a workflow "NoInputsWorkflow"
		When workflow "NoInputsWorkflow" is saved "1" time
		And I Debug "http://localhost:3142/secure/NoInputsWorkflow.debug?" in Browser
		Then The Debug in Browser content contains has children with no Inputs and Ouputs

Scenario: Executing Assign workflow with valid inputs
		Given I have a workflow "ValidAssignedVariableWF"
		And "ValidAssignedVariableWF" contains an Assign "ValidAssignVariables" as
		  | variable    | value |
		  | dateMonth | February |
		  | dateYear | 2017 |
		When workflow "ValidAssignedVariableWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/ValidAssignedVariableWF.debug?" in Browser
		Then The Debug in Browser content contains has inputs and outputs

Scenario: Executing Assign workflow with invalid variable
		Given I have a workflow "InvalidAssignedVariableWF"
		And "InvalidAssignedVariableWF" contains an Assign "InvalidAssignVariables" as
		  | variable    | value |
		  | d@teMonth | February |
		When workflow "InvalidAssignedVariableWF" is saved "1" time
		And I Debug "http://rsaklfdylan:3142/secure/Acceptance%20Tests/InvalidAssignedVariableWF.debug?" in Browser
		Then The Debug in Browser content contains has error messagge ""invalid variable assigned to d@teMonth""


#Scenario: Executing Hello World workflow
#Scenario: Executing a Sequence workflow
#Scenario: Executing a Foreach workflow
#Scenario: Executing a Dotnet plugin workflow
#Scenario: Executing a Recordset sort workflow