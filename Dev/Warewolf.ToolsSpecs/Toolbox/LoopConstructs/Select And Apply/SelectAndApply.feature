Feature: SelectAndApply
	In order to execute select and apply
	As a Warewolf user
	I want to add a tool that will allow me to construct and execute tools using an alias within the select and apply

#Scenario: Execute a selectAndApply over a tool using a recordset with 3 rows
#	Given I open New Workflow
#	And I drag a new Select and Apply tool to the design surface  
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |	
#	And Alias is "[[bob]]"
#	And Datasource is "[[rs(*).field]]"
#	And the underlying dropped activity is "SelectTestTool"
#	When the selectAndApply tool is executed
#	Then the selectAndApply executes 3 times
#	And the execution has "NO" error
#	And the debug inputs as
#	    | [[rs(*).field]] |
#	    | As = [[bob]]    |

#Scenario: Execute a selectAndApply over a tool using a complexobject with 3 properties
#	Given I open New Workflow
#	And I drag a new Select and Apply tool to the design surface  
#	Given There is a complexobject in the datalist with this shape
#	| rs              | value |
#	| [[Person().name]] | Micky |
#	| [[Person().name]] | John  |
#	| [[Person().name]] | Scott |  
#	And Alias is "[[bob]]"
#	And Datasource is "[[Person(*).name]]"
#	And the underlying dropped activity is "SelectTestTool"
#	When the selectAndApply tool is executed
#	Then the selectAndApply executes 3 times
#	And the execution has "NO" error
#	And the debug inputs as
#	    | [[Person(*).name]] |
#	    | As = [[bob]]    |

Scenario: Number Format tool with complext object
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given There is a complexobject in the datalist with this shape
	| rs                 | value |
	| [[Person().Level]] | 0.3   |
	| [[Person().Level]] | 0.45  |
	| [[Person().Level]] | 0.12  |  
	And Alias is "[[bob]]"
	And Datasource is "[[Person(*).Level]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[bob]] | Up       | 2              | 3                | [[bob]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person(1).Level]]" has a value of "0.300"
	And "[[Person(2).Level]]" has a value of "0.450"
	And "[[Person(3).Level]]" has a value of "0.120"

Scenario: Number Format tool with complext object deeper level
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given There is a complexobject in the datalist with this shape
	| rs                     | value |
	| [[Person.Score().Avg]] | 0.3   |
	| [[Person.Score().Avg]] | 0.45  |
	| [[Person.Score().Avg]] | 0.12  |
	And Alias is "[[Avg]]"
	And Datasource is "[[Person.Score(*).Avg]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Avg]] | Up       | 2              | 3                | [[Avg]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person.Score(1).Avg]]" has a value of "0.300"
	And "[[Person.Score(2).Avg]]" has a value of "0.450"
	And "[[Person.Score(3).Avg]]" has a value of "0.120"

@ignore
#Double stars not support at this stage
Scenario: Number Format tool with complext object multi array
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
	Given There is a complexobject in the datalist with this shape
	| rs                     | value |
	| [[Person().Score().Avg]] | 0.3   |
	| [[Person().Score().Avg]] | 0.45  |
	| [[Person().Score().Avg]] | 0.12  |
	And Alias is "[[Avg]]"
	And Datasource is "[[Person(*).Score(*).Avg]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Avg]] | Up       | 2              | 3                | [[Avg]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person(1).Score(1).Avg]]" has a value of "0.300"
	And "[[Person(2).Score(2).Avg]]" has a value of "0.450"
	And "[[Person(3).Score(3).Avg]]" has a value of "0.120"

#Scenario: Execute a selectAndApply over a tool using an empty recordset
#	Given I open New Workflow
#	And I drag a new Select and Apply tool to the design surface  
#	Given Alias is "[[bob]]"
#	And Datasource is "[[bs(*).field]]"
#	And the underlying dropped activity is "Tool"
#	When the selectAndApply tool is executed
#	Then the selectAndApply executes 0 times
#	And the execution has "AN" error

#Scenario: Execute a selectAndApply over a tool throws an error
#	Given I open New Workflow
#	And I drag a new Select and Apply tool to the design surface  
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |	
#	And Alias is "[[bob]]"
#	And Datasource is "[[rs(*).field]]"
#	And the underlying dropped activity is "Tool"
#	And the tool throws an error
#	When the selectAndApply tool is executed
#	Then the selectAndApply executes 3 times
#	And the execution has "AN" error
#	And the debug inputs as
#	    | [[rs(*).field]] |
#	    | As = [[bob]]    |