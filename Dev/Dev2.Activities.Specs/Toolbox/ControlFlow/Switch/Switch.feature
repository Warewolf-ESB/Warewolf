Feature: Switch
	In order to branch based on the data
	As Warewolf user
	I want tool has multiple branching decisions based on the data

Scenario: Add two numbers
	Given I need to switch on variable "[[A]]" with the value "30"	
	And the switch cases are 
	| case |
	| 10   |
	| 20   |
	| 30   |
	| 40   |
	| 50   |	
	When the switch tool is executed
	Then the switch result should be ""
