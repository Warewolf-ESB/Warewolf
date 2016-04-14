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


	Scenario: Assign two json values to scalar
	Given I assign the value A to a variable "[[rec.a(1)]]"	
	And I assign the value B to a variable "[[rec.a(2)]]"
	And I assign the value [[rec.a(1)]][[rec.a(2)]] to a variable "[[Scalar]]"
	When the assign tool is executed
	Then the value of "[[Scalar]]" equals "AB"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable       | New Value                     |
	| 1 | [[rec.a(1)]] = | A                             |
	| 2 | [[rec.a(2)]] = | B                             |
	| 3 | [[Scalar]] =   | [[rec.a(1)]][[rec.a(2)]] = AB |
	And the debug output as
	| # |                  |
	| 1 | [[rec.a(1)]] = A |
	| 2 | [[rec.a(2)]] = B |
	| 3 | [[Scalar]] = AB  |


	Scenario: Assign two json and data 
	Given I assign the value 1 to a variable "[[rec.a(1)]]"	
	And I assign the value 2 to a variable "[[rec.a(2)]]"
	And I assign the value Test[[rec.a(1)]].Warewolf[[rec.a(2)]] to a variable "[[Lr.a(1)]]"
	When the assign tool is executed
	Then the value of "[[Lr.a(1)]]" equals "Test1.Warewolf2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable       | New Value       |
	| 1 | [[rec.a(1)]] = | 1               |
	| 2 | [[rec.a(2)]] = | 2               |
	| 3 | [[Lr.a(1)]] =  | Test[[rec.a(1)]].Warewolf[[rec.a(2)]] = Test1.Warewolf2 |
	And the debug output as
	| # |                                |
	| 1 | [[rec.a(1)]] = 1               |
	| 2 | [[rec.a(2)]] = 2               |
	| 3 | [[Lr.a(1)]]  = Test1.Warewolf2 |


	Scenario: Assign two json with index as variable to scalr
	Given I assign the value Test to a variable "[[rec.test(1)]]"	
	And I assign the value Warewolf to a variable "[[rec.test(2)]]"
	And I assign the value 1 to a variable "[[a]]"
	And I assign the value 2 to a variable "[[b]]"
	And I assign the value [[rec.test([[a]])]][[rec.test([[b]])]] to a variable "[[c]]"
	When the assign tool is executed
	Then the value of "[[c]]" equals "TestWarewolf"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable          | New Value                                             |
	| 1 | [[rec.test(1)]] = | Test                                                  |
	| 2 | [[rec.test(2)]] = | Warewolf                                              |
	| 3 | [[a]]           = | 1                                                     |
	| 4 | [[b]]           = | 2                                                     |
	| 5 | [[c]]           = | [[rec.test([[a]])]][[rec.test([[b]])]] = TestWarewolf |
	And the debug output as
	| # |                            |
	| 1 | [[rec.test(1)]] = Test     |
	| 2 | [[rec.test(2)]] = Warewolf |
	| 3 | [[a]]  = 1                 |
	| 4 | [[b]]  = 2                 |
	| 5 | [[c]]  = TestWarewolf      |


	Scenario: Assign two json with index as json variable to scalr
	Given I assign the value Test to a variable "[[rec.test(1)]]"	
	And I assign the value Warewolf to a variable "[[rec.test(2)]]"
	And I assign the value 1 to a variable "[[Index.a(1)]]"
	And I assign the value 2 to a variable "[[Index.a(2)]]"
	And I assign the value [[rec.test([[Index.a(1)]])]][[rec.test([[Index.a(2)]])]] to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "TestWarewolf"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable          | New Value                                                               |
	| 1 | [[rec.test(1)]] = | Test                                                                    |
	| 2 | [[rec.test(2)]] = | Warewolf                                                                |
	| 3 | [[Index.a(1)]]  = | 1                                                                       |
	| 4 | [[Index.a(2)]]  = | 2                                                                       |
	| 5 | [[Result]]      = | [[rec.test([[Index.a(1)]])]][[rec.test([[Index.a(2)]])]] = TestWarewolf |
	And the debug output as
	| # |                            |
	| 1 | [[rec.test(1)]] = Test     |
	| 2 | [[rec.test(2)]] = Warewolf |
	| 3 | [[Index.a(1)]]  = 1        |
	| 4 | [[Index.a(2)]]  = 2        |
	| 5 | [[Result]]  = TestWarewolf |


	Scenario: Assign a json to a scalar
	Given I assign the value 10 to a variable "[[rec.set(1)]]"	
	And I assign the value 20 to a variable "[[rec.set(2)]]"
	And I assign the value 30 to a variable "[[rec.set(3)]]"
	And I assign the value "[[rec.set(*)]]" to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals "30"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value           |
	| 1 | [[rec.set(1)]] = | 10                  |
	| 2 | [[rec.set(2)]] = | 20                  |
	| 3 | [[rec.set(3)]] = | 30                  |
	| 4 | [[var]]        = | [[rec.set(1)]] = 10 |
	|   |                  | [[rec.set(2)]] = 20 |
	|   |                  | [[rec.set(3)]] = 30 |
	And the debug output as
	| # |                     |
	| 1 | [[rec.set(1)]] = 10 |
	| 2 | [[rec.set(2)]] = 20 |
	| 3 | [[rec.set(3)]] = 30 |
	| 4 | [[var]] = 30        |


	Scenario: Assign a scalar equal to a json
	Given I assign the value 30 to a variable "[[var]]"
	And I assign the value "[[var]]" to a variable "[[rec.set()]]"
	When the assign tool is executed
	Then the value of "[[rec.set(1)]]" equals "30"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable        | New Value     |
	| 1 | [[var]]       = | 30            |
	| 2 | [[rec.set()]] = | [[var]]  = 30 |
	And the debug output as
	| # |                     |
	| 1 | [[var]] = 30        |
	| 2 | [[rec.set(1)]] = 30 |	 


	Scenario: Assign the value of a negative json index
	Given I assign the value 10 to a variable "[[rec.set()]]"	
	And I assign the value [[rec.set(-1)]] to a variable "[[var]]"
	When the assign tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| # | Variable        | New Value         |
	| 1 | [[rec.set()]] = | 10                |
	And the debug output as
	| # |                     |
	| 1 | [[rec.set(1)]] = 10 |

	Scenario: Assign a variable to mixed scalar, char and json values
	Given I assign the value Hello to a variable "[[var]]"	
	And I assign the value World to a variable "[[rec.set(1)]]"
	And I assign the value [[var]] [[rec.set(1)]] ! to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals "Hello World !"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value     |
	| 1 | [[var]]        = | Hello         |
	| 2 | [[rec.set(1)]] = | World         |
	| 3 | [[value]]      = | [[var]] [[rec.set(1)]] ! = Hello World ! |
	And the debug output as
    | # |                           |
    | 1 | [[var]] = Hello           |
    | 2 | [[rec.set(1)]] = World    |
    | 3 | [[value]] = Hello World ! |


	Scenario: Assign multiple variables to the end of a json
	Given I assign the value 10 to a variable "[[rec.set()]]"	
	And I assign the value 20 to a variable "[[rec.set()]]"
	And I assign the value 30 to a variable "[[rec.set()]]"
	And I assign the value [[rec.set(3)]] to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals 30
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable        | New Value           |
	| 1 | [[rec.set()]] = | 10                  |
	| 2 | [[rec.set()]] = | 20                  |
	| 3 | [[rec.set()]] = | 30                  |
	| 4 | [[value]]     = | [[rec.set(3)]] = 30 |
	And the debug output as
    | # |                     |
    | 1 | [[rec.set(1)]] = 10 |
    | 2 | [[rec.set(2)]] = 20 |
    | 3 | [[rec.set(3)]] = 30 |
    | 4 | [[value]] = 30      |



	Scenario: Assign all json values to a single variable
	Given I assign the value 10 to a variable "[[rec.set(1)]]"	
	And I assign the value 20 to a variable "[[rec.set(2)]]"
	And I assign the value 30 to a variable "[[rec.set(3)]]"
	And I assign the value "" to a variable "[[rec.set(*)]]"
	When the assign tool is executed
	Then the value of "[[rec.set(3)]]" equals ""
	And the value of "[[rec.set(2)]]" equals ""
	And the value of "[[rec.set(1)]]" equals ""
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable            | New Value |
	| 1 | [[rec.set(1)]] =    | 10        |
	| 2 | [[rec.set(2)]] =    | 20        |
	| 3 | [[rec.set(3)]] =    | 30        |
	| 4 | [[rec.set(1)]] = 10 |           |
	|   | [[rec.set(2)]] = 20 |           |
	|   | [[rec.set(3)]] = 30 | " "       |
	And the debug output as
    | # |                     |
    | 1 | [[rec.set(1)]] = 10 |
    | 2 | [[rec.set(2)]] = 20 |
    | 3 | [[rec.set(3)]] = 30 |
    | 4 | [[rec.set(1)]] = "" |
    |   | [[rec.set(2)]] = "" |
    |   | [[rec.set(3)]] = "" |

	Scenario: Assign all json values to all json
	Given I assign the value 10 to a variable "[[rec.set(1)]]"	
	And I assign the value 20 to a variable "[[rec.set(2)]]"
	And I assign the value 30 to a variable "[[rec.set(3)]]"
	And I assign the value Hello to a variable "[[rs.val()]]"
	And I assign the value "[[rec.set(*)]]" to a variable "[[rs.val(*)]]"
	When the assign tool is executed
	Then the value of "[[rs.val(1)]]" equals 10
	And the value of "[[rs.val(2)]]" equals 20
	And the value of "[[rs.val(3)]]" equals 30
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable              | New Value           |
	| 1 | [[rec.set(1)]] =      | 10                  |
	| 2 | [[rec.set(2)]] =      | 20                  |
	| 3 | [[rec.set(3)]] =      | 30                  |
	| 4 | [[rs.val()]] =        | Hello               |
	| 5 | [[rs.val(1)]] = Hello | [[rec.set(1)]] = 10 |
	|   |                       | [[rec.set(2)]] = 20 |
	|   |                       | [[rec.set(3)]] = 30 |
	And the debug output as
    | # |                       |
    | 1 | [[rec.set(1)]] = 10   |
    | 2 | [[rec.set(2)]] = 20   |
    | 3 | [[rec.set(3)]] = 30   |
    | 4 | [[rs.val(1)]] = Hello |
    | 5 | [[rs.val(1)]] = 10    |
    |   | [[rs.val(2)]] = 20    |
    |   | [[rs.val(3)]] = 30    |


	Scenario: Assign values to different columns in a reccord set
       Given I assign the value 10 to a variable "[[rec.a()]]"       
       And I assign the value 20 to a variable "[[rec.b()]]"
       And I assign the value 30 to a variable "[[rec.c()]]"
       And I assign the value [[rec.a()]] to a variable "[[d]]"
       And I assign the value [[rec.b()]] to a variable "[[e]]"
       And I assign the value [[rec.c()]] to a variable "[[f]]"
       When the assign tool is executed
       Then the value of "[[d]]" equals 10
       And the value of "[[e]]" equals 20
       And the value of "[[f]]" equals 30
       And the execution has "NO" error
       And the debug inputs as
       | # | Variable      | New Value        |
       | 1 | [[rec.a()]] = | 10               |
       | 2 | [[rec.b()]] = | 20               |
       | 3 | [[rec.c()]] = | 30               |
       | 4 | [[d]]     =   | [[rec.a(1)]] = 10 |
       | 5 | [[e]]     =   | [[rec.b(1)]] = 20 |
       | 6 | [[f]]     =   | [[rec.c(1)]] = 30 |
       And the debug output as
    | # |                   |
    | 1 | [[rec.a(1)]] = 10 |
    | 2 | [[rec.b(1)]] = 20 |
    | 3 | [[rec.c(1)]] = 30 |
    | 4 | [[d]] = 10        |
    | 5 | [[e]] = 20        |
    | 6 | [[f]] = 30        |


	#Scenario: Assign values to recordsets
