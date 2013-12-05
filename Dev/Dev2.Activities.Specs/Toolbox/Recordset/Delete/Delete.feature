Feature: Delete
	In order to delete records
	As a Warewolf user
	I want a tool that takes a record set and deletes it

Scenario: Delete last record in a recordset 
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rs()]]"
	When the delete tool is executed
	Then the delete result should be "Success"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |

Scenario: Delete a recordset that does not exist
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rd()]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |


	#Delete the first record
	#Delete an indexed record e.g. rs(2)
	#Delete an indexed record using a variable index e.g.  rs([[index]])
	#Delete all records e.g rs(*)

#Scenario: Delete an index that does not 
