@NewServerSource
Feature: NewServerSource
	In order to connect to other Warewolf servers
	As a Warewolf user
	I want to be able to manager connections to Warewolf servers

Scenario: Opening New Server Source
	Given I have New Server Source opened
	And Address is focused 
	And selected protocol is "http" 
	And server port is "3142" 
	And Authentication Type as "Windows"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	Then validation message is "The server connection must be tested with a valid adderess before you can save"

Scenario: Creating New Source as windows
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as address
   And I select protocol as "http"
   And I enter server port as "3142" 
   And "Save" is "Disabled"
   And Authentication Type as "Windows"
   When I Test Connection to remote server
   And Connecton to remote server is successful
   Then Save is "Enabled"
   When I save the server source
   Then the save dialog is opened

Scenario: Test connection is unsuccessfull
   Given I have New Server Source opened
   And I entered "ABSCD" as address
   And I select protocol as "http"
   And I enter server port as "3142" 
   And "Save" is "Disabled"
   And Authentication Type as "Windows"
   When I Test Connection to remote server
   Then Connecton to remote server is unsuccessful
   And the validation message is "Connection Error: An error occured while sending the request."
   And "Save" is "Disabled"

Scenario: Creating New Source as User
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as address
   And I select protocol as "http"
   And I enter server port as "3142" 
   And "Save" is "Disabled"
   And Authentication Type as "User"
   Then server Username field is "Visible"
   And server Password field is "Visible"
   And "Test" is "Enabled"
   And "Save" is "Disabled"
   And the message as "The server connection must be tested with a valid username and password before you can save" 
   When I enter Username as "IntegrationTester"
   And I enter Password as "I73573r0"
   When I Test Connection to remote server
   Then Connecton to remote server is successful
   And "Save" is "Enabled"
   When I save the server source
   Then the save dialog is opened

Scenario: Creating server source Authentication error
Given I have New Server Source opened
   And I entered "SANDBOX-1" as address
   And I select protocol as "http"
   And I enter server port as "3142" 
   And "Save" is "Disabled"
   And Authentication Type as "User"
   Then server Username field is "Visible"
   And server Password field is "Visible"
   And "Test" is "Enabled"
   And "Save" is "Disabled"
   And the message as "The server connection must be tested with a valid username and password before you can save" 
   When I enter Username as "#$##$"
   And I enter Password as "I73573r0"
    When I Test Connection to remote server
   Then Connecton to remote server is unsuccessful
   And validation message is "Connection Error: Unauthorized"
   And "Save" is "Disabled"

Scenario: Creating New Source as Public
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as address
   And I select protocol as "http"
   And I enter server port as "3142" 
   And "Save" is "Disabled"
   And Authentication Type as "Public"
   Then server Username field is "InVisible"
   And server Password field is "InVisible"
   And "Test" is "Enabled"
   And "Save" is "Disabled"
   When I Test Connection to remote server
   Then Connecton to remote server is successful
   And "Save" is "Enabled"
   When I save the server source
   Then the save dialog is opened
  
Scenario: Editing Saved Server Source Authentication 
   Given I Open server source "ServerSource"
   And tab is opened with name as "Edit - ServerSource"
   And remote server name is "SANDBOX-1"
   And selected protocol is "http"
   And server port is "3142" 
   And Authentication Type selected is "User"
   Then server Username field is "Visible"
   And server Password field is "Visible"
   And server Username is "Integrationtester"
   And server Password is is "I73573r0"
   And "Test" is "Enabled"
   And "Save" is "Disabled"
   And Authentication Type as "Public"
   Then server Username field is "InVisible"
   And server Password field is "InVisible"
   And "Test" is "Enabled"
   When I Test Connection to remote server
   Then Connecton to remote server is successful
   Then tab name is "*Edit - ServerSource"
   And "Save" is "Enabled"
   When I save the server source
   Then Save Dialog is not opened
   And server source "ServerSource" is saved


