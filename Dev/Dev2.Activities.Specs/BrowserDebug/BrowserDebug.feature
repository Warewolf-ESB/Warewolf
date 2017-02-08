Feature: BrowserDebug
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Executing an empty workflow
Given I create a new unsaved workflow with name "Unsaved 1"
	  When '1' unsaved WF "Unsaved 1" is executed
	  Then the workflow execution has "AN" error

#Scenario: Executing a workflow with no inputs and outputs
#Scenario: Executing Assign workflow with invalid variable
#Scenario: Executing Assign workflow with inputs and outputs
#Scenario: Executing Assign workflow with invalid variable
#Scenario: Executing Hello World workflow
#Scenario: Executing a Sequence workflow
#Scenario: Executing a Foreach workflow
#Scenario: Executing a Dotnet plugin workflow
#Scenario: Executing a Recordset sort workflow