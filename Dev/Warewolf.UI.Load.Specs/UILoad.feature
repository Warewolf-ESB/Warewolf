@UILoad
Feature: UILoad
	As a Warewolf Studio user
	I can only tolerate so much lag

Scenario: Studio UI Load Test
	Given there are "20" duplicates of All Tools workflow in the explorer
	And I start the timer
	When I Select Local Server Source From Explorer
	And I Refresh Explorer
	Then the timer duration is between "5" and "60" seconds
	And I Click Deploy Ribbon Button
	And I Select LocalServerSource From Deploy Tab Destination Server Combobox
	And I Select localhost from the source tab
	Given I start the timer
	When I Click Deploy Tab Deploy Button with no version conflict dialog
	Then the timer duration is between "160" and "320" seconds
	When I open "Debug Output UI Load Testing" workflow
	Given I start the timer
	When I Debug with input of "200"
	Then the timer duration is between "60" and "120" seconds
	Given I start the timer
	When I Filter the Debug with "Match Item"
	Then the timer duration is between "10" and "30" seconds
	When I open "Variable List UI Load Testing" workflow
	#Given I start the timer
	#When I Enter variable text as "[[new_variable]]" and value text as "new value" into assign row 1
	#Then the timer duration is between "180" and "360" seconds
	Given I start the timer
	When I Remove Assign Row 1 With Context Menu
	And I Click VariableList Scalar Row1 Delete Button
	Then the timer duration is between "30" and "60" seconds
	Given I have "20" All Tools workflows tabs open
	And I start the timer
	When I Filter the Explorer with "All Tools"
	And I Open Explorer First Item With Double Click
	Then the timer duration is between "30" and "60" seconds
	Given I start the timer
	When I open "Large Workflow UI Load Testing" workflow
	Then the timer duration is between "45" and "90" seconds
	Given I start the timer
	When I Click Close Workflow Tab Button
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I Select Show Dependencies In Explorer Context Menu for service "Dependencies Graph UI Load Testing"
	Then the timer duration is between "10" and "60" seconds
	Given I have "100" scheduled tasks
	And I start the timer
	When I Click Scheduler Ribbon Button
	Then the timer duration is between "5" and "30" seconds
	When I create a new scheduled task using shortcut
	And I Click Scheduler ResourcePicker Button
	And I Select Service "Tests UI Load Testing" In Service Picker
	And I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab
	Given I start the timer
	When I Click Save Ribbon Button And Wait For Save
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I delete the UI Load Test scheduled task
	Then the timer duration is between "5" and "30" seconds
	When I Filter the Explorer with "Tests UI Load Testing"
	And I start the timer
	And I Click Show Explorer First Item Tests From Explorer Context Menu
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I Click The Create a New Test Button
	And I Click Save Ribbon Button And Wait For Save
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I Delete The First Test
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I close the Studio
	Then the timer duration is between "10" and "60" seconds
	Given I start the timer
	When I start the Studio
	Then the timer duration is between "10" and "60" seconds
	
