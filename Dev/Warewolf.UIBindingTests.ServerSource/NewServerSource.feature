@ServerSource
Feature: NewServerSource
	In order to connect to other Warewolf servers
	As a Warewolf user
	I want to be able to manager connections to Warewolf servers

@ServerSource
Scenario: Create New Server Source
	Given I open New Server Source
	Then "New Server Source" tab is opened
	And selected protocol is "https" 
	And server port is "3143" 
	And Authentication Type as "Windows"
	And "Test" is "Disabled"
	And "Save" is "Disabled"
	Then validation message is ""

@ServerSource
Scenario: Creating New Source as windows
	Given I open New Server Source	
	And I type Server as "SANDBOX-1"
	And "Test" is "Enabled"
	And I select protocol as "http"
	And I enter server port as "3142" 
	And "Save" is "Disabled"
	And Authentication Type as "Windows"
    Then server Username field is "Collapsed"
    And server Password field is "Collapsed"
   	When I Test Connection to remote server
	When Test Connecton is "Passed"
	Then validation message is "Passed"
	Then "Save" is "Enabled"
	When I save the server source
	Then the save dialog is opened

@ServerSource
Scenario: Creating New Source as User And HTTPS
	Given I open New Server Source
	And I type Server as "SANDBOX-1"
	And I select protocol as "https"
	And I enter server port as "3143" 
	And "Save" is "Disabled"
	And Authentication Type as "User"
	Then server Username field is "Visible"
	And server Password field is "Visible"
	And "Test" is "Disabled"
	And "Save" is "Disabled"
	When I enter Username as "IntegrationTester"
	And I enter Password as "I73573r0"
	When I Test Connection to remote server
	When Test Connecton is "Passed"
	And "Save" is "Enabled"
	When I save the server source
	Then the save dialog is opened

@ServerSource
Scenario: Creating server source Authentication error
	Given I open New Server Source
	And I type Server as "SANDBOX-1"
	And I select protocol as "http"
    And I enter server port as "3142" 
    And "Save" is "Disabled"
	And Authentication Type as "User"
	When I enter Username as "#$##$"
	And I enter Password as "I73573r0"
	When I Test Connection to remote server
	Then Test Connecton is "Failed"
	And validation message is "Connection Error: Unauthorized"
	And "Save" is "Disabled"

@ServerSource
Scenario: Creating server source Authentication error Shows correct error message
	Given I open New Server Source
	And I type Server as "SANDBOX-1"
	And I select protocol as "http"
    And I enter server port as "3142" 
    And "Save" is "Disabled"
	And Authentication Type as "User"
	When I enter Username as "#$##$"
	And I enter Password as "I73573r0"
	When I Test Connection to remote server
	Then Test Connecton is "Failed"
	And validation message is "Connection Error: Unauthorized"
	And "Save" is "Disabled"
	And the error message is "Connection Error: Unauthorized"

@ServerSource
Scenario: Creating New Source as Public
	Given I open New Server Source
	And I type Server as "SANDBOX-1"
	And I select protocol as "http"
	And I enter server port as "3142" 
	And "Save" is "Disabled"
	And Authentication Type as "Public"
	Then server Username field is "Collapsed"
	And server Password field is "Collapsed"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	When I Test Connection to remote server
	When Test Connecton is "Passed"
	And "Save" is "Enabled"
	When I save the server source
	Then the save dialog is opened
  
@ServerSource
Scenario: Editing Saved Server Source Authentication 
	Given I open "ServerSource" server source
	Then "ServerSource" tab is opened
	And Server as "SANDBOX-1"
	And selected protocol is "https"
	And server port is "3143" 
	And Authentication Type selected is "User"
	Then server Username field is "Visible"
	And server Password field is "Visible"
	And server Username is "Integrationtester"
	And server Password is is "I73573r0"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	And Authentication Type as "Public"
	Then server Username field is "Collapsed"
	And server Password field is "Collapsed"
	And "Test" is "Enabled"
	When I Test Connection to remote server
	When Test Connecton is "Passed"
	Then tab name is "ServerSource *"
	And "Save" is "Enabled"
	Then I save the server source


@ServerSource
Scenario: Creating New Source as windows with external server address
	Given I open New Server Source	
	And I type Server as "test-warewolf.cloudapp.net"
	And "Test" is "Enabled"
	And I select protocol as "http"
	And I enter server port as "3142" 
	And "Save" is "Disabled"
	And Authentication Type as "Public"
    Then server Username field is "Collapsed"
    And server Password field is "Collapsed"
   	When I Test Connection to remote server
	When Test Connecton is "Passed"
	Then validation message is "Passed"
	Then "Save" is "Enabled"
	When I save the server source
	Then the save dialog is opened

@ServerSource
Scenario: Editing Saved Server Source Authentication with external server address 
	Given I open "TestWarewolf" server source
	Then "TestWarewolf" tab is opened
	And Server as "test-warewolf.cloudapp.net"
	And selected protocol is "http"
	And server port is "3142" 
	And Authentication Type selected is "Public"
	Then server Username field is "Collapsed"
	And server Password field is "Collapsed"
	And "Test" is "Enabled"
	And "Save" is "Disabled"