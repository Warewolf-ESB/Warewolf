Feature: StudioTestFrameworkAdvancedRecordset
	In order to validate sql executing over a recordset
	As a Warewolf developer
	I want to be what sql is supported

Scenario: Test WF with Advanced Recordset
	Given I have a workflow "AdvancedRecsetTestWF"
	And "AdvancedRecsetTestWF" contains an Assign "assignrecordset" as
    | variable              | value |
    | [[person(1).name]]    | Bob   |
    | [[person(2).name]]    | Alice |
    | [[person(1).surname]] | Smith |
    | [[person(2).surname]] | Jones |
	And "AdvancedRecsetTestWF" contains Advanced Recordset "selectall" with Query "Select * from person"	 
	 | MappedTo | MappedFrom              |
	 | name     | [[TableCopy().name]]    |
	 | surname  | [[TableCopy().surname]] |
	And I save workflow "AdvancedRecsetTestWF"
	Then the test builder is open with "AdvancedRecsetTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "assignrecordset" as TestStep with
	| Variable Name         | Condition | Value |
	| [[person(1).name]]    | =         | Bob   |
	| [[person(2).name]]    | =         | Alice |
	| [[person(1).surname]] | =         | Smith |
	| [[person(2).surname]] | =         | Jones |  
	And I Add "selectall" as TestStep with 
	| Variable Name            | Condition | Value |
	| [[TableCopy(2).name]]    | =         | Alice |
	| [[TableCopy(2).surname]] | =         | Jones |	
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "AdvancedRecsetTestWF" is deleted as cleanup
