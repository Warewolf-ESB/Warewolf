@UILoad
Feature: UILoad
	As a Warewolf Studio user
	I can only tolerate so much lag

Scenario: Connect remote server
	Given The Warewolf Studio is running
	And I start the timer
	When I Select Local Server Source From Explorer
	Then the timer duration is less than "5" seconds

Scenario: Refresh Explorer
	Given The Warewolf Studio is running
	And I start the timer
	When I refresh localhost
	Then the timer duration is less than "5" seconds

Scenario: Refresh Deploy Source Server
	Given The Warewolf Studio is running
	And I open deploy
	And I start the timer
	When I Click Deploy Tab Source Refresh Button
	Then the timer duration is less than "5" seconds

Scenario: Deploy Resources
	Given The Warewolf Studio is running
	And I Click Deploy Ribbon Button
	And I Select localhost from the source tab
	And I start the timer
	When I Click Deploy Tab Deploy Button
	Then the timer duration is less than "5" seconds

Scenario: Get Debug Output
	Given The Warewolf Studio is running
	And I open "Debug Output UI Load Testing"
	And I start the timer
	When I Press F6
	Then the timer duration is less than "5" seconds

Scenario: Add A Variable
	Given The Warewolf Studio is running
	And I open "Variable List UI Load Testing"
	And I start the timer
	When I Enter variable text as "[[new_variable]]" and value text as "new value" into assign row 1
	Then the timer duration is less than "5" seconds

Scenario: Remove A Variable
	Given The Warewolf Studio is running
	And I open "Variable List UI Load Testing"
	And I start the timer
	When I Remove Assign Row 1 With Context Menu
	Then the timer duration is less than "5" seconds

Scenario: Open New Tab
	Given The Warewolf Studio is running
	And I open "30" new workflow tabs
	And I start the timer
	When I open a new worklow tab
	Then the timer duration is less than "5" seconds

Scenario: Close New Tab
	Given The Warewolf Studio is running
	And I start the timer
	When I open "Large Workflow UI Load Testing"
	Then the timer duration is less than "5" seconds
	And I start the timer
	When I close worklow tab
	Then the timer duration is less than "5" seconds

Scenario: Open Dependencies Graph
	Given The Warewolf Studio is running
	And I start the timer
	When I open "Dependencies Graph UI Load Testing"
	Then the timer duration is less than "5" seconds

Scenario: Open Scheduler View And Add Remove Scheduled Tasks
	Given The Warewolf Studio is running
	And there are "100" scheduled tasks
	And I start the timer
	When I open scheduler
	Then the timer duration is less than "5" seconds
	Given I start the timer
	When I add a scheduled task
	Then the timer duration is less than "5" seconds
	Given I start the timer
	When I remove a scheduled task
	Then the timer duration is less than "5" seconds

Scenario: Open Test View And Add Remove Tests
	Given The Warewolf Studio is running
	And I start the timer
	When I open "Tests UI Load Testing" workflow tabs
	Then the timer duration is less than "5" seconds
	Given I start the timer
	When I add a test
	Then the timer duration is less than "5" seconds
	Given I start the timer
	When I remove a test
	Then the timer duration is less than "5" seconds
	
