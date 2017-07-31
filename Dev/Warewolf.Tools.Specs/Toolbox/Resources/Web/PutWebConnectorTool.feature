@Resources
Feature: Put Web Connector Tool
	In order to create New Web Put Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Put Request.

# layout of tool not ready

Scenario: Open new Put Web Tool
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled
	And Put Edit is Disabled
	When I Select "WebHeloo" as a Put web Source
	Then Put Header is Enabled
	And  Put Header appears as
	| Header | Value |
	And Put Edit is Enabled
	And  Put Body is Enabled 
	And Put Url is Visible
	And Put Query is Enabled
	And Put Generate Outputs is Enabled
	And Put mapped outputs are
	| Output | Output Alias |

Scenario: Create Put Web Service with different methods
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled
	And Put Edit is Disabled
	When I Select "Dev2CountriesWebService" as a Put web Source
	Then Put Header is Enabled
	And  Put Header appears as
	| Header | Value |
	And Put Body is Enabled 
	And Put Url is Visible
	And Put Edit is Enabled
	And Put Query is Enabled
	And Put Generate Outputs is Enabled
	When I click Put Generate Outputs
	Then the Put Generate Outputs window is shown
	And Put Variables are Enabled
	When Put Test Inputs is Successful
	Then the Put response is loaded
	When I click Put Done
	Then Put Mapping is Enabled
	And Put mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |
	


Scenario: Adding parameters in Put Web Connectgor Tool request headers is updating variables
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled
	When I Select "Dev2CountriesWebService" as a Put web Source
	Then Put Header is Enabled
	And Put Body is Enabled 
	And Put Url is Visible
	And Put Query is Enabled
	And Put Generate Outputs is Enabled
	And I enter "?extension=[[extension]]&prefix=[[prefix]]" as Put Query String
	And Put Url as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx"
	And I add Put Header as
         | Name  | Value |
         | [[a]] | T     |
	When I click Put Generate Outputs
	Then the Put Generate Outputs window is shown
	And Put Input variables are
	| Name  |
	| [[a]] |
	| [[extension]] |
	| [[prefix]] |
	And Put Test is Enabled
	And Put Paste is Enabled
	When Put Test Inputs is Successful
	And I click Put Done
	Then Put Mapping is Enabled
    And Put mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |

 Scenario: Editing Put Web Service
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled
	When I Select "Dev2CountriesWebService" as a Put web Source
	And Put New is Enabled
	And Put Edit is Enabled
	When I click Put Edit
	Then the "Dev2CountriesWebService" Put Source tab is opened

Scenario: Changing Put Web Connector Sources 
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled
	When I Select "WebHeloo" as a Put web Source
	Then Put Header is Enabled
	And Put Body is Enabled 
	And Put Url is Visible
	And Put Query is Enabled
	And Put Generate Outputs is Enabled
	And I click Put Generate Outputs
	Then the Put Generate Outputs window is shown
	When Put Test Inputs is Successful
	Then Put Response appears as "{"rec" : [{"a":"1","b":"a"}]}"
	When I click Put Done
	Then Put Mapping is Enabled
	And Put mapped outputs are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	When I Select "Google Address Lookup" as a Put web Source
	Then Put Header is Enabled
	And Put Body is Enabled 
	And Put Url is Visible
	And Put Query is Enabled
	And Put Generate Outputs is Enabled
	And Put Mappings is Disabled
	
#wolf-1034
Scenario: Put Web Connector Tool returns text
	And I drag Web Put Request Connector Tool onto the design surface
    And Put New is Enabled	
	When I Select "TestingReturnText" as a Put web Source
	Then Put Header is Enabled
	And  Put Url is Visible 
	And Put Generate Outputs is Enabled
	And I click Put Generate Outputs
	Then the Put Generate Outputs window is shown
	When Put Test Inputs is Successful
	And I click Put Done
	Then Put Mapping is Enabled
	And Put mapped outputs are
	| Output   | Output Alias |
	| a | [[rec().a]]     |
