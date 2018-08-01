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
