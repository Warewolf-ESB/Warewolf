Feature: PluginService
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

	I want to be told the sum of two numbers

@PluginService
Scenario: Opening Plugin Service tab
	Given I click "New Data Base Service Connector"
	Then "New Plugin Service" tab is opened
	And "Select a source" is focused
	And "1 Select a source" is "Enabled"
	And "2 Select a namespace" is "Disabled"
	And "3 Select an action" is "Disabled" 
	And "4 Provide Test Values" is "Disabled" 
	And "Test" is "Disabled"
	And "Save" is "Disabled"
    And "5 Edit Dfault and Mapping Names" is "Disabled" 
	And Inputs looks like
	| Input |Default Value|Required Field|Empty Null|
	Then  Outputs looks like
	| Output     | Output Alias |



Scenario: Creating Plugin Service by selecting existing source
    Given I have "New Plugin Service" tab opened
	When I select "testingPluginSrc" as source
	And "2 Select a namespace" is "Enabled"
	And "3 Select an action" is "Disabled" 
	When I selece "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "Select an action" is "loaded"
	When I select "" as action
	And "4 Provide Test Values" is "Enabled" 
	And "Test" is "Enabled"
	| host | port | from | to |
	| test | 23   | 21   | 21 |
	When test connection is "successfull"
	Then Save is "Enabled"
	And "5 Edit Dfault and Mapping Names" is "Enabled" 
	And Inputs looks like
	| Input   | Default Value | Required Field | Empty Null |
	| host    |               | Selected       | Selected   |
	| port    |               | Selected       | Selected   |
	| from    |               | Selected       | Selected   |
	| to      |               | Selected       | Selected   |
	| subject |               | Selected       | Selected   |
	| body    |               | Selected       | Selected   |
	Then  Outputs looks like
	| Output | Output Alias |
	| result | result       |




Scenario: Opening saved Plugin Service 
	Given I have "Edit Plugin Service - IntegrationTestPluginNull" tab opened
	And "testingPluginSrc" is selected as source
	And "Edit" button is "Enabled"
	And "2 Select a namespace" is "Enabled"
	And "3 Select an action" is "Enabled" 
	When I selece "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "Select an action" is "loaded"
	And action is selected as ""
	And "4 Provide Test Values" is "Enabled" 
	And "Test" is "Enabled"
	| host | port | from | to |
	| test | 23   | 21   | 21 |
	When test connection is ""
	Then Save is "Disabled"
	And "5 Edit Dfault and Mapping Names" is "Enabled" 
	And Inputs looks like
	| Input   | Default Value | Required Field | Empty Null |
	| host    |               | Selected       | Selected   |
	| port    |               | Selected       | Selected   |
	| from    |               | Selected       | Selected   |
	| to      |               | Selected       | Selected   |
	| subject |               | Selected       | Selected   |
	| body    |               | Selected       | Selected   |
	Then  Outputs looks like
	| Output | Output Alias |
	| result | result       |
	
	
	
	
Scenario: While editing service, changing plugin source resets all the steps 
	Given I have "Edit Plugin Service - IntegrationTestPluginNull" tab opened
	And "testingPluginSrc" is selected as source
	And "Edit" button is "Enabled"
	And "2 Select a namespace" is "Enabled"
	And "3 Select an action" is "Enabled" 
	When I selece "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "Select an action" is "loaded"
	And action is selected as ""
	And "4 Provide Test Values" is "Enabled" 
	And "Test" is "Enabled"
	When test connection is ""
	Then Save is "Disabled"
	And "5 Edit Dfault and Mapping Names" is "Enabled" 
	When I edit source as "Email Plugin" as source		
	Then "2 Select a namespace" is "Enabled"
	And slect namespace is as "choose"
	And "3 Select an action" is "Disabled" 
	And "4 Provide Test Values" is "Disabled" 
	And "Test" is "Disabled"
	And "Save" is "Disabled"
    And "5 Edit Dfault and Mapping Names" is "Disabled" 
	And Inputs looks like
	| Input |Default Value|Required Field|Empty Null|
	Then  Outputs looks like
	| Output     | Output Alias |
	
	
	
Scenario: Creating Plugin Service by selecting existing source
    Given I have "New Plugin Service" tab opened
	When I select "testingPluginSrc" as source
	And "2 Select a namespace" is "Enabled"
	And "3 Select an action" is "Disabled" 
	When I selece "Unlimited Framework Plugins EmailPlugin" as namespace
	Then "Select an action" is "loaded"
	When I select "" as action
	And "4 Provide Test Values" is "Enabled" 
	And "Test" is "Enabled"
	| host | port | from | to |
	| test | 23   | 21   | 21 |
	When test connection is "Unsuccessfull"
	Then the "Test Result" has validation error "True"
	Then Save is "Disabled"
	And "5 Edit Dfault and Mapping Names" is "Enabled" 
	And Inputs looks like
	| Input   | Default Value | Required Field | Empty Null |
	Then  Outputs looks like
	| Output | Output Alias |
	
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	








  




   




































