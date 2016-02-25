Feature: PluginConnector
	In order to use .net dlls
	As a warewolf user
	I want to be able to create plugin services

# Layout of tool not yet available



Scenario: Create new Plugin Tool
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 


Scenario: Create new Plugin Tool and Select a Source
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	When I select the Source "Echo"
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 


Scenario: Create new Plugin Tool and Select a Namespace
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	When I select the Source "Echo"
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	When I select the NameSpace "Echo Me" 
	Then Selected Namespace is "Echo Me"
	And Selected Method is Null
	And the available methods in the dropdown are
	| Name    |
	| Echome |
	| GetPeople  |
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 


Scenario: Create new Plugin Tool and Select a Namespace and Action
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	When I select the Source "Echo"	
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	When I select the NameSpace "Echo Me" 
	And I select the Method "GetPeople" 
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	And Selected Namespace is "Echo Me"
	And Selected Method is "GetPeople" 
	And the available methods in the dropdown are
	| Name    |
	| Echome |
	| GetPeople  |
	And Inputs are
	| Input | Default Value | Required Field | Empty Null |
	| Name  |               | False          | False      |
	| Value | Value         | False          | false      |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	And Validate is "Enabled"


Scenario: Create new Plugin Tool and Select a Namespace and Action and test
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	When I select the Source "Echo"	
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	When I select the NameSpace "Echo Me" 
	And I select the Method "GetPeople" 
	Then  "Sources" combobox is enabled
	And  Selected Source is "Echo"
	And the Namespaces are 
	| Name    |
	| Echo Me |
	| Person  |
	And Selected Namespace is "Echo Me"
	And Selected Method is "GetPeople" 
	And the available methods in the dropdown are
	| Name    |
	| Echome |
	| GetPeople  |
	And Inputs are
	| Input | Default Value | Required Field | Empty Null |
	| Name  |               | False          | False      |
	| Value | Value         | False          | false      |
	And Outputs are
	| Output | Output Alias |
	When I validate Sucessfully
	Then Outputs are
	| Output | Output Alias |
	| Name   | Name         |
	| Age    | Age          |