#	Given I assign the value 1 to a variable "[[AB.a()]]"	
#	And I assign the value a to a variable "[[CD.a()]]"
#	And I assign the value b to a variable "[[CD.a()]]"
#	And I assign the value 2 to a variable "[[AB.a()]]"	
#	When the assign tool is executed
#	Then the value of "[[AB.a(2)]]" equals 2
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable      | New Value |
#	| 1 | [[AB.a()]]  = | 1         |
#	| 2 | [[CD.a()]]  = | a         |
#	| 3 | [[CD.a()]]  = | b         |
#	| 4 | [[AB.a()]]  = | 2         |
#	And the debug output as
#    | # |                 |
#    | 1 | [[AB.a(1)]] = 1 |
#    | 2 | [[CD.a(1)]] = a |
#    | 3 | [[CD.a(2)]] = b |
#    | 4 | [[AB.a(2)]] = 2 |


Scenario: Assign a scalar equal to a calculation
	Given I assign the value 30 to a variable "[[var]]"
	And I assign the value "=30-[[var]]" to a variable "[[Result]]"	
	When the assign tool is executed
	Then the value of "[[Result]]" equals "0"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable     | New Value          |
	| 1 | [[var]]    = | 30                 |
	| 2 | [[Result]] = | 30-[[var]] = 30-30 |
	And the debug output as
	| # |                |
	| 1 | [[var]] = 30   |
	| 2 | [[Result]] = 0 |


	Scenario: Assign the value of a negative json index
	Given I assign the value 10 to a variable "[[rec.set()]]"	
	And I assign the value [[rec.set(-1)]] to a variable "[[var]]"
	When the assign tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| # | Variable        | New Value         |
	| 1 | [[rec.set()]] = | 10                |
	And the debug output as
	| # |                     |
	| 1 | [[rec.set(1)]] = 10 |



# TODO
# add to the seq, foreach specs, adding this tool
# debug output -> json 
