Feature: Connect Control
	In order to create a Remote server connection
	As a Warewolf user
	I want to be able to create a new Remote server connection in studio

#
#Scenario: Creating new server connection from Explorer          
#	   Given I am connected to the server "localhost"
#	   And "Edit" is "Disabled" in "Explorer" connect control	 
#	   Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |       
#	   When I select "New Remote Server..." from the connections list in the "explorer"       
#	   Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog       
#       And I select authentication type "Windows"       
#       When I click Test Connection           
#       When I click Save Connection       
#	   When I save the connection as "Servername"       
#       When I click Save
#       Then new server "Servername" should be "connected" in "Explorer"
##
#Scenario: Creating new server connection as Auth Type as User from Explorer      
#	   Given I am connected to the server "localhost"
#	   And "Edit" is "Disabled" in "Explorer" connect control
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "explorer"       
#       Then I enter the address "http://SANDBOX-1:3142/dsf" in the connections dialog    
#       And I select authentication type "User" 	   
#	   And Enter username as "IntegrationTester" as password as "I73573r0"
#       When I click Test Connection         
#       When I click Save Connection       
#       When I save the connection as "Servername"       
#       When I click Save
#       Then new server "Servername" should be "connected" in "Explorer"
##
#Scenario: Creating new server connection from Explorer 1
#       Given I am connected to the server "localhost"
#	   And "Edit" is "Disabled" in "Explorer" connect control
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "explorer"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Public" 
#       When I click Test Connection       
#       When I click Save Connection       
#       When I save the connection as "Servername"       
#       When I click Save
#       Then new server "Servername" should be "connected" in "Explorer"
##
#Scenario: Creating new server connection from Settings       
#	   Given I click on "Manage Settings"
#	   Then "Settings" tab is opened	        
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "settings"       
#	   Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows" 
#       When I click Test Connection            
#       When I click Save Connection       
#       When I save the connection as "Servername"       
#       When I click Save
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Settings Security"	   
##
#Scenario: Creating new server connection from Scheduler       
#	   Given I click on "Scheduler"
#	   Then "Scheduler" tab is opened       
#       Then the following "scheduler" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "scheduler"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Test Connection         
#       When I click Save Connection       
#       When I save the connection as "Servername"       
#       When I click Save
#       Then new server "Servername" should be "connected" in "Explorer"
#	   And new server "Servername" should be "connected" in "Scheduler"	   
##
#Scenario: Creating new server connection from Deploy Source server       
#	   Given I click on "Deploy"
#	   Then "Deploy" tab is opened
#	   And "Source Server" is connected to "localhost"
#	   And "Edit" is "Disabled" in "Source Server" connect control
#	   And "Disconnect" is "Disabled" in "Source Server" connect control
#	   And "Destination Server" is connected to "localhost"	   
#	   And "Edit" is "Disabled" in "Destination Server" connect control
#	   And "Disconnect" is "Disabled" in "Destination Server" connect control       
#       Then the following "source" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "source"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Test Connection       
#       When I click Save Connection       
#       When I save the connection as "ServernameS"       
#       When I click Save
#       Then new server "ServernameS" should be "connected" in "Explorer"
#	   And new server "ServernameS" should be "connected" in "Deploy Source Server"	   
##
#Scenario: Creating new server connection from Deploy Destination server       
#	   Given I click on "Deploy"
#	   Then "Deploy" tab is opened
#	   And "Source Server" is connected to "localhost"
#	   And "Edit" is "Disabled" in "Source Server" connect control
#	   And "Disconnect" is "Disabled" in "Source Server" connect control
#	   And "Destination Server" is connected to "localhost"
#	   And "Edit" is "Disabled" in "Destination Server" connect control
#	   And "Disconnect" is "Disabled" in "Destination Server" connect control       
#       Then the following "destination" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |
#       When I select "New Remote Server..." from the connections list in the "destination"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#	   When I click Test Connection         
#       When I click Save Connection       
#	   When I save the connection as "ServernameD"       
#       When I click Save
#       Then new server "ServernameD" should be "connected" in "Explorer"
#	   And new server "ServernameD" should be "connected" in "Deploy Destination Server"
#	   #And "Deploy Destination Server" is updated to new server "ServernameD" Resources
##
#Scenario: Connecting Saved Remote servers from connect control in Explorer       	          
#	   Given I select "ServernameD" from the connections list in the "explorer"
#	   Then saved server "ServernameD" is selected in "explorer" connect control 
#	   And "Edit" is "Enabled" in "Explorer" connect control
#	   And "Connect" is "Enabled" in "Explorer" connect control	   
#	   When I click on "Connect" in "Explorer" conncet control	   
#	   Then all "ServernameD" resources should be "visible" in "Explorer"
#
#Scenario Outline: Connecting Saved Remote servers from connect control in Deploy tab       
#	   Given I click on "Deploy"
#	   Then "Deploy" tab is opened
#       And "All" saved servers are "shown" in '<Servername>' connections	   
#	   When I select "Savedserver" from the connections list in the '<Servername>'
#	   And "Edit" is "Enabled" in '<Servername>' connect control	  
#	   And "Edit" is "Enabled" in "Explorer" connect control
#	   And 'Connect' is 'Enabled' in '<Servername>' connect control
#	   When I click on 'Connect' in '<Servername>' connect control	   
#	   And all "Savedserver" resources should be "visible" in '<Servername>'
#	   Then '<Servername1>' is connected to "localhost"
#Examples:
#       | No | Servername                | Servername1                 |
#       | 1  | Deploy Source Server      | Deploy Destination Serverer |
#       | 2  | Deploy Destination Server | Deploy Source Serve         |
##
#Scenario Outline: Connecting Saved Remote servers from connect control in Settings and Scheduler       
#	   Given I click on '<Tab>'	   
#       And "All" saved servers are "shown" in '<Tab>' connections	   
#	   When I select "Savedserver" from the connections list in the '<Tab>'
#	   Then saved server "Savedserver" is selected in '<Tab>' connect control 
#	   Then "Edit" is "Enabled" in '<Tab>' connect control
#	   And "Connect" is "Enabled" in '<Tab>' connect control
#	   When I click on "Connect" in "Servername" connect control	   
#	   Then new server "savedserver" should be "connected" in '<Tab>'
#Examples:
#       | No | Tab       |
#       | 1  | Settings  |
#       | 2  | Scheduler |
##
##
#Scenario Outline: Editing a Remote servers in settings and scheduler on connect control       
#	   Given I click on '<Tab>'	   
#       Then "All" saved servers are "shown" in '<Tab>' connections	   
#	   When I select "Savedserver" from the connections list in the '<Tab>'
#	   Then saved server "Savedserver" is selected in '<Tab>' connect control 	   
#	   And "Edit" is "Enabled" in '<Tab>' connect control
#	   And "Connect" is "Enabled" in '<Tab>' connect control	   
#	   When I click on "Connect" in '<Tab>' conncet control
#	   Then new server "Servername" should be "connected" in '<Tab>'
#	   And new server "savedserver" should be "connected" in '<Tab>'
#	   When I click on "Edit" in '<Tab>' connect control	   
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog       
#	   And I select authentication type "Windows"       
#	   When I click Save Connection
#       When I click Save
#	   And "Edit" server "Servername" should be "connected" in '<Tab>'
#	   Then new server "Servername" should be "connected" in '<Tab>'
#Examples:
#       | No | Tab             |
#       | 1  | Manage Settings |
#       | 2  | Scheduler       |
#      
##       
#Scenario: Renamed saved server sources in explorer should update in Connect control drop down      
#	  Given I select "Savedserver" from the connections list in the "Explorer"	
#	  And right click rename "Savedserver" to "newname"	 	  
#	  Then the following "explorer" connections are shown
#	   | Server  |
#	   | newname |
##
#Scenario: Deleting Server source in explorer should update in connect control       
#       Given I am connected to the server "localhost"
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |   
#       When I select "New Remote Server..." from the connections list in the "explorer"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Test Connection          
#       When I click Save Connection       
#       When I save the connection as "Servername"       
#       When I click Save       
#	   #And select server source "Servername" in "Explorer"
#	   #When select "Delete" on right click "options"
#	   When I right click delete the server "Servername" from the Explorer	   
#	   #And "servername" saved servers are "Invisible" in "Explorer" connections
#	   Then the server "Servername" will not be in the explorer connections
##
##
#Scenario: Removing connected server in explorer should disconnect remote server in connect control       
#       Given I am connected to the server "localhost"
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            | 
#       When I select "New Remote Server..." from the connections list in the "explorer"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Test Connection       
#       When I click Save Connection       
#       When I save the connection as "FireServer"       
#       When I click Save
#       Then new server "FireServer" should be "connected" in "Explorer"	   
#	   #And select server source "New Server" in "Explorer"
#	   #When select "Remove" on right click "options"
#	   When I right click and remove the server "FireServer" from the Explorer	   
#	   Then new server "FireServer" should be "Disconnected" in "Explorer"	   
##
#Scenario Outline: Disconnecting from Explorer       
#	   Given I am connected to the server "Servername"	
#	   When I click on '<Tab>'
#	   Then the following "CurrentTab" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            |        
#	   Then saved server "ServernameD" is selected in '<Tab>' connect control
#	   And Connect control "Disconnect" option is "Enabled" in '<Tab>'
#	   When I click on "Disconnect" on connect control in '<Tab>' 
#	   And "Connect" is "Enabled" in '<Tab>' connect control	     	   
#Examples: 
#       | No | Tab       |
#       | 1  | Settings  |
#       | 2  | Explorer  |
#       | 3  | Scheduler |
##
##
#Scenario: Clicking on cancel on New Server creation dialog from Explorer       
#       Given I am connected to the server "localhost"
#       Then the following "explorer" connections are shown
#	   | Server               |
#	   | New Remote Server... |
#	   | localhost            | 
#       When I select "New Remote Server..." from the connections list in the "explorer"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Cancel
#	   Then saved server "localhost" is selected in "explorer" connect control   	   
##
#Scenario Outline: Clicking on cancel on New Server creation dialog from settings and Scheduler            
#	   Given I click on '<Tab>'	   
#       When I select "New Remote Server..." from the connections list in the "scheduler"
#              Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Cancel
#	   Then saved server "localhost" is selected in '<Tab>' connect control   	   	   
#Examples: 
#      | No | Tab       |
#      | 1  | Settings  |
#      | 2  | Scheduler |
##
#Scenario Outline: Clicking on cancel on New Server creation dialog from Deploy       
#       Given I click on "Deploy"	   
#       When I select "New Remote Server..." from the connections list in the "source"       
#       Then I enter the address "http://TST-CI-REMOTE:3142/dsf" in the connections dialog
#       And I select authentication type "Windows"
#       When I click Cancel
#       Then saved server "localhost" is selected in '<Serverside>' connect control   		   
#	   And "Connect" is "Enabled" in '<Serverside>' connect control
#Examples: 
#      | No | Serverside  |
#      | 1  | source      |
#      | 2  | destination |