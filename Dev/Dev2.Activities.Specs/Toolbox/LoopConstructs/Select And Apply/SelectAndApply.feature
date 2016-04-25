Feature: SelectAndApply
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Execute a selectAndApply over a tool using a recordset with 3 rows
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |	
	And Alias is "[[bob]]"
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is "SelectTestTool"
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	    | [[rs(*).field]] |
	    | As = [[bob]]    |


Scenario: Execute a selectAndApply over a tool using an empty recordset
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given Alias is "[[bob]]"
	And Datasource is "[[bs(*).field]]"
	And the underlying dropped activity is "Tool"
	When the selectAndApply tool is executed
	Then the selectAndApply executes 0 times
	And the execution has "AN" error

Scenario: Execute a selectAndApply over a tool throws an error
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |	
	And Alias is "[[bob]]"
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is "Tool"
	And the tool throws an error
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "AN" error
	And the debug inputs as
	    | [[rs(*).field]] |
	    | As = [[bob]]    |