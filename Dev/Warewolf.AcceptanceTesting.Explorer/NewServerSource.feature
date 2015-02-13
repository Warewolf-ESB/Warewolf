Feature: NewServerSource
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@NewServerSource
Scenario: Opening New Server Source
	Given I have New Server Source opened
	And "Textbox" is focussed 
	And prtocall selected as "http" 
	And server port as "3142" 
	And Address textbox is "Visible"
	And Authentication type as
	| Windows  | User | Public |
	| Selected |      |        |
	And the test is "Enabled"
	And the save is "Disabled"
	And the message as "The server connection must be tested with a valid adderess before you can save"


Scenario: Creating New Source as windows
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows  | User | Public |
    | Selected |      |        |
   When I Test Connection
   Then Test Connecton is "Successful"
   And Save is "Enabled"
   When I save the source
   Then the save dialog is opened

Scenario: Windows Test connection is unsuccessfull
   Given I have New Server Source opened
   And I entered "ABSCD" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows  | User | Public |
    | Selected |      |        |
   When I Test Connection
   Then Test Connecton is "UnSuccessful"
   And the validation message as "Connection Error: An error occured while sending the request."
   And Save is "Disabled"
   And the message as "Test Connection"


Scenario: Test connection is unsuccessfull 
   Given I have New Server Source opened
   And I entered "rsaklf@#$" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows  | User | Public |
    | Selected |      |        |
   When I Test Connection
   Then Test Connecton is "UnSuccessful"
   And the validation message as "Connection Error: An error occured while sending the request."
   And Save is "Disabled"
   And the message as "Test Connection"

Scenario: Creating New Source as User
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows | User     | Public |
    |         | Selected |        |
   Then Username field is "Visible"
   And Password field is "Visible"
   And the test is "Enabled"
   And Save is "Disabled"
   And the message as "The server connection must be tested with a valid adderess before you can save" 
   When I type Username as "IntegrationTester"
   And I type Password as "I73573r0"
   When I Test Connection
   Then Test Connecton is "Successful"
   And Save is "Enabled"
   When I save the source
   Then the save dialog is opened

Scenario: User Test Connection is Unssuccesful
   Given I have New Server Source opened
   And I entered "RSAKLF" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows | User     | Public |
    |         | Selected |        |
   Then Username field is "Visible"
   And Password field is "Visible"
   And the test is "Enabled"
   And Save is "Disabled"
   And the message as "The server connection must be tested with a valid adderess before you can save" 
   When I type Username as "IntegrationTester"
   And I type Password as "I73573r0"
   When I Test Connection
   Then Test Connecton is "UnSuccessful"
   And the validation message as "Connection Error: An error occured while sending the request."
   And Save is "Disabled"
   And the message as "Test Connection"

Scenario: Creating server source Authentication error
   Given I have New Server Source opened
   And I entered "RSAKLF" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows | User     | Public |
    |         | Selected |        |
   Then Username field is "Visible"
   And Password field is "Visible"
   And the test is "Enabled"
   And Save is "Disabled"
   And the message as "The server connection must be tested with a valid adderess before you can save" 
   When I type Username as "#$##$"
   And I type Password as "I73573r0"
   When I Test Connection
   Then Test Connecton is "UnSuccessful"
   And the validation message as "Connection Error: Unauthorized"
   And Save is "Disabled"
   And the message as "Test Connection"

Scenario: Creating New Source as Public
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows | User | Public   |
    |         |      | Selected |
   Then Username field is "InVisible"
   And Password field is "InVisible"
   And the test is "Enabled"
   And Save is "Disabled"
   And the message as "The server connection must be tested with a valid adderess before you can save" 
   When I Test Connection
   Then Test Connecton is "Successful"
   And Save is "Enabled"
   When I save the source
   Then the save dialog is opened


Scenario: Connecting to server which has no permissions
   Given I have New Server Source opened
   And I entered "SANDBOX-1" as remote server name
   And I selcted prtocall as "http" from dropdown
   And I server port as "3142" 
   And Save is "Disabled"
   And I Select Authentication Typ as
    | Windows | User | Public   |
    |         |      | Selected |
   Then Username field is "InVisible"
   And Password field is "InVisible"
   And the test is "Enabled"
   And Save is "Disabled"
   And the message as "The server connection must be tested with a valid adderess before you can save" 
   When I Test Connection
   Then Test Connecton is "UnSuccessful"
   And the validation message as "Connection Error: Unauthorized"
   And Save is "Disabled"
   And the message as "Test Connection"












