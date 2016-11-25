@Explorer
Feature: Explorer
	In order to manage services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Drag on Remote Subworkflow from Explorer and Execute it
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
	And I Click Explorer Connect Remote Server Button
	And I Wait For Explorer First Remote Server Spinner
	And I Filter the Explorer with "workflow1"
	And I Drag Explorer Remote workflow1 Onto Workflow Design Surface
	And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflow"
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	And I Click Debug Output Workflow1 Name
	And I Select Show Dependencies In Explorer Context Menu for service "LocalWorkflowWithRemoteSubworkflow"
	
Scenario: Deploy and Reverse Deploy View Only Workflow
	Given The Warewolf Studio is running
	When I Set Resource Permissions For "DeployViewOnly" to Group "Public" and Permissions for View to "true" and Contribute to "false" and Execute to "false"
	And I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server Connect Button
	And I Deploy "DeployViewOnly" From Deploy View
	And I Select localhost (Connected) From Deploy Tab Destination Server Combobox
	And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
	And I Click Deploy Tab Source Server Connect Button
	And I Deploy "DeployViewOnly" From Deploy View

Scenario: Opening and Editing workflow from Explorer Remote
	Given The Warewolf Studio is running
	When I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
	And I Click Explorer Connect Remote Server Button
	And I Filter the Explorer with "Hello World"
	When I open "Hello World" in Remote Connection Integration

 Scenario: Deleting a Resource localhost
   Given The Warewolf Studio is running
   When I Click New Workflow Ribbon Button
   And I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
   And I Click Explorer Connect Remote Server Button
   And I Wait For Explorer First Remote Server Spinner
   And I Filter the Explorer with "workflow1"
   And I Drag Explorer Remote workflow1 Onto Workflow Design Surface
   And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer Localhost First Item
   And I Select Delete FromExplorerContextMenu
   And I Click MessageBox Yes 
  
 Scenario: Deleting a Resource Remote
   Given The Warewolf Studio is running  
   When I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
   And I Click Explorer Connect Remote Server Button
   And I Wait For Explorer First Remote Server Spinner
   And I Click New Workflow Ribbon Button
   And I Filter the Explorer with "workflow1"
   And I Drag Explorer Remote workflow1 Onto Workflow Design Surface
   And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer First Remote Server First Item
   And I Select Delete FromExplorerContextMenu
   And I Click MessageBox Yes 

Scenario: Deleting Resource in localhost Server
   Given the explorer is visible
   When I open "localhost" server
   And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
   Then I should see "5" folders
   Then I should see the path "localhost\Folder 1\Resource 1"
    And I choose to "OK" Any Popup Messages
   When I delete "localhost\Folder 1\Resource 1"
   Then I should not see the path "localhost\Folder 1\Resource 1"

Scenario: Deleting Resource in localhost Server with Tests
   Given the explorer is visible
   When I open "localhost" server
   And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
   And I create 2 Tests for "localhost\Folder 1\Resource 1"
   Then "localhost\Folder 1\Resource 1" has 2 tests  
   Then I should see "5" folders
   Then I should see the path "localhost\Folder 1\Resource 1"
    And I choose to "OK" Any Popup Messages
   When I delete "localhost\Folder 1\Resource 1"
   Then I should not see the path "localhost\Folder 1\Resource 1"
   And "localhost\Folder 1\Resource 1" has 0 tests

Scenario: Checking versions 
  Given the explorer is visible
  When I open "localhost" server
  And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
  Then I should see "5" folders
  And I Setup  "3" Versions to be returned for "localhost\Folder 1\Resource 1"
  When I Show Version History for "localhost\Folder 1\Resource 1"
  Then I should see "3" versions with "View" Icons in "localhost\Folder 1\Resource 1"
  When I search for "Resource 1" in explorer
  Then I should see the path "localhost\Folder 1\Resource 1"
  Then I should see "3" versions with "View" Icons in "localhost\Folder 1\Resource 1"

Scenario: Clear filter
  Given the explorer is visible
  And I open "localhost" server
  When I open Resource "Folder 1"
  And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
  Then I should see the path "localhost\Folder 1\Resource 1"
  When I search for "Folder 1" in explorer
  Then I should see the path "localhost\Folder 1"
  Then I should not see the path "localhost\Folder 1\Resource 1"
  Then I should not see the path "localhost\Folder 2"
  When I search for "Resource 1" in explorer
  When I open Resource "Folder 1"
  Then I should see the path "localhost\Folder 1\Resource 1"
  When I clear "Explorer" Filter
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"

Scenario: Renaming Folder And Workflow Service on a remote server
	Given the explorer is visible
	And I connect to "Remote Connection Integration" server
    And I open "Remote Connection Integration" server
	When I rename "Remote Connection Integration\Folder 2" to "Folder New"
	Then I should see "18" children for "Folder New"
	When I open Resource "Folder New"
	And I create the "Remote Connection Integration\Folder New\Resource 1" of type "WorkflowService" 
	And I create the "Remote Connection Integration\Folder New\Resource 2" of type "WorkflowService" 
	Then I should see the path "Remote Connection Integration\Folder New"
	Then I should see the path "Remote Connection Integration\Folder New\Resource 1"
	And I should not see the path "Remote Connection Integration\Folder 2"
	When I rename "Remote Connection Integration\Folder New\Resource 1" to "WorkFlow1"	
	Then I should see the path "Remote Connection Integration\Folder New\WorkFlow1"
	When I rename "Remote Connection Integration\Folder New\Resource 2" to "WorkFlow1"	
	Then Conflict error message occurs
