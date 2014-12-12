Feature: Explorer
	In order to use my services and connectors
	As a Warewolf User
	I want to have a way to see, open, move and rename services and connectors

Scenario: Default explorer with only localhost
	Given I have connected to localhost
	When localhost has loaded
	Then I should see localhost resources

Scenario: Adding a new environment
	Given I have connected to localhost
	And localhost has loaded
	Then I should see localhost resources
	When I connect to a remote server "RSAKLFSVRTEST"
	And I should see "RSAKLFSVRTEST" resources

Scenario: Saving a new resource
	Given I have connected to localhost
	And localhost has loaded
	Then I should see localhost resources
	When new resource "TestSaveResource" is saved
	Then "TestSaveResource" should be in the explorer

Scenario: Renaming a resource
	Given I have connected to localhost
	And localhost has loaded
	Then I should see localhost resources
	When resource "ResourceForRename" is renamed to "ResourceHasBeenRenamed"
	Then "ResourceHasBeenRenamed" should be in the explorer
	And "ResourceForRename" should not be in the explorer

Scenario: Moving a resource

Scenario: Deleting a resource

Scenario: Display resource permissions

Scenario: Edit a resource

Scenario: Disconnect remote environment

Scenario: Remove remote environment

Scenario: Refresh environment

Scenario: Filter