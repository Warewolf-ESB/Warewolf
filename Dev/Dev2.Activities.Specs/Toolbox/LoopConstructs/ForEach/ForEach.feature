Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

Scenario: Execute a foreach for every record in a recordset	using an activity
	Given I there is a recordset in the datalist with this shape
	| rs              | value | name     | type   |
	| [[rs().row]]    | 1     | rowone   | input  |
	| [[rs().row]]    | 2     | rowtwo   | input  |
	| [[rs().row]]    | 3     | rowthree | input  |
	| [[test().data]] |       | rowone   | output |
	| [[test().data]] |       | rowtwo   | output |
	| [[test().data]] |       | rowthree | output |
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Activity"
	When the foreach tool is executed
	Then the foreach will loop over 3 records
	

Scenario: Execute a foreach for every record in a recordset	using a Tool
	Given I there is a recordset in the datalist with this shape
	| rs              | value | name     | type   |
	| [[rs().row]]    | 1     | rowone   | input  |
	| [[rs().row]]    | 2     | rowtwo   | input  |
	| [[rs().row]]    | 3     | rowthree | input  |
	| [[test().data]] |       | rowone   | output |
	| [[test().data]] |       | rowtwo   | output |
	| [[test().data]] |       | rowthree | output |
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach will loop over 3 records