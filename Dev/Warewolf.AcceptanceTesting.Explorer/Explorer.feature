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
	Then I should see "18" children for "Folder 2"

Scenario: Rename folder
	Given the explorer is visible
	And I open "localhost" server
	When I rename "Folder 2" to "Folder New"
	Then I should see "18" children for "Folder New"
	And I should not see "Folder 2"
	
Scenario: Search explorer
	Given the explorer is visible
	And I open "localhost" server
	When I search for "Folder 3"
	Then I should see "Folder 3" only
	And I should not see "Folder 1"
	And I should not see "Folder 2"
	And I should not see "Folder 4"
	And I should not see "Folder 5"

Scenario: Creating Folder in localhost
   #Creating Folder
   Given the explorer is visible
   When I open "localhost" server
   Then I should see "5" folders
   When I create "MyNewFolder" in "localhost"
   Then I should see the path "localhost/MyNewFolder" 

#Scenario: Creating And Deleting Folder in localhost
#  #Creating Folder
#  Given the explorer is visible
#  When I open "localhost" server
#  Then I should see "5" folders
#  When I create "New Folder" in "localhost"
#  Then I should see "New Folder" in "localhost" server
#  And I should see "6" folders
#  #Deleting Folders
#  When I delete "New Folder" in "localhost" server
#  Then I should see "5" folders
#  And I should not see "New Folder"
#  #Creating Subfolder In a Folder
#  When I open "Folder 2"
#  Then I should see "18" children for "Folder 2"
#  When I create "New Folder" in "Folder 2"
#  Then I should see "19" children for "Folder 2"
#  And I should see "New Folder" in "Folder 2"
#  #Deleting Sub Folder
#  When I delete "New Folder" in "Folder 2"
#  Then I should see "18" children for "Folder 2" 
#  And I should not see "New Folder" in "Folder 2"
#
#
#
#cenario: Deleting Resource in folders
#   Given the explorer is visible
#   When I open "localhost" server
#   Then I should see "5" folders
#	When I open "Folder 6"
#	Then I should see "4" children for "Folder 6"
#	And I should see "deleteresouce" in "Folder 6"
#	When I delete "deleteresouce" in "Folder 6"
#   Then I should not see "deleteresouce" in "Folder 6"
#	And I should see "3" children for "Folder 6"
#
#cenario: Deleting Resource in localhost Server
#   Given the explorer is visible
#   When I open "localhost" server
#   Then I should see "5" folders
#	And I should see "5" resources in "localhost"
#	And I should see "Resource" in "localhost" 
#	When I delete "Resource" in "localhost"
#   Then I should not see "Resource" in "localhost"
#	And I should see "4" resources in "localhost"
#
#
#
#cenario: Opening Versions in Explorer
#  Given the explorer is visible
#  When I open "localhost" server
#  Then I should see "5" folders
#  When I open "Folder 1" 
#  #Testing Resource Icons
#  Then I should see "2" workflows with "View,Execute" Icons
#  And I should see "2" DB Services with "View,Execute" Icons
#  And I should see "2" Web Services with "View,Execute" Icons
#  And I should see "2" Plugin Services with "View,Execute" Icons
#  When I open "WF1" in "Folder 1"
#  Then "WF1" is opened
#  #Opening Version History
#  When I Show Version History for "WF1" in "Folder 1"
#  Then I should see "3" versions with "View" Icons
#  And I should not see "3" versions with "View,Execute" Icons
#  When I open "v.1" of "WF1" in "Folder 1"
#  Then "v.1" is opened
#  When I Make "v.1" the current version of "WF1" in "Folder 1"
#  Then I should see "4" versions with "View" Icons
#  Then I should see "OldVersion" in "Folder 1"
#  #Deleting Versions
#  When I Delete Version "v.2"
#  Then I should not see "v.2"
#  And I should see "3" versions with "View" Icons
#
#
#cenario: No Version history option for services and sources.
#  Given the explorer is visible
#  When I open "localhost" server
#  Then I should see "Webservice" in "localhost" server
#  When I Show Version History for "service" in "localhost" server is "False"
#  Then I should see "Pluginservice" in "localhost" server
#  When I Show Version History for "Pluginservice" in "localhost" server is "False"
#  Then I should see "Remoteserver" in "localhost" server
#  When I Show Version History for "Remoteserver" in "localhost" server is "False"
# 
#
#
#cenario: Creating Services Under Localhost 
#	Given the explorer is visible
#	When I open "New Service" in "localhost" server
#	Then "Unsavesd1" is opened
#	When I open "New Database Connector" in "localhost" server
#	Then "New Database Service" is opened
#	When I open "New Plugin Connector" in "localhost" server
#	Then "New Plugin Service" is opened
#	When I open "New Web Service Connector" in "localhost" server
#	Then "New Web Service" is opened
#	When I open "New Remote Warewolf Source" in "localhost" server
#	Then "New Server" is opened
#	When I open "New Plugin Source" in "localhost" server
#	Then "New Plugin Source" is opened
#	When I open "New Web Source" in "localhost" server
#	Then "New Web Source" is opened
#	When I open "New Email Source" in "localhost" server
#	Then "New Email Source" is opened
#	When I open "New Dropbox Source" in "localhost" server
#	Then "Dropbox Source" is opened
#
#
#cenario: Creating Services Under Explorer Folder 
#	Given the explorer is visible
#	When I open "New Service" for "Folder1" in "localhost" server
#	Then "Unsavesd1" is opened
#	When I open "New Database Connector" for "Folder1" in "localhost" server
#	Then "New Database Service" is opened
#	When I open "New Plugin Connector" for "Folder1" in "localhost" server
#	Then "New Plugin Service" is opened
#	When I open "New Web Service Connector" for "Folder1" in "localhost" server
#	Then "New Web Service" is opened
#	When I open "New Remote Warewolf Source" for "Folder1" in "localhost" server
#	Then "New Server" is opened
#	When I open "New Plugin Source" for "Folder1" in "localhost" server
#	Then "New Plugin Source" is opened
#	When I open "New Web Source" for "Folder1" in "localhost" server
#	Then "New Web Source" is opened
#	When I open "New Email Source" for "Folder1" in "localhost" server
#	Then "New Email Source" is opened
#	When I open "New Dropbox Source" for "Folder1" in "localhost" server
#	Then "Dropbox Source" is opened
#	When I Deploy "Folder 1" of "localhost" server
#	Then "Deploy" is opened
#
#
#
#cenario: Context Menu Items for workflow.
#	Given the explorer is visible
#	When I open "Open" for "workflow" in "localhost" server
#	Then "workflow" is opened
#	And I open "New Database Connector" for "workflow" in "localhost" server is "False"
#	And I open "New Plugin Connector" for "workflow" in "localhost" server is "False"
#	And I open "New Web Service Connector" for "workflow" in "localhost" server is "False"
#	And I open "New Remote Warewolf Source" for "workflow" in "localhost" server is "False"
#	And I open "New Plugin Source" for "workflow" in "localhost" server is "False"
#	And I open "New Web Source" for "workflow" in "localhost" server is "False"
#	And I open "New Email Source" for "workflow" in "localhost" server is "False"
#	And I open "New Dropbox Source" for "workflow" in "localhost" server is "False"
#	And I Deploy "workflow" of "localhost" server
#	And "Deploy" is opened
#	And I open Dependencies of "workflow" in "localhost" server
#	And "Ones*Dependants" is opened
#
#
#cenario: Opening Dependencies Of All Services In Explorer
#   Given the explorer is visible
#	When I open Show Dependencies of "WF1" in "Folder1"
#	Then "WF1 Dependents" is opened
#	When I open Show Dependencies of "WebServ1" in "Folder1"
#	Then "WebServ1 Dependents" is opened
#	When I open Show Dependencies of "DB Service1" in "Folder1"
#	Then "DB Service1 Dependents" is opened
#	When I open Show Dependencies of "PluginServ1" in "Folder1"
#	Then "PluginServ1 Dependents" is opened
#
#
#
#cenario: Renaming Folder And Workflow Service
#	Given the explorer is visible
#	And I open "localhost" server
#	When I rename "Folder 2" to "Folder New"
#	Then I should see "18" children for "Folder New"
#	And I should not see "Folder 2"
#	Given I rename "WF1" of "Follder 1" to "WorkFlow1" in "localhost" server 
#	Then I should see "WorkFlow1" of "Folder1" in "localhost" server
#	And I should not see "WF1" in "Folder1"
#	Given I rename "WF2" of "Follder 1" to "WorkFlow1" in "Localhost" server 
#	Then Conflict message is occured
#
#cenario: Renaming Service in explorer
#   Given the explorer is visible
#	And I open "localhost" server
#	And I should see "Renameresource" in "localhost"
#	When I rename "Renameresource" to "renamed" in "localhost" server
#	Then I should see "renamed" in "localhost" server
#	Then Conflict message should be occured
#
#
#cenario: Rename conflicting resources
#   Given the explorer is visible
#	And I open "localhost" server
#	And I should see "Conflict" in "localhost"
#	And I should see "Renameresource" in "localhost"
#	When I rename "Conflict" to "Renameresource" in "localhost" server
#	Then I should see "renameconflict" in "localhost" server
#	Then Conflict message should be occured
#
#
#
#	
#cenario: Renaming Workflow Service Is Creating Version In Version History
#	Given the explorer is visible
#	And I open "localhost" server
#	Given I rename "WF2" of "Follder 1" to "WorkFlow2" in "Localhost" server 
#	Then I should see "WorkFlow2" of "Folder1" in "localhost" server
#	And I should not see "WF2" in "Folder1"
#	When I Show Version History for "WF2" in "Folder 1"
#   Then I should see "1" versions with "View" Icons
#   And I should not see "1" version with "Execute" Icons
#
#
#cenario: Opening Resources from remote server
#   Given the explorer is visible
#	When I Connected to Remote Server "Remote"
#	And I open "Remote" server
#	Then I should see "10" folders
#	
#
#cenario: Searching resources by using filter
#  Given the explorer is visible
#  And I open "localhost" server
#  When I search for "Folder 1" in explorer
#  Then I should see "Folder 1" in "localhost" server 
#  And I search for "deleteresouce" in explorer
#  Then I should see "deleteresouce" in "Follder 1" 
#
#cenario: Checking versions 
#  Given the explorer is visible
#  And I open "localhost" server
#  When I search for "WF1" in explorer
#  Then I should see "WF1" in "Follder 1" 
#  When I Show Version History for "WF1" in "Folder 1"
#  Then I should see "3" versions with "View" Icons
#  And I should not see "3" versions with "View,Execute" Icons
#