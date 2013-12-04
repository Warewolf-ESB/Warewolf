Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

Scenario: Execute a foreach for every record in a recordset	
	Given I there is a recordset in the datalist with this shape
	| rs       | value |
	| rs().row | 1     |
	| rs().row | 2     |
	| rs().row | 3     |
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	When the foreach tool is executed
	Then the foreach will loop over 3 records
	