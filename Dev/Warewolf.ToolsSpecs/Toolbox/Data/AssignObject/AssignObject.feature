@Data
Feature: AssignObject
	In order to use json 
	As a Warewolf user
	I want a tool that assigns data to json objects

Scenario: Assign a value to a json object
	Given I assign the value "Bob" to a json object "[[@Person.Name]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Name]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable			| New Value		|
	| 1 | [[@Person.Name]] = | Bob			|
	And the debug output as
	| # |						|
	| 1 | [[@Person.Name]] = Bob |

Scenario: Assign values to json objects
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Smith"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value	|
	| 1 | [[@Person.FirstName]] =	| Bob		|
	| 2 | [[@Person.Surname]] =		| Smith		|
	And the debug output as
	| # |								|
	| 1 | [[@Person.FirstName]] = Bob	|
	| 2 | [[@Person.Surname]] = Smith	|

Scenario: Assign values with different types to json objects
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
	And I assign the value 21 to a json object "[[@Person.Age]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Smith"
	And the json object "[[@Person.Age]]" equals "21"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value	|
	| 1 | [[@Person.FirstName]] =	| Bob		|
	| 2 | [[@Person.Surname]] =		| Smith		|
	| 3 | [[@Person.Age]] =			| 21		|
	And the debug output as
	| # |								|
	| 1 | [[@Person.FirstName]] = Bob	|
	| 2 | [[@Person.Surname]] = Smith	|
	| 3 | [[@Person.Age]] = 21			|

Scenario: Assign a value with plus in it to a json object
	Given I assign the value "+10" to a json object "[[@Person.Score]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score]]" equals "+10"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value	|
	| 1 | [[@Person.Score]] =	| +10   |
	And the debug output as 
	| # |							|
	| 1 | [[@Person.Score]] = +10	|

Scenario: Assign a value with minus in it to a json object
	Given I assign the value "-10" to a json object "[[@Person.Score]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score]]" equals "-10"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value	|
	| 1 | [[@Person.Score]] =	| -10   |
	And the debug output as 
	| # |							|
	| 1 | [[@Person.Score]] = -10	|

Scenario: Assign a json object value to a json object
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "[[@Person.FirstName]]" to a json object "[[@Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value						|
	| 1 | [[@Person.FirstName]] =	| Bob							|
	| 2 | [[@Person.Surname]] =		| [[@Person.FirstName]] = Bob	|
	And the debug output as
	| # |								|
	| 1 | [[@Person.FirstName]] = Bob	|
	| 2 | [[@Person.Surname]] = Bob		|

Scenario: Assign a json object value to a json object overwriting the existing value
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
	And I assign the value "[[@Person.FirstName]]" to a json object "[[@Person.Surname]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Bob"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable						| New Value						|
	| 1 | [[@Person.FirstName]] =		| Bob							|
	| 2 | [[@Person.Surname]] =			| Smith							|
	| 3 | [[@Person.Surname]] = Smith	| [[@Person.FirstName]] = Bob	|
	And the debug output as
	| # |								|
	| 1 | [[@Person.FirstName]] = Bob	|
	| 2 | [[@Person.Surname]] = Smith	|
	| 3 | [[@Person.Surname]] = Bob		|

Scenario Outline: Assign a value to an invalid json object
	Given I assign the value "[[@Person.Score]]" to a json object "<var>"
	When the assign object tool is executed
	Then the execution has "<error>" error
	And the execution has "parse error" error	
   And the debug inputs as
	| # | Variable | New Value |	
   And the debug output as
	| # |          |			|
   Examples:
	| no | var                | error |
	| 1  | [[@Person..Score]] | AN    |
	| 2  | @                  | AN    |
	| 3  | @.                 | AN    |
	| 4  | @()                | AN    |
	| 5  | @().               | AN    |
	| 6  | @.Field            | AN    |
	| 7  | @Object.           | AN    |
	| 8  | @1                 | AN    |
	| 9  | @1.                | AN    |
	| 10 | @1.1               | AN    |
	| 11 | @Rec1.             | AN    |
	| 12 | @Rec1.1            | AN    |
	| 13 | @1Rec              | AN    |
	| 14 | @Rec1.#Field#      | AN    |
	| 15 | @Rec1.1Field       | AN    |
	| 16 | @Var;iable         | AN    |
	| 17 | @(Rec1@)           | AN    |
	| 18 | [[@(Rec1@)]]       | AN    |
	| 19 | @(Rec)             | AN    |
	| 12 | @;;;;p             | AN    |	

