Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Scenario: Simple workflow executing against the server
	 Given I have a workflow "TestWF"
	 And "TestWF" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | yes      |
	  | [[rec().a]] | warewolf |
	  When "TestWF" is executed
	  Then the workflow execution has "NO" error
	  And the "Rec To Convert" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 0x4141    |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Rec To Convert" debug outputs as    
	  |   | Records                  |
	  | 1 | [[rec().a]] =  0x4141    |
	  | 2 | [[rec().a]] =   warewolf |
