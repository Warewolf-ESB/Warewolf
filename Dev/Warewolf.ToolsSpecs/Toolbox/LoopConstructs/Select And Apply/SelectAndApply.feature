Feature: SelectAndApply
	In order to execute select and apply
	As a Warewolf user
	I want to add a tool that will allow me to construct and execute tools using an alias within the select and apply

Scenario: Execute a selectAndApply tool with a mocked test tool with a recordSet array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[@Score().Value]]	| 0.3   |
	| [[@Score().Value]]	| 0.45  |
	| [[@Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*).Value]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	| [[@Score(*).Value]]	|
	| As = [[Score]]		|

Scenario: Execute a selectAndApply tool with a mocked test tool with a json object array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[@Score().Value]]	| 0.3   |
	| [[@Score().Value]]	| 0.45  |
	| [[@Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*).Value]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	| [[@Score(*).Value]]	|
	| As = [[Score]]		|

Scenario: Execute a selectAndApply tool with a Number Format tool with a recordSet array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[@Score().Value]]	| 0.3   |
	| [[@Score().Value]]	| 0.45  |
	| [[@Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*).Value]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Score(1).Value]]" has a value of "0.300"
	And "[[@Score(2).Value]]" has a value of "0.450"
	And "[[@Score(3).Value]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[@Score().Value]]	| 0.3   |
	| [[@Score().Value]]	| 0.45  |
	| [[@Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*).Value]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Score(1).Value]]" has a value of "0.300"
	And "[[@Score(2).Value]]" has a value of "0.450"
	And "[[@Score(3).Value]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a Recordset array of literals
	Given There is a complexobject in the datalist with this shape
	| rs          | value |
	| [[@Score()]] | 0.3   |
	| [[@Score()]] | 0.45  |
	| [[@Score()]] | 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*)]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Score(1)]]" has a value of "0.300"
	And "[[@Score(2)]]" has a value of "0.450"
	And "[[@Score(3)]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object array of literals
	Given There is a complexobject in the datalist with this shape
	| rs          | value |
	| [[@Score()]] | 0.3   |
	| [[@Score()]] | 0.45  |
	| [[@Score()]] | 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Score(*)]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Score(1)]]" has a value of "0.300"
	And "[[@Score(2)]]" has a value of "0.450"
	And "[[@Score(3)]]" has a value of "0.120"

Scenario: Execute a selectAndApply tool with a Number Format tool with a RecordSet
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

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[@Person.Score]]	| 0.3	|
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Score]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error

Scenario: Execute a selectAndApply tool with a Number Format tool with a RecordSet literal
	Given There is a complexobject in the datalist with this shape
	| rs						| value |
	| [[Person.Score.Value]]	| 0.3	|
	And Alias is "[[Score]]"
	And Datasource is "[[Person.Score.Value]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error

Scenario: Execute a selectAndApply tool with a Number Format tool with a json object literal
	Given There is a complexobject in the datalist with this shape
	| rs						| value |
	| [[@Person.Score.Value]]	| 0.3	|
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Score.Value]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error

Scenario: Execute a selectAndApply tool with a Number Format tool with json object array within a json object
	Given There is a complexobject in the datalist with this shape
	| rs					| value |
	| [[@Person.Score()]]	| 0.3	|
	| [[@Person.Score()]]	| 0.45	|
	| [[@Person.Score()]]	| 0.12	|
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Score(*)]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Score(1)]]" has a value of "0.300"
	And "[[@Person.Score(2)]]" has a value of "0.450"
	And "[[@Person.Score(3)]]" has a value of "0.120"

Scenario: Execute a selectAndApply over a tool using a recordset with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	| [[rs(*).field]]	|
	| As = [[rs]]		|

Scenario: Execute a selectAndApply over a tool using a JSON Object with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[@rs().field]] | 1     |
	| [[@rs().field]] | 2     |
	| [[@rs().field]] | 3     |
	And Alias is "[[rs]]"
	And Datasource is "[[@rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	| [[@rs(*).field]]	|
	| As = [[rs]]		|

Scenario: Execute a selectAndApply over a tool using a recordset with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	| [[rs().field]] | 6     |
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 4 times
	And the execution has "NO" error
	And the debug inputs as
	| [[rs(*).field]]	|
	| As = [[rs]]		|

Scenario: Execute a selectAndApply over a tool using a JSON object with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[@rs().field]] | 1     |
	| [[@rs().field]] | 2     |
	| [[@rs().field]] | 3     |
	| [[@rs().field]] | 6     |
	And Alias is "[[rs]]"
	And Datasource is "[[@rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 4 times
	And the execution has "NO" error
	And the debug inputs as
	| [[@rs(*).field]]	|
	| As = [[rs]]		|

