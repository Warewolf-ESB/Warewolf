#Feature: Connect Control
#	In order to create a Remote server connction
#	As a Warewolf user
#	I want to be able to create a new Remote server connection in studio
#
#
#Scenario: Creating new server connection from Explorer
#       Given I have Warewolf running
#       And I open the connections in “Explorer”
#       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
#       When I select "New Remote Server" in “Explorer” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected"
#
#Scenario: Creating new server connection from Settings
#       Given I have Warewolf running
#	   When I click on "Manage Settings"
#	   Then "Settings" tab is opened
#       And I open the connections in “Settings”
#       Then "localhost" and "New Remote Server" are shown in “Settings” connections
#       When I select "New Remote Server" in “Explorer” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Settings"
#	   And "Server Permissions" are updated to new server "Servername"
#
#Scenario: Creating new server connection from Sceduler
#       Given I have Warewolf running
#	   When I click on "Scheduler"
#	   Then "Scheduler" tab is opened
#       And I open the connections in “Scheduler”
#       Then "localhost" and "New Remote Server" are shown in “Scheduler” connections
#       When I select "New Remote Server" in “Scheduler” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Scheduler"
#	   And "Scheduler" tasks are updated to new server "Servername"
#
#Scenario: Creating new server connection from Deploy Source server
#       Given I have Warewolf running
#	   When I click on "Deploy"
#	   Then "Deploy" tab is opened
#	   And "Source Server" is connected to "localhost"
#	   And "Destination Server" is connected to "localhost"
#       When I open the connections in “Source Server”
#       Then "localhost" and "New Remote Server" are shown in “Source Server” connections
#       When I select "New Remote Server" in “Source Server” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Source Server"
#	   And "Source Server" is updated to new server "Servername" Resources
#
#Scenario: Creating new server connection from Deploy Destination server
#       Given I have Warewolf running
#	   When I click on "Deply"
#	   Then "Deploy" tab is opened
#	   And "Source Server" is connected to "localhost"
#	   And "Destination Server" is connected to "localhost"
#       When I open the connections in “Destination server”
#       Then "localhost" and "New Remote Server" are shown in “Destination server” connections
#       When I select "New Remote Server" in “Destination server” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Destination server"
#	   And "Destination server" is updated to new server "Servername" Resources
#
#Scenario: Connecting Saved Remote servers from connect control in Explorer
#       Given I have Warewolf running
#	   When I open the connections in “Explorer”
#       Then "All" saved servers are shown in “Explorer” connections
#	   When I select "Savedserver" in "Explorer" connection
#	   Then saved server "Savedserver" is selected and "Connect" is visible
#	   When I click on "Connect" in "Explorer" connection
#	   Then "Savedserver" should be "connected"
#	   And all "Savedserver" resources should be "visible" in "Explorer"
#
#Scenario Outline: Connecting Saved Remote servers from connect control in Deploy tab
#       Given I have Warewolf running
#	   When I click on "Deply"
#	   Then I open the connections in 'Servername'
#       And "All" saved servers are "shown" in 'Servername' connections
#	   When I select "Savedserver" in 'Servername'" connection
#	   Then saved server "Savedserver" is selected and "Connect" is visible
#	   When I click on "Connect" in "Servername" connection
#	   Then "Savedserver" should be "connected"
#	   And all "Savedserver" resources should be "visible" in 'Servername'
#Examples:
#       | No | Servername       |
#       | 1  | Sourceserver     |
#       | 2  | Destinatioserver |
#
#
#Scenario Outline: Connecting Saved Remote servers from connect control in Settings and Scheduler
#       Given I have Warewolf running
#	   When I click on 'Tab'
#	   Then I open the connections in 'Tab'
#       And "All" saved servers are "shown" in 'Tab' connections
#	   When I select "Savedserver" in 'Tab'" connection
#	   Then saved server "Savedserver" is selected and "Connect" is visible
#	   When I click on "Connect" in "Servername" connection
#	   Then "Savedserver" should be "connected"
#	   And 'Tab' should be updated with connected "savedserver"
#Examples:
#       | No | Tab       |
#       | 1  | Settings  |
#       | 2  | Scheduler |
#
#
#Scenario Outline: Editing a Remote servers in settings and scheduler on connect control
#       Given I have Warewolf running
#	   When I click on 'Tab'
#	   Then I open the connections in 'Tab'
#       And "All" saved servers are "shown" in 'Tab' connections
#	   When I select "Savedserver" in 'Tab'" connection
#	   Then saved server "Savedserver" is selected and "Connect" is visible
#	   When I click on "Connect" in "Tab" connection
#	   Then "Savedserver" should be "connected"
#	   And 'Tab' should be updated with connected "savedserver"
#	   When I click on "edit" on 'Tab' connections
#	   Then "Edit" "Savedserver" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “Edit” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “Edit” dialog
#       And Test Connection is "Successful" in “Edit” dialog
#       Then “Save Connection" should be "Enabled" in “Edit” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       And enter Name "Savedserver" in “Save” dialog is "disabled"
#	   And "New Folder" is "disabled" in "Save" dialog
#       And “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then "Edit" server "Servername" should be "connected" in "Explorer"
#	   And "Edit" server "Servername" should be "connected" in 'Tab'
#	   And 'Tab' should be updated with connected "Servername"
#Examples:
#       | No | Tab       |
#       | 1  | Settings  |
#       | 2  | Scheduler |
#      
#       
#Scenario: Renamed saved server sources in explorer should update in Connect control drop down
#      Given I have Warewolf running
#	  And select server source "Source" in "Explorer"
#	  And right click on selected "Source" in "Explorer"
#	  And select "Rename" on right click "options"
#	  And "Rename" the selected source to "newname"
#	  When I open the connections in "Explorer"
#	  Then "All" saved servers are "visible" in "Explorer" connections
#	  And "newname" should be available in "Explorer" connections
#
#
#Scenario: Deleting Server source in explorer should update in connect control
#       Given I have Warewolf running
#       And I open the connections in “Explorer”
#       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
#       When I select "New Remote Server" in “Explorer” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected"
#	   And select server source "Servername" in "Explorer"
#	   When select "Delete" on right click "options"
#	   Then new server "Servername" should be "Disconnected"
#	   And I open the connections in “Explorer”
#	   And "servername" saved servers are "Invisible" in "Explorer" connections
#
#
#Scenario: Removing connected server in explorer should disconnect remote server in connect control
#       Given I have Warewolf running
#       And I open the connections in “Explorer”
#       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
#       When I select "New Remote Server" in “Explorer” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Test Connection" in “New Server” dialog
#       And Test Connection is "Successful" in “New Server” dialog
#       Then “Save Connection" should be "Enabled" in “New Server” dialog
#       When I click "Save Connection"
#       Then "Save" dialog is displayed
#       When I enter Name "Servername" in “Save” dialog
#       Then “Save" should be “Enabled” in “Save” dialog
#       When I click on “Save" in “Save” dialog
#       Then new server "Servername" should be "connected"
#	   And select "New Server" server in "Explorer"
#	   When select "Remove" on right click "options"
#	   Then "New server" Servername should be "Disconnected"
#	   And I open the connections in “Explorer”
#	   And "New server" saved servers are "Diconnected" in "Explorer" connections
#
#Scenario Outline: Disconnecting from Explorer
#       Given I have Warewolf running
#	   And new remote server "Servername" is "connected"
#       And I open the connections in '<Tab>'
#       Then "localhost", "New Remote Server" and "Servername" are shown in “Explorer” connections
#       When I select "Servername" in “Explorer” connections
#       Then "Conncect control" is "Servername" 
#	   And Connect control "Disconnect" option is "Enabled" in '<Tab>'
#	   When I click on "Disconnect" on "Connect control"
#	   Then "Conncect control" is "Servername" with "Connect" option
#	   And all "Servernamer" resources should be "Invisible" in "Explorer"
#	   And all "Servername" permission "Blank" in "settings"
#	   And all "Servername" Schedules "Blank" in "scheduler"
#Examples: 
#       | No | Tab       |
#       | 1  | Settings  |
#       | 2  | Explorer  |
#       | 3  | Scheduler |
#
#
#
#Scenario: Clicking on cancel on New Server creation dialog from Explorer
#       Given I have Warewolf running
#       And I open the connections in “Explorer”
#       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
#       When I select "New Remote Server" in “Explorer” connections
#       Then "New Server" dialog is displayed
#       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Cancel" in “New Server” dialog
#       Then "Conncect control" is "Localhost" 
#	   And Connect control "Disconnect" option is "Disabled" in "Explorer"
#
#Scenario Outline: Clicking on cancel on New Server creation dialog from settings and Scheduler
#       Given I have Warewolf running
#       And click on '<Tab> tab
#	   And I open the connections in '<Tab>'
#       When I select "New Remote Server" in '<Tab>' connections
#       Then "New Server" dialog is displayed
#       And I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Cancel" in “New Server” dialog
#       Then "Conncect control" is "Localhost" in '<Tab>'
#	   And Connect control "Disconnect" option is "Disabled" in '<Tab>'
#Examples: 
#      | No | Tab       |
#      | 1  | Settings  |
#      | 2  | Scheduler |
#
#Scenario Outline: Clicking on cancel on New Server creation dialog from Deploy
#       Given I have Warewolf running
#       And click on "Deploy" tab
#	   And I open the connections in '<Serverside>'
#       When I select "New Remote Server" in '<Serverside>' connections
#       Then "New Server" dialog is displayed
#       And I enter Address "RSAKLFREMOTE" in “New Server” dialog
#       And I select Authentication Type "Windows" in “New Server” dialog
#       When I click "Cancel" in “New Server” dialog
#       Then "Conncect control" is "Localhost" in '<Serverside>'
#	   And Connect control "Disconnect" option is "Disabled" in '<Serverside>'
#Examples: 
#      | No | Serverside        |
#      | 1  | Source Server     |
#      | 2  | Destintion Server |
#
#
#
