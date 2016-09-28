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
	And I Click Close Workflow Tab Button
	And I Filter the Explorer with "Local_DoubleDice"
	And I Open Explorer First Item Dependancies With Context Menu
	And I Click ViewSwagger From ExplorerContextMenu
	And I Open Explorer First Item Dependancies With Context Menu 
	And I Open Explorer First Item Version History With Context Menu 
	And I Rename LocalWorkflow To SecodWorkFlow 
	And I Open Explorer First Item Dependancies With Context Menu 
	And I Click Duplicate From Explorer Context Menu for Service "SecondWorkFlow"
	And I Enter Duplicate workflow name 
	And I Click UpdateDuplicateRelationships 
	And I Click Duplicate From Duplicate Dialog 
	And I Filter the Explorer with "SecondWorkflow"
	And I Open Explorer First Item Dependancies With Context Menu 
	And I Select Show Dependencies In Explorer Context Menu for service "SecondWorkflow"
	And I Click Close Dependecy Tab 
	And I Click View Api From Context Menu
