Feature: ControlFlow - Sequence
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Sequence
Scenario: Four tool names is visible in sequence small view
	Given I have Sequence small view on design surface
	And I have Assign in Sequence small view
	And I have Data Merge in Sequence small view
	And I have Data Split in Sequence small view
	And I have Copy in Sequence small view
	Then Scroll bar is "Disabled"
	And drop activity is visible
	
	##Large view must have done button so please add Done button for large view like other large views.
Scenario: Tools are visible when I expand large view
	Given I have Sequence small view on design surface
	And I have "Assign" in Sequence small view
	And I have "Data Merge" in Sequence small view
	And I have "Data Split" in Sequence small view
	And I have "Copy" in Sequence small view
	And I have "Create" in Sequence small view
	Then Scroll bar is "Enabled"
	And drop activity is visible	
	When I open Sequence large view
	Then "Assign" is "visible" in sequence large view
	Then "Data Merge" is "visible" in sequence large view
	Then "Data Split" is "visible" in sequence large view
	Then "Copy" is "visible" in sequence large view
	Then "Create" is "visible" in sequence large view
	And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
    And End this workflow is "Unselected"
    And Done button is "Visible"

Scenario: Service in sequence
	Given I have Sequence small view on design surface
	And I have "Service" in Sequence small view
	And I have "Data Merge" in Sequence small view
	When I open Sequence large view
	Then "Service" is "visible" in sequence large view
	And "Data Merge" is "visible" in sequence large view
	And Done button is "Visible"
	When I click on "Done"
	Then Sequence small view is visible