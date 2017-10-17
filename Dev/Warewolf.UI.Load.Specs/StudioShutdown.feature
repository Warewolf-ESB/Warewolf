@StudioStartup
Feature: StudioStartup
	As a Warewolf Studio user
	I have to wait for hte Studio to start

Scenario: Studio Startup
	Given I start the server with "UITests" resources
	And "30" items already exist in the studio workspace
	And I start the timer
	When I start the studio
	Then the timer duration is less than "5" seconds
