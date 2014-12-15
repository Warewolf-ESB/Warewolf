Feature: Deploy
	In order to share and consume resources with other Warewolf users	
	As a Warewolf user 
	I should be able to Deploy and update workflow from Source server to destination server

#NOTE : A conflict occurs when a resource with the same ID exists in both the source and destination server

Scenario: Ensure the deploy is successful when there are no conflicts
	Given I have selected a work flow  that "doesn't" exist in the destination server
	When I click the deploy
	Then the workflow "should" be deployed on the destination server
	
Scenario: Ensure the resource is deployed successfully when user selects OK	
	Given I have selected a work flow  that "does" exist in the destination server
	And the user selects "OK" when prompted to continue
	When I click the deploy	
	Then the workflow "should" be deployed on the destination server

Scenario: Ensure the resource is not deployed when user selects Cancel	
	Given I have selected a work flow  that "does" exist in the destination server
	And the user selects "Cancel" when prompted to continue
	When I click the deploy	
	Then the workflow "shouldn't" be deployed on the destination server
