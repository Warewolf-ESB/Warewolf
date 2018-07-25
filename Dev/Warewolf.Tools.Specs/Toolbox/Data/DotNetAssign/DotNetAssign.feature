Feature: DotNetAssign
	In order to use variables
	As a Warewolf user
	I want a tool that assigns data to variables

@mytag
Scenario: DotNetAssign multiple variables to the same recordset
	Given I dotnetassign the value "Kim" to a variable "[[person().name]]"
	And I dotnetassign the value "bob" to a variable "[[person().name]]"
	And I dotnetassign the value "jack" to a variable "[[person().name]]"
	When the dotnetassign tool is executed
	And the execution has "NO" error
	Then the debug inputs count equals "3"
	And the debug inputs as
	| # | Variable            | New Value |
	| 1 | [[person().name]] = | Kim       |
	| 2 | [[person().name]] = | bob       |
	| 3 | [[person().name]] = | jack      |
	And the debug output as
    | # |                     |
    | 1 | [[person(1).name]] = Kim |
    | 2 | [[person(2).name]] = bob |
    | 3 | [[person(3).name]] = jack |   
