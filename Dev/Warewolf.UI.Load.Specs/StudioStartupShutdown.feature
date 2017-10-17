@StudioStartupShutdown
Feature: StudioStartupShutdown
	As a Warewolf Studio user
	I have to wait for the Studio to shutdown fully
	
Scenario: Studio Startup And Shutdown
	Given I open "30" tabs
	And I start the timer
	When I close the studio
	Then the timer duration is less than "5" seconds
	Given I open "30" new tabs
	And I start the timer
	When I shutdown the studio
	Then the timer duration is less than "5" seconds
