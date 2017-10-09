@Explorer
Feature: Explorer
	In order to manage services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Drag on Remote Subworkflow from Explorer and Execute it
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	And I Connect To Remote Server
	And I Filter the Explorer with "GenericResource"
	And I Drag Explorer Remote GenericResource Onto Workflow Design Surface
	And I Save With Ribbon Button And Dialog As "LocalGenericResourceWithRemoteSubworkflow"
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	And I Click Debug Output GenericResource Name
	
Scenario: Opening and Editing Workflow from Explorer Remote
	Given The Warewolf Studio is running
	When I Connect To Remote Server
	And I Filter the Explorer with "Hello World"
	When I open "Hello World" in Remote Connection Integration
	
Scenario: Opening Workflow local and remote using right click
   Given The Warewolf Studio is running
   When I Connect To Remote Server
   And I Filter the Explorer with "Hello World"
   And I RightClick Explorer First Remote Server First Item
   And I Select Open From Explorer Context Menu
   Then Remote "Hello World - Remote Connection Integration" is open
   When I RightClick Explorer Localhost First Item
   And I Select Open From Explorer Context Menu
   Then Local "Hello World" is open

 Scenario: Deleting a Resource localhost
   Given The Warewolf Studio is running
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   When I RightClick Explorer Localhost First Item
   And I Select Delete From Explorer Context Menu
   When I Click MessageBox Yes
 
 Scenario: Deleting a Folder in localhost
   Given The Warewolf Studio is running
   When I Filter the Explorer with "FolderToDelete" 
   And I RightClick Explorer Localhost First Item
   And I Select Delete From Explorer Context Menu
   And I Click MessageBox Yes 

 Scenario: Filter Should Clear On Connection Of Remote Server
   Given The Warewolf Studio is running
   When I Filter the Explorer with "Hello World" 
   When I Connect To Remote Server
   Then Filter Textbox is cleared

 Scenario: Deleting a Resource Remote
   Given The Warewolf Studio is running
   When I Connect To Remote Server
   And I Click New Workflow Ribbon Button
   And I validate and delete the existing resource with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "GenericResource"
   And I Drag Explorer Remote GenericResource Onto Workflow Design Surface
   And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer First Remote Server First Item
   And I Select Delete From Explorer Context Menu
   And I Click MessageBox Yes 

Scenario: Clear filter  
   Given The Warewolf Studio is running 
   When I Filter the Explorer with "Hello World"
   Then Filter Textbox has "Hello World"
   And I Click Explorer Filter Clear Button
   Then Filter Textbox is cleared

Scenario: Drag on service from Explorer and change input and output
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	And I Filter the Explorer with "Hello World"
	And I Drag Explorer workflow Onto Workflow Design Surface
	And I change Hello World input variable
	And I change Hello World output variable
