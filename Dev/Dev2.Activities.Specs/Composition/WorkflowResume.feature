Feature: WorkflowResume
	When a workflow execution is suspended
	I want to Resume

@ResumeWorkflowExecution
Scenario: When Resuming a Workflow it will always resume from the latest Version
	Given I have a workflow "ResumeWorkflowFromVersion"
	And "ResumeWorkflowFromVersion" contains an Assign "VarsAssign" as
		| variable    | value |
		| [[rec().a]] | New   |
		| [[rec().a]] | Test  |
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	And Resource "ResumeWorkflowFromVersion" has rights "Execute" for "ResumeSpecsUser" with password "ASfas123@!fda" in "Users" group
	Then workflow "ResumeWorkflowFromVersion" has "0" Versions in explorer
	When "ResumeWorkflowFromVersion" is executed
	Then the workflow execution has "NO" error
	And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = New  |
		| 2 | [[rec(2).a]] = Test |
	Then I update "ResumeWorkflowFromVersion" by adding "AnotherVarsAssign" as
		| variable     | value              |
		| [[variable]] | NewlyAddedVariable |
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	Then I update "ResumeWorkflowFromVersion" by adding "ThirVarAssign" as
		| variable                | value               |
		| [[ThirdAssignVariable]] | ThirdAssignVariable |
	When workflow "ResumeWorkflowFromVersion" is saved "1" time
	And I reload Server resources
	And I resume the workflow "ResumeWorkflowFromVersion" at "VarsAssign" from version "2"  with user "ResumeSpecsUser"
	Then the workflow execution has "NO" error
	And the "VarsAssign" in Workflow "ResumeWorkflowFromVersion" debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = New  |
		| 2 | [[rec(2).a]] = Test |
	Then CleanupUserGroups workflow "ResumeWorkflowFromVersion"

@ResumeWorkflowExecution
Scenario: Resuming a workflow with an Invalid User Returns Authentication Error
	Given I have a workflow "ResumeWorkflowWithInvalidUser"
	And "ResumeWorkflowWithInvalidUser" contains an Assign "VarsAssign" as
		| variable    | value |
		| [[rec().a]] | New   |
		| [[rec().a]] | Test  |
	When workflow "ResumeWorkflowWithInvalidUser" is saved "1" time
	And Resource "ResumeWorkflowWithInvalidUser" has rights "Execute" for "ResumeSpecsUser" with password "ASfas123@!fda" in "Users" group
	Then Resume workflow "ResumeWorkflowWithInvalidUser" at "VarsAssign" tool with user "InvalidUsername"
	Then Resume has "AN" error
	Then Resume message is "Authentication Error resuming. User InvalidUsername is not authorized to execute the workflow."
	Then CleanupUserGroups workflow "ResumeWorkflowWithInvalidUser"

@ResumeWorkflowExecution
Scenario: Resuming a workflow with a Valid User returns Execution Completed
	Given I have a workflow "ResumeWorkflowWithValidUser"
	And "ResumeWorkflowWithValidUser" contains an Assign "VarsAssign" as
		| variable    | value |
		| [[rec().a]] | New   |
		| [[rec().a]] | Test  |
	When workflow "ResumeWorkflowWithValidUser" is saved "1" time
	And Resource "ResumeWorkflowWithValidUser" has rights "Execute" for "ResumeSpecsUser" with password "ASfas123@!fda" in "Users" group
	Then Resume workflow "ResumeWorkflowWithValidUser" at "VarsAssign" tool with user "ResumeSpecsUser"
	Then Resume has "NO" error
	Then Resume message is "Execution Completed."
	Then CleanupUserGroups workflow "ResumeWorkflowWithValidUser"

#@ResumeWorkflowExecution
#Scenario: Resuming a workflow with a Valid User without setting correct inputs And Resume From SetTheOutputVariable tool returns error from workflow execution
#	Given I have a workflow "ResumeWorkflowWithValidUserWrongValues"
#	And "ResumeWorkflowWithValidUserWrongValues" contains an Assign "VarsAssign" as
#		| variable    | value |
#		| [[rec().a]] | New   |
#		| [[rec().a]] | Test  |
#	When workflow "ResumeWorkflowWithValidUserWrongValues" is saved "1" time
#	And Resource "ResumeWorkflowWithValidUserWrongValues" has rights "Execute" for "ResumeSpecsUser" with password "ASfas123@!fda" in "Users" group
#	Then Resume workflow "ResumeWorkflowWithValidUserWrongValues" at "VarsAssign" tool with user "ResumeSpecsUser"
#	Then Resume has "AN" error
#	Then Resume message is "Scalar value { Name } is NULL: [[Message]] Hello [[Name]]."