@UILoad
Feature: UILoad
	As a Warewolf Studio user
	I can only tolerate so much lag

Scenario: Connect remote server
	Given The Warewolf Studio is running
	And I start the timer
	When I Select Local Server Source From Explorer
	Then the timer duration is less than "30" seconds

Scenario: Refresh Explorer
	Given The Warewolf Studio is running
	And I start the timer
	When I Refresh Explorer
	Then the timer duration is less than "30" seconds

Scenario: Refresh Deploy Source Server
	Given The Warewolf Studio is running
	And I Click Deploy Ribbon Button
	And I start the timer
	When I Click Deploy Tab Source Refresh Button
	Then the timer duration is less than "20" seconds

Scenario: Deploy Resources
	Given The Warewolf Studio is running
	And I Click Deploy Ribbon Button
	And I Select LocalServerSource From Deploy Tab Destination Server Combobox
	And I Select localhost from the source tab
	And I start the timer
	When I Click Deploy Tab Deploy Button
	Then the timer duration is less than "160" seconds

Scenario: Get Debug Output
	Given The Warewolf Studio is running
	And I open "Debug Output UI Load Testing"
	And I start the timer
	When I Debug with input of "100"
	Then the timer duration is less than "60" seconds

Scenario: Add Remove A Variable
	Given The Warewolf Studio is running
	And I open "Variable List UI Load Testing"
	And I start the timer
	When I Enter variable text as "[[new_variable]]" and value text as "new value" into assign row 1
	Then the timer duration is less than "180" seconds
	Given I start the timer
	When I Remove Assign Row 1 With Context Menu
	Then the timer duration is less than "180" seconds
	
Scenario: Open New Tab
	Given The Warewolf Studio is running
	And I have "20" new workflow tabs open
	And I start the timer
	When I Click New Workflow Ribbon Button
	Then the timer duration is less than "20" seconds

Scenario: Close New Tab
	Given The Warewolf Studio is running
	And I start the timer
	When I open "Large Workflow UI Load Testing"
	Then the timer duration is less than "45" seconds
	Given I start the timer
	When I Click Close Workflow Tab Button
	Then the timer duration is less than "30" seconds

Scenario: Open Dependencies Graph
	Given The Warewolf Studio is running
	And I start the timer
	When I Select Show Dependencies In Explorer Context Menu for service "Dependencies Graph UI Load Testing"
	Then the timer duration is less than "60" seconds

@SchedulerView
Scenario: Open Scheduler View And Add Remove Scheduled Tasks
	Given The Warewolf Studio is running
	And I have "100" scheduled tasks
	And I start the timer
	When I Click Scheduler Ribbon Button
	Then the timer duration is less than "5" seconds
	When I create a new scheduled task using shortcut
	And I Click Scheduler ResourcePicker Button
	And I Select Service "Tests UI Load Testing" In Service Picker
	And I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab
	Given I start the timer
	When I Click Save Ribbon Button And Wait For Save
	Then the timer duration is less than "15" seconds
	Given I start the timer
	When I delete the UI Load Test scheduled task
	Then the timer duration is less than "5" seconds

Scenario: Open Test View And Add Remove Tests
	Given The Warewolf Studio is running
	When I Filter the Explorer with "Tests UI Load Testing"
	And I start the timer
	And I Click Show Explorer First Item Tests From Explorer Context Menu
	Then the timer duration is less than "60" seconds
	Given I start the timer
	When I Click The Create a New Test Button
	And I Click Save Ribbon Button And Wait For Save
	Then the timer duration is less than "15" seconds
	Given I start the timer
	When I Delete The First Test
	Then the timer duration is less than "30" seconds
	
