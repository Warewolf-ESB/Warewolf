Feature: WebConnector
	In order to create New Web Service in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Service.
# layout of tool not ready
@ignore
Scenario: Opening Web Service
	Given I open New Web Service Connector
	Then "New Web Service Connector" tab is opened
	And Select Request Method & Source is focused
	And "New" is "Enabled"
	And "1 Select Request Method and Source" is "Enabled"
	And "Edit" is "Enabled"
	And "2 Request" is "Disabled"
	And "3 Variables" is "Disabled" 
	And "4 Response" is "Disabled" 
	And "5 Defaults and Mapping" is "Disabled" 
	And "Test" is "Disabled"
	And "Save" is "Disabled"
	
@ignore
Scenario Outline: Create Web Service with different methods
	Given I open New Web Service Connector
	Then "New Web Service Connector" tab is opened
	And Select Request Method & Source is focused
	When I select Method "<Method>"
	And I select "Dev2CountriesWebService" as data source
	Then "2 Request" is "Enabled"
	And "2 Request Body" is "<Body>"
	And "3 Variables" is "Enabled" 
	And "Test" is "Enabled"
	When Test Connection is "Successful"
	Then "4 Response" is "Enabled" 
	And the response is loaded
	And "5 Defaults and Mapping" is "Enabled" 
	Then input mappings are
	| Input | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output      | Output Alias |
	| CountryID   | CountryID    |
	| Description | Description  |
	And "Save" is "Enabled"
	And I save as "Testing Web Service Connector"
	Then Save Dialog is opened
	Then title is "Testing Web Service Connector"
	And "Testing Web Service Connector" tab is opened
	Examples:
	| Method  | Body     |
	| Get     | Disabled |
	| Post    | Enabled  |
	| Head    | Enabled  |
	| Put     | Enabled  |
	| Delete  | Enabled  |
	| Trace   | Enabled  |
	| Options | Enabled  |
	
@ignore
 Scenario: Editing Web Service
	Given I open "Dev2GetCountriesWebService" 
	Then "Dev2GetCountriesWebService" tab is opened	
	And method is selected as "Get"
	And "Dev2CountriesWebService" selected as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request" is "Enabled"
	And "3 Variables" is "Enabled" 
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
	Given I open New Web Service Connector
	Then "New Web Service Connector" tab is opened
	When I select "Get" as Method
	And I select "Dev2CountriesWebService" as data source with default query as '?extension=[[extension]]&prefix=[[prefix]]' and URL as "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx" 
	And "New" is "Enabled"
	Then "2 Request" is "Enabled"
	And I edit the headers as
         | name  | Value |
         | [[a]] | T     |
	And "3 Variables" is "Enabled" 
	And "Test" is "Enabled"
	When Test Connection is "Successful"
	Then "4 Response" is "Enabled" 
	And "Paste" tool is "Enabled" 
	When I click the "Paste" tool
	And "5 Defaults and Mapping" is "Enabled"
	And "Save" is "Enabled"
    Then service input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension | json          |                |               |
	| prefix    | a             |                |               |
	| [[a]]     | T             |                |               |
	When I save
	Then Save Dialog is opened
 	
 @ignore
Scenario: Edit Web source
	Given I open New Web Service Connector
	And I select "Dev2CountriesWebService" as data source
	And "Edit" is "Enabled"
	And I click "Edit"
	Then "Dev2CountriesWebService" is opened in another tab
 @ignore
Scenario: Changing Sources 
	Given I click "New Web Service Connector"
	Then "New Web Service Connector" tab is opened
	And Select Request Method & Source is focused
	And "New" is "Enabled"
	And "1 Select Request Method and Source" is "Enabled"
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request" is "Enabled"
	And "3 Variables" is "Enabled" 
	When Test Connection is "Successful"
	Then "4 Response" is "Enabled" 
	And the response is loaded
	And "5 Defaults and Mapping" is "Enabled" 
	And I change data source to "Google Address Lookup" 
@ignore
#wolf-1034
Scenario: Web Connector returns text
	Given I open New Web Service Connector
	And I select "Testing Return Text" as data source
    And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request" is "Enabled"
	And "3 Variables" is "Enabled" 
	When Test Connection is "Successful"
	And "4 Response" is "Enabled" 
	Then the response is loaded
	And "5 Defaults and Mapping" is "Enabled"
	And "Save" is "Enabled"
	When Test Connection is "Successful"
	Then input mappings are
	| Input | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output   | Output Alias |
	| Response | Response     |