Feature: Deploy
	In order to share and consume resources with other Warewolf users
	
	As a Warewolf user I should be able to Deploy and update workflow from Source server to destination server
	
	Scenario: Ensure that user must be able to select workflow in source server

	Given I have created a workflow
	When open the deploy
	And select the source server
	Then I can be able to select the created workflow

	Scenario: Ensure that user must be able to deploy the workflow from source server to destination server

	Given I have selected a work flow in the source server
	When I click the deploy
	Then the workflow should appear on the destination server
	
	Scenario: Ensure that system should show validation message when deploying the existing workflow in destinaion server
	
	Given I have the workflow in source server 
	When i click the deploy
	Then the system should show validation message ("Overwrite") with "yes" or "no" buttons if we deploy the existing workflow in destination server