@WebService
Feature: WebService
	In order to create New Web Service in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Service.

## @ New Web Service Requirements
##* Ensure New Web Service tab is opened when click on 'New Webservice' button.
##* Ensure Step 1 is visible when 'New Webservice' tab opened.
##* Ensure Step 2,3,4,5,6,7 is Disabled when 'New Webservice' tab opened.
##* Ensure user is able to select method for the source
##* Ensure user is able to selects "GET" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "POST" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "PUT" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "DELETE" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "TRACE" as 'Request Method' in 'New Webservice' tab
##* Ensure when user selcts "GET" as 'Request Method' then Step 4 is disabled
##* Ensure when user selcts "POST" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "PUT" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "DELETE" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "TRACE" as 'Request Method' then Step 4 is Enabled
##* Ensure user is able to 'Select Source' in 'New Webservice' tab.
##* Ensure Step 2,3,3,5,6 is enabled when user selects source in 'New Web Service' tab
##* Ensure Step 7 is disabled when user selects source in 'New Web Service' tab
##* Ensure user is able to create New Source by clcking on 'New' button.
##* Ensure Edit button is Disabled when source is not selected.
##* Ensure Edit button is Enabled when source is selected.
##* Ensure Step 2 is enabled when Source is selected in step 1
##* Ensure 'Test' button is enabled when user selects source in Step 1
##* Ensure service is testing when user clicks on test button
##* Ensure Step 7 is enabled when response loaded after click on test
##* Ensure Step 7 is enabled when user edit the response in response dialog
##* Ensure user save button is Enabled when the user click on test
## Ensure adding parameters to Step 2 adds them as inputs in Step 5
## Ensure changing parameters to Step 2 changes them in Step 5
## Ensure adding parameters to Step 3 adds them as inputs in Step 5
## Ensure changing parameters to Step 3 changes them in Step 5
## Ensure adding parameters to Step 4 adds them as inputs in Step 5
## Ensure changing parameters to Step 4 changes them in Step 5
## Ensure changing Source in Step 1 changes Request URL in Step 3

Scenario: Opening Web Service
	Given I click New Web Service Connector
	Then "New Web Service" tab is opened
	And Select Request Method & Source is focused
	And "New" is "Enabled"
	And "1 Select Request Method & Source" is "Enabled"
	And "Edit" is "Disabled"
	And "2 Request headers" is "Disabled"
	And "3 Request URL" is "Disabled" 
	And "4 Request Body" is "Disabled" 
	And "5 Request Variables" is "Disabled" 
	And "Test" is "Disabled"
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Disabled"
	And "Save" is "Disabled"
	
	
Scenario: Creating Web Service
	Given I click New Web Service Connector
	Then "New Web Service" tab is opened	
	When I select "GET" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Disabled" 
	And "5 Request Variables" is "Enabled" 
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	When I Test Connection
	Then response is loaded
	And "Save" is "Disabled"
	When Test Connecton is "Successful"
	Then input mappings are
	| Input        | Default Value | Required Field | Empty is Null |
	| Country Name | US            |                |               |
	And output mappings are
	| Output                           | Output Alias                                       |
	| Pr_CitiesGetCountriesCountryID   | DocumentElement().Pr_CitiesGetCountriesCountryID   |
	| Pr_CitiesGetCountriesDescription | DocumentElement().Pr_CitiesGetCountriesDescription |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened

	
	
Scenario: Creating Web Service with method as POST
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "POST" as Method
	And I select "Dev2CountriesWebService" as data source
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Enabled" 
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	When I Test Connection
	Then response is loaded
	Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension | json          |                |               |
	| prefix    | a             |                |               |
	And output mappings are
	| Output      | Output Alias                   |
	| CountryID   | UnnamedArrayData().CountryID   |
	| Description | UnnamedArrayData().Description |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened
	
	
Scenario: Creating Web Service with method as PUT
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "PUT" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Enabled" 
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	When I Test Connection
	Then response is loaded
	Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output      | Output Alias                   |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened 
	
Scenario: Creating Web Service with method as DELETE
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "DELETE" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Enabled" 
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	When I Test Connection
	Then response is loaded
	Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output      | Output Alias                   |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened	
		
Scenario: Creating Web Service with method as TRACE
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "TRACE" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Enabled" 
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
	And "Save" is "Disabled"
	When I Test Connection
	Then response is loaded
	Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output      | Output Alias                   |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened		
		 
 Scenario: Editing Web Service
	Given I open "GetCountries" webservice
	Then "Edit Web Service - GetCountries" tab is opened	
	And method is selected as "GET"
	And "Dev2CountriesWebService" selected as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Disabled" 
	And "5 Request Variables" is "Enabled" 
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	When I Test Connection
	Then response is loaded
	And "Save" is "Disabled"
	When Test Connecton is "Successful"
	Then input mappings are
	| Input        | Default Value | Required Field | Empty is Null |
	| Country Name | US            |                |               |
	And output mappings are
	| Output                           | Output Alias                                       |
	| Pr_CitiesGetCountriesCountryID   | DocumentElement().Pr_CitiesGetCountriesCountryID   |
	| Pr_CitiesGetCountriesDescription | DocumentElement().Pr_CitiesGetCountriesDescription |
	And "Save" is "Enabled" 
	When I save the webservice
	Then Save Dialog is opened

 
Scenario: Adding perameters in request headers is updating variables 
	Given I click "New Web Service Connector" webservice
	Then "New Web Service" tab is opened	
	When I select "GET" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	When I type "Request Headers" as
	|          |
	| [[test]] |
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Disabled" 
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	| [[test]]         |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
    Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension | json          |                |               |
	| prefix    | a             |                |               |
	| test      |               |                |               |
	
 
 
 Scenario: Adding perameters in request URL 
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "GET" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And request URL has "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx" and "?extension=[[extension]]&prefix=[[prefix]]"
	When I change request url to "?extension=[[variable1]]&prefix=[[variable2]]"
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                 |
	| [[variable1]] = |
	| [[variable2]] =      |
	And "6 Response" is "Disabled" 
	And "7 Edit Default and Mapping Names" is "Enabled"
	And "Test" is "Enabled"
    Then input mappings are
	| Input         | Default Value | Required Field | Empty is Null |
	| [[variable1]] | json          |                |               |
	| [[variable2]]      | a             |                |               |

	
 
 
 
 Scenario: Adding variables at request body
	Given I click "New Web Service Connector"
	Then "New Web Service" tab is opened	
	When I select "DELETE" as Method
	And I select "Dev2CountriesWebService" as data source
	And "New" is "Enabled"
	And "Edit" is "Enabled"
	Then "2 Request headers" is "Enabled"
	And "3 Request URL" is "Enabled" 
	And "4 Request Body" is "Enabled" 
	When I type "Request Body" as
	|       |
	| [[a]] |
	| [[b]] |
	And "5 Request Variables" is "Enabled" 
	And "5 Request Variables" are
	|                  |
	| extension = json |
	| prefix  = a      |
	| [[a]]            |
	| [[b]]            |
	And "6 Response" is "Disabled" 
	Then input mappings are
	| Input     | Default Value | Required Field | Empty is Null |
	| extension |               |                |               |
	| prefix    |               |                |               |
	| a         |               |                |               |
	| b         |               |                |               |
	And output mappings are
	| Output      | Output Alias                   |
	And "Save" is "Enabled" 
	
 
 
 
 
 
 
 
 
 
 
 
 
 
 
  