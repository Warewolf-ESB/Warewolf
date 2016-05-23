Feature: Get Web Service Tool
	In order to create New Web Get Request Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Get Request.


Scenario: Open new Web Tool
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
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

Scenario: Create Web Service 
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select "Dev2CountriesWebService" as web Source
	Then Header is Enabled
	And  Header is added as
	| Header | Value |
	| [[a]]  | test  |
	And Url is Enabled
	And Query is Enabled
	And Generate Outputs is Enabled
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	And  Variables to test appear as
	| Inputs |   |
	| [[a]]  | 1 |
	When Test Request Variables is Successful
	Then the response is loaded
	When I click Done
	Then Mapping is Enabled 
	And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Done" is "Enabled"	

 Scenario: Editing Web Service Source
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Disabled
	When I Select Dev2CountriesWebService as Source
	Then New is Enabled
	And Edit is Enabled
	When I click Edit
	Then the Dev2CountriesWebService Source tab is opened

Scenario: Adding parameters in request headers is updating variables 
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select "Dev2CountriesWebService" as web Source
	Then Header is Enabled
	And  Url is Visible 
	And  Query is Visible 
	And Generate Outputs is Enabled
	And Query String equals "?extension=[[extension]]&prefix=[[prefix]]"
	And Url as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx" 
	And  Header is added as
	| Header | Value |
	| [[a]]  | test  |
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	And Web Inputs is Enabled
	And Web Test is Enabled
	And Web Paste is Enabled
	When Test Request Variables is Successful
	And I click Done
	Then Mapping is Enabled
		And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Done" is "Enabled"

Scenario: Changing Sources 
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select WebHeloo as Source
	Then Header is Enabled
	And Url is Visible 
	And Generate Outputs is Enabled
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	When Test Request Variables is Successful
	Then Web Outputs appear as
	|                               |
	| {"rec" : [{"a":"1","b":"a"}]} |
	When I click Done
	Then the response is loaded
	And Mapping is Enabled
	And output mappings are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	When I change Source from "WebHeloo"  to "Dev2CountriesWebService" 
	Then Header is Enabled
	And Url is Enabled 
	And Generate Outputs is Enabled
	And Mappings is Disabled

#wolf-1034 re-opened worked as a connector not as a tool
Scenario: Web Connector Tool returns text
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
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

 Scenario: Web Connector Tool cancel generate outputs for 
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select WebHeloo as Source
	Then Header is Enabled
	And Url is Visible 
	Then the Generate Outputs window is opened
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	And I click Test
	Then Web Outputs appear as
	|                               |
	| {"rec" : [{"a":"1","b":"a"}]} |
	When I click Cancel
	Then the response is loaded
	And Mapping is Disabled	

 #Wolf-1412
Scenario: Web Connector Tool re-generate outputs for 
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select WebHeloo as Source
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	When Test Request Variables is Successful
	Then Web Outputs appear as
	|                               |
	| {"rec" : [{"a":"1","b":"a"}]} |
	When I click Done
	Then the response is loaded
	And Mapping is Enabled
	And output mappings are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	And Recordset Name equals rec
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	And I click Cancel
	And Mapping is Enabled
	And output mappings are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |

Scenario: Web Connector Tool remember previous values
	Given I open New Workflow
	And I drag Web Get Request Connector Tool onto the design surface
	Then Source is Enabled
    And New is Enabled
	And Edit is Enabled
	When I Select WebHeloo as Source
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	When I click Generate Outputs
	Then the Generate Outputs window is opened
	When Test Request Variables is Successful
	Then Web Outputs appear as
	|                               |
	| {"rec" : [{"a":"1","b":"a"}]} |
	When I click Done
	Then the response is loaded
	And Mapping is Enabled
	And output mappings are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	And Recordset Name equals rec
	When I change Source from "WebHeloo"  to "Dev2CountriesWebService" 
	Then Header is Enabled
	And  Url is Visible 
	And Generate Outputs is Enabled
	And Mappings is Disabled
	When I change Source from "Dev2CountriesWebService"  to "WebHeloo" 
	Then Header is Enabled
	And  Url is Visible 
	And Mapping is Enabled
	And output mappings are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |

