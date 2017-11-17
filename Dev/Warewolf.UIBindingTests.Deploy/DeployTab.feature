@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Menus.XamDataTree.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Editors.XamComboEditor.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Editors.XamRichTextEditor.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DockManager.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
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
	 Then I select Destination Server as "localhost"
	 And destination "localhost" is connected
	 Then "Deploy" is "Disabled"
	 And the validation message is "Source and Destination cannot be the same."	  

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

moveScenario: Deploy Summary is showing new and overiding resources 
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

Scenario: Deploy Based on permission Deploy From
	 Given I have deploy tab opened
	 And I cannot deploy from source
	 And selected Source Server is "localhost"
	 And source is connected
	 And I select "bob" from Source Server
	  Then I select Destination Server as "DestinationServer"
	 When selected Destination Server is "DestinationServer"
	 And destination "DestinationServer" is connected

Scenario: Deploy resource Tests message
	 Given I have deploy tab opened
	 When I select Destination Server as "DestinationServer"
	 Then source is connected
	 And destination "DestinationServer" is connected
	 And I deploy
	 Then deploy is successfull
	 And the Deploy validation message is "1 Resource Deployed Successfully."
