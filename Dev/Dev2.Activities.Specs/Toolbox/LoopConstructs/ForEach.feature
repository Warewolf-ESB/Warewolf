Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

Scenario: Execute a tool a number of times	
	Given I have the count to execute a recordset with this shape
	| rs       |
	| rs().row |
	| rs().row |
	| rs().row |	
	And I have the foreach type as "No. of Executes"
	And I have the number of executes as "6"
	When the foreach tool is executed
	Then the foreach result should be as follows ""