Scenario: Assign an invalid value to a json object
	Given I assign the value "[[@Person..Score]]" to a json object "[[@Person..Score]]"
	When the assign object tool is executed
	Then the execution has "AN" error
	And the execution has "parse error" error

Scenario: Assign multiple json variables to a json object
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
	And I assign the value "[[@Person.FirstName]][[@Person.Surname]]" to a json object "[[@Person.FullName]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Smith"
	And the value of "[[@Person.FullName]]" equals BobSmith
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value											|
	| 1 | [[@Person.FirstName]] =	| Bob												|
	| 2 | [[@Person.Surname]] =		| Smith												|
	| 3 | [[@Person.FullName]] =		| [[@Person.FirstName]][[@Person.Surname]] = BobSmith	|
	And the debug output as
    | # |									|
    | 1 | [[@Person.FirstName]] = Bob		|
    | 2 | [[@Person.Surname]] = Smith		|
    | 3 | [[@Person.FullName]]  = BobSmith	|

Scenario: Assign multiple json variables to a json object with a literal
	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
	And I assign the value "[[@Person.FirstName]] the killa [[@Person.Surname]]" to a json object "[[@Person.FullName]]"
	When the assign object tool is executed
	Then the json object "[[@Person.FirstName]]" equals "Bob"
	And the json object "[[@Person.Surname]]" equals "Smith"
	And the value of "[[@Person.FullName]]" equals Bob the killa Smith
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable					| New Value																	|
	| 1 | [[@Person.FirstName]] =	| Bob																		|
	| 2 | [[@Person.Surname]] =		| Smith																		|
	| 3 | [[@Person.FullName]] =		| [[@Person.FirstName]] the killa [[@Person.Surname]] = Bob the killa Smith	|
	And the debug output as
    | # |												|
    | 1 | [[@Person.FirstName]] = Bob					|
    | 2 | [[@Person.Surname]] = Smith					|
    | 3 | [[@Person.FullName]]  = Bob the killa Smith	|

Scenario: Assign values to a json object array within a json object
	Given I assign the value "11" to a json object "[[@Person.Score(1)]]"
	And I assign the value "22" to a json object "[[@Person.Score(2)]]"
	And I assign the value "33" to a json object "[[@Person.Score(3)]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score(1)]]" equals "11"
	And the json object "[[@Person.Score(2)]]" equals "22"
	And the json object "[[@Person.Score(3)]]" equals "33"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value	|
	| 1 | [[@Person.Score(1)]] =	| 11		|
	| 2 | [[@Person.Score(2)]] =	| 22		|
	| 3 | [[@Person.Score(3)]] =	| 33		|
	And the debug output as
    | # |							|
    | 1 | [[@Person.Score(1)]] = 11	|
    | 2 | [[@Person.Score(2)]] = 22	|
    | 3 | [[@Person.Score(3)]] = 33	|

Scenario: Assign values to a json object array
	Given I assign the value "11" to a json object "[[@Score(1)]]"
	And I assign the value "22" to a json object "[[@Score(2)]]"
	And I assign the value "33" to a json object "[[@Score(3)]]"
	When the assign object tool is executed
	Then the json object "[[@Score(1)]]" equals "11"
	And the json object "[[@Score(2)]]" equals "22"
	And the json object "[[@Score(3)]]" equals "33"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable			| New Value	|
	| 1 | [[@Score(1)]] =	| 11		|
	| 2 | [[@Score(2)]] =	| 22		|
	| 3 | [[@Score(3)]] =	| 33		|
	And the debug output as
    | # |					|
    | 1 | [[@Score(1)]] = 11	|
    | 2 | [[@Score(2)]] = 22	|
    | 3 | [[@Score(3)]] = 33	|

