Feature: Connect Control
	In order to create a Remote server connection
	As a Warewolf user
	I want to be able to create a new Remote server connection in studio


Scenario: Creating new server connection from Explorer
       Given I have Warewolf running
	   Then "localhost" is "connected" in "Explorer" connect control
	   And "Edit" is "Disabled" in "Explorer" connect control
	   And "Disconnect" is "Disabled" in "Explorer" conncet control
       And I open the connections in "Explorer" connect control
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"

Scenario: Creating new server connection as Auth Type as User from Explorer
       Given I have Warewolf running
	   Then "localhost" is "connected" in "Explorer" connect control
	   And "Edit" is "Disabled" in "Explorer" connect control
	   And "Disconnect" is "Disabled" in "Explorer" conncet control
       And I open the connections in "Explorer" connect control
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://SANDBOX-1:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Uder" in “New Server” dialog
	   Then "Username" and "Password" text box "Opened"
	   And Enter "username" as "dev2/IntegrationTester"
	   And Password "Password" as "I73573r0"
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"

Scenario: Creating new server connection from Explorer 1
       Given I have Warewolf running
	   Then "localhost" is "connected" in "Explorer" connect control
	   And "Edit" is "Disabled" in "Explorer" connect control
	   And "Disconnect" is "Disabled" in "Explorer" conncet control
       And I open the connections in "Explorer" connect control
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Public" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"

Scenario: Creating new server connection from Settings
       Given I have Warewolf running
	   When I click on "Manage Settings"
	   Then "Settings" tab is opened
	   And "Securitytab" with "Server Permission" is "visible"
       And I open the connections in “Settings” connect control
       Then "localhost" and "New Remote Server" are shown in “Settings” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And new server "Servername" should be "connected" in "Settings Security"
	   And "Settings Security" are updated to new server "Servername"

Scenario: Creating new server connection from Sceduler
       Given I have Warewolf running
	   When I click on "Scheduler"
	   Then "Scheduler" tab is opened
       And I open the connections in “Scheduler” connect control
       Then "localhost" and "New Remote Server" are shown in “Scheduler” connections
       When I select "New Remote Server" in “Scheduler” connections
       Then "New Server" dialog is displayed
       Then I enter Address "RSAKLFREMOTE" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And new server "Servername" should be "connected" in "Scheduler"
	   And "Scheduler" tasks are updated to new server "Servername"

Scenario: Creating new server connection from Deploy Source server
       Given I have Warewolf running
	   When I click on "Deploy"
	   Then "Deploy" tab is opened
	   And "Source Server" is connected to "localhost" in connect control
	   And "Edit" is "Disabled" in "Source Server" connect control
	   And "Disconnect" is "Disabled" in "Source Server" conncet control
	   And "Destination Server" is connected to "localhost"
	   And "Edit" is "Disabled" in "Destination Server" connect control
	   And "Disconnect" is "Disabled" in "Destination Server" conncet control
       When I open the connections in “Source Server”
       Then "localhost" and "New Remote Server" are shown in “Source Server” connections
       When I select "New Remote Server" in “Source Server” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And new server "Servername" should be "connected" in "Deploy Source Server"
	   And "Deploy Source Server" is updated to new server "Servername" Resources

Scenario: Creating new server connection from Deploy Destination server
       Given I have Warewolf running
	   When I click on "Deply"
	   Then "Deploy" tab is opened
	   And "Source Server" is connected to "localhost" in connect control
	   And "Edit" is "Disabled" in "Source Server" connect control
	   And "Disconnect" is "Disabled" in "Source Server" conncet control
	   And "Destination Server" is connected to "localhost"
	   And "Edit" is "Disabled" in "Destination Server" connect control
	   And "Disconnect" is "Disabled" in "Destination Server" conncet control
       When I open the connections in “Destination server”
       Then "localhost" and "New Remote Server" are shown in “Destination server” connections
       When I select "New Remote Server" in “Destination server” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And new server "Servername" should be "connected" in "Deploy Destination Server"
	   And "Deploy Destination Server" is updated to new server "Servername" Resources

