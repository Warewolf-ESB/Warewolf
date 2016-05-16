Feature: SwitchDialog
	In order to create a switch function
	As a Warewolf User
	I want to be shown the switch window setup

Scenario: Using switch tool
	Given I open a "New Workflow"
	And drop a "Switch" tool onto the design surface
	Then the Switch tool window is opened
	And "Variable to Switch on" is "Enabled"
	And "Display text" is "Enabled"
	And I have variable "[[var]]" equals "4s"
	And "Variable to Switch on" equals "[[var]]"
	Then "Display text" is changed to "[[var]]"


Scenario: Assigning values to Display  
	Given I open a "New Workflow"
	And drop a "Switch" tool onto the design surface
	Then the Switch tool window is opened
	And I have variable "[[var]]" equals "4s"
	And I have variable "[[rec().set]]" equals "Age"
	And "Variable to Switch on" equals "[[var]]"
	And "Display text" equals "[[rec().set]]"
	And "Done" is selected
	And I set the default arm
	When I set the switch arm as "1"
	Then "1" is the display text 

# workitem for validation for popups in backlog.
Scenario: Assigning variable to switch arms
	Given I open a "New Workflow"
	And drop a "Switch" tool onto the design surface
	Then the Switch tool window is opened
	And "Variable to Switch on" equals "[[var]]"
	And "Display text" equals "[[var]]"
	And "Done" is selected
	And the switch tool window is closed
	And "[[var]]" is visible in the variable list
	And I set the default arm
	When I set the switch arm as "[[var]]"
	And a validation error is shown 
