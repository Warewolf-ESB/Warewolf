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

@DeployTab
Scenario: Deploy Tab
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "localhost"
	 And destination "localhost" is connected
	 Then "Deploy" is "Disabled"
	 And the validation message is "Source and Destination cannot be the same."	  

@DeployTab
Scenario: Deploy is successfull
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer"
	 And destination "DestinationServer" is connected
	 When I select "Utility - Date and Time" from Source Server
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."

@DeployTab
Scenario: Conflicting resources on Source and Destination server cancel deploy
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected	
	 Then I select Destination Server as "DestinationServer" with SameName confilcts
	 And I select "Control Flow - Sequence" from Source Server
	 When I click Cancel on Resource exists in the destination server popup
	 And I deploy 
	 Then Resource exists in the destination server popup is shown
	 | # | Source Resource                  | Destination Resource |
	 | 1 | Examples\Control Flow - Sequence | Examples\\Control Flow - Sequence  |
	 Then deploy is not successfull

Scenario: Conflicting resources on Source and Destination server OK deploy
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected	
	 Then I select Destination Server as "DestinationServer" with SameName confilcts
	 And I select "Control Flow - Sequence" from Source Server
	 When I click OK on Resource exists in the destination server popup
	 And I deploy 
	 Then Resource exists in the destination server popup is shown
	 | # | Source Resource                  | Destination Resource |
	 | 1 | Examples\Control Flow - Sequence | Examples\\Control Flow - Sequence  |
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."

@DeployTab
Scenario: Conflicting resources on Source and Destination server deploy is not successful
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 When I select Destination Server as "DestinationServer"
	 Then selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And I select "bob" from Source Server
	 When I click Cancel on Resource exists in the destination server popup	 
	 Then deploy is not successfull

@DeployTab	 
Scenario: Deploying a connector with a source
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 When I select Destination Server as "DestinationServer"
	 Then selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And Calculation is invoked
	 Then New Resource is "0"	 
	 When I select "FetchPlayers" from Source Server
	 Then "Deploy" is "Enabled" 
	 And Select All Dependencies is "true"
	 When I Select All Dependecies
	 Then  I select "DemoDB" from Source Server
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "2 Resources Deployed Successfully."

@DeployTab
Scenario: Selected for deploy items type is showing on deploy tab
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer" with items
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 When I select "Utility - Date and Time" from Source Server
	 And I select "FetchPlayers" from Source Server
	 And I select "DemoDB" from Source Server
	 And Calculation is invoked
	 Then Services is "1"
	 And Sources is "1"

@DeployTab
Scenario: Deploy Summary is showing new and overiding resources 
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer" with items
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And I select "FetchPlayers" from Source Server
	 And Calculation is invoked
	 Then New Resource is "1"
	 And Override is "0"
	 When I select "bob" from Source Server
	 Then New Resource is "1"
	 And Override is "1"
	 When I Unselect "bob" from Source Server
	 And Calculation is invoked
	 Then New Resource is "1"
	 And Override is "0"
	
#Wolf-1106
@DeployTab
Scenario: Deploying items from one server to the next with the same name
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer" with SameName different ID confilcts
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And I select "Control Flow - Sequence" from Source Server
	 When I deploy
	 Then the User is prompted to "Rename or Delete" one of the resources

#Wolf-312
@DeployTab
Scenario: Warning message no longer appears
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer"
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And I select "bob" from Source Server
	 When I click OK on Resource exists in the destination server popup
	 And I deploy 
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."	 

#wolf-117
@DeployTab
Scenario: Deploying to an Older server version
	Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 Then I select Destination Server as "DestinationServer"
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 And destination Server Version is "0.0.0.1"
	 And I select "Utility - Date and Time" from Source Server
	 When I deploy 	
	 Then a warning message appears "Deploying to an older server version could result in resources not working on destination server"	 
	 And deploy is successfull

@DeployTab
Scenario: Deploy Based on permission Deploy To
	 Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And source is connected
	 And I select "bob" from Source Server
	 And I cannot deploy to destination
	 Then I select Destination Server as "DestinationServer"
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected
	 Then "Deploy" is "Disabled" 
	 And the validation message is "Destination server permission Deploy To not allowed."

@DeployTab
Scenario: Deploy Based on permission Deploy From
	 Given I have deploy tab opened
	 And I cannot deploy from source
	 And selected Source Server is "localhost"
	 And source is connected
	 And I select "bob" from Source Server
	  Then I select Destination Server as "DestinationServer"
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected

@DeployTab
Scenario: Deploy resource Tests message
	 Given I have deploy tab opened
	 When I select Destination Server as "DestinationServer"
	 Then source is connected
	 And destination "DestinationServer" is connected
	 And I deploy
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."


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