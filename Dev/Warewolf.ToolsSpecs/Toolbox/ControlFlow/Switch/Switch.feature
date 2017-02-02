Feature: Switch
	In order to branch based on the data
	As Warewolf user
	I want tool has multiple branching decisions based on the data

Scenario: Ensure that a variable evaluates to the value on the datalist
	Given I need to switch on variable "[[A]]" with the value "30"		
	When the switch tool is executed
	Then the variable "[[A]]" will evaluate to "30"
	Then the execution has "NO" error
	And the debug inputs as
	| Switch on   |
	| [[A]] =  30 |


Scenario: Ensure that a negative index throws an error	
	Given I need to switch on variable "[[rec(-1).val]]" with the value "Moses Mabida Stadium"		
	When the switch tool is executed	
	Then the execution has "AN" error


Scenario Outline: Ensure that a variable/recordset evaluates to the value on the datalist
	Given I need to switch on variable "<variable>" with the value "<val>"		
	When the switch tool is executed
	Then the variable "<variable>" will evaluate to "<val>"
	Then the execution has "NO" error
	And the debug inputs as
	| Switch on  |
	| "<switch>" |
Examples: 
| variable     | val | switch           |
| [[a]]        |     | [[a]] =          |
| [[rec().a]]  | 3   | [[rec().a]] = 3  |
| [[rec(1).a]] | 3   | [[rec(1).a]] = 3 |
| [[rec(*).a]] | 3   | [[rec(*).a]] = 3 |