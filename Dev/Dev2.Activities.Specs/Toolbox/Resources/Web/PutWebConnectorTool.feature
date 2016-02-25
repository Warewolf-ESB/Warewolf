Feature: Put Web Connector Tool
	In order to create New Web Put Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Put Request.

#Tool Not created. Wolf-1416


# layout of tool not ready
@ignore
Scenario: Open new Web Tool
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select WebHeloo as Source
	And Request header is enabled
	And Request Url is enabled
	And Generate Outputs is enabled
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	

#Wolf-1416
@ignore
Scenario: Create Web Service with different methods
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select Dev2CountriesWebService as Source
	Then Header is Enabled
	And  Header appears as
	| Header | Value |
	And  Body is Enabled 
	And Url is Enabled
	And Query is Enabled
	And Generate Outputs is Enabled
	When I click "Generate Outputs"
	Then the Generate Outputs window is opened
	And Variables is Enabled
	When Test Variables is Successful
	Then the response is loaded
	When I click Done
	Then Mapping is Enabled
	And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Done" is "Enabled"
	
#Wolf-1416	
@ignore
 Scenario: Editing Web Service
	Given I open New Workflow
	And I drag Web Put Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select Dev2CountriesWebService as Source
	And New is Enabled
	And Edit is Enabled
	When I click Edit
	Then the Dev2CountriesWebService Source tab is opened

#Wolf-1416
 @ignore
Scenario: Adding parameters in request headers is updating variables 
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select Dev2CountriesWebService as Source
	Then Header is Enabled
	And Body is Visible 
	And Url is Visible 
	And Query is Visible 
	And Generate Outputs is Enabled
	And Query String equals ?extension=[[extension]]&prefix=[[prefix]]
	And Url as http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx 
	And I edit the Header as
         | name  | Value |
         | [[a]] | T     |
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	And Inputs is Enabled
	And Test is Enabled
	And Paste is Enabled
	And I Paste into Response
	When Test Inputs is Successful
	And I click Done
	Then Mapping is Enabled
    Then service input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension | json          |                |               |
	| prefix    | a             |                |               |
	| [[a]]     | T             |                |               |

 #Wolf-1416	
# Is this still relevant because nothing will change
 @ignore
Scenario: Changing Sources 
	Given I open New Workflow
	And I drag Web Put Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select Dev2CountriesWebService as Source
	And  Body is Visible 
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	When I change Source from Dev2CountriesWebService  to Google Address Lookup 
	And  Body is Visible 
	Then Header is Enabled
	And Url is Enabled 
	And Generate Outputs is Enabled
	And Mappings is Disabled 

#Wolf-1416
@ignore
#wolf-1034
Scenario: Web Connector Tool returns text
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select TestingReturnText as Source
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	And I click Generate Outputs
	Then Generate Outputs window is Enabled
	And Variables is Enabled 
	And I click Test
	When Test is Successful
	And Outputs is Enabled 
	Then the response is loaded
	And I click Done
	And Mapping is Enabled
	And output mappings are
	| Output   | Output Alias |
	| Response | Response     |