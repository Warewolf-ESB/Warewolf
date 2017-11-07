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
	 
Scenario: Merge Workflow with Assign tool As First Tool And Split tool as Second tool count
	 Given I Load workflow "WorkflowWithDifferentToolSequence" from "localhost"
	 And I Load workflow "WorkflowWithDifferentToolSequence" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkflowWithDifferentToolSequence"
	 Then Current workflow contains "2" tools
	 And Different workflow contains "2" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is "0" 