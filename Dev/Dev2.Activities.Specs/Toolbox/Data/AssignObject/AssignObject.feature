Feature: AssignObject
	In order to use json 
	As a Warewolf user
	I want a tool that assigns data to json objects

Scenario: Assign a value to a json object
	Given I assign the value "Bob" to a json object "[[Person.Name]]"
	When the assign object tool is executed
	Then the json object "[[Person.Name]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable			| New Value		|
	| 1 | [[Person.Name]] = | Bob			|
	And the debug output as
	| # |						|
	| 1 | [[Person.Name]] = Bob |

Scenario: Assign a variable to a json object
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the value "[[Person.Surname]]" to a json object "[[Person.FirstName]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable						| New Value						|
	| 1 | [[Person.FirstName]] =		| Bob							|
	| 2 | [[Person.Surname]] =			| Smith							|
	| 3 | [[Person.FirstName]] = Bob	| [[Person.Surname]] = Smith	|
	And the debug output as
	| # |								|
	| 1 | [[Person.FirstName]] = Bob	|
	| 2 | [[Person.Surname]] = Smith	|
	| 3 | [[Person.FirstName]] = Smith	|
