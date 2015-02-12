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

@ignore
Scenario: Saving a Workflow in localhost
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I should see "5" folders
	When I save "localhost/Newworkflow"
	Then "NewWorkflow" is visible in "localhost"

@ignore	
Scenario: Saving a Workflow in localhost folder
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I open "Folder 1" in save dialog 
	When I save "Folder 1/Newworkflow"
	Then "NewWorkflow" is visible in "Folder 1"	

	
Scenario: Save button is Enabled when I enter new name for resource
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	And I open "Folder 1" in save dialog 
	When I enter name "Savewf"
	Then save button is "Enabled"
	And validation message is ""

Scenario: Save button is disabled when name is empty
	Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 1" in save dialog 
	When I enter name ""
	Then save button is "Disabled"
	And validation message is "'Name' cannot be empty."

Scenario: Save with duplicate name and expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 2" in save dialog 
	When I enter name "Folder 2 Child 1"
	Then save button is "Disabled"
	And validation message is "Service with name 'Folder 2 Child 1' already exists."

Scenario: Save resource names with special character expect validation
    Given the Save Dialog is opened
	And the "localhost" server is visible in save dialog
	When I open "Folder 1" in save dialog 
	When I enter name "Save@#$"
	Then save button is "Disabled"
	And validation message is "'Name' contains invalid characters."

#FEATURES TO BE IMPLEMENTED
@ignore
Scenario: Opening saved workflow and saving
    Given I have an "Saved" workflow open
	When I press "Ctrl+s"
	Then the "Saved" workflow is saved "True"

@ignore
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

@ignore
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

@ignore
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

@ignore
Scenario: Save textbox is updating names when selecting resource names
    Given I have an New workflow "Unsaved1" is open
	When I press "Ctrl+s"
	Given the Save Dialog is opened
	Then save button is "Disabled"
	And cancel button is "Enabled"
    And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I select "localhost/Folder 1"
	When I open "Folder 1" in save dialog 
	Then I should see "8" children for "Folder 1"
	When I select "localhost/Folder 1/children1"
	Then save textbox  name is "children1"
	When I save "localhost/children1"
	Then validation message is thrown "True"
	Then validation message is "Name already exists"
	Then the "children1" workflow is saved "False"

@ignore
Scenario: Path is updating on save dialog when selcting folders
    Given I have an New workflow "Unsaved1" is open
	When click "Save"
	Then the Save Dialog is opened
	And the path in the title as "localhost\"
	And the "localhost" server is visible in save dialog
	And I should see "5" folders in "localhost" save dialog
	When I select "localhost/Folder 1"
	Then the path in the title as "localhost\Folder 1\"

@ignore
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