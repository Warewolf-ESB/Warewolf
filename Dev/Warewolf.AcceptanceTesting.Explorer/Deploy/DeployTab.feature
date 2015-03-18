@Deploy
Feature: DeployTab
	In order to Deploy resource.
	As a warewolf user
	I want to Deploy aresource from one server to another server.

###REQUIREMENTS
#Ensure Deploy Tab is opening when user click on deploy.
#Ensure Source Server default connected to Localhost.
#Ensure Destination Server default connected to Localhost.
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
#Ensure when user selects 'Select All Dependencies' on right click context menu on a selected resource then dependecies are selected.
#Ensure when user selects 'Select all dependencies' by using mouse right click on a unselected resource then dependencies and resource will be selected.
#Ensure While deploying a service without dependencies then popup message should appear 
#Ensure when user is deploying conflicting resources then conflict message is thrown. 
#Ensure user is able to see Deploy summary in deploy tab
#Ensure user is able to see how many new resources are selected and how many resources are overriding.
#Ensure Save button us disabled when Deploy ta is active.

Scenario: Deploy Tab
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
	 And "localhost" is visible
	 When selected Destination Server is "localhost"
	 And "localhost" is visible
	 Then the validation message is "Source and Destination cannot be the same"
	 And "Deploy" is "Disabled"
	 And "Select All Dependencies" is "Disabled"	 

Scenario: Connect control Edit and Connect buttons are enabling 
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     When selected Destination Server is "Remote"
	 Then Destination Server edit is "Enabled"
	 And "localhost" is visibe
	 And the validation message as ""
	 And "Deploy" is "Disabled"
	 And "Select All Dependencies" is "Disabled"

Scenario: Deploy button is enabling when selecting resource in source side
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     And selected Destination Server is "Remote"
	 When I select "Examples\Utility - Date and Time" from Source Server
	 Then "Deploy" is "Enabled" 


Scenario: Deploy is successfull
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     And selected Destination Server is "Remote"
	 And I select "Examples\Utility - Date and Time" from Source Server
	 When I deploy 
	 Then deploy is successfull
	 And the validation message is "Items deployed successfully"
	 And "Examples\Utility - Date and Time" is visible on Destination Server


Scenario: Conflicting resources on Source and Destination server
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     And selected Destination Server is "Remote"
	 And I select "Examples\Utility - Date and Time" from Source Server
	 And I deploy 
	 Then "Resource exists in the destination server" popup is shown
	 | # | Source Resource         | Destination Resource    |
	 | 1 | Utility - Date and Time | Utility - Date and Time |
	 When I click OK on "Resource exists in the destination server" popup
	 Then deploy is successfull
	 And the validation message is "Items deployed successfully"

Scenario: Conflicting resources on Source and Destination server
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     And selected Destination Server is "Remote"
	 And I select "Examples\Utility - Date and Time" from Source Server
	 And I deploy 
	 Then "Resource exists in the destination server" popup is shown
	 | # | Source Resource         | Destination Resource    |
	 | 1 | Utility - Date and Time | Utility - Date and Time |
	 When I click Cancel on "Resource exists in the destination server" popup
	 Then deploy is not successfull
	 And the validation message is ""


Scenario: Select all Dependecies is selecting dependecies
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     And selected Destination Server is "Remote"
	 When I select "My Category\Double Roll and Check" from Source Server
	 Then "Deploy" is "Enabled" 
	 And "Select All Dependencies" is "Enabled"
	 When I click "Select All Dependecies" 
	 Then "My Category\Double Roll" from Source Server is "Selected"

#coded ui
@ignore
Scenario: Mouse right click select Dependecies is selecting dependecies
     Given I have deploy tab opened
	 And selected "Source Server" is "localhost"
     And selected "Destination Server" is "Remote"
	 When I select "My Category\Double Roll and Check" from Source Server
	 Then "Deploy" is "Enabled" 
	 When I Select All Dependecies"
	 Then "My Category\Double Roll" is "Selected"
	 And "Deploy" is "Enabled" 



Scenario: Filtering Resources on Destination side
     Given I have deploy tab opened
	 And selected Destination Server is "localhost"
     When I type "Utility - Date and Time" in Destination Server filter
	 Then "Examples\Utility - Date and Time" from Destination Server is "Visible"
	 And "Examples\Utility - Date and Time Difference" from Destination Server is "Visible"
	 And I select "Examples\Utility - Date and Time" from Destination Server
	  When I clear filter on Destination Server
	 Then "Examples\Utility - Date and Time" from Destination Server is "Visible"
	 And "Examples\Data - Data - Data Split" from Destination Server is "Visible"
	 And "Examples\Control Flow - Switch" from Destination Server is "Visible"
	 And "Examples\Control Flow - Sequence" from Destination Server is "Visible"
	 And "Examples\File and Folder - Copy" from Destination Server is "Visible"
	 And "Examples\File and Folder - Create" from Destination Server is "Visible"

Scenario: Filtering and clearing filter on source side
     Given I have deploy tab opened
	 And selected Source Server is "localhost"
     When I type "Utility - Date and Time" in Source Server filter
	 Then "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And "Examples\Data - Data - Data Split" from Source Server is "Not Visible"
	 And "Examples\Control Flow - Switch" from Source Server is "Not Visible"
	 And "Examples\Control Flow - Sequence" from Source Server is "Not Visible"
	 And "Examples\File and Folder - Copy" from Source Server is "Not Visible"
	 And "Examples\File and Folder - Create" from Source Server is "Not Visible"
	 When I clear filter on Source Server
	 Then "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And "Examples\Data - Data - Data Split" from Source Server is "Visible"
	 And "Examples\Control Flow - Switch" from Source Server is "Visible"
	 And "Examples\Control Flow - Sequence" from Source Server is "Visible"
	 And "Examples\File and Folder - Copy" from Source Server is "Visible"
	 And "Examples\File and Folder - Create" from Source Server is "Visible"


Scenario: Deploy is successfull when filter is on on both sides
     Given I have deploy tab opened
	 And selected Destination Server is "localhost"
	 And selected Destination Server is "Remote"
     When I type "Utility - Date and Time" in Destination Server filter
	 Then "Examples\Utility - Date and Time" from Source Server is "Visible"
	 And I select "Examples\Utility - Date and Time" from Source Server
	 When I type "Utility - Date and Time" in Destination Server filter
	 Then "Examples\Utility - Date and Time" from Destination Server is "Visible"
	 And "Deploy" is "Enabled"
	 When I deploy 
	 Then "Resource exists in the destination server" popup is shown
	 | # | Source Resource         | Destination Resource    |
	 | 1 | Utility - Date and Time | Utility - Date and Time |
	 When I click OK on "Resource exists in the destination server" popup
	 Then deploy is successfull
	 And the validation message is "Items deployed successfully"


