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

Scenario: Assign values to json objects
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value	|
	| 1 | [[Person.FirstName]] =	| Bob		|
	| 2 | [[Person.Surname]] =		| Smith		|
	And the debug output as
	| # |								|
	| 1 | [[Person.FirstName]] = Bob	|
	| 2 | [[Person.Surname]] = Smith	|

Scenario: Assign values with different types to json objects
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the value "21" to a json object "[[Person.Age]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Smith"
	And the json object "[[Person.Age]]" equals "21"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value	|
	| 1 | [[Person.FirstName]] =	| Bob		|
	| 2 | [[Person.Surname]] =		| Smith		|
	| 3 | [[Person.Age]] =			| 21		|
	And the debug output as
	| # |								|
	| 1 | [[Person.FirstName]] = Bob	|
	| 2 | [[Person.Surname]] = Smith	|
	| 3 | [[Person.Age]] = 21			|

Scenario: Assign a value with plus in it to a json object
	Given I assign the value "+10" to a json object "[[Person.Score]]"
	When the assign object tool is executed
	Then the value of "[[Person.Score]]" equals +10
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value	|
	| 1 | [[Person.Score]] =	| +10   |
	And the debug output as 
	| # |							|
	| 1 | [[Person.Score]] = +10	|

Scenario: Assign a value with minus in it to a json object
	Given I assign the value "-10" to a json object "[[Person.Score]]"
	When the assign object tool is executed
	Then the value of "[[Person.Score]]" equals -10
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value	|
	| 1 | [[Person.Score]] =	| -10   |
	And the debug output as 
	| # |							|
	| 1 | [[Person.Score]] = -10	|

Scenario: Assign a json object value to a json object
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "[[Person.FirstName]]" to a json object "[[Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value						|
	| 1 | [[Person.FirstName]] =	| Bob							|
	| 2 | [[Person.Surname]] =		| [[Person.FirstName]] = Bob	|
	And the debug output as
	| # |								|
	| 1 | [[Person.FirstName]] = Bob	|
	| 2 | [[Person.Surname]] = Bob		|

Scenario: Assign a json object value to a json object overwriting the existing value
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the value "[[Person.FirstName]]" to a json object "[[Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable						| New Value						|
	| 1 | [[Person.FirstName]] =		| Bob							|
	| 2 | [[Person.Surname]] =			| Smith							|
	| 3 | [[Person.Surname]] = Smith	| [[Person.FirstName]] = Bob	|
	And the debug output as
	| # |								|
	| 1 | [[Person.FirstName]] = Bob	|
	| 2 | [[Person.Surname]] = Smith	|
	| 3 | [[Person.Surname]] = Bob		|

Scenario: Assign a value to an invalid json object
	Given I assign the value "[[Person.Score]]" to a json object "[[Person..Score]]"
	When the assign object tool is executed
	Then the execution has "AN" error
	And the execution has "parse error" error

Scenario: Assign an invalid value to a json object
	Given I assign the value "[[Person..Score]]" to a json object "[[Person..Score]]"
	When the assign object tool is executed
	Then the execution has "AN" error
	And the execution has "parse error" error

@ignore
#failing - person.name = bob, person.age = 25, staff = person -> is this valid?
Scenario: Assign a populated json object to a new json object
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the json object "[[Person]]" to a json object "[[Staff]]"
	When the assign object tool is executed
	Then the json object "[[Staff.Person.FirstName]]" equals "Bob"
	Then the json object "[[Staff.Person.Surname]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value		|
	And the debug output as
	| # |								|

@ignore
#failing - person.name = bob, person.age = 25, staff.subordinate = person -> is this valid?
Scenario: Assign a populated json object to a child of a new json object
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the json object "[[Person]]" to a json object "[[Staff.Subordinate]]"
	When the assign object tool is executed
	Then the json object "[[Staff.Subordinate.Person.FirstName]]" equals "Bob"
	Then the json object "[[Staff.Subordinate.Person.Surname]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value				|
	And the debug output as
	| # |											|

@ignore
#failing - person.name = bob, person.age = 25, staff(1) = person -> is this valid?
Scenario: Assign a populated json object to a new json object array
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the json object "[[Person]]" to a json object "[[Staff(1)]]"
	When the assign object tool is executed
	Then the json object "[[Staff(1).Person.FirstName]]" equals "Bob"
	Then the json object "[[Staff(1).Person.Surname]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value				|
	And the debug output as
	| # |											|

Scenario: Assign multiple json variables to a json object
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the value "[[Person.FirstName]][[Person.Surname]]" to a json object "[[Person.FullName]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Smith"
	And the value of "[[Person.FullName]]" equals BobSmith
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value											|
	| 1 | [[Person.FirstName]] =	| Bob												|
	| 2 | [[Person.Surname]] =		| Smith												|
	| 3 | [[Person.FullName]] =		| [[Person.FirstName]][[Person.Surname]] = BobSmith	|
	And the debug output as
    | # |									|
    | 1 | [[Person.FirstName]] = Bob		|
    | 2 | [[Person.Surname]] = Smith		|
    | 3 | [[Person.FullName]]  = BobSmith	|

