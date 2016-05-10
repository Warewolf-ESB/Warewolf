Feature: TabControl
	In order to have a familiar working environment
	As a Warewolf User
	I want Tabs that are intuative

#1 Closing new unchanged workflow tab
#1 Closing new changed tab and Canceling close dialog
#1 Closing new changed tab and No on close dialog
#1 Closing new changed tab and Yes on close dialog
#2 Making changes on tabs, makes * and saving removes *
#3 Make many changes and bulk close
#4 Make many changes and bulk save with saved and unsaved and new and edited
#5 Persist open tabs after shut down and restart
#6 Short cut Keys

@TabControl
Scenario: 1 Change Workflow And Close dialog Options
	Given I have a New Workflow open
	Then "New Workflow 1" tab is opened
	When "New Workflow 1" tab is closed
	Then do you want to close dialog is not opened
	Then save dialog is not opened
	Then I have a New Workflow open
	And tab is "New Workflow 1"
	And I make changes to "New Workflow 1"
	Then tab is "New Workflow 1 *"
	When "New Workflow 1 *" tab is closed
	Then do you want to close "New Workflow 1" dialog is open
	When I say "Cancel" on close dialog
	Then do you want to close dialog is closed
	And tab is "New Workflow 1 *"
	When "New Workflow 1" tab is closed
	Then do you want to close dialog is open
	When I say "No" on close dialog
	Then do you want to close dialog is closed
	And "New Workflow 1" tab is closed
	Then do you want to close dialog is not opened
	Then I have a New Workflow open
	And tab is "New Workflow 1"
	And I make changes to "New Workflow 1"
	When "New Workflow 1 *" tab is closed
	Then do you want to close "New Workflow 1" dialog is open
	When I say "Yes" on close dialog
	Then the save dialog is opened
	
Scenario Outline: Change Tab And Save
	Given I have a <item> open
	Then "<item>" tab is opened
	When I make changes to "<item>"
	Then "<item> *" tab is opened
	When I save "<item> *" tab as "SavedItem"
	Then "SavedItem" tab is open
	And "<item> *" tab is closed
	Examples: 
	| item                        |
	| New Plugin Connector 1      |
	| New Plugin Source 1         |
	| New Database Connector 1    |
	| New Database Source 1       |
	| New Web Service Connector 1 |
	| New Web Service Source 1    |
	| New Email Source 1          |
	| New Server Source 1         |
	| Settings                    |
	| Scheduler                   |

Scenario: 3 Change Multiple And Bulk Close
	Given I have a "New Plugin Connector" open
	When I make changes to "New Plugin Connector"
	And I have a "New Database Connector" open
	When I make changes to "New Database Connector"
	And I have a "New Web Service Connector" open
	When I make changes to "New Web Service Connector"
	And I have a "New Plugin Source" open
	When I make changes to "New Plugin Source"
	And I close all but this "New Plugin Source *"
	Then do you want to close "New Database Connector" dialog is open
	When I say "Yes" on close dialog
	And I save "New Database Connector" as "SavedItem"
	Then do you want to close "New Web Service Connector" dialog is open
	When I say "No" on close dialog
	And "New Web Service Connector *" tab is closed
	Then do you want to close "New Plugin Source" dialog is open
	When I say "Cancel" on close dialog
	Then "New Plugin Connector *" tab is "Visible"
	Then "New Plugin Source * *" tab is "Visible"
	
Scenario: 4 Change Multiple And Bulk Save
	Given I have a "New Plugin Connector" open
	When I make changes to "New Plugin Connector"
	And I have a "Settings" open
	When I make changes to "Settings"
	And I have a "SavedWebService" open
	When I make changes to "SavedWebService"
	And I have a "New Plugin Source" open
	When I make changes to "New Plugin Source"
	And I have "SavedServerSource" open
	And I have "SavedWorkflow" open
	Then "Settings *" tab is "Visible"
	Then "SavedWebService *" tab is "Visible"
	And I press "CTRL+SHIFT+S"
	Then the save dialog is opened
	And I save "New Plugin Connector" as "SavedPluginConnector"
	Then the save dialog is opened
	And I say "Cancel" on save dialog
	Then "SavedPluginConnector" tab is "Visible"
	Then "Settings" tab is "Visible"
	Then "SavedWebService" tab is "Visible"
	Then "New Plugin Source " tab is "Visible"
	Then "SavedServerSource" tab is "Visible"
	Then "SavedWorkflow" tab is "Visible"