Scenario: Assign a value to all elements of a json object array within a json object
	Given I assign the value "11" to a json object "[[@Person.Score(1)]]"
	And I assign the value "22" to a json object "[[@Person.Score(2)]]"
	And I assign the value "33" to a json object "[[@Person.Score(3)]]"
	And I assign the value "44" to a json object "[[@Person.Score(*)]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score(1)]]" equals "44"
	And the json object "[[@Person.Score(2)]]" equals "44"
	And the json object "[[@Person.Score(3)]]" equals "44"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value	|
	| 1 | [[@Person.Score(1)]] =	| 11		|
	| 2 | [[@Person.Score(2)]] =	| 22		|
	| 3 | [[@Person.Score(3)]] =	| 33		|
	| 4 | [[@Person.Score(*)]] =	| 44		|
	And the debug output as
    | # |							|
    | 1 | [[@Person.Score(1)]] = 11	|
    | 2 | [[@Person.Score(2)]] = 22	|
    | 3 | [[@Person.Score(3)]] = 33	|
	| 4 | [[@Person.Score(1)]] = 44	|
    |   | [[@Person.Score(2)]] = 44	|
    |   | [[@Person.Score(3)]] = 44	|

Scenario: Assign a value to all elements of a json object array
	Given I assign the value "11" to a json object "[[@Score(1)]]"
	And I assign the value "22" to a json object "[[@Score(2)]]"
	And I assign the value "33" to a json object "[[@Score(3)]]"
	And I assign the value "44" to a json object "[[@Score(*)]]"
	When the assign object tool is executed
	Then the json object "[[@Score(1)]]" equals "44"
	And the json object "[[@Score(2)]]" equals "44"
	And the json object "[[@Score(3)]]" equals "44"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable			| New Value	|
	| 1 | [[@Score(1)]] =	| 11		|
	| 2 | [[@Score(2)]] =	| 22		|
	| 3 | [[@Score(3)]] =	| 33		|
	| 4 | [[@Score(*)]] =	| 44		|
	And the debug output as
    | # |					|
    | 1 | [[@Score(1)]] = 11	|
    | 2 | [[@Score(2)]] = 22	|
    | 3 | [[@Score(3)]] = 33	|
	| 4 | [[@Score(1)]] = 44	|
    |   | [[@Score(2)]] = 44	|
    |   | [[@Score(3)]] = 44	|

Scenario: Assign a value to the end of a json object array within a json object
	Given I assign the value "11" to a json object "[[@Person.Score()]]"
	And I assign the value "22" to a json object "[[@Person.Score()]]"
	And I assign the value "33" to a json object "[[@Person.Score()]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score(1)]]" equals "11"
	And the json object "[[@Person.Score(2)]]" equals "22"
	And the json object "[[@Person.Score(3)]]" equals "33"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value	|
	| 1 | [[@Person.Score()]] =	| 11		|
	| 2 | [[@Person.Score()]] =	| 22		|
	| 3 | [[@Person.Score()]] =	| 33		|
	And the debug output as
    | # |							|
    | 1 | [[@Person.Score()]] = 11	|
    | 2 | [[@Person.Score()]] = 22	|
    | 3 | [[@Person.Score()]] = 33	|

Scenario: Assign a value to a new json object array within a json object
	Given I assign the value "11" to a json object "[[@Person.Score1()]]"
	And I assign the value "22" to a json object "[[@Person.Score2()]]"
	And I assign the value "33" to a json object "[[@Person.Score3()]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score1(1)]]" equals "11"
	And the json object "[[@Person.Score2(1)]]" equals "22"
	And the json object "[[@Person.Score3(1)]]" equals "33"
    And the execution has "NO" error
    And the debug inputs as
    | # | Variable				| New Value     |
    | 1 | [[@Person.Score1()]] = | 11			|
    | 2 | [[@Person.Score2()]] = | 22			|
    | 3 | [[@Person.Score3()]] = | 33			|
    And the debug output as
    | # |							|
    | 1 | [[@Person.Score1()]] = 11	|
    | 2 | [[@Person.Score2()]] = 22	|
    | 3 | [[@Person.Score3()]] = 33	|

