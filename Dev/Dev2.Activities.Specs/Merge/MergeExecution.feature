Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag

Scenario: Merge AssignOnlyWithNoOutput Workflow with Same Version
	 Given I Load workflow "AssignOnlyWithNoOutput" from "localhost"
	 And I Load workflow "AssignOnlyWithNoOutput" from "Remote Connection Integration"	 
	 When Merge Window is opened with "AssignOnlyWithNoOutput"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "0"
	 And Merge variable conflicts is "0"

