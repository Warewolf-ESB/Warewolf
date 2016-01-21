﻿Feature: PluginConnector
	In order to use .net dlls
	As a warewolf user
	I want to be able to create plugin services

# Layout of tool not yet available
@ignore
Scenario: Opening Plugin Service Connector tab
	Given I open New Plugin Service Connector
	And "New Plugin Connector" tab is opened
	And Select a source is focused
	And "1 Select a Source" is "Enabled"
	And "2 Select a Namespace" is "Disabled"
	And "3 Select an Action" is "Disabled" 
	And "4 Provide Test Values" is "Disabled" 
	And "Test" is "Disabled"
	And "Save" is "Disabled"
    And "5 Defaults and Mapping" is "Disabled" 
	And input mappings are
	| Input | Default Value | Required Field | Empty Null |
	Then output mappings are
	| Output | Output Alias |

@ignore
Scenario: Create new Plugin Source
	Given I open New Plugin Tool
	Then  "Sources" combobox is enabled
	And  Selected Source is null
	And Selected Namespace is Null
	And Selected Method is Null
	And Inputs are
	| Input   | Default Value | Required Field | Empty Null |
	And Outputs are
	| Output | Output Alias |
	And aa
	And there are "no" validation errors of "" 
@ignore
Scenario: Creating Plugin Service by selecting existing source
	Given I open New Plugin Service Connector
	And "New Plugin Connector" tab is opened
	When I select "testingPluginSrc" as source
	And "2 Select a Namespace" is "Enabled"
	And "3 Select an Action" is "Disabled" 
	When I select "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "Select an action" is "Enabled"
	When I select "DummySent" as action
	And "4 Provide Test Values" is "Enabled" 
	And "Test" is "Enabled"
	When "Test" is clicked
	And Test Connection is "Successful"
	Then "5 Default and Mapping" is "Enabled" 
	And "Save" is "Enabled"
	And input mappings are
	| Input   | Default Value | Required Field | Empty Null |
	| data    |               | Selected       | Selected   |
	Then output mappings are
	| Output | Output Alias |
	| Name   | Name         |
	When "Save" is clicked
	Then the Save Dialog is opened
	
@ignore
Scenario: Opening saved Plugin Service 
	Given I open "IntegrationTestPluginNull" 
	And "IntegrationTestPluginNull" tab is opened
	And "testingPluginSrc" is selected as source
	And "2 Select a Namespace" is "Enabled"
	And "3 Select an Action" is "Enabled"
	And "4 Provide Test Values" is "Enabled"
	And "5 Default and Mapping" is "Disabled"   
	And I change the source to "PrimitivePlugintest"
	Then "2 Select a namespace" is "Enabled"
	And "3 Select an Action" is "Disabled"
	And "4 Provide Test Values" is "Disabled"
	And "5 Default and Mapping" is "Disabled" 
	And "Save" is "Disabled" 
    When I select "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "3 Select an Action" is "Enabled"
	When I select "FetchStringvalue" as action
	Then Provide Test Values VM is "Enabled"
	And "Test" is "Enabled"
	When Test Connection is "Successful"
	Then "5 Default and Mapping" is "Enabled" 
	Then "Save" is "Enabled"
	And input mappings are
	| Input | Default Value | Required Field | Empty Null |
	| data  |               |                |            |
	Then output mappings are
	| Output | Output Alias |
	| Name   | Name         |
	When I save as "IntegrationTestPluginNull"
    Then the Save Dialog is opened
    Then title is "IntegrationTestPluginNull"

@ignore
Scenario: Refreshing plugin source action step 
	Given I open "IntegrationTestPluginNull"
	And "IntegrationTestPluginNull" tab is opened
	And "2 Select a Namespace" is "Enabled"
	And "3 Select an Action" is "Enabled"
	And "4 Provide Test Values" is "Enabled"
	And "5 Default and Mapping" is "Disabled"   
	When "Refresh" is clicked
	Then "3 Select an action" is "Enabled" 
	When I select "FetchStringvalue" as action
	And "4 Provide Test Values" is "Enabled" 
	Then "5 Default and Mapping" is "Disabled" 
	And "Test" is "Enabled"
	When Test Connection is "Successful"
	And "5 Default and Mapping" is "Enabled" 
	Then "Save" is "Enabled"
	And input mappings are
	| Input | Default Value | Required Field | Empty Null |
	| data  |               | Checked        | Checked    |
	Then output mappings are
	| Output | Output Alias |
	| Name   | Name         |
	
	
@ignore
Scenario: Plugin service GetType test
	Given I open New Plugin Service Connector
	When I select "IntegrationTestPluginNull" as source
	And "2 Select a Namespace" is "Enabled"
	When I select "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "3 Select an Action" is "Enabled" 
	And "GetType" is not an Action