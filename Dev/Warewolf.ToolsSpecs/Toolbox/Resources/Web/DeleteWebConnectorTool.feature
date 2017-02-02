Feature: Delete Web Connector Tool
	In order to create New Web Delete Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Delete Request.


Scenario: Open new Deletge Web Tool
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	When I Select "WebHeloo" as a Delete web Source
	Then Delete Header is Enabled
	And  Delete Header appears as
	| Header | Value |
	And Delete Edit is Enabled
	And  Delete Body is Enabled 
	And Delete Url is Visible
	And Delete Query is Enabled
	And Delete Generate Outputs is Enabled
	And Delete mapped outputs are
	| Output | Output Alias |

Scenario: Create Delete Web Service with different methods
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	When I Select "Dev2CountriesWebService" as a Delete web Source
	Then Delete Header is Enabled
	And  Delete Header appears as
	| Header | Value |
	And Delete Body is Enabled 
	And Delete Url is Visible
	And Delete Edit is Enabled
	And Delete Query is Enabled
	And Delete Generate Outputs is Enabled
	When I click Delete Generate Outputs
	Then the Delete Generate Outputs window is shown
	And Delete Variables are Enabled
	When Delete Test Inputs is Successful
	Then the Delete response is loaded
	When I click Delete Done
	Then Delete Mapping is Enabled
	And Delete mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |
	


Scenario: Adding parameters in Delete Post Web Connector Tool request headers is updating variables
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled
	When I Select "Dev2CountriesWebService" as a Delete web Source
	Then Delete Header is Enabled
	And Delete Body is Enabled 
	And Delete Url is Visible
	And Delete Query is Enabled
	And Delete Generate Outputs is Enabled
	And I enter "?extension=[[extension]]&prefix=[[prefix]]" as Delete Query String
	And Delete Url as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx"
	And I add Delete Header as
         | Name  | Value |
         | [[a]] | T     |
	When I click Delete Generate Outputs
	Then the Delete Generate Outputs window is shown
	And Delete Input variables are
	| Name  |
	| [[a]] |
	| [[extension]] |
	| [[prefix]] |
	And Delete Test is Enabled
	And Delete Paste is Enabled
	When Delete Test Inputs is Successful
	And I click Delete Done
	Then Delete Mapping is Enabled
    And Delete mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |

 Scenario: Editing Delete Web Service
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled
	When I Select "Dev2CountriesWebService" as a Delete web Source
	And Delete New is Enabled
	And Delete Edit is Enabled
	When I click Delete Edit
	Then the "Dev2CountriesWebService" Delete Source tab is opened

Scenario: Changing Delete Web Connector Tool Sources 
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled
	When I Select "WebHeloo" as a Delete web Source
	Then Delete Header is Enabled
	And Delete Body is Enabled 
	And Delete Url is Visible
	And Delete Query is Enabled
	And Delete Generate Outputs is Enabled
	And I click Delete Generate Outputs
	Then the Delete Generate Outputs window is shown
	When Delete Test Inputs is Successful
	Then Delete Response appears as "{"rec" : [{"a":"1","b":"a"}]}"
	When I click Delete Done
	Then Delete Mapping is Enabled
	And Delete mapped outputs are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	When I Select "Google Address Lookup" as a Delete web Source
	Then Delete Header is Enabled
	And Delete Body is Enabled 
	And Delete Url is Visible
	And Delete Query is Enabled
	And Delete Generate Outputs is Enabled
	And Delete Mappings is Disabled

	
Scenario: Delete Web Connector Tool returns text
	And I drag Web Delete Request Connector Tool onto the design surface
    And Delete New is Enabled	
	When I Select "TestingReturnText" as a Delete web Source
	Then Delete Header is Enabled
	And  Delete Url is Visible 
	And Delete Generate Outputs is Enabled
	And I click Delete Generate Outputs
	Then the Delete Generate Outputs window is shown
	When Delete Test Inputs is Successful
	And I click Delete Done
	Then Delete Mapping is Enabled
	And Delete mapped outputs are
	| Output | Output Alias |
	| a      | [[rec().a]]  |