Scenario: 5 Persiste Menu state and tabs through shutdown
	Given I "start" the studio
	And "New Workflow 1" tab is opened
	And "New Workflow 2" tab is opened
	When I make changes to "New Workflow 2"
	And "New Workflow 3" tab is opened
	And "SavedWorkflow" tab is opened
	When I make changes to "New Workflow 3"
	Then I "Lock" the Main Menu "Open"
	And I "shut" down the studio
	And I "start" the studio
	Then "New Workflow 1" tab is "Visible"
	Then "New Workflow 2 *" tab is "Visible"
	Then "New Workflow 3 *" tab is "Visible"
	Then "SavedWorkflow" tab is "Visible"
	And the main menu is "Locked Open"
	And I "Unlock" the main menu
	And I "shut down" the studio
	And I "start" the studio
	Then the main menu is "Unlocked"

Scenario: 6 Short Cut Keys Are Working
	Given I "start" the studio
	And "SavedWorkflow" tab is opened
	When I make changes to "SavedWorkflow"
	Then "SavedWorkflow *" tab is "Visible"
	And I press "CTRL+S"
	Then "SavedWorkflow" tab is "Visible"
	And I press "CTRL+R"
	Then the debug input window is "Visible"
	And I press "ESC"
	Then the debug input window is "Invisible"
	And I press "CTRL+T"
	Then "SavedWorkflow" executes in "Debug" mode
	And I press "CTRL+Y"
	Then "SavedWorkflow" executes in "Browser" mode	
	And I press "CTRL+W"
	Then "New Workflow 1" tab is "Visible"
	And I press "CTRL+SHIFT+W"
	Then "New Web Service Connector 1" tab is "Visible"
	And I press "CTRL+SHIFT+D"
	Then "New Database Connector 1" tab is "Visible"
	And I press "CTRL+SHIFT+P"
	Then "New Plugin Connector 1" tab is "Visible"
	And I press "CTRL+W"
	Then "New Workflow 2" tab is "Visible"
	And I press "CTRL+SHIFT+W"
	Then "New Web Service Connector 2" tab is "Visible"
	And I press "CTRL+SHIFT+D"
	Then "New Database Connector 2" tab is "Visible"
	And I press "CTRL+SHIFT+P"
	Then "New Plugin Connector 2" tab is "Visible"
	And I press "CTRL+SHIFT+N"
	Then "New .Net Connector" tab is "Visible"
	And I press "CTRL+D"
	Then "Deploy" tab is "Visible"

Scenario: Closing unsaved Workflows
	Given "Hello World" tab is opened
	And I add variable "[[Surname]]" equals "tubman"
	Then "Hello World *" tab is "Visible"
	And "Hello World *" tab is closed
	Then a warning message appears prompting to "Save"
	When "Save" is clicked
	Then the save dialog is opened


#Wolf-1115
Scenario: Closing unsaved tab on remote server
	Given I "Start" the studio
	And selected Source Server is "Remote Intergration Connection"
	And "New" is clicked
	And "New Workflow - Remote Intergration Connection" tab is opened
	And I drag an "Assign" tool into the workflow
	And "New Workflow - Remote Intergration Connection *" tab is visible
	And I disconnect from Source Server is "Remote Intergration Connection"
	And I close "New Workflow - Remote Intergration Connection *" tab
	Then a warning message appears prompting to "Save"
	When "Save" is clicked
	And the Save Dialog is opened
	Then "New Workflow - Remote Intergration Connection *" Server is changed to "Local"
	And "New Workflow - Remote Intergration Connection *" is saved as "Test -Local"
	

Scenario: Opening the same workflow multiple times
	Given I open "Dev2GetCountriesWebService" 
	Then "Dev2GetCountriesWebService" tab is opened	
	And I open "Dev2GetCountriesWebService" 
	Then focus is put on "Dev2GetCountriesWebService"
	And No new tabs are opened