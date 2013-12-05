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
	And I sort a record "[[rs(*).row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | are      |
	| rs().row | best     |
	| rs().row | so far   |
	| rs().row | the      |
	| rs().row | user     |
	| rs().row | Warewolf |
	| rs().row | You      |


	#sort records backward using start notation rec(*).row
	#sort records backward using start notation rec().row
	
	#sort records forward using start notation rec(*).row
	#sort records forward using start notation rec().row