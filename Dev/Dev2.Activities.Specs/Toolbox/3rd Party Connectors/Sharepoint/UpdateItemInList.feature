@sharepoint
Feature: UpdateItemInList
	In order to update and item in a SharePoint list
	As a Warewolf user
	I want to a tool that allows updating the item

Background: Setup for sharepoint scenerio
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I create 2 items in the list

Scenario: Update all items in list with static data
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 |
	| 1 | Name       | Updated From Warewolf                    |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |