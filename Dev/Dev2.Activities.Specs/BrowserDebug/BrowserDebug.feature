Feature: BrowserDebug
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Executing an empty workflow
		Given I have a workflow "BlankWorkflow"
		When "BlankWorkflow" is executed
		Then the workflow execution has "AN" error
		And I Debug "http://localhost:3142/secure/BlankWorkflow.debug?" in Browser
		And The Debug in Browser content contains "The workflow must have at least one service or activity connected to the Start Node."

Scenario: Executing a workflow with no inputs and outputs
		Given I have a workflow "AssignedVariableWF"
		When "AssignedVariableWF" is executed
		Then the workflow execution has "NO" error
		And I Debug "http://localhost:3142/secure/AssignedVariableWF.debug?" in Browser
		And The Debug in Browser content contains has children "The workflow must have at least one service or activity connected to the Start Node."

Scenario: Executing Assign workflow with inputs and outputs
		Given I have a workflow "AssignedVariableWF"
		And I assign the value "10" to a variable "[[var]]"
		When "AssignedVariableWF" is executed
		Then the workflow execution has "NO" error
		And I Debug "http://localhost:3142/secure/AssignedVariableWF.debug?" in Browser
		And The Debug in Browser content contains has inputs and outputs

Scenario: Executing Assign workflow with invalid variable
		Given I have a workflow "AssignedVariableWF"
		When "AssignedVariableWF" is executed
		Then the workflow execution has "AN" error
		And I Debug "http://localhost:3142/secure/AssignedVariableWF.debug?" in Browser
		And The Debug in Browser content contains has invalid variables "The workflow must have valid variables"


#Scenario: Executing Hello World workflow
#Scenario: Executing a Sequence workflow
#Scenario: Executing a Foreach workflow
#Scenario: Executing a Dotnet plugin workflow
#Scenario: Executing a Recordset sort workflow