Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Merge AssignOnlyWithNoOutput Workflow with Same Version
	 Given I Load workflow "AssignOnlyWithNoOutput" from "localhost"
	 And I Load workflow "AssignOnlyWithNoOutput" from "Remote Connection Integration"	 
	 When Merge Window is opened with "AssignOnlyWithNoOutput"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has no Conflicting tools


Scenario: Merge VersionHelloWorld Workflow 
	 Given I Load workflow "Hello World" from "localhost"
	 And I Load workflow "VersionHelloWorld" from "Remote Connection Integration"	 
	 When Merge Window is opened with "VersionHelloWorld"
	 Then Current workflow contains "6" tools
	 And Different workflow contains "7" tools
	 And Merge conflicts count is "7"
	 And Merge variable conflicts is false
	 And Merge window has "0" Conflicting tools


