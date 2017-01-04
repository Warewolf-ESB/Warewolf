﻿@Explorer
Feature: Explorer
	In order to manage services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Drag on Remote Subworkflow from Explorer and Execute it
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	And I Select Remote Connection Integration From Explorer Remote Server Dropdown List
	And I Click Explorer Connect Remote Server Button
	And I Wait For Explorer First Remote Server Spinner
	And I Filter the Explorer with "workflow1"
	And I Drag Explorer Remote workflow1 Onto Workflow Design Surface
	And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflow"
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	And I Click Debug Output Workflow1 Name
	
Scenario: Opening and Editing workflow from Explorer Remote
	Given The Warewolf Studio is running
	When I Select Remote Connection Integration From Explorer Remote Server Dropdown List
	And I Click Explorer Connect Remote Server Button
	And I Filter the Explorer with "Hello World"
	When I open "Hello World" in Remote Connection Integration
	And I Try Close Workflow
	And I Click Explorer Connect Remote Server Button

 Scenario: Deleting a Resource localhost
   Given The Warewolf Studio is running
   When I Create New Workflow using shortcut
   And I Filter the Explorer with "workflow1"
   And I Drag Explorer workflow Onto Workflow Design Surface
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

 Scenario: Filter Should Clear On Connection Of Remote Server
   Given The Warewolf Studio is running
   When I Filter the Explorer with "Hello World" 
   When I Select Remote Connection Integration From Explorer Remote Server Dropdown List
   And I Click Explorer Connect Remote Server Button
   Then Filter Textbox is cleared
   And I Click Explorer Connect Remote Server Button

 Scenario: Deleting a Resource Remote
   Given The Warewolf Studio is running
   When I Select Remote Connection Integration From Explorer Remote Server Dropdown List
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

Scenario: Clear filter  
   Given The Warewolf Studio is running 
   When I Filter the Explorer with "Hello World"
   Then Filter Textbox has "Hello World"
   And I Click Explorer Filter Clear Button
   Then Filter Textbox is cleared
      
Scenario: Refresh Remote Server Refreshes Only The Remote Server
	Given The Warewolf Studio is running	
	When I Connect To Remote Server
	And I Double Click Localhost Server
	And I Select Connected RemoteConnectionIntegration From Explorer
	And I Refresh Explorer Withpout Waiting For Spinner
	Then Remote Server Refreshes