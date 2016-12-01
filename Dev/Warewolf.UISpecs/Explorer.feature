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
	And I Click Close Dependecy Tab
	And I Click Explorer Connect Remote Server Button
	And I Try Close Workflow
	
Scenario: Opening and Editing workflow from Explorer Remote
	Given The Warewolf Studio is running
	When I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
	And I Click Explorer Connect Remote Server Button
	And I Filter the Explorer with "Hello World"
	When I open "Hello World" in Remote Connection Integration
	And I Try Close Workflow
	And I Click Explorer Connect Remote Server Button

 Scenario: Deleting a Resource localhost
   Given The Warewolf Studio is running
   When I Click New Workflow Ribbon Button
   And I Filter the Explorer with "workflow1"
   And I Drag Explorer workflow1 Onto Workflow Design Surface
   And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer Localhost First Item
   And I Select Delete FromExplorerContextMenu
   And I Click MessageBox Yes 
   
 Scenario: Deleting a Folder in localhost
   Given The Warewolf Studio is running 
   When I Filter the Explorer with "FolderToDelete" 
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
   And I Click Explorer Connect Remote Server Button

 Scenario: Deleting a Folder in Remote
   Given The Warewolf Studio is running  
   When I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
   And I Click Explorer Connect Remote Server Button
   And I Wait For Explorer First Remote Server Spinner
   And I Filter the Explorer with "Examples"
   And I RightClick Explorer First Remote Server First Item
   And I Select Delete FromExplorerContextMenu
   And I Click MessageBox Yes
   And I Click Explorer Connect Remote Server Button


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
	Given The Warewolf Studio is running 
	When I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
    And I Click Explorer Connect Remote Server Button
    And I Wait For Explorer First Remote Server Spinner
    And I Filter the Explorer with "Examples"
	When I Rename First Remote Resource FromContextMenu to "Renamed"
	And I Click Explorer Filter Clear Button
	When I Filter the Explorer with "Renamed"
	And I Rename LocalFolder To SecondFolder
	And I Rename FolderItem ToNewFolderItem
	And I Wait For Explorer Localhost Spinner
	And I Click Explorer Connect Remote Server Button
	
