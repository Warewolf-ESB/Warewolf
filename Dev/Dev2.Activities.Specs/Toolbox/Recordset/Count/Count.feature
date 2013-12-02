Feature: Count
	In order to count records
	As a Warewolf user
	I want a tool that takes a record set counts it

@NumberOfRecordsInARecordset
Scenario: Count a number of records in a recordset
	Given I have a recordset with this shape
	| rs       |
	| rs().row |
	| rs().row |
	| rs().row |	
	When the count tool is executed
	Then the result count should be 3
