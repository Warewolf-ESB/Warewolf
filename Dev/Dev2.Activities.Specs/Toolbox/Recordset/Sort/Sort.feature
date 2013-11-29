Feature: Sort
	In order to sort a recordset
	As a Warewolf user
	I want a tool I can use to arrange records in either ascending or descending order 

Scenario: Sort a recordset
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the recordser will be 
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
