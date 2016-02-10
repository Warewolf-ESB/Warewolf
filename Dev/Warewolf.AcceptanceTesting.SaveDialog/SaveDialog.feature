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

#coded ui
@Ignore
Scenario: Saving a Workflow in localhost
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I save "localhost\Newworkflow"
	Then "Newworkflow" is visible in "localhost"

#coded ui
@Ignore
Scenario: Saving a Workflow in localhost nested folder and shows correctly on reload
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I open "Folder 1" in save dialog 
	When I create folder "This" in "localhost\Folder 1"
	When I save "localhost\Folder 1\This\Newworkflow"
	Then "Newworkflow" is visible in "localhost\Folder 1\This"	
	And the Save Dialog is opened
	And I open "localhost\Folder 1\This" in save dialog 
	Then "Newworkflow" is visible in "localhost\Folder 1\This"	
	
Scenario: Save button is Enabled when I enter new name for resource and filter
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	Then save button is "Disabled"
	And Filter is "Folder 1"
	Then I should see "1" folders
	When I refresh the filter
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
	And validation message is "An item with name 'Folder 2 Child 1' already exists in this folder."

Scenario: Save resource names with special character expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 1" in save dialog 
	When I enter name "Save@#$"
	Then save button is "Disabled"
	And validation message is "'Name' contains invalid characters."


Scenario: Cancel Saving a Workflow in localhost folder
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I open "Folder 1" in save dialog 
	When I enter name "NewWorkflow"
	Then save button is "enabled"
	When I cancel
	Then "NewWorkflow" is not visible in "Folder 1"

#coded ui
@Ignore
Scenario: Save with Filter
	Given the Save Dialog is opened
	When I refresh the filter
	Then "Folder 1" is visible in "localhost"

#coded ui
@Ignore
Scenario: Context Menu Folder actions
	Given the Save Dialog is opened
	When I context menu "Create New Folder" "Testing" on "localhost"
	Then "Testing" is visible in "localhost"
	And I open "Testing" in save dialog
	#When I press "CTRL+SHIFT+F" this is will need to happen from the CodedUI
	#Then I add folder "Nested" this is will need to happen from the CodedUI
	#Then "Nested" is visible in "localhost\Testing"	
	#When I open "localhost\Testing\Nested" in save dialog 
	When I context menu "Rename" "localhost\Testing" to "Old Testing!@#"
	Then the item name is "Old Testing"
	And I open "Old Testing" in save dialog
	#When I press "F2" this is will need to happen from the CodedUI
	#Then I type "Very Old Testing" this is will need to happen from the CodedUI
	#Then "Nested" is visible in "localhost\Very Old Testing" this is will need to happen from the CodedUI
	When I context menu "Delete" folder "localhost\Old Testing"
	And I Cancel the delete confirmation
	Then "Old Testing" is visible in "localhost"
	When I context menu "Delete" folder "localhost\Old Testing"
	Then I confirm the deletion
	Then "Old Testing" is not visible in "localhost"


@ignore
#Wolf-981
Scenario: Ensure new folder can be created in root
	Given the Save Dialog is opened
	And the "Localhost" server is visible in save dialog
	When I click "Crtl+Shft+F" 
	Then "New Folder" is visible in "localhost"
	And "New Folder" name is the focus
	When I rename "New Folder" to "TestFolder"
	Then "TestFolder" is visible in "localhost"


Scenario: Saving a Workflow in localhost with an existing name
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And "Hello World" exists
	When I attempt to save a workflow as "Hello World"
	Then an error message appear with the value "An item with name 'Hello World' already exists in this folder."

#coded ui
#wolf-1115
@ignore
Scenario: saving to a disconnected server
	Given I have "Hello World" opened on "Remote Integration Connection
	And I click "Save"
	And the Save Dialog is opened
	And the "Remote Integration Connection" server is visible in save dialog
	When I close the save dialog
	And I "Disconnect" from "Remote Integration Connection"
	And I open the save dialog
	Then a warning message should appear "The workflow is linked to a disconnected server. To save the workflow, select cancel and reconnect the server, or select Ok to discard your changes"

#Wolf-1023
@ignore
Scenario Outline: Context menu in save dialog on server name
	Given the Save Dialog is opened
	And I right-click on '<File>'
	Then the context menu should appear as'<context>'
	Examples: 
	| File                       | context                    |
	| localhost                  | New Folder                 |
	| localhost\Acceptance Tests | New Folder, Rename, Delete |
	| localhost\Hello World      | Rename, Delete             |



#wold-1364
@ignore
Scenario Outline: Create a new folder to save a new Source
	Given I hava a '<Source>' tab opened
	And I "Right-click" on "Localhost"  in the Explorer
	And I click "New Folder"
	Then I Click "Save"
	Then the Save Dialog is opened
	Examples: 
	| Source                   |
	| New Database Source      |
	| New Web Source           |
	| New Remote Server Source |
	| New Plugin Source        |


