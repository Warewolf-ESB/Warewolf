Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "TestWF" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | yes      |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssign' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |	
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssign' debug outputs as    
	  | # |                          |
	  | 1 | [[rec(1).a]] =  yes      |
	
	 