Scenario: Assign multiple json variables to a json object with a literal
	Given I assign the value "Bob" to a json object "[[Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[Person.Surname]]"
	And I assign the value "[[Person.FirstName]] the killa [[Person.Surname]]" to a json object "[[Person.FullName]]"
	When the assign object tool is executed
	Then the json object "[[Person.FirstName]]" equals "Bob"
	And the json object "[[Person.Surname]]" equals "Smith"
	And the value of "[[Person.FullName]]" equals Bob the killa Smith
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value																	|
	| 1 | [[Person.FirstName]] =	| Bob																		|
	| 2 | [[Person.Surname]] =		| Smith																		|
	| 3 | [[Person.FullName]] =		| [[Person.FirstName]] the killa [[Person.Surname]] = Bob the killa Smith	|
	And the debug output as
    | # |												|
    | 1 | [[Person.FirstName]] = Bob					|
    | 2 | [[Person.Surname]] = Smith					|
    | 3 | [[Person.FullName]]  = Bob the killa Smith	|

Scenario: Assign values to a json object array
	Given I assign the value "11" to a json object "[[Person.Score(1)]]"
	And I assign the value "22" to a json object "[[Person.Score(2)]]"
	And I assign the value "33" to a json object "[[Person.Score(3)]]"
	When the assign object tool is executed
	Then the json object "[[Person.Score(1)]]" equals "11"
	Then the json object "[[Person.Score(2)]]" equals "22"
	Then the json object "[[Person.Score(3)]]" equals "33"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value	|
	| 1 | [[Person.Score(1)]] =	| 11		|
	| 2 | [[Person.Score(2)]] =	| 22		|
	| 3 | [[Person.Score(3)]] =	| 33		|
	And the debug output as
    | # |							|
    | 1 | [[Person.Score(1)]] = 11	|
    | 2 | [[Person.Score(2)]] = 22	|
    | 3 | [[Person.Score(3)]] = 33	|

Scenario: Assign a json object array to a new json object
	Given I assign the value "11" to a json object "[[Person.Score(1)]]"
	And I assign the value "22" to a json object "[[Person.Score(2)]]"
	And I assign the value "33" to a json object "[[Person.Score(3)]]"
	And I assign the value "[[Person.Score(*)]]" to a json object ""[[Person.CurrentScore]]""
	When the assign object tool is executed
	Then the json object "[[Person.CurrentScore(1)]]" equals "11"
	Then the json object "[[Person.CurrentScore(2)]]" equals "22"
	Then the json object "[[Person.CurrentScore(3)]]" equals "33"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value	|
	| 1 | [[Person.Score(1)]] =	| 11		|
	| 2 | [[Person.Score(2)]] =	| 22		|
	| 3 | [[Person.Score(3)]] =	| 33		|
	And the debug output as
    | # |							|
    | 1 | [[Person.Score(1)]] = 11	|
    | 2 | [[Person.Score(2)]] = 22	|
    | 3 | [[Person.Score(3)]] = 33	|

Scenario: Assign a json variable with a calculate expression
	Given I assign the value "=SUM(1,2,3)+1" to a json object "[[Person.Score]]"
	When the assign object tool is executed
	Then the json object "[[Person.Score]]" equals "7"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value			|
	| 1 | [[Person.Score]] =	| SUM(1,2,3)+1		|
	And the debug output as
    | # |						|
    | 1 | [[Person.Score]] = 7	|

# failing - TODO: calculate wolf-1600
Scenario: Assign a json variable with a calculate expression using json objects
	Given I assign the value "1" to a json object "[[Person.Score(1)]]"
	And I assign the value "2" to a json object "[[Person.Score(2)]]"
	And I assign the value "3" to a json object "[[Person.Score(3)]]"
	And I assign the value "=SUM(Person.Score(*))+1" to a json object "[[Person.TotalScore]]"
	When the assign object tool is executed
	Then the json object "[[Person.TotalScore]]" equals "7"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value															|
	| 1 | [[Person.Score1]] =		| 1																	|
	| 2 | [[Person.Score2]] =		| 2																	|
	| 3 | [[Person.Score3]] =		| 3																	|
	| 4 | [[Person.TotalScore]] =	| SUM(Person.Score(Person.Score1,Person.Score2,Person.Score3))+1	|
	And the debug output as
    | # |							|
    | 1 | [[Person.Score1]] = 1		|
    | 2 | [[Person.Score2]] = 2		|
    | 3 | [[Person.Score3]]  = 3	|
	| 3 | [[Person.TotalScore]] = 7	|

# failing - TODO: calculate wolf-1600
Scenario: Assign a json variable with a calculate expression using json array
	Given I assign the value "1" to a json object "[[Person.Score(1)]]"
	And I assign the value "2" to a json object "[[Person.Score(2)]]"
	And I assign the value "3" to a json object "[[Person.Score(3)]]"
	And I assign the value "=SUM(Person.Score(*))+1" to a json object "[[Person.TotalScore]]"
	When the assign object tool is executed
	Then the json object "[[Person.TotalScore]]" equals "7"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable						| New Value					|
	| 1 | [[Person.Score(1)]] =			| 1							|
	| 2 | [[Person.Score(2)]] =			| 2							|
	| 3 | [[Person.Score(3)]] =			| 3							|
	| 4 | [[Person.TotalScore]] =		| SUM(Person.Score(*))+1	|
	And the debug output as
    | # |							|
    | 1 | [[Person.Score(1)]] = 1	|
    | 2 | [[Person.Score(2)]] = 2	|
    | 3 | [[Person.Score(3)]]  = 3	|
	| 3 | [[Person.TotalScore]] = 7	|

# TODO
# add to the seq, foreach specs, adding this tool
# debug output -> json 
