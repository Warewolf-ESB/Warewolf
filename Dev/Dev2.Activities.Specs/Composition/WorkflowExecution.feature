Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssign' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssign' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  
Scenario: Workflow with multiple tools executing against the server
	  Given I have a workflow "WorkflowWithAssignAndCount"
	  And "WorkflowWithAssignAndCount" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |
	  And "WorkflowWithAssignAndCount" contains Count Record "CountRec" on "[[rec()]]" into "[[count]]"
	  When "WorkflowWithAssignAndCount" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssignAndCount' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssignAndCount' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  And the 'CountRec' in WorkFlow 'WorkflowWithAssignAndCount' debug inputs as
	  | Recordset            |
	  | [[rec(1).a]] = yes |
	  | [[rec(2).a]] = no |
	  And the 'CountRec' in Workflow 'WorkflowWithAssignAndCount' debug outputs as    
	  |               |
	  | [[count]] = 2 |
	
Scenario: Simple workflow executing against the server with a database service
	 Given I have a workflow "TestDbServiceWF"
	 And "TestDbServiceWF" contains a database service "Fetch" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  |                  |               | dbo_proc_SmallFetch(*).Value | [[rec().fetch]] |
	 And "TestDbServiceWF" contains Count Record "Count" on "[[rec()]]" into "[[count]]"
	  When "TestDbServiceWF" is executed
	  Then the workflow execution has "NO" error
	  And the 'Fetch' in WorkFlow 'TestDbServiceWF' debug inputs as
	  |  |
	  |  |
	  And the 'Fetch' in Workflow 'TestDbServiceWF' debug outputs as
	  |                      |
	  | [[rec(9).fetch]] = 5 |
	  And the 'Count' in WorkFlow 'TestDbServiceWF' debug inputs as
	  | Recordset            |
	  | [[rec(1).fetch]] = 1 |
	  | [[rec(2).fetch]] = 2 |
	  | [[rec(3).fetch]] = 1 |
	  | [[rec(4).fetch]] = 2 |
	  | [[rec(5).fetch]] = 1 |
	  | [[rec(6).fetch]] = 2 |
	  | [[rec(7).fetch]] = 1 |
	  | [[rec(8).fetch]] = 2 |
	  | [[rec(9).fetch]] = 5 |
	 And the 'Count' in Workflow 'TestDbServiceWF' debug outputs as    
	 |               |
	 | [[count]] = 9 |
	
	  