Scenario: Execute a selectAndApply over an activity using a recordset with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]"
	And the underlying dropped activity is a(n) "Activity" tool
	When the selectAndApply tool is executed
	And the execution has "NO" error

Scenario: Execute a selectAndApply over an activity using a JSON Object with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]"
	And the underlying dropped activity is a(n) "Activity" tool
	When the selectAndApply tool is executed
	And the execution has "NO" error

Scenario: Execute a selectAndApply over an activity using a recordset with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	| [[rs().field]] | 6     |	
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]"
	And the underlying dropped activity is a(n) "Activity" tool
	When the selectAndApply tool is executed
	And the execution has "NO" error

Scenario: Execute a selectAndApply over an activity using a JSON Object with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	| [[rs().field]] | 6     |	
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]"
	And the underlying dropped activity is a(n) "Activity" tool
	When the selectAndApply tool is executed
	And the execution has "NO" error

Scenario: Execute a selectAndApply over a tool null alias
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[rs().field]] | 1		|
	And Alias is ""
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

	Scenario: Execute a selectAndApply over a tool null alias for JSON Objects
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[@rs().field]] | 1		|
	And Alias is ""
	And Datasource is "[[@rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error


Scenario: Execute a selectAndApply over a tool null datasource
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[rs().field]] | 1		|
	And Alias is ""
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

Scenario: Execute a selectAndApply over a tool null datasource for JSON Objects
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[@rs().field]] | 1		|
	And Alias is ""
	And Datasource is "[[@rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

Scenario: Execute a selectAndApply over a tool null value
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[rs().field]] | NULL		|
	And Alias is ""
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

Scenario: Execute a selectAndApply over a tool null value for JSON Objects
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[@rs().field]] | NULL		|
	And Alias is ""
	And Datasource is "[[@rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

Scenario: Number Format tool with complext object multi array
	Given There is a complexobject in the datalist with this shape
	| rs                    | value |
	| [[@Person().Score()]]	| 0.3   |
	| [[@Person().Score()]]	| 0.45  |
	| [[@Person().Score()]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person(*).Score(*)]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person(1).Score(1)]]" has a value of "0.300"
	And "[[@Person(2).Score(1)]]" has a value of "0.450"
	And "[[@Person(3).Score(1)]]" has a value of "0.120"

Scenario: Number Format tool with complext object multi array and field
	Given There is a complexobject in the datalist with this shape
	| rs                    | value |
	| [[@Person.Member().Team().Score]]	| 0.3   |
	| [[@Person.Member().Team().Score]]	| 0.45  |
	| [[@Person.Member().Team().Score]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member().Team().Score]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member(1).Team(1).Score]]" has a value of "0.300"
	And "[[@Person.Member(2).Team(1).Score]]" has a value of "0.450"
	And "[[@Person.Member(3).Team(1).Score]]" has a value of "0.120"

Scenario: Number Format tool with complex object multi array and field and multi values
	Given There is a complexobject in the datalist with this shape
	| rs                                  | value |
	| [[@Person.Member(1).Team(1).Score]] | 0.3   |
	| [[@Person.Member(1).Team(2).Score]] | 0.45  |
	| [[@Person.Member(2).Team(1).Score]] | 0.12  |
	| [[@Person.Member(2).Team(2).Score]] | 0.11  |
	| [[@Person.Member(2).Team(3).Score]] | 0.13  |
	| [[@Person.Member(3).Team(1).Score]] | 0.14  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member().Team().Score]]"
	And I use a Number Format tool configured as
		| Number    | Rounding | Rounding Value | Decimals to show | Result    |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member(1).Team(1).Score]]" has a value of "0.300"
	And "[[@Person.Member(1).Team(2).Score]]" has a value of "0.450"
	And "[[@Person.Member(2).Team(1).Score]]" has a value of "0.120"
	And "[[@Person.Member(2).Team(2).Score]]" has a value of "0.110"
	And "[[@Person.Member(2).Team(3).Score]]" has a value of "0.130"
	And "[[@Person.Member(3).Team(1).Score]]" has a value of "0.140"

Scenario: Number Format tool with complex non array
	Given There is a complexobject in the datalist with this shape
	| rs                    | value |
	| [[@Person.Member.Team.Score]]	| 0.3   |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member.Team.Score]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member.Team.Score]]" has a value of "0.300"