Scenario: Assign a json variable with a calculate expression
	Given I assign the value "=SUM(1,2,3)+1" to a json object "[[@Person.Score]]"
	When the assign object tool is executed
	Then the json object "[[@Person.Score]]" equals "7"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable				| New Value			|
	| 1 | [[@Person.Score]] =	| SUM(1,2,3)+1		|
	And the debug output as
    | # |						|
    | 1 | [[@Person.Score]] = 7	|

Scenario: Assign a json variable with a calculate expression using json objects
	Given I assign the value "1" to a json object "[[@Person.Score(1)]]"
	And I assign the value "2" to a json object "[[@Person.Score(2)]]"
	And I assign the value "3" to a json object "[[@Person.Score(3)]]"
	And I assign the value "=SUM([[@Person.Score(*)]])+1" to a json object "[[@Person.TotalScore]]"
	When the assign object tool is executed
	Then the json object "[[@Person.TotalScore]]" equals "4"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable                 | New Value                             |
	| 1 | [[@Person.Score(1)]] =   | 1                                     |
	| 2 | [[@Person.Score(2)]] =   | 2                                     |
	| 3 | [[@Person.Score(3)]] =   | 3                                     |
	| 4 | [[@Person.TotalScore]] = | SUM([[@Person.Score(*)]])+1 =SUM(1)+1 |
	|   |                          | SUM([[@Person.Score(*)]])+1 =SUM(2)+1 |
	|   |                          | SUM([[@Person.Score(*)]])+1 =SUM(3)+1 |
	And the debug output as
    | # |                            |
    | 1 | [[@Person.Score(1)]] = 1   |
    | 2 | [[@Person.Score(2)]] = 2   |
    | 3 | [[@Person.Score(3)]] = 3   |
    | 4 | [[@Person.TotalScore]] = 4 |

Scenario: Assign a json variable with a calculate expression using json array
	Given I assign the value "1" to a json object "[[@Person.Score(1).val]]"
	And I assign the value "2" to a json object "[[@Person.Score(2).val]]"
	And I assign the value "3" to a json object "[[@Person.Score(3).val]]"
	And I assign the value "=SUM([[@Person.Score(*).val]])+1" to a json object "[[@Person.TotalScore]]"
	When the assign object tool is executed
	Then the json object "[[@Person.TotalScore]]" equals "4"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable                   | New Value                                 |
	| 1 | [[@Person.Score(1).val]] = | 1                                         |
	| 2 | [[@Person.Score(2).val]] = | 2                                         |
	| 3 | [[@Person.Score(3).val]] = | 3                                         |
	| 4 | [[@Person.TotalScore]] =   | SUM([[@Person.Score(1).val]])+1 =SUM(1)+1 |
	|   |                            | SUM([[@Person.Score(2).val]])+1 =SUM(2)+1 |
	|   |                            | SUM([[@Person.Score(3).val]])+1 =SUM(3)+1 |
	And the debug output as
    | # |                              |
    | 1 | [[@Person.Score(1).val]] = 1 |
    | 2 | [[@Person.Score(2).val]] = 2 |
    | 3 | [[@Person.Score(3).val]] = 3 |
    | 4 | [[@Person.TotalScore]] = 4   |

