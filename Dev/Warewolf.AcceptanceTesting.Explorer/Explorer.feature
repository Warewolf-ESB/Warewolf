@Explorer
Feature: Explorer
	In order to manage my service
	As a Warewolf User
	I want explorer view of my resources with management options

Scenario: Connected to localhost server
	Given the explorer is visible
	When I open "localhost" server
	Then I should see "5" folders

Scenario: Expand a folder
	Given the explorer is visible
	And I open "localhost" server
	When I open "Folder 2"
	Then I should see "6" children for "Folder 2"

