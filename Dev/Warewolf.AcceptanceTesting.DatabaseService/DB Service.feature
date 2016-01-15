@DbService
Feature: DB Service
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

@DbService
Scenario: Creating DB Service
	Given I open New DataBase Service Connector
	Then "New DB Connector" tab is opened
	And "Data Source" is focused
	And "1 Data Source" is "Enabled"
	And "2 Select Action" is "Disabled"
	And "3 Test Connector and Calculate Outputs" is "Disabled" 
	And "4 Defaults and Mapping" is "Disabled" 
	And "Save" is "Disabled"
	When I select "DemoDB" as data source
	Then "2 Select Action" is "Enabled"
	When I select "dbo.ImportOrder" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled" 
	And "Test" is "Enabled"
	And inputs are
	| ProductId |
	| 1         |	
	And "4 Defaults and Mapping" is "Disabled" 
	When I test the action
	Then outputs are
	| Recordset Name     | Result |
	| dbo.ImportOrder(1) | 1      |
	And "4 Defaults and Mapping" is "Enabled" 
    And "Save" is "Enabled"
	And input mappings are
	| Inputs    | Required Field | Empty is Null |
	| ProductId |                |               |
	And output mappings are
	| Output | Output Alias | Recordset Name  |
	| result | result       | dbo_ConverToInt |
	When I save
	Then Save Dialog is opened 

@DbService
Scenario: Opening Saved DB Service
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   Then "1 Data Source" is "Enabled"
   And Data Source is focused
   And "DemoDB" is selected as the data source
   Then "2 Select Action" is "Enabled"
   And "dbo.InsertDummyUser" is selected as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And inputs are
   | fname  | lname | username | password | lastAccessDate |
   | Change | Test  | wolf     | Dev      | 10/1/1990      |
   And "Test" is "Enabled"
   And "4 Defaults and Mapping" is "Enabled" 
   And input mappings are
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               |                |               |
	  | lname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
   And output mappings are
	| Output | Output Alias | Recordset Name      |
	| UserID | UserID       | dbo_InsertDummyUser |
   And "Save" is "Disabled"

@DbService
 Scenario: Editing DB Service Mappings
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   Then "1 Data Source" is "Enabled"
   And Data Source is focused
   When "DemoDB" is selected as the data source
   Then "2 Select Action" is "Enabled"
   And "dbo.InsertDummyUser" is selected as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And Inspect Data Connector hyper link is "Visible"
   And "Save" is "Disabled"
   When inputs are
      | fname  | lname | username | password | lastAccessDate |
      | Change | Test  | wolf     | Dev      | 10/1/1990      |
   And "Test" is "Enabled"
   When I test the action
   And "Save" is "Enabled"
   Then outputs are
	  | Recordset Name         | UserID |
	  | ddo_InsertDummyUser(1) |     1   |
   And "4 Defaults and Mapping" is "Enabled" 
   When input mappings are 
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               |                |               |
	  | lname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
   And "Save" is "Enabled"
   And output mappings are
	  | Output | Output Alias | Recordset Name      |
	  | result | result       | dbo_InsertDummyUser |
   When I save
   Then "InsertDummyUser" is saved

@DbService
 Scenario: Editing DB Service and Test Execution is unsuccesful
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   Then "1 Data Source" is "Enabled"
   And Data Source is focused
   When "DemoDB" is selected as the data source
   Then "2 Select Action" is "Enabled"
   And "dbo.InsertDummyUser" is selected as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And Inspect Data Connector hyper link is "Visible"
   And inputs are
   | fname  | lname | username | password | lastAccessDate |
   | Change | Test  | wolf     | Dev      | 10/1/1990      |
   And "Test" is "Enabled"   
   And "Save" is "Disabled"  
   When testing the action fails
   Then "4 Defaults and Mapping" is "Disabled" 
   And input mappings are
	| Inputs         | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output | Output Alias | Recordset Name      |
	And "Save" is "Disabled"