Scenario: Assign two json and data 
	Given I assign the value 1 to a json object "[[@rec.a(1)]]"	
	And I assign the value 2 to a json object "[[@rec.a(2)]]"
	And I assign the value "Test[[@rec.a(1)]].Warewolf[[@rec.a(2)]]" to a json object "[[@Lr.a(1)]]"
	When the assign object tool is executed
	Then the value of "[[@Lr.a(1)]]" equals "Test1.Warewolf2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable       | New Value       |
	| 1 | [[@rec.a(1)]] = | 1               |
	| 2 | [[@rec.a(2)]] = | 2               |
	| 3 | [[@Lr.a(1)]] =  | Test[[@rec.a(1)]].Warewolf[[@rec.a(2)]] = Test1.Warewolf2 |
	And the debug output as
	| # |                                |
	| 1 | [[@rec.a(1)]] = 1               |
	| 2 | [[@rec.a(2)]] = 2               |
	| 3 | [[@Lr.a(1)]]  = Test1.Warewolf2 |

Scenario: Assign the value of a negative json index
	Given I assign the value 10 to a json object "[[@rec.set()]]"	
	And I assign the value "[[@rec.set(-1)]]" to a json object "[[var]]"
	When the assign object tool is executed
	Then the execution has "AN" error

Scenario: Assign a record set variable equal to a group calculation (sum)
	Given I assign the value 30 to a json object "[[@rec(1).a]]"
	And I assign the value 30 to a json object "[[@rec(1).b]]"
	And I assign the value "=SUM([[@rec(1).a]],[[@rec(1).b]])" to a json object "[[@Result.a]]"
	When the assign object tool is executed
	Then the value of "[[@Result.a]]" equals "60"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value    |
	| 1 | [[@rec(1).a]]   = | 30           |
	| 2 | [[@rec(1).b]]   = | 30           |
	| 3 | [[@Result.a]] =     | SUM([[@rec(1).a]],[[@rec(1).b]]) = SUM(30,30) |
	And the debug output as
	| # |                   |
	| 1 | [[@rec(1).a]] = 30 |
	| 2 | [[@rec(1).b]] = 30 |
	| 3 | [[@Result.a]] = 60 |


Scenario: Assign a variable equal to a group calculation with scalar and recordset
	Given I assign the value 1 to a json object "[[@a.b]]"
	And I assign the value 2 to a json object "[[@b.a]]"
	And I assign the value "[[@a.b]]" to a json object "[[@rec(1).a]]"
	And I assign the value "[[@b.a]]" to a json object "[[@rec(1).b]]"
	And I assign the value "=SUM([[@rec(1).a]],[[@rec(1).b]])" to a json object "[[@Result.a]]"
	When the assign object tool is executed
	Then the value of "[[@Result.a]]" equals "3"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | New Value                                 |
	| 1 | [[@a.b]]          = | 1                                         |
	| 2 | [[@b.a]]          = | 2                                         |
	| 3 | [[@rec(1).a]]   =   | [[@a.b]] = 1                               |
	| 4 | [[@rec(1).b]]   =   | [[@b.a]] = 2                               |
	| 5 | [[@Result.a]] =     | SUM([[@rec(1).a]],[[@rec(1).b]]) = SUM(1,2) |  
	And the debug output as
	| # |                  |
	| 1 | [[@a.b]] = 1      |
	| 2 | [[@b.a]] = 2      |  
	| 3 | [[@rec(1).a]] = 1 |
	| 4 | [[@rec(1).b]] = 2 |
	| 5 | [[@Result.a]] = 3 |  

Scenario: Assign a json string to json object variable
	Given I assign the string "{"Five":"5"}" to a json object "[[@Numbers]]"
	When the assign object tool is executed
	Then the value of "[[@Numbers.Five]]" equals "5"

Scenario: Assign a json string to json object variable has two records
	Given I assign the string "{"Five":"5", "Ten":"10"}" to a json object "[[@Numbers]]"
	When the assign object tool is executed
	Then the value of "[[@Numbers.Five]]" equals "5"
	Then the value of "[[@Numbers.Ten]]" equals "10"

