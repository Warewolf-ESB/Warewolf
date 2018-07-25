Feature: DotNetAssign
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Assign multiple variables to the same recordset
	Given I dotnetassign the value "Kim" to a variable "[[person().name]]"
	And I dotnetassign the value "bob" to a variable "[[person().name]]"
	And I dotnetassign the value "jack" to a variable "[[person().name]]"
	When the dotnetassign tool is executed
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable            | New Value |
	| 1 | [[person().name]] = | Kim       |
	| 2 | [[person().name]] = | bob       |
	| 3 | [[person().name]] = | jack      |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[value]] = 30      |
