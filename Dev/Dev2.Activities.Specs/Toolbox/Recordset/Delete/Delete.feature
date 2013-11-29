Feature: Delete
	In order to delete records
	As a Warewolf user
	I want a tool that takes a record set and deletes it

Scenario: Delete records in a recordset
	Given I have the following recordset
	| rs       |
	| rs().row |
	| rs().row |
	| rs().row |	
	When the delete tool is executed
	Then the delete result should be "Success"
