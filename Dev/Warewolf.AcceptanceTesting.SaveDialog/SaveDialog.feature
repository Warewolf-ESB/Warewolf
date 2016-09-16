@SaveDialog
Feature: SaveDialog
	In order to save resources
	As a Warewolf user
	I want a save dialog

## Ensure system can create Folder from Save Dialog under localhost
## Ensure system can save a Workflow in localhost
## Ensure system can save a Workflow in localhost folder
## Ensure system can save button is Enabled when I enter new name for resource and filter
## Ensure system can save with duplicate name and expect validation
## Ensure system can save resource names with special character expect validation
## Ensure system can cancel Saving a Workflow in localhost folder
## Ensure system can save with Filter
## Context Menu Folder actions
## Ensure system can save a Workflow in localhost with the correct Permission


Scenario: Creating Folder from Save Dialog under localhost
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders
	When I create "New Folder" in "localhost"
	Then I should see "6" folders
	
Scenario: Save button is Enabled when I enter new name for resource and filter
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	Then save button is "Disabled"
	And Filter is "Folder 1"
	Then I should see "1" folders
	And I open "Folder 1" in save dialog 
	When I enter name "Savewf"
	Then save button is "enabled"
	And validation message is ""

Scenario: Save with duplicate name and expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 2" in save dialog 
	When I enter name "Folder 2 Child 1"
	Then save button is "Disabled"
	And validation message is "An item with name "Folder 2 Child 1" already exists in this folder."

Scenario: Save resource names with special character expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 1" in save dialog 
	When I enter name "Save@#$"
	Then save button is "Disabled"
	And validation message is ""Name" contains invalid characters."


Scenario: Saving a Workflow in localhost with an existing name
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And "Hello World" exists
	When I attempt to save a workflow as "Hello World"
	Then an error message appear with the value "An item with name "Hello World" already exists in this folder."


