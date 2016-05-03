Feature: SelectAndApply
	In order to execute select and apply
	As a Warewolf user
	I want to add a tool that will allow me to construct and execute tools using an alias within the select and apply

Scenario: Execute a selectAndApply tool with a mocked test tool with a json object array of json objects
	Given There is a complexobject in the datalist with this shape
	| rs				| value |
	| [[Score().Value]]	| 0.3   |
	| [[Score().Value]]	| 0.45  |
	| [[Score().Value]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[Score(*).Value]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	| [[Score(*).Value]]	|
	| As = [[Score]]		|

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

# invalid input, should fail on validation as this is not a collection
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
	Then the execution has "AN" error

# invalid input, should fail on validation as this is not a collection
Scenario: Execute a selectAndApply tool with a Number Format tool with a json object literal
	Given There is a complexobject in the datalist with this shape
	| rs						| value |
	| [[Person.Score.Value]]	| 0.3	|
	And Alias is "[[Score]]"
	And Datasource is "[[Person.Score.Value]]"
	And I use a Number Format tool configured as
		| Number  | Rounding | Rounding Value | Decimals to show | Result  |
		| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "AN" error

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

# copied from forEach
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
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |	
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
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |	
	| 4     |
	And the execution has "NO" error

Scenario Outline: Execute a selectAndApply over a tool 
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	And Alias is "[[rs]]"
	And Datasource is "[[rs(*).field]]"
	And I have a variable "<Variable>" with the value "<value>"
	And I have selected the selectAndApply type as "<Type>" and used "<Variable>"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the selectAndApply executes <value> times
	And the execution has "<Error>" error
Examples: 
| Type           | Variable     | value | Error | Message                                                  |
| InRecordset    | " "          | 0     | AN    | Invalid Recordset                                        |
| InRecordset    | 11           | 0     | AN    | Invalid Recordset                                        |
| InRecordset    | Test         | 0     | AN    | Invalid characters have been entered as Recordset        |
| InRecordset    | [[var]]      | 0     | AN    | Scalar not allowed                                       |
| InRecordset    | [[q]]        | 0     | AN    | Scalar not allowed                                       |
| InRecordset    | [[rec(1).a]] | 0     | AN    |                                                          |
# TODO add complex object scenarios

Scenario: Execute a selectAndApply over a tool null alias
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[rs().field]] | 1		|
	And Alias is ""
	And Datasource is "[[rs(*).field]]"
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

Scenario: Execute a selectAndApply over a tool null value
	Given There is a recordset in the datalist with this shape
	| rs             | value	|
	| [[rs().field]] | NULL		|
	And Alias is ""
	And Datasource is "[[rs(*).field]]"
	And the underlying dropped activity is a(n) "SelectAndApplyTestTool" tool
	When the selectAndApply tool is executed
	Then the execution has "AN" error

# failing - not sure y
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

@ignore
#Double stars not support at this stage
Scenario: Number Format tool with complext object multi array
	Given I open New Workflow
	And I drag a new Select and Apply tool to the design surface  
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
