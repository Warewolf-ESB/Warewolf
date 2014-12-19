@ignore
Feature: Connection
	In order to use connections to other Warewolf servers
	As a Warewolf user
	I want to have a way to manage my servers

Scenario: Add a new server
	Given I have connected to localhost
	And localhost has loaded
	When I add a new server with name "TestNewServer"
	Then "TestNewServer" should exist in my server list

Scenario: Connect to a server
	Given I have connected to localhost
	And localhost has loaded
	And I have "TestNewServer" added
	And I select "TestNewServer"
	When I connect "TestNewServer"
	Then "TestNewServer" is connected
	
Scenario: Disconnect a server
	Given I have connected to localhost
	And localhost has loaded
	And "TestNewServer" is connected
	And I select "TestNewServer"
	When I disconnect "TestNewServer"
	Then "TestNewServer" is disconnected

Scenario: Refresh a server
	Given I have connected to localhost
	And localhost has loaded
	And "TestNewServer" is connected
	And I select "TestNewServer"
	When I refresh
	Then "TestNewServer" resources are refreshed

Scenario: Edit a server
	Given I have connected to localhost
	And localhost has loaded
	And I select "TestNewServer"
	When I edit "TestNewServer"
	Then "TestNewServer" has new details