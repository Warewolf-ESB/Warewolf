@Resources
Feature: WcfServiceConnector
	In Order to access an Wcf endpoint
	as a Warewolf user.

Scenario: create Wcf tool
	Given I open New Wcf Tool
	Then  "Sources" wcf combobox is enabled
	And  Selected wcf Source is null
	And Selected wcf Method is Null
	And wcf Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And wcf Outputs are
	| Output | Output Alias |
	And wcf Recordset is ""
	And there are "no" wcf validation errors of ""

Scenario: Create new wcf Tool and Select a Source
	Given I open New Wcf Tool
	Then  "Sources" wcf combobox is enabled
	And  Selected wcf Source is null
	And Selected wcf Method is Null
	And wcf Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And wcf Outputs are
	| Output | Output Alias |
	And wcf Recordset is ""
	And there are "no" wcf validation errors of ""
	When I select the wcf Source "Echo"
	Then  "Sources" wcf combobox is enabled
	And  Selected wcf Source is "Echo"
	And Selected wcf Method is Null
	And wcf Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And wcf Outputs are
	| Output | Output Alias |
	And wcf Recordset is ""
	And there are "no" wcf validation errors of ""

Scenario: Create new wcf Tool and Select a Action
	Given I open New Wcf Tool
	Then  "Sources" wcf combobox is enabled
	And  Selected wcf Source is null
	And Selected wcf Method is Null
	And wcf Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And wcf Outputs are
	| Output | Output Alias |
	And wcf Recordset is ""
	And there are "no" wcf validation errors of ""
	When I select the wcf Source "Echo"
	Then  "Sources" wcf combobox is enabled
	And  Selected wcf Source is "Echo"
	And I select the wcf Method "GetPeople" 
	Then "Sources" wcf combobox is enabled
	And  Selected wcf Source is "Echo"
	And Selected wcf Method is "GetPeople" 
	And the available wcf methods in the dropdown are
	| Name    |
	| Echome |
	| GetPeople |
	And wcf Inputs are
	| Input | Default Value | Required Field | Empty Null |
	| Name  |               | False          | False      |
	| Value | Value         | False          | false      |
	And wcf Outputs are
	| Output | Output Alias |
	And wcf Recordset is ""
	And there are "no" wcf validation errors of ""
	And Validate wcf is "Enabled"