Scenario: Connecting Saved Remote servers from connect control in Explorer
       Given I have Warewolf running
	   When I open the connections in “Explorer” connect control
       Then "All" saved servers are shown in “Explorer” connections
	   When I select "Savedserver" in "Explorer" connections
	   Then saved server "Savedserver" is selected in "Servername" connect control 
	   And "Edit" is "Enabled" in "Servername" connect control
	   And "Connect" is "Enaabled" in "Servername" conncet control
	   When I click on "Connect" in "Servername" connect control
	   When I click on "Connect" in "Explorer" conncet control
	   Then "Savedserver" should be "connected"
	   And all "Savedserver" resources should be "visible" in "Explorer"

Scenario Outline: Connecting Saved Remote servers from connect control in Deploy tab
       Given I have Warewolf running
	   When I click on "Deploy"
	   Then "Deploy" tab is opened
	   And I open the connections in '<Servername>' connect control
       And "All" saved servers are "shown" in '<Servername>' connections
	   When I select "Savedserver" in '<Servername>' connections
	   Then saved server "Savedserver" is selected in '<Servername>' connect control 
	   And "Edit" is "Enabled" in '<Servername>' connect control
	   And "Connect" is "Enaabled" in '<Servername>' conncet control
	   When I click on "Connect" in '<Servername>' connect control
	   Then "Savedserver" should be "connected"
	   And all "Savedserver" resources should be "visible" in '<Servername>'
	   And '<Servername1>' is connected to "localhost"
Examples:
       | No | Servername                | Servername1                  |
       | 1  | Deploy Source Server      | Deploy Destination Serverer |
       | 2  | Deploy Destination Server | Deploy Source Serve         |


Scenario Outline: Connecting Saved Remote servers from connect control in Settings and Scheduler
       Given I have Warewolf running
	   When I click on '<Tab>'
	   Then I open the connections in '<Tab>' connect control
       And "All" saved servers are "shown" in '<Tab>' connections
	   When I select "Savedserver" in '<Tab>' connections
	   Then saved server "Savedserver" is selected in '<Tab>' connect control 
	   Then "Edit" is "Enabled" in '<Tab> connect control
	   And "Connect" is "Enabled" in '<Tab> conncet control
	   When I click on "Connect" in "Servername" connect control
	   Then "Savedserver" should be "connected"
	   And '<Tab>' should be updated with connected "savedserver"
Examples:
       | No | Tab       |
       | 1  | Settings  |
       | 2  | Scheduler |


Scenario Outline: Editing a Remote servers in settings and scheduler on connect control
       Given I have Warewolf running
	   When I click on '<Tab>'
	   Then I open the connections in '<Tab>' connect control
       And "All" saved servers are "shown" in '<Tab>' connections
	   When I select "Savedserver" in '<Tab>' connections
	   Then saved server "Savedserver" is selected in '<Tab>' connect control 
	   And "Edit" is "Enabled" in '<Tab> connect control
	   And "Connect" is "Enabled" in '<Tab> conncet control
	   When I click on "Connect" in '<Tab>' conncet control
	   Then "Savedserver" should be "connected"
	   And '<Tab>' should be updated with connected "savedserver"
	   When I click on "Edit" on '<Tab>' conncet control
	   Then "New Server" dialog is displayed
       And I enter Address "http://RSAKLFREMOTE:3142/dsf" in “Edit--Savedserver” dialog
       And I select "Authentication Type" "Windows" in “New Server” dialog
       When I click "Test" Connection" in “Edit” dialog
       And Test Connection is "Successful" in “Edit” dialog
       Then “Save Connection" should be "Enabled" in “Edit” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       And enter Name "Savedserver" in “Save” dialog is "disabled"
	   And "New Folder" is "disabled" in "Save" dialog
       And “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog      
	   And "Edit" server "Servername" should be "connected" in '<Tab>'
	   And '<Tab>' should be updated with connected "Servername"
Examples:
       | No | Tab                      |
       | 1  | Settings Security Server |
       | 2  | Scheduler Server         |
      
       
Scenario: Renamed saved server sources in explorer should update in Connect control drop down
      Given I have Warewolf running
	  And select server source "Source" in "Explorer"
	  And right click on selected "Source" in "Explorer"
	  And select "Rename" on right click "options"
	  And "Rename" the selected source to "newname"
	  When I open the connections in "Explorer"
	  Then "All" saved servers are "visible" in "Explorer" connections
	  And "newname" should be available in "Explorer" connections