Scenario: Evaluating recursive recordset variable in a group calculation
	Given I assign the value 1 to a json object "[[@rec(1).a]]"
	And I assign the value "rec(1).a" to a json object "[[@rec(1).b]]"
	And I assign the value "=sum(1+1)" to a json object "[[@Result.a]]"
	When the assign object tool is executed
	Then the value of "[[@Result.a]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable          | New Value |
	| 1 | [[@rec(1).a]]   = | 1         |
	| 2 | [[@rec(1).b]]   = | rec(1).a  |
	| 3 | [[@Result.a]] =   | sum(1+1)  |  
	And the debug output as
	| # |                          |
	| 1 | [[@rec(1).a]] = 1        |
	| 2 | [[@rec(1).b]] = rec(1).a |
	| 3 | [[@Result.a]] =  2       |


Scenario: Evaluating recursive invalid recordset variable in a group calculation
	Given I assign the value 1 to a json object "[[@rec(1).a]]"
	And I assign the value "rec(1).a*" to a json object "[[@rec(1).b]]"
	And I assign the value "=[[[[@rec(1).b]]]]+1" to a json object "[[@Result.c]]"
	When the assign object tool is executed
	Then the execution has "AN" error

#failing - person.name = bob, person.age = 25, staff = person -> is this valid?
#Scenario: Assign a populated json object to a new json object
#	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
#	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
#	And I assign the json object "[[@Person]]" to a json object "[[@Staff]]"
#	When the assign object tool is executed
#	Then the json object "[[@Staff.Person.FirstName]]" equals "Bob"
#	And the json object "[[@Staff.Person.Surname]]" equals "Smith"
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable					| New Value		|
#	And the debug output as
#	| # |								|

#failing - person.name = bob, person.age = 25, staff.subordinate = person -> is this valid?
#Scenario: Assign a populated json object to a child of a new json object
#	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
#	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
#	And I assign the json object "[[@Person]]" to a json object "[@[Staff.Subordinate]]"
#	When the assign object tool is executed
#	Then the json object "[[@Staff.Subordinate.Person.FirstName]]" equals "Bob"
#	And the json object "[[@Staff.Subordinate.Person.Surname]]" equals "Smith"
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable					| New Value				|
#	And the debug output as
#	| # |											|

#failing - person.name = bob, person.age = 25, staff(1) = person -> is this valid?
#Scenario: Assign a populated json object to a new json object array
#	Given I assign the value "Bob" to a json object "[[@Person.FirstName]]"
#	And I assign the value "Smith" to a json object "[[@Person.Surname]]"
#	And I assign the json object "[[@Person]]" to a json object "[[@Staff(1)]]"
#	When the assign object tool is executed
#	Then the json object "[[@Staff(1).Person.FirstName]]" equals "Bob"
#	And the json object "[[@Staff(1).Person.Surname]]" equals "Smith"
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable					| New Value				|
#	And the debug output as
#	| # |											|

#failing - staff(1) = Bob, staff(2) = OtherBob, staff(3) = OtherOtherBob, hitList = staff -> is this valid?
#Scenario: Assign a json object array to a new json object
#	Given I assign the value "11" to a json object "[[@Person.Score(1)]]"
#	And I assign the value "22" to a json object "[[@Person.Score(2)]]"
#	And I assign the value "33" to a json object "[[@Person.Score(3)]]"
#	And I assign the value "[[@Person.Score(*)]]" to a json object ""[[@Person.CurrentScore(*)]]""
#	When the assign object tool is executed
#	Then the json object "[[@Person.CurrentScore(1)]]" equals "11"
#	And the json object "[[@Person.CurrentScore(2)]]" equals "22"
#	And the json object "[[@Person.CurrentScore(3)]]" equals "33"
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable				| New Value	|
#	| 1 | [[@Person.Score(1)]] =	| 11		|
#	| 2 | [[@Person.Score(2)]] =	| 22		|
#	| 3 | [[@Person.Score(3)]] =	| 33		|
#	And the debug output as
#    | # |							|
#    | 1 | [[@Person.Score(1)]] = 11	|
#    | 2 | [[@Person.Score(2)]] = 22	|
#    | 3 | [[@Person.Score(3)]] = 33	|