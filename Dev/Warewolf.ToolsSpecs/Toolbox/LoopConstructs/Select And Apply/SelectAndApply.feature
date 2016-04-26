Feature: SelectAndApply
	In order to execute select and apply
	As a Warewolf user
	I want to add a tool that will allow me to construct and execute tools using an alias within the select and apply

Scenario: Execute a selectAndApply tool with a mocked test tool with a json object array
	Given There is a complexobject in the datalist with this shape
	| rs			| value |
	| [[Score()]]	| 0.3   |
	| [[Score()]]	| 0.45  |
	| [[Score()]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[Score(*)]]"
	And the underlying dropped activity is a mocked test tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	    | [[Score(*)]]		|
	    | As = [[Score]]	|

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[Person.Score]]	| 0.3	|
	And Alias is "[[Score]]"
	And Datasource is "[[Person.Score]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person.Score]]" has a value of "0.300"

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[Score().Value]]	| 0.3   |
	| [[Score().Value]]	| 0.45  |
	| [[Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[Score(*).Value]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Score(1).Value]]" has a value of "0.300"
	And "[[Score(2).Value]]" has a value of "0.450"
	And "[[Score(3).Value]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object array of literals
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[Score()]]	| 0.3   |
	| [[Score()]]	| 0.45  |
	| [[Score()]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[Score(*)]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Score(1)]]" has a value of "0.300"
	And "[[Score(2)]]" has a value of "0.450"
	And "[[Score(3)]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with json object array within a json object
	Given There is a complexobject in the datalist with this shape
	| rs					| value |
	| [[Person.Score()]]	| 0.3	|
	| [[Person.Score()]]	| 0.45	|
	| [[Person.Score()]]	| 0.12	|
	And Alias is "[[Score]]"
	And Datasource is "[[Person.Score(*)]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person.Score(1)]]" has a value of "0.300"
	And "[[Person.Score(2)]]" has a value of "0.450"
	And "[[Person.Score(3)]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object array within a json object array
	Given There is a complexobject in the datalist with this shape
	| rs                    | value |
	| [[Person().Score()]]	| 0.3   |
	| [[Person().Score()]]	| 0.45  |
	| [[Person().Score()]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[Person(*).Score(*)]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[Person(1).Score(1)]]" has a value of "0.300"
	And "[[Person(2).Score(2)]]" has a value of "0.450"
	And "[[Person(3).Score(3)]]" has a value of "0.120"

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