@SaveDialog
Feature: SaveDialog
	In order to save resources
	As a Warewolf user
	I want a save dialog

Scenario: Creating Folder from Save Dialog under localhost
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders
	When I create "New Folder" in "localhost"
	And I should see "6" folders

Scenario: Creating Folder from Save Dialog under folder
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I create "New Folder" in "Folder 1"
	Then I should see "9" children for "Folder 1"

#CODED UI
#Scenario: Right click Items on folder
#    Given the Save Dialog is opened 
#	And the "localhost" server is visible in save dialog
#	And I should see "5" folders in "localhost" save dialog
#	When I right click on "folder 1"
#	Then I should see "Rename"
#	And I should see "Delete"
#	And I should see "New Folder"
#	And I shouldn't see "New workflow service"
#	And I shouldn't see "New Plugin service"

Scenario: Saving a Workflow in localhost
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I save "localhost/Newworkflow"
	Then "NewWorkflow" is visible in "localhost"
	
Scenario: Saving a Workflow in localhost folder
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I save "Folder 1/Newworkflow"
	Then "NewWorkflow" is visible in "Folder 1"	
	
Scenario: Save button is Enabled when I enter new name for resource
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I enter name "Savewf"
	Then save button is "Enabled"
	And validation message is ""
	Then I should see "9" children for "Folder 1"

Scenario: Save button is disabled when name is empty
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I enter name ""
	Then save button is "Disabled"
	And validation message is "'Name' cannot be empty"

Scenario: Save with duplicate name and expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I save "Folder 1/Savewf"
	And validation message is ""
	Then I should see "9" children for "Folder 1"
	When I enter name "Savewf"
	Then save button is "Disabled"
	And validation message is "Name already exists"

Scenario: Save resource names with special character expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I enter name "Save@#$"
	Then save button is "Disabled"
	And validation message is "'Name' contains invalid characters."

Scenario: Renaming Folders in Save Dialog
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I rename "Folder 1" to "renamed" in "localhost" save dialog
	Then I should see "8" children for "renamed"
	And I should not see "Folder 1" in "localhost"

Scenario: Renaming Folders with duplicate names
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I rename "Folder 1" to "renamed" in "localhost" save dialog
	Then I should see "8" children for "renamed"
	And I should not see "Folder 1" in "localhost"
	When I rename "Folder 2" to "renamed" in "localhost" save dialog
	Then validation message is thrown "True"

Scenario: Filtering Resources in save dialog
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I search for "deleteresouce" in save dialog
    Then I should see "deleteresouce" in "Follder 1"

Scenario: Refresh resources in save dialog
    Given the Save Dialog is opened
    And the "localhost" server is visible in save dialog
    When I refresh resources in savedialog
	Then save dilog localhost is refreshed "True"

Scenario: Opening saved workflow and saving
    Given I have an "Saved" workflow open
	When I press "Ctrl+s"
	Then the "Saved" workflow is saved "True"

Scenario: Opening New workflow and saving
    Given I have an New workflow "Unsaved1" is open
	When I press "Ctrl+s"
	Given the Save Dialog is opened
	Then save button is "Disabled"
	And cancel button is "Enabled"
	When I enter name "New"
	Then save button is "Enabled"
	When I save "localhost/New"
	Then the "New" workflow is saved "True"

Scenario: Opening Save dialog and canceling
    Given I have an New workflow "Unsaved1" is open
	When I press "Ctrl+s"
	Given the Save Dialog is opened
	Then save button is "Disabled"
	And cancel button is "Enabled"
	When I enter name "New"
	Then save button is "Enabled"
	When I cancel the save dialog
	Then the save dialog is closed
	Then the "New" workflow is saved "False"

Scenario: Saving multiple workflows by using shortcutkey
    Given I have an New workflow "Unsaved1" is open
	And I have an New workflow "Unsaved2" is open
	And I have an "Saved1" workflow open
	And I have an "Saved2" workflow open
	And I have an "Saved3" workflow open
	When I press "Ctrl+Shift+S"
	Then the "Saved1" workflow is saved "True" 
	Then the "Saved2" workflow is saved "True" 
	Then the "Saved3" workflow is saved "True" 
    And the Save Dialog is opened 
	When I cancel the save dialog
	And the Save Dialog is opened 

Scenario: Savve textbox is updating names when selecting resource names
    Given I have an New workflow "Unsaved1" is open
	When I press "Ctrl+s"
	Given the Save Dialog is opened
	Then save button is "Disabled"
	And cancel button is "Enabled"
    And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I select "localhost/Folder 1"
	When I open "Folder 1" in "localhost" save dialog 
	Then I should see "8" children for "Folder 1"
	When I select "localhost/Folder 1/children1"
	Then save textbox  name is "children1"
	When I save "localhost/children1"
	Then validation message is thrown "True"
	Then validation message is "Name already exists"
	Then the "children1" workflow is saved "False"

Scenario: Path is updating on save dialog when selcting folders
    Given I have an New workflow "Unsaved1" is open
	When click "Save"
	Then the Save Dialog is opened
	And the path in the title as "localhost\"
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I select "localhost/Folder 1"
	Then the path in the title as "localhost\Folder 1\"

Scenario: Star is representing the workflow is unsaved
   Given I have an New workflow "Unsaved1" is open 
   When I edit "Unsaved1"
   Then the New workflow "Unsaved1" is open with Star notation
   When I save "Unsaved1"
   Then the New workflow "Unsaved1" is open without Star notation
   When I edit "Unsaved1"
   Then I have an "Unsaved1" workflow open with Star notation
   When I save "Unsaved1"
   Then I have an "Unsaved1" workflow without Star notation