@DbService
Scenario: Refresh in select Action
	Given I click New Data Base Service Connector
	Then "New DB Connector" tab is opened
	And "1 Data Source" is "Enabled"
	And "2 Select Action" is "Disabled"
	And "3 Test Connector and Calculate Outputs" is "Disabled" 
	And "4 Defaults and Mapping" is "Disabled" 
	And "Save" is "Disabled"
	When I select "DemoDB" as data source
	Then "2 Select Action" is "Enabled"
	And "Refresh" is "Enabled"
	When I select "dbo.InsertDummyUser" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled"
	And inputs are
   | fname | lname | username | password | lastAccessDate |
	| Change | Test  | wolf     | Dev | 10/1/1990     |
   When I select Refresh
   Then "3 Test Connector and Calculate Outputs" is "Enabled"
   And "Test" is "Enabled"
   And "Save" is "Disabled"
   When I test the action
	Then outputs are
	| Recordset Name         | UserID |
	| dbo.InsertDummyUser(1) | 1  |
	And "4 Defaults and Mapping" is "Enabled" 
    And "Save" is "Enabled"
	When I save
	Then Save Dialog is opened 

@DbService
Scenario: Changing Actions
	Given I click New Data Base Service Connector
	Then "New DB Connector" tab is opened
	And "1 Data Source" is "Enabled"
	And "2 Select Action" is "Disabled"
	And "3 Test Connector and Calculate Outputs" is "Disabled" 
	And "4 Defaults and Mapping" is "Disabled" 
	And "Save" is "Disabled"
	And Data Source is focused
	When I select "DemoDB" as data source
	Then "2 Select Action" is "Enabled"
	When I select "dbo.InsertDummyUser" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled" 
	And inputs are
	| fname | lname | username | password | lastAccessDate |
	| Change | Test  | wolf     | Dev | 10/1/1990     |
	When I select "dbo.ImportOrder" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled"
	And "Test" is "Enabled"
	And inputs are
	| ProductId |
	| 1         |
	And "4 Defaults and Mapping" is "Disabled" 
	When I test the action
	Then outputs are
	| Recordset Name     | Result |
	| dbo.ImportOrder(1) | 1      |
	And "4 Defaults and Mapping" is "Enabled" 
    And "Save" is "Enabled"
	And input mappings are
	| Inputs    | Required Field | Empty is Null |
	| ProductId |                |               |
	And output mappings are
	| Output | Output Alias | Recordset Name  |
	| result | result       | dbo.ImportOrder |
	When I save
	Then Save Dialog is opened 


	
#WOLF-860 moved to workflow execution

#Scenario: Ensure recordset values can be saved to a variable
#	Given I have a new Workspace opened
#	And I have a saved Data Connector called "MyDataCon"
#	And "MyDataCon" returns [[dbo_GetCountries().CountryID]],[[dbo_GetCountries().RecordName]] and [[dbo_GetCountries().Description]]
#	When I drop "MyDataCon" on the design surface
#	And I open the Database Connector to a large view
#	When I change  [[dbo_GetCountries().Description]] to "[[variable]]"
#	And "MyDataCon" is executed
#	Then the workflow execution has "NO" error     
#	And the debug output is
#	|              |                           |
#	| [[variable]] | Murali,Murali,india,india |
   


#Scenario:  Mixing scalars and recordset input and outputs
#	Given I click "New DataBase Service Connector"
#	Then "New DB Connector" tab is opened
#	And Data Source is focused
#	And "1 Data Source" is "Enabled"
#	And "2 Select Action" is "Disabled"
#	And "3 Test Connector and Calculate Outputs" is "Disabled" 
#	And "4 Edit Default and Mapping Names" is "Disabled" 
#	And "Save" is "Disabled"
#	When I select "GreenPoint" as data source
#	Then "2 Select Action" is "Enabled"
#	When I select "dbo.Pr_CitiesGetByCountry" as the action
#	Then "3 Test Connector and Calculate Outputs" is "Enabled" 
#	And "Test" is "Enabled"
#	And inputs are
#	| CountryName  | Prefix |
#	| South Africa | Sa     |
#	And "4 Edit Default and Mapping Names" is "Disabled" 
#	When I test the action
#	Then outputs are
#	| Record Name                  | CityID | City      |
#	| dbo_Pr_CitiesgetByCountry(1) | 8873   | Saldanha  |
#	| dbo_Pr_CitiesgetByCountry(2) | 8864   | Sasolburg |
#	And "4 Edit Default and Mapping Names" is "Enabled" 
#    And "Save" is "Enabled"
#	And input mappings are
#	| Inputs      | Default value | Required Field | Empty is Null |
#	| CountryName |               |                |               |
#	| Prefix      |               |                |               |  
#	And output mappings are
#	| Output | Output Alias | Recordset Name            |
#	| CityID | CityID       | dbo.Pr_CitiesGetByCountry |
#	| City   | City         | dbo.Pr_CitiesGetByCountry |
#	When I save
#	Then Save Dialog is opened 	

































