Feature: Explorer
	In order to manage services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions
  
@DragFromExplorer
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
	
@OpenFromExplorer
Scenario: Opening and Editing Workflow from Explorer Remote
	Given The Warewolf Studio is running
	When I Connect To Remote Server
	And I Filter the Explorer with "Hello World"
	When I open "Hello World" in Remote Connection Integration
	
@OpenFromExplorer
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

@DeleteFromExplorer
 Scenario: Deleting a Resource localhost
   Given The Warewolf Studio is running
   When I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer Localhost First Item
   And I Select Delete From Explorer Context Menu
   When I Click MessageBox Yes
 
@DeleteFromExplorer
 Scenario: Deleting a Folder in localhost
   Given The Warewolf Studio is running
   When I Filter the Explorer with "FolderToDelete" 
   And I RightClick Explorer Localhost First Item
   And I Select Delete From Explorer Context Menu
   And I Click MessageBox Yes 

 @FilterExplorer
 Scenario: Filter Should Clear On Connection Of Remote Server
   Given The Warewolf Studio is running
   When I Filter the Explorer with "Hello World" 
   When I Connect To Remote Server
   Then Filter Textbox is cleared

@DeleteFromExplorer
@Ignore #TODO: Re-introduce this test once the move to the new domain (premier.local) is done
 Scenario: Deleting a Resource Remote
   Given The Warewolf Studio is running
   When I Connect To Remote Server
   And I Try Remove "LocalWorkflowWithRemoteSubworkflowToDelete" From Remote Server Explorer
   And I validate and delete the existing resource with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Click New Workflow Ribbon Button
   And I Filter the Explorer with "GenericResource"
   And I Drag Explorer Remote GenericResource Onto Workflow Design Surface
   And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I Filter the Explorer with "LocalWorkflowWithRemoteSubworkflowToDelete"
   And I RightClick Explorer First Remote Server First Item
   And I Select Delete From Explorer Context Menu
   And I Click MessageBox Yes 

@FilterExplorer
Scenario: Clear filter  
   Given The Warewolf Studio is running 
   When I Filter the Explorer with "Hello World"
   Then Filter Textbox has "Hello World"
   And I Click Explorer Filter Clear Button
   Then Filter Textbox is cleared

@DragFromExplorer
Scenario: Drag on service from Explorer and change input and output
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	And I Filter the Explorer with "Hello World"
	And I Drag Explorer workflow Onto Workflow Design Surface
	And I change Hello World input variable
	And I change Hello World output variable
