@Deploy 
Feature: DeploySpecs

Scenario: Select All resources to deploy
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select LocalhostConnected From Deploy Tab Destination Server Combobox
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I filter for "DateTime" on the source filter
	And I Select localhost from the source tab 
	And Deploy Button is enabled  "true"

Scenario: Deploy is enabled when I change server after validation thrown
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select LocalhostConnected From Deploy Tab Destination Server Combobox
	And I filter for "Hello world" on the source filter
	And Deploy Button is enabled  "false"
	And The deploy validation message is "Source and Destination cannot be the same."
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I Select "Hello world" from the source tab 
	And Deploy Button is enabled  "true"

Scenario: Cancel Deploy Returns to Deploy Tab
	Given The Warewolf Studio is running
	When I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy From Explorer Context Menu
	And I Click Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server Remote Connection Intergration Item
	Then Deploy Button Is Enabled
	When I Click Deploy Tab Deploy Button And Cancel
	Then The Deploy Tab is visible

Scenario: Deploy Conflicting Resource With Resource In A Different Path
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I Enter "ResourceToDeployInADifferentPath" Into Deploy Source Filter
	And I Select Deploy First Source Item
	And I Click Deploy Tab Deploy Button
	And I Select Remote Connection Integration From Explorer
	And I Filter the Explorer with "ResourceToDeployInADifferentPath"
	Then First remote Item should be "ResourceToDeployInADifferentPath"

Scenario: Changing Selected Server On Deploy Source While Connected To Remote Server On the Explorer
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
	And I Select Remote Connection Integration From Explorer
	And I Select localhost From Deploy Tab Source Server Combobox

Scenario: Deploy From RemoteConnection
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
    And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
    And Resources is visible on the tree
	And I Select "Hello World" from the source tab 
	And I Click Deploy button
