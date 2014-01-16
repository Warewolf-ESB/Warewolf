Feature: Deploy
	In order to share and consume resources with other Warewolf users	
	As a Warewolf user 
	I should be able to Deploy and update workflow from Source server to destination server

#NOTE : A conflict occurs when a resource with the same ID exists in both the source and destination server

Scenario: Ensure the deploy is successful when there are no conflicts
	Given I have selected a work flow in the source server
	And the workflow does not exist in the destination server
	When I click the deploy
	Then the workflow should be deployed on the destination server
	
Scenario: Ensure the deploy shows a message when there is a conflicts	
	Given I have selected a work flow in the source server
	And The workflow does exists in the destination server
	When I click the deploy
	Then The system prompts the user to overwrite with OK or Cancel buttons

Scenario: Ensure the deploy proceeds if user clicks OK	
	When  The user selects OK from the message
	Then the workflow should be deployed on the destination server

Scenario: Ensure the deploy stops if user clicks Cancel	
	When  The user selects Cancel from the message
	Then the deploy should be cancelled


