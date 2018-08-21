Feature: WorkflowResume
	When a workflow execution fails
	I want to Resume

Scenario: Resuming a non existent workflow returns error message
	Given I have server running at "localhost"
	And I resume workflow "acb75027-ddeb-47d7-814e-a54c37247ec1"
	Then an error "this workflow is  not resumable"
	
Scenario: Resuming a workflow returns resume message
	Given I have server running at "localhost"
	And I resume workflow "acb75027-ddeb-47d7-814e-a54c37247ec1"
	Then Resume message is "workflow not resumable"

Scenario: Resuming Workflow From a specific Version 
	Given I have a workflow "ResumeWorkflowFromVersion"
	And "ResumeWorkflowFromVersion" contains an Assign "VarsAssign" as
	| variable    | value |
	| [[rec().a]] | New   |
	| [[rec().a]] | Test  |	 
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	Then workflow "ResumeWorkflowFromVersion" has "0" Versions in explorer
	When "WorkflowWithAssignAndCount" is executed
	Then the workflow execution has "NO" error
	And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as    
	| # |                     |
	| 1 | [[rec(1).a]] = New  |
	| 2 | [[rec(2).a]] = Test |
	Then I update "VarsAssign" inputs in "ResumeWorkflowFromVersion" as
	| variable    | value   |
	| [[rec().a]] | Updated |
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	Then I update "VarsAssign" inputs in "ResumeWorkflowFromVersion" as
	| variable    | value       |
	| [[rec().a]] | LastUpdated |
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	And I reload Server resources
	And I resume the workflow "ResumeWorkflowFromVersion" at "VarsAssign" from version "2"
	Then the workflow execution has "NO" error
	And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as    
	| # |                        |
	| 1 | [[rec(1).a]] = New     |
	| 2 | [[rec(2).a]] = Test    |
	| 3 | [[rec(3).a]] = Updated |
