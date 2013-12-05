Feature: FindRecordIndex
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset
	Given I have the following recordset to search
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search type is "Starts With" and criteria is "Warewolf"
	When the find records index tool is executed
	Then the index result should be 5

