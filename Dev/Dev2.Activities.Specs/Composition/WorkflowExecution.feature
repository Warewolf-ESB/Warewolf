Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Scenario: Simple workflow executing against the server
	 Given I have a workflow "TestWF"
	 And "TestWF" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | yes      |	 
	  When "TestWF" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'TestWF' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |	
	  And the 'Rec To Convert' in Workflow 'TestWF' debug outputs as    
	  | # |                          |
	  | 1 | [[rec(1).a]] =  yes      |
	
	 
