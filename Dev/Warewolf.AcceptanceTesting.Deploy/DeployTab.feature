@DeployTab
Feature: DeployTab
	In order to Deploy resource.
	As a warewolf user
	I want to Deploy aresource from one server to another server.

#Present in Document
#Ensure Deploy Tab is opening when user click on deploy.
#Deploy button is enabling when selecting resource in source side
#Ensure Deploy screen deploys successfull from one serve to the next
#Conflicting resources on Source and Destination server
#Conflicting resources on Source and Destination server deploy is not successful
#Select all Dependecies is selecting dependecies
#Deploying a connector with a source
#Mouse right click select Dependecies is selecting dependecies
#Filtering and clearing filter on source side
#Deploy is successfull when filter is on on both sides
#Selected for deploy items type is showing on deploy tab
#Deploy Summary is showing new and overiding resources
#Not allowing to deploy when source and destination servers are same 
#One server with different names in both sides not allow to deploy
#Deploy is enabled when I change server after validation thrown
#Deploy a resource without dependency is showing popup
#Wolf-1106 Deploying items from one server to the next with the same name

Scenario: Deploy Tab
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 And selected Destination Server is "localhost"
	 And destination is connected
	 Then "Deploy" is "Disabled"
	 And the validation message is "Source and Destination cannot be the same."	 
	 
Scenario: Deploy button is enabling when selecting resource in source side
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 When I select "Examples\Utility - Date and Time" from Source Server
	 Then "Deploy" is "Enabled" 
	 When "Remote" is Disconnected
	 Then "Deploy" is "Disabled"

Scenario: Deploy is successfull
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 When I select "Examples\Utility - Date and Time" from Source Server
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."

Scenario: Conflicting resources on Source and Destination server
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 And I select "Examples\bob" from Source Server
	 When I click OK on Resource exists in the destination server popup
	 And I deploy 
	 Then Resource exists in the destination server popup is shown
	 | # | Source Resource | Destination Resource |
	 | 1 | bob             | DifferentNameSameID  |
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."

Scenario: Conflicting resources on Source and Destination server deploy is not successful
      Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 And I select "Examples\bob" from Source Server
	 When I click Cancel on Resource exists in the destination server popup	 
	 Then deploy is not successfull
	 
Scenario: Deploying a connector with a source
     Given I have deploy tab opened
	  And selected Source Server is "localhost"
	 And source is connected
	 When selected Destination Server is "Remote"
	 And destination is connected
	 Then New Resource is "0"	 
	 When I select "DB Service\FetchPlayers" from Source Server
	 Then "Deploy" is "Enabled" 
	 And "Select All Dependencies" is "Enabled"
	 When I Select All Dependecies
	 Then "sqlServers\DemoDB" from Source Server is "Selected"
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "2 Resources Deployed Successfully."	 

Scenario: Deploying a connector with a source should set deploy state
     Given I have deploy tab opened
	  And selected Source Server is "localhost"
	 And source is connected
	 When selected Destination Server is "Remote"
	 And destination is connected
	 Then New Resource is "0"
	 And source have deployable resources in "sqlServers\DemoDB"
	 When I select "DB Service\FetchPlayers" from Source Server
	 Then "Deploy" is "Enabled" 
	 And "Select All Dependencies" is "Enabled"
	 When I Select All Dependecies
	 Then "sqlServers\DemoDB" from Source Server is "Selected"
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "2 Resources Deployed Successfully."
	 And resources in "sqlServers\DemoDB" are now unDeployable

Scenario: Filtering and clearing filter on source side
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     When I type "Date and Time" in Source Server filter
	 Then visibility of "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And visibility of "Examples\Data - Data - Data Split" from Source Server is "Not Visible"
	 And visibility of "Examples\Control Flow - Switch" from Source Server is "Not Visible"
	 And visibility of "Examples\Control Flow - Sequence" from Source Server is "Not Visible"
	 And visibility of "Examples\File and Folder - Copy" from Source Server is "Not Visible"
	 And visibility of "Examples\File and Folder - Create" from Source Server is "Not Visible"
	 When I type "" in Source Server filter
	 Then  visibility of "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And visibility of "Examples\Data - Data - Data Split" from Source Server is "Visible"
	 And visibility of "Examples\Control Flow - Switch" from Source Server is "Visible"
	 And visibility of "Examples\Control Flow - Sequence" from Source Server is "Visible"
	 And visibility of "Examples\File and Folder - Copy" from Source Server is "Visible"
	 And visibility of "Examples\File and Folder - Create" from Source Server is "Visible"

Scenario: Deploying with filter enabled
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
     When I type "Date and Time" in Source Server filter
	 Then visibility of "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And visibility of "Examples\Data - Data - Data Split" from Source Server is "Not Visible"
	 And visibility of "Examples\Control Flow - Switch" from Source Server is "Not Visible"
	 And visibility of "Examples\Control Flow - Sequence" from Source Server is "Not Visible"
	 And visibility of "Examples\File and Folder - Copy" from Source Server is "Not Visible"
	 And visibility of "Examples\File and Folder - Create" from Source Server is "Not Visible"
	 When I select "Examples\Utility - Date and Time" from Source Server
	 When I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."

