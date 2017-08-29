Feature: Deploy Feature
In order to schedule workflows
	As a Warewolf user
	I want to setup schedules

Scenario: Deploy a renamed resource to localhost
Given I am Connected to source server "tst-ci-remote"
And I select resource "RenamedWorkFlowToDeploy" 
And And the localhost resource is "OriginalWorkFlowName"
When I Deploy resource to localhost
And I reload the local resource
Then And the localhost resource is "RenamedWorkFlowToDeploy"
