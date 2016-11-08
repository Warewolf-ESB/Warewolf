﻿@Explorer
Feature: Explorer
	In order to manage services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Explorer Context Menu Items
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Dice Roll Example Onto DesignSurface
	And I Click Subworkflow Done Button
	And I Drag Dice Onto Dice On The DesignSurface
	And I Click Workflow CollapseAll
	And I Save With Ribbon Button And Dialog As "Local_DoubleDice"	
	And I Filter the Explorer with "Local_DoubleDice"
	And I Open Explorer First Item Context Menu
	And I Click ViewSwagger From ExplorerContextMenu
	And I Open Explorer First Item Context Menu 
	And I Open Explorer First Item Version History With Context Menu 
	And I Rename LocalWorkflow To SecodWorkFlow 
	And I Open Explorer First Item Context Menu 
	And I Click Duplicate From Explorer Context Menu for Service "SecondWorkFlow"
	And I Enter Duplicate workflow name 
	And I Click UpdateDuplicateRelationships 
	And I Click Duplicate From Duplicate Dialog 
	And I Filter the Explorer with "SecondWorkflow"
	And I Open Explorer First Item Context Menu 
	And I Select Show Dependencies In Explorer Context Menu for service "SecondWorkflow"
	And I Click Close Dependecy Tab 
	And I Click View Api From Context Menu

Scenario: Drag on Remote Subworkflow from Explorer and Execute it
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Select "Remote Connection Integration" From Explorer Remote Server Dropdown List
	And I Click Explorer RemoteServer Connect Button
	And I Wait For Spinner "ExplorerTree.FirstRemoteServer"
	And I Filter the Explorer with "workflow1"
	And I Drag Explorer Remote workflow1 Onto Workflow Design Surface
	And I Save With Ribbon Button And Dialog As "LocalWorkflowWithRemoteSubworkflow"
	And I DoubleClick Explorer Localhost First Item
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	And I Click Debug Output Workflow1 Name
	And I Select Show Dependencies In Explorer Context Menu for service "LocalWorkflowWithRemoteSubworkflow"
	And I Click Close Dependecy Tab 
	And I Click Close Workflow Tab Button
	
Scenario: Deploy and Reverse Deploy View Only Workflow
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Dice Roll Example Onto DesignSurface
	And I Save With Ribbon Button and Dialog As "DeployViewOnly" and Append Unique Guid
	And I Click Close Workflow Tab Button
	And I Set Resource Permissions For "DeployViewOnly" to Group "Public" and Permissions for View to "true" and Contribute to "false" and Execute to "false"
	And I Click Deploy Ribbon Button
	##Possible version confict dialog
	And I Try Click Message Box OK
	##Possible deploy conflict dialog
	And I Try Click Message Box OK
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server Connect Button
	And I Deploy "DeployViewOnly" From Deploy View
	And I Try Click Message Box OK
	And I Select RemoteConnectionIntegrationConnected From Deploy Tab Source Server Combobox
	And I Select LocalhostConnected From Deploy Tab Destination Server Combobox
	And I Deploy "DeployViewOnly" From Deploy View