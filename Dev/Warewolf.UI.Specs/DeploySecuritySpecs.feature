@DeploySecurity
Feature: DeploySecuritySpecs

Scenario: Changing Authentication Type of Resource And Save Keeps the Changes
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
	And I Click Deploy Tab Source Server Edit Button
	And I change Server Authentication type and validate

Scenario: Changing Server AuthenticationType from Deploy And Save Edit Server From Explorer Has Changes
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	And I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox
	And I Click Deploy Tab Source Server Edit Button
	And I change Server Authentication From Deploy And Validate Changes From Explorer

Scenario: Changing Resource Permissions From Explorer Deploy Shows Changes
	Given The Warewolf Studio is running
	When I Click Deploy Ribbon Button
	Given I setup Public Permissions for "ResourceWithViewAndExecutePerm" for localhost
	And I setup Public Permissions for "ResourceWithViewAndExecutePerm" for Remote Server
	When I Select Remote Connection Integration From Explorer
	And I Click Edit Server Button From Explorer Connect Control
	And I Change Permissions For Resource "ResourceWithViewAndExecutePerm" and Validate

Scenario: Changing Connected Server Source Auth And Reconnect
	Given I Connect To Server With Changed Auth
	When I Click Edit Server Button From Explorer Connect Control
	Given Server Source Wizard Tab Test Button Is Enabled
	And Server Source Wizard Tab Test Button Is Enabled
	When I Click Server Source Wizard Test Connection Button For Valid Server Source
	And I Click Save Ribbon Button
	Then I Connect To Server With Changed Auth
	When I Click Edit Server Button From Explorer Connect Control
	And I Select Server Authentication Windows
	Then Server Source Wizard Tab Test Button Is Enabled
	When I Click Server Source Wizard Test Connection Button For Valid Server Source
	And I Click Save Ribbon Button
	Then I Connect To Server With Changed Auth
