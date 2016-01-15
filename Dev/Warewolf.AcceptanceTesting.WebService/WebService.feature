@WebService
Feature: WebService
	In order to create New Web Service in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Service.

## @ New Web Service Requirements
##* Ensure New Web Service tab is opened when click on 'New Webservice' button.
##* Ensure Step 1 is visible when 'New Webservice' tab opened.
##* Ensure Step 2,3,4,5 is Disabled when 'New Webservice' tab opened.
##* Ensure user is able to select method for the source
##* Ensure user is able to selects "GET" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "POST" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "PUT" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "DELETE" as 'Request Method' in 'New Webservice' tab
##* Ensure user is able to selects "TRACE" as 'Request Method' in 'New Webservice' tab
##* Ensure when user selects "GET" as 'Request Method' then Step 2 is disabled
##* Ensure when user selcts "POST" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "PUT" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "DELETE" as 'Request Method' then Step 4 is Enabled
##* Ensure when user selcts "TRACE" as 'Request Method' then Step 4 is Enabled
##* Ensure user is able to 'Select Source' in 'New Webservice' tab.
##* Ensure Step 2,3 is enabled when user selects source in 'New Web Service' tab
##* Ensure Step 4,5 is disabled when user selects source in 'New Web Service' tab
##* Ensure user is able to create New Source by clcking on 'New' button.
##* Ensure Edit button is Disabled when source is not selected.
##* Ensure Edit button is Enabled when source is selected.
##* Ensure Step 2 is enabled when Source is selected in step 1
##* Ensure 'Test' button is enabled when user selects source in Step 1
##* Ensure service is testing when user clicks on test button
##* Ensure Step 5 is enabled when response loaded after click on test
##* Ensure Step 5 is enabled when user edit the response in response dialog
##* Ensure user save button is Enabled when the user click on test

## Ensure adding parameters to Step 2 adds them as inputs in Step 5
## Ensure changing parameters to Step 2 changes them in Step 5
## Ensure adding parameters to Step 3 adds them as inputs in Step 5
## Ensure changing parameters to Step 3 changes them in Step 5
## Ensure adding parameters to Step 4 adds them as inputs in Step 5
## Ensure changing parameters to Step 4 changes them in Step 5
## Ensure changing Source in Step 1 changes Request URL in Step 3

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
 	
 
Scenario: Edit Web source
	Given I open New Web Service Connector
	And I select "Dev2CountriesWebService" as data source
	And "Edit" is "Enabled"
	And I click "Edit"
	Then "Dev2CountriesWebService" is opened in another tab
 
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


 
 
 
 
 
 
 
  