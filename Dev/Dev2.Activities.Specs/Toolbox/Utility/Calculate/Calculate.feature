Feature: Calculate
	In order to perform basic calculations
	As a Warewolf user
	I want a tool that I can input a formula and will calculate and retun a result

@mytag
Scenario: Calculate using a given formula
	Given I have the formula "mod(sqrt(49), 7)"	
	When the calculate tool is executed
	Then the result should be 0
