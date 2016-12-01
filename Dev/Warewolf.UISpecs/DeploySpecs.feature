Feature: DeploySpecs

Scenario: Deploying From Explorer Opens The Deploy With Resource Already Checked
	Given I Filter the Explorer with "Hello World"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy FromExplorerContextMenu
	And I Filter Deploy Source With "Hello World"
	Then Filtered Resourse Is Checked For Deploy
	And I Click Close Deploy Tab Button

Scenario: Deploying From Explorer Opens The Deploy With All Resources in Folder Already Checked
	Given I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy FromExplorerContextMenu
	And I Filter Deploy Source With "Unit Tests"
	Then Filtered Resourse Is Checked For Deploy
	And I Click Close Deploy Tab Button

Scenario: Cancel Deploy Returns to Deploy Tab
	Given I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy FromExplorerContextMenu
	And I Click Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server New Remote Server Item
	And I Click Deploy Tab Destination Server Connect Button
	Then Deploy Button Is Enabled
	When I Click Deploy Ribbon Button
	Then Deploy Version Conflict Window Shows
	And I Click MessageBox Cancel
	And Deploy Window Is Still Open

Scenario: Deploy Disconnect Clears Destination
	Given I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy FromExplorerContextMenu
	And I Click Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server New Remote Server Item
	And I Click Deploy Tab Destination Server Connect Button
	Then Deploy Button Is Enabled
	When I Click Deploy Ribbon Button
	Then Deploy Version Conflict Window Shows
	And I Click MessageBox Cancel
	And Deploy Window Is Still Open
	Then I Click Deploy Tab Destination Server Connect Button
	And Destination Deploy Information Clears
