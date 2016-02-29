Feature: Post Web Connector Tool
	In order to create New Web Post Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Post Request.

# layout of tool not ready

Scenario: Open new Web Tool
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "WebHeloo" as a web Source
	Then Header is Enabled
	And  Header appears as
	| Header | Value |
	And Edit is Enabled
	And  Body is Enabled 
	And Url is Visible
	And Query is Enabled
	And Generate Outputs is Enabled
	And mapped outputs are
	| Output | Output Alias |

Scenario: Create Web Service with different methods
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "Dev2CountriesWebService" as a web Source
	Then Header is Enabled
	And  Header appears as
	| Header | Value |
	And Body is Enabled 
	And Url is Visible
	And Edit is Enabled
	And Query is Enabled
	And Generate Outputs is Enabled
	When I click Generate Outputs
	Then the Generate Outputs window is shown
	And Variables are Enabled
	When Test Inputs is Successful
	Then the response is loaded
	When I click Done
	Then Mapping is Enabled
	And mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |
	


Scenario: Adding parameters in request headers is updating variables 
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	When I Select "Dev2CountriesWebService" as a web Source
	Then Header is Enabled
	And Body is Enabled 
	And Url is Visible
	And Query is Enabled
	And Generate Outputs is Enabled
	And I enter "?extension=[[extension]]&prefix=[[prefix]]" as Query String
	And Url as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx"
	And I add Header as
         | Name  | Value |
         | [[a]] | T     |
	When I click Generate Outputs
	Then the Generate Outputs window is shown
	And Input variables are
	| Name  |
	| [[a]] |
	| [[extension]] |
	| [[prefix]] |
	And Test is Enabled
	And Paste is Enabled
	When Test Inputs is Successful
	And I click Done
	Then Mapping is Enabled
    And mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |

 Scenario: Editing Web Service
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	When I Select "Dev2CountriesWebService" as a web Source
	And New is Enabled
	And Edit is Enabled
	When I click Edit
	Then the "Dev2CountriesWebService" Source tab is opened

Scenario: Changing Sources 
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	When I Select "WebHeloo" as a web Source
	Then Header is Enabled
	And Body is Enabled 
	And Url is Visible
	And Query is Enabled
	And Generate Outputs is Enabled
	And I click Generate Outputs
	Then the Generate Outputs window is shown
	When Test Inputs is Successful
	Then Response appears as "{"rec" : [{"a":"1","b":"a"}]}"
	When I click Done
	Then Mapping is Enabled
	And mapped outputs are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	When I Select "Google Address Lookup" as a web Source
	Then Header is Enabled
	And Body is Enabled 
	And Url is Visible
	And Query is Enabled
	And Generate Outputs is Enabled
	And Mappings is Disabled


@ignore
#wolf-1034
Scenario: Web Connector Tool returns text
	Given I open New Workflow
	And I drag Web Post Request Connector Tool onto the design surface
    And New is Enabled
	And Edit is Enabled
	When I Select "TestingReturnText" as a web Source
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	And I click Generate Outputs
	Then the Generate Outputs window is shown
	When Test Inputs is Successful
	And I click Done
	Then Mapping is Enabled
	And mapped outputs are
	| Output   | Output Alias |
	| Response | Response     |