Scenario: Deleting Server source in explorer should update in connect control
       Given I have Warewolf running
       And I open the connections in “Explorer”
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And select server source "Servername" in "Explorer"
	   When select "Delete" on right click "options"
	   Then new server "Servername" should be "Disconnected" in "Explorer"
	   And I open the connections in “Explorer”
	   And "servername" saved servers are "Invisible" in "Explorer" connections


Scenario: Removing connected server in explorer should disconnect remote server in connect control
       Given I have Warewolf running
       And I open the connections in “Explorer” connect control
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Test Connection" in “New Server” dialog
       And Test Connection is "Successful" in “New Server” dialog
       Then “Save Connection" should be "Enabled" in “New Server” dialog
       When I click "Save Connection"
       Then "Save" dialog is displayed
       When I enter Name "Servername" in “Save” dialog
       Then “Save" should be “Enabled” in “Save” dialog
       When I click on “Save" in “Save” dialog
       Then new server "Servername" should be "connected" in "Explorer"
	   And select "New Server" server in "Explorer"
	   When select "Remove" on right click "options"
	   Then "New server" Servername should be "Disconnected"
	   And I open the connections in “Explorer”
	   And "New server" saved servers are "Diconnected" in "Explorer" connections

Scenario Outline: Disconnecting from Explorer
       Given I have Warewolf running
	   And new remote server "Servername" is "connected"
       And I open the connections in '<Tab>' connect control
	   Then following are "Shown" in "Explorer" connections
	   | New Remote Server    |
	   | localhost(Connected) |
	   | Servername           |
       When I select "Servername" in “Explorer” connections
       Then "Conncect control" is "Servername" 
	   And Connect control "Disconnect" option is "Enabled" in '<Tab>'
	   When I click on "Disconnect" on connect control in '<Tab>' 
	   Then "Conncect control" is "Servername" with "Connect" option
	   And all "Servernamer" resources should be "Invisible" in "Explorer"
	   And all "Servername" permission "Blank" in "settings"
	   And all "Servername" Schedules "Blank" in "scheduler"
Examples: 
       | No | Tab       |
       | 1  | Settings  |
       | 2  | Explorer  |
       | 3  | Scheduler |



Scenario: Clicking on cancel on New Server creation dialog from Explorer
       Given I have Warewolf running
       And I open the connections in “Explorer” connect control
       Then "localhost" and "New Remote Server" are shown in “Explorer” connections
       When I select "New Remote Server" in “Explorer” connections
       Then "New Server" dialog is displayed
       Then I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Cancel" in “New Server” dialog
       Then "Conncect control" is "Localhost" 
	   And Connect control "Disconnect" option is "Disabled" in "Explorer"

Scenario Outline: Clicking on cancel on New Server creation dialog from settings and Scheduler
       Given I have Warewolf running
       And click on '<Tab> tab
	   And I open the connections in '<Tab>' connect control
       When I select "New Remote Server" in '<Tab>' connections
       Then "New Server" dialog is displayed
       And I enter Address "http://RSAKLFREMOTE:3142/dsf" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Cancel" in “New Server” dialog
       Then "Conncect control" is "Localhost" in '<Tab>'
	   And Connect control "Disconnect" option is "Disabled" in '<Tab>'
Examples: 
      | No | Tab       |
      | 1  | Settings  |
      | 2  | Scheduler |

Scenario Outline: Clicking on cancel on New Server creation dialog from Deploy
       Given I have Warewolf running
       And click on "Deploy" tab 
	   And I open the connections in '<Serverside>' connect control
       When I select "New Remote Server" in '<Serverside>' connections
       Then "New Server" dialog is displayed
       And I enter Address "RSAKLFREMOTE" in “New Server” dialog
       And I select Authentication Type "Windows" in “New Server” dialog
       When I click "Cancel" in “New Server” dialog
       Then "Conncect control" is "Localhost" in '<Serverside>'
	   And Connect control "Disconnect" option is "Disabled" in '<Serverside>'
Examples: 
      | No | Serverside        |
      | 1  | Source Server     |
      | 2  | Destintion Server |