Scenario: Selected for deploy items type is showing on deploy tab
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 When I select "Examples\Utility - Date and Time" from Source Server
	 And I select "DB Service\FetchPlayers" from Source Server
	 And I select "sqlServers\DemoDB" from Source Server

	 Then Services is "1"
	 And Sources is "1"


Scenario: Deploy Summary is showing new and overiding resources 
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 And I select "Examples\bob" from Source Server
	 Then New Resource is "1"
	 And Override is "0"
	 When I select "DB Service\FetchPlayers" from Source Server
	 Then New Resource is "2"
	 And Override is "0"
	 When I Unselect "Examples\bob" from Source Server
	 Then Override is "0"

Scenario: Deploy is enabled when I change server after validation thrown
     Given I have deploy tab opened
	  And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "localhost"
	 And destination is connected
	 When I select "Examples\Utility - Date and Time" from Source Server
	 Then "Deploy" is "Disabled" 
	 And the validation message is "Source and Destination cannot be the same."
     When selected Destination Server is "Remote"
	 And destination is connected
	 Then "Deploy" is "Enabled" 
	  And the validation message is ""
#
#Scenario: Deploy is disabled when I select resource with only View Permissions
#     Given I have deploy tab opened
#	 And selected Source Server is "localhost"
#	 And source is connected
#     When selected Destination Server is "Remote"
#	 And destination is connected
#	 When I select "Examples\bob" from Source Server
#	 Then "Deploy" is "Disabled" 

#new feature?
	
#Wolf-1106
Scenario: Deploying items from one server to the next with the same name
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 And I select "Examples\bob" from Source Server
	 When I deploy
	 Then the User is prompted to "Rename or Delete" one of the resources

#Wolf-312
Scenario: Warning message no longer appears
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	 And I select "Examples\bob" from Source Server
	 When I click OK on Resource exists in the destination server popup
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."	 

#wolf-117
Scenario: Deploying to an Older server version
	Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
     And destination is connected
	 And destination Server Version is "0.0.0.1"
	 And I select "Utility - Date and Time" from Source Server
	 When I deploy 	
	 Then a warning message appears "Deploying to an older server version could result in resources not working on destination server"
	 Then deploy is successfull


Scenario: Select All resources to deploy
    Given I have deploy tab opened
	And selected Source Server is "localhost"
	 And source is connected
     When selected Destination Server is "Remote"
	 And destination is connected
	And I check "localhost" on Source Server
	Then "All" the resources are checked
	And "Deploy" is "Enabled"

Scenario: Deploy Based on permission Deploy To
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 And I select "Examples\bob" from Source Server
     And I cannot deploy to destination
	 When selected Destination Server is "Remote"
	 And destination is connected
	 Then "Deploy" is "Disabled" 
	 And the validation message is "Destination server permission Deploy To not allowed."

Scenario: Deploy Based on permission Deploy From
     Given I have deploy tab opened
	 And I cannot deploy from source
	 And selected Source Server is "localhost"
	 And source is connected
	 And I select "Examples\bob" from Source Server
     When selected Destination Server is "Remote"
	 And destination is connected

###REQUIREMENTS Check to see what needs to be included



#Ensure user is not allowed to edit localhost conection in Source server.
#Ensure user is not allowed to edit localhost connection in Destination server.
#Ensure user is not allowed to disconnect local host Source and Destination servers.
#Ensure user is able to create remote connection from Source Server Side.
#Ensure user is able to create remote connection from Destination Server Side.
#Ensure source and Destination server is updated when remote connection is created.
#Ensure Deploy is thrown validation message when sorce and destinaton servers are same.
#Ensure Deploy button is not enabling when source and destination servers are same.
#Ensure Deploy button is enabling when user selects a resource to deploy.  
#Ensure when user selects resource which is already in destination server then both sides resource is highlighted 
#Ensure Select All Dependencies button is Enabling when selected resource has got dependencies.
#Ensure Select All Dependencies button is Disabled when selected resource has no dependencies.
#Ensure user is able to know the number of dependencies for selected resources.
#Ensure user is able to disconnect and connect servers from both source and deploy connect control.
#Ensure user is able to filter resources in Source side.
#Ensure user is able to filter resources in Destination side.
#Ensure Filter clear option on both server and destination side is clearing filter box.
#Ensure when user mouse right click on any resource in source side then Select All Dependencies option is available in context menu.
#Ensure when user selects "Select All Dependencies" on right click context menu on a selected resource then dependecies are selected.
#Ensure when user selects "Select all dependencies" by using mouse right click on a unselected resource then dependencies and resource will be selected.
#Ensure While deploying a service without dependencies then popup message should appear 
#Ensure when user is deploying conflicting resources then conflict message is thrown. 
#Ensure user is able to see Deploy summary in deploy tab
#Ensure user is able to see how many new resources are selected and how many resources are overriding.
#Ensure Save button is disabled when Deploy to is active.
#Deploying a connector with a source