@Deploy
Feature: Deploy Feature
In order to schedule workflows
	As a Warewolf user
	I want to setup schedules

@ignore
Scenario: Deploy a renamed resource to localhost
Given I am Connected to remote server "tst-ci-remote"
And I reload the destination resources
And the destination resource is "RenamedWorkFlowToDeploy"
And I select resource "OriginalWorkFlowName" from source server
And And the localhost resource is "OriginalWorkFlowName"
When I Deploy resource to remote
And I reload the destination resources
Then the destination resource is "OriginalWorkFlowName"
