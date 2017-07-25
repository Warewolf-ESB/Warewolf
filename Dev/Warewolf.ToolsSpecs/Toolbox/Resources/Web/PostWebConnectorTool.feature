@Resources
Feature: Post Web Connector Tool
	In order to create New Web Post Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Post Request.

# layout of tool not ready

Scenario: Open new Post Web Tool
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled
	And Post Edit is Disabled
	When I Select "WebHeloo" as a Post web Source
	Then Post Header is Enabled
	And  Post Header appears as
	| Header | Value |
	And Post Edit is Enabled
	And Post Body is Enabled 
	And Post Url is Visible
	And Post Query is Enabled
	And Post Generate Outputs is Enabled
	And Post mapped outputs are
	| Output | Output Alias |

Scenario: Create Web Service with different methods
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled
	And Post Edit is Disabled
	When I Select "Dev2CountriesWebService" as a Post web Source
	Then Post Header is Enabled
	And  Post Header appears as
	| Header | Value |
	And Post Body is Enabled
	And Post Url is Visible
	And Post Edit is Enabled
	And Post Query is Enabled
	And Post Generate Outputs is Enabled
	When I click Post Generate Outputs
	Then Post the Generate Outputs window is shown
	And Post Variables are Enabled
	When Post Test Inputs is Successful
	Then Post the response is loaded
	When I click Post Done
	Then Post Mapping is Enabled
	And Post mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |
	


Scenario: Adding parameters in Post Post Web Connector Tool request headers is updating variables
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled
	When I Select "Dev2CountriesWebService" as a Post web Source
	Then Post Header is Enabled
	And Post Body is Enabled 
	And Post Url is Visible
	And Post Query is Enabled
	And Post Generate Outputs is Enabled
	And I enter "?extension=[[extension]]&prefix=[[prefix]]" as Post Query String
	And Post Url as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx"
	And I add Post Header as
         | Name  | Value |
         | [[a]] | T     |
	When I click Post Generate Outputs
	Then Post the Generate Outputs window is shown
	And Post Input variables are
	| Name  |
	| [[a]] |
	| [[extension]] |
	| [[prefix]] |
	And Post Test is Enabled
	And Post Paste is Enabled
	When Post Test Inputs is Successful
	And I click Post Done
	Then Post Mapping is Enabled
    And Post mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |

 Scenario: Editing Post Web Service
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled
	When I Select "Dev2CountriesWebService" as a Post web Source
	And Post New is Enabled
	And Post Edit is Enabled
	When I click Post Edit
	Then the "Dev2CountriesWebService" Post Source tab is opened

Scenario: Changing Post Post Web Connector Tool Sources
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled
	When I Select "WebHeloo" as a Post web Source
	Then Post Header is Enabled
	And Post Body is Enabled 
	And Post Url is Visible
	And Post Query is Enabled
	And Post Generate Outputs is Enabled
	And I click Post Generate Outputs
	Then Post the Generate Outputs window is shown
	When Post Test Inputs is Successful
	Then Post Response appears as "{"rec" : [{"a":"1","b":"a"}]}"
	When I click Post Done
	Then Post Mapping is Enabled
	And Post mapped outputs are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	When I Select "Google Address Lookup" as a Post web Source
	Then Post Header is Enabled
	And Post Body is Enabled 
	And Post Url is Visible
	And Post Query is Enabled
	And Post Generate Outputs is Enabled
	And Post Mappings is Disabled


#wolf-1034 re-opened worked as a connector not as a tool
Scenario: Post Web Connector Tool returns text
	And I drag Web Post Request Connector Tool onto the design surface
    And Post New is Enabled	
	When I Select "TestingReturnText" as a Post web Source
	Then Post Header is Enabled
	And Post Url is Visible 
	And Post Generate Outputs is Enabled
	And I click Post Generate Outputs
	Then Post the Generate Outputs window is shown
	When Post Test Inputs is Successful
	And I click Post Done
	Then Post Mapping is Enabled
	And Post mapped outputs are
	| Output   | Output Alias |
	| a | [[rec().a]]     |
