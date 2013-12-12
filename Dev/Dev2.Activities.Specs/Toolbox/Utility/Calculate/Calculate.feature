Feature: Calculate
	In order to perform basic calculations
	As a Warewolf user
	I want a tool that I can input a formula and will calculate and retun a result

Scenario: Calculate using a given formula
	Given I have the formula "mod(sqrt(49), 7)"	
	When the calculate tool is executed
	Then the calculate result should be "0"
	And the calculate execution has "NO" error

Scenario: Calculate using multiple scalars and recordset inputs
	Given I have a calculate variable "[[var]]" equal to "1"
	And I have a calculate variable "[[var2]]" equal to "20"
	And I have the formula "((([[var]]+[[var]])/[[var2]])+[[var2]]*[[var]])"
	When the calculate tool is executed
	Then the calculate result should be "20.1"
	And the calculate execution has "NO" error

Scenario: Calculate using Recordset (*) input in an agregate function like SUM
	Given I have a calculate variable "[[var(*).int]]" equal to 
	| var().int	|
	| 1			|
	| 2			|
	| 3			|
	And I have the formula "SUM([[var(*).int]])"
	When the calculate tool is executed
	Then the calculate result should be "6"
	And the calculate execution has "NO" error

Scenario: Calculate using incorrect formula
	Given I have the formula "asdf"
	When the calculate tool is executed
	Then the calculate result should be ""
	And the calculate execution has "AN" error

Scenario: Calculate using variable as full calculation
	Given I have a calculate variable "[[var]]" equal to "sum(1,2,3)-5"
	And I have the formula ""
	When the calculate tool is executed
	Then the calculate result should be "1"
	And the calculate execution has "NO" error

Scenario: Calculate using a negative index recordset value
	Given I have the formula "[[my(-1).formula]]"
	When the calculate tool is executed
	Then the calculate execution has "AN" error