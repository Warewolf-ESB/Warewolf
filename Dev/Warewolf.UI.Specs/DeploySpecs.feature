@Deploy 
Feature: DeploySpecs

Scenario: Refresh does not uncheck all resources
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select LocalhostConnected From Deploy Tab Destination Server Combobox
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
    And I Select localhost checkbox from the source tab 
	Then Source explorer first item is checked
	When I Click Deploy Tab Source Refresh Button
	Then Source explorer first item is checked

Scenario: Deploy is enabled when I change server after validation thrown
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select LocalhostConnected From Deploy Tab Destination Server Combobox
	And I filter for "Hello world" on the source filter
	Then Deploy Button is enabled  "false"
	When The deploy validation message is "Source and Destination cannot be the same."
	And I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox
	And I Select "Hello world" from the source tab 
	Then Deploy Button is enabled  "true"

Scenario: Cancel Deploy Returns to Deploy Tab
	Given The Warewolf Studio is running
	When I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	And I Select Deploy From Explorer Context Menu
	And I Click Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server Remote Connection Integration Item
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
	And I Collapse Localhost
	And I Select RemoteConnectionIntegration From Explorer
	And First Remote Server has loaded
	And I Filter the Explorer with "ResourceToDeployInADifferentPath"
	And I Refresh Explorer
	Then First remote Item should exist

Scenario: Changing Selected Server On Deploy Source While Connected To Remote Server On the Explorer
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
	And I Select RemoteConnectionIntegration From Explorer
	And I Select localhost From Deploy Tab Source Server Combobox

