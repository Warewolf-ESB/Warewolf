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


##Test Cases
@Deploy
Scenario: Deploy Tab
     Given I have deploy tab opened
	#Source Server Side
	 And "Source Server" selected as "localhost (Connected)"
	 And Source Server edit is "Disabled"
	 And Source server connection is "Disabled"
	 And "localhost (http://rsaklfmurali:3142/)" is visibe
	#Destination Server Side
	 And "Destination Server" selected as "localhost (Connected)"
	 And Destination Server edit is "Disabled"
	 And Destination server connection is "Disabled"
	 And "localhost (http://rsaklfmurali:3142/)" is visibe
	 And 'Deploy' has validation "True"
	 And the validation message as "Source and Destination cannot be the same"
	 And 'Deploy' button is "Disabled"
	 And "Select All Dependencies" button is "Disabled"
	 And deploy has a validation message "True"
	 And the message as "Source and Destination cannot be the same" 



Scenario: Connect control Edit and Connect buttons are enabling 
     Given I have deploy tab opened
	 And "Source Server" selected as "localhost (Connected)"
     When "Destination Server" selected as "Remote (Connected)"
	 Then Destination Server edit is "Enabeled"
	 And Destination server connection is "Enabeled"
	 And "localhost (http://rsaklfmurali:3142/)" is visibe
	 And 'Deploy' has validation "False"
	 And the validation message as ""
	 And 'Deploy' button is "Disabled"
	 And "Select All Dependencies" button is "Disabled"
	 
	 

Scenario: Deploy button is enabling when selecting resource in source side
     Given I have deploy tab opened
	 And "Source Server" selected as "localhost (Connected)"
     When "Destination Server" selected as "Remote (Connected)"
	 Then I select a resource "Examples\Utility - Date and Time"
	 And 'Deploy' button is "Enabled" 


Scenario: Resource Deploy is successfull
     Given I have deploy tab opened
	 And "Source Server" selected as "localhost (Connected)"
     When "Destination Server" selected as "Remote (Connected)"
	 Then I select a resource "Examples\Utility - Date and Time"
	 And 'Deploy' button is "Enabled" 
	 When I deploy 
	 Then deploy is "successfull"
	 And the validation message as "Items deployed successfully"
	 And "Examples\Utility - Date and Time" is visible in destination side


Scenario: Deploy is showing conflicting resources while deploy
     Given I have deploy tab opened
	 And "Source Server" selected as "localhost (Connected)"
     When "Destination Server" selected as "Remote (Connected)"
	 Then I select a resource "Examples\Utility - Date and Time"
	 And 'Deploy' button is "Enabled" 
	 When I deploy 
	 Then "Resource exists in the destination server" popup is shown
	 | # | Source Resource         | Destination Resource    |
	 | 1 | Utility - Date and Time | Utility - Date and Time |
	 When I select ok in "Resource exists in the destination server" 
	 Then deploy is "successfull"
	 And the validation message as "Items deployed successfully"
	 And "Examples\Utility - Date and Time" is visible in destination side














