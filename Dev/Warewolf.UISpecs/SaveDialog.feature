@SaveDialog
Feature: SaveDialog
	In order to save services
	As a warewolf studio user
	I want to give the workflow a name and location
	
Scenario: Create WorkFlow In Folder Opens Save Dialog With Folder Already Selected 
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I RightClick Explorer Localhost First Item
	Then I Select NewWorkflow From Explorer Context Menu
	And I Make Workflow Savable
	And I Click Save Ribbon Button to Open Save Dialog
	Then I Enter Service Name Into Save Dialog As "TestService"
	And I Click SaveDialog Save Button
	And "TestService" Resource Exists In Windows Directory "C:\ProgramData\Warewolf\Resources\Unit Tests"

Scenario: Filter Save Dialog Close And ReOpen Clears The Filter
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "Hello World"
	And Filtered Item Exists
	And I Click SaveDialog CancelButton
	Then Explorer Items appear on the Explorer Tree
	And I Click Save Ribbon Button to Open Save Dialog
	Then Explorer Items appear on the Save Dialog Explorer Tree


Scenario: Create New Folder In Localhost Then Open Context Menu Server From Save Dialog
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "Created Another Folder"
	And I RightClick Save Dialog Localhost
	And I Select New Folder From SaveDialog Context Menu
	And I Enter New Folder Name as "Created Another Folder"
	And I RightClick Save Dialog Localhost First Item
	And Context Menu Has Two Items

Scenario: Create New Folder In Localhost From Save Dialog Then Delete In Main Explorer
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "I_Will_Delete_This_Folder"
	And I RightClick Save Dialog Localhost
	And I Select New Folder From SaveDialog Context Menu
	And I Name New Folder as "I_Will_Delete_This_Folder"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "I_Will_Delete_This_Folder"
	And I RightClick Explorer Localhost First Item
	And I Select Delete From Explorer Context Menu
	And I Click MessageBox Yes
	And Folder Is Removed From Explorer

Scenario: Create New Folder In Localhost Server From Save Dialog
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "New Created Folder"
	And I RightClick Save Dialog Localhost
	And I Select New Folder From SaveDialog Context Menu
	And I Name New Folder as "New Created Folder"
	And I Click SaveDialog CancelButton
	And I Refresh Explorer
	And I Filter the Explorer with "New Created Folder"
	Then Explorer Contain Item "New Created Folder"

Scenario: Create New Folder In Existing Folder As A Sub Folder From Save Dialog
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "Acceptance Tests"
	And I Create New Folder Item using Shortcut 
	And I Name New Sub Folder as "New Created Sub Folder"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "New Created Sub Folder"
	Then Explorer Contain Sub Item "New Created Sub Folder"

Scenario: Rename Resource From Save Dialog
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "ResourceToRename"
	And I RightClick Save Dialog Localhost First Item
	And I Select Rename From SaveDialog Context Menu
	And I Rename Save Dialog Explorer First Item To "ResourceRenamed"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "ResourceRenamed"
	Then Explorer Contain Item "ResourceRenamed"

Scenario: Delete Resource From Save Dialog
	Given The Warewolf Studio is running
	And I Create New Workflow using shortcut
	And I Make Workflow Savable And Then Save
	And I Filter Save Dialog Explorer with "ResourceToDelete"
	And I RightClick Save Dialog Localhost First Item
	And I Select Delete From SaveDialog Context Menu
	And I Click MessageBox Yes
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "ResourceToDelete"
	Then Explorer Does Not Contain Item "ResourceToDelete"


