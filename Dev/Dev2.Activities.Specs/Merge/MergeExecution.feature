Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag

Scenario: Merge Workflow with Different Version
	 Given I have a workflow "MergeWithVersionAssignTest"
	 And "MergeWithVersionAssignTest" contains an Assign "VarsAssign" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |	 
	  When workflow "MergeWithVersionAssignTest" is saved "1" time
	  Then workflow "MergeWithVersionAssignTest" has "0" Versions in explorer
	  When workflow "MergeWithVersionAssignTest" is saved "2" time
	  Then workflow "MergeWithVersionAssignTest" has "2" Versions in explorer
	  And explorer as 
	  | Explorer        |
	  | MergeWithAssign |
	  | v.2 DateTime    |
	  | v.1 DateTime    |
	  When workflow "MergeWithVersionAssignTest" merge is opened
	  Then Current workflow contains "1" tools
	  And Different workflow contains "2" tools
	  And explorer as 
	  | Explorer          |
	  | MergeWithAssign   |
	  | v.2 DateTime Save |
	  | v.1 DateTime Save |
	  And workflow "MergeWithVersionAssignTest" is deleted as cleanup

