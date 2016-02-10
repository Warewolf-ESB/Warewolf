Feature: Post Web Connector Tool
	In order to create New Web Post Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Post Request.

# layout of tool not ready
@ignore
Scenario: Open new Web Tool
	Given I open New Web Service Tool
	Then "Sources" combobox is enabled
	And Selected Source is null
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	And "Request header" is enabled
	And "Request Url" is enabled
	And "Validate" is enabled
	And Outputs are
	| Output | Output Alias |
	And Recordset is ""
	And there are "no" validation errors of "" 
	


@ignore
Scenario: Create Web Service with different methods
	Given I open New Web Service Connector
	Then "Sources" combobox is enabled
	And Selected Source is null
	And I select "Dev2CountriesWebService" as data source
	Then "Request Header" is "Enabled"
	And "Request Url" is "Enabled"
	And "Validate" is "Enabled"
	When I click "Validate"
	Then the "Test" window is opened
	And "Request Variables" is "Enabled" 
	Then "Response Body" is "Enabled" 
	When Test "Request Variables" is "Successful"
	Then the response is loaded
	When I click "Done"
	Then "Mapping" is "Enabled" 
	And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Done" is "Enabled"
	
	
@ignore
 Scenario: Editing Web Service
	Given I open "Dev2GetCountriesWebService" 
	Then "Dev2GetCountriesWebService" tab is opened	
	And method is selected as "Get"
	And "Dev2CountriesWebService" selected as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "Request Header" is "Enabled"
	And "Request Url" is "Enabled" 
	And "4 Response" is "Enabled" 
	Then input mappings are
	| Input | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Save" is "Enabled" 

 @ignore
Scenario: Adding parameters in request headers is updating variables 
	Given I open New Web Service Tool
	Then "Sources" combobox is enabled
	And Selected Source is null
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	And "Request header" is enabled
	And "Request Url" is enabled
	And "Validate" is enabled
	And I select "Dev2CountriesWebService" as "Source"
	And "Query String" equals "?extension=[[extension]]&prefix=[[prefix]]' 
	And "Request URL" as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx" 
	And I edit the "Request Header" as
         | name  | Value |
         | [[a]] | T     |
	When I click "Validate"
	Then "Request Variables" is "Enabled" 
	And "Test" is "Enabled"
	Then "Response Body" is "Enabled" 
	And "Response"  is "Enabled" 
	And I "Paste" into "Request
	When Test "Request Variables" is "Successful"
	And I click "Done"
	Then "Mapping" is "Enabled"
    Then service input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension | json          |                |               |
	| prefix    | a             |                |               |
	| [[a]]     | T             |                |               |

 	
 @ignore
Scenario: Edit Web source
	Given I open New Web Service Tool
	And I select "Dev2CountriesWebService" as "Source"
	And "Edit" is "Enabled"
	And I click "Edit"
	Then "Dev2CountriesWebService" tab is opened

# Is this still relevant because nothing will change
 @ignore
Scenario: Changing Sources 
	Given I click "New Web Service Tool"
	Then "Sources" combobox is enabled
	And Selected Source is null
	And "New" is "Enabled"
	And I select "Dev2CountriesWebService" as "Source"
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "Request header" is "Enabled"
	And "Request Url" is "Enabled" 
	And "Validate" is "Enabled"
	When I change "Source" from "Dev2CountriesWebService"  to "Google Address Lookup" 
	Then "Request header" is "Enabled"
	And "Request Url" is "Enabled" 


@ignore
#wolf-1034
Scenario: Web Connector Tool returns text
	Given I open New Web Service Connector Tool
	And I select "Testing Return Text" as "Source"
    And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "Request header" is "Enabled"
	And "Request Url" is "Enabled" 
	And "Validate" is "Enabled"
	And I click "Validate"
	And "Response Variables" is "Enabled" 
	And "Response Body" is "Enabled" 
	When Test "Response Variables" is "Successful"
	Then the response is loaded
	And I click "Done"
	And "Mapping" is "Enabled"
	And output mappings are
	| Output   | Output Alias |
	| Response | Response     |
