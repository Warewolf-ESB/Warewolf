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
	And the delete execution has "NO" error

Scenario: Delete an invalid recordset (recordset with no fields declared)
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[GG()]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the delete execution has "AN" error

Scenario: Delete the first record in a recordset 
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rs(1)]]"
	When the delete tool is executed
	Then the delete result should be "Success"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 2   |
	| rs().row | 3   |	
	And the delete execution has "NO" error
	
Scenario: Delete a record using an index from a variable
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And an index "[[index]]" exists with a value "2"
	And I delete a record "[[rs([[index]])]]"
	When the delete tool is executed
	Then the delete result should be "Success"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 1   |	
	| rs().row | 3   |
	And the delete execution has "NO" error

Scenario: Delete a record using a star notation
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rs(*)]]"
	When the delete tool is executed
	Then the delete result should be "Success"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	And the delete execution has "NO" error

Scenario: Delete a record using a negative integer -1
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rs(-1)]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And the delete execution has "AN" error

Scenario: Delete a record that does not exist
	Given I have the following recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I delete a record "[[rs(5)]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And the delete execution has "AN" error

Scenario: Delete a record an empty recordset
	Given I have the following recordset
	| rs       | row |
	And I delete a record "[[rs()]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the recordset "[[rs().row]]" will be as follows
	| rs       | row |
	And the delete execution has "AN" error

Scenario: Delete a scalar insted of a recordset
	Given I have a delete variable "[[var]]" equal to ""
	And I delete a record "[[var]]"
	When the delete tool is executed
	Then the delete result should be "Failure"
	And the delete execution has "AN" error

