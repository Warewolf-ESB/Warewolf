@DBService
Feature: DB Service
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Creating DB Service
	Given I click New Data Base Service Connector
	Then "New DB Service" tab is opened
	And Data Source is focused
	And "1 Data Source" is "Enabled"
	And "2 Select Action" is "Disabled"
	And "3 Test Connector and Calculate Outputs" is "Disabled" 
	And "4 Edit Default and Mapping Names" is "Disabled" 
	And "Save" is "Disabled"
	When I select "DemoDB" as data source
	Then "2 Select Action" is "Enabled"
	When I select "dbo.ConverToint" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled" 
	And "Test" is "Enabled"
	And inputs are
	| charValue |
	| 1         |	
	And "4 Edit Default and Mapping Names" is "Disabled" 
	When I test the action
	Then outputs are
	| Recordset Name     | Result |
	| dbo_ConverToInt(1) | 1      |
	And "4 Edit Default and Mapping Names" is "Enabled" 
    And "Save" is "Enabled"
	And input mappings are
	| charValue | Required Field | Empty is Null |
	And output mappings are
	| Output | Output Alias | Recordset Name  |
	| result | result       | dbo_ConverToInt |
	When I save
	Then Save Dialog is opened 



Scenario: Opening Saved DB Service
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And Data Source is focused
   And "DemoDB" is selected as the data source
   And "dbo.InsertDummyUser" is selected as the action
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And inputs are
   | fname | lname | username | password | lastAccessDate |
   | Test  | Ware  | wolf     | Dev      | 12/1/1990      |
   And "Test" is "Enabled"
   And "4 Edit Default and Mapping Names" is "Enabled" 
   And input mappings are
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               |                |               |
	  | iname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
   And output mappings are
	| Output | Output Alias | Recordset Name      |
	| UserID | UserID       | dbo_InsertDummyUser |
   And "Save" is "Disabled"

Scenario: Editing Saved DB Service By selecting Source
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And Data Source is focused
   And "DemoDB" is selected as the data source
   And "dbo.InsertDummyUser" is selected as the action
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "Test" is "Enabled"
   And "4 Edit Default and Mapping Names" is "Enabled" 
   And "Save" is "Disabled"
   When I select "DemoDB" as data source  
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Disabled" 
   And "Test" is "Disabled"
   And "4 Edit Default and Mapping Names" is "Disabled" 
   And "Save" is "Disabled"
   Then I select "dbo.ConverToint" as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And Inspect Data Connector hyper link is "Visible"
   And "Test" is "Enabled"
   And inputs are
   | charValue |
   | 1         |	
   And "4 Edit Default and Mapping Names" is "Disabled" 
   When I test the action
   Then outputs are
   | Recordset Name     | Result |
   | dbo_ConverToInt(1) | 1      |
   And "4 Edit Default and Mapping Names" is "Enabled" 
   And "Save" is "Enabled"
   And input mappings are
   | charValue | Required Field | Empty is Null |
   |           |                |               |
   And output mappings are
   | Output | Output Alias | Recordset Name  |
   | result | result       | dbo_ConverToInt |
   When I save
   Then "InsertDummyUser" is saved
   And Save Dialog is not opened 

 Scenario: Editing DB Service Mappings
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And Data Source is focused
   And "DemoDB" is selected as the data source
   And "dbo.InsertDummyUser" is selected as the action
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
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
	  | ddo_InsertDummyUser(1) | 14378  |
   And "4 Edit Default and Mapping Names" is "Enabled" 
   When input mappings are 
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               | Yes            |               |
	  | iname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
   And "Save" is "Enabled"
   And output mappings are
	  | Output | Output Alias | Recordset Name      |
	  | UserID | UserID       | dbo_InsertDummyUser |
   When I save
   Then "InsertDummyUser" is saved
   And Save Dialog is not opened 



 Scenario: Editing DB Service and Test Execution is unsuccesful
   Given I open "InsertDummyUser" service
   And "Edit:InsertDummyUser" tab is opened
   And Data Source is focused
   And "DemoDB" is selected as the data source
   And "dbo.InsertDummyUser" is selected as the action
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And Inspect Data Connector hyper link is "Visible"
   And inputs are
   | fname | Iname | usernam | password | lastAccessDate |
   | Test  | Ware  | wolf    | Dev      | 12/1/1990      |
   And "Test" is "Enabled"   
   And "Save" is "Disabled"  
   When I test the action
   And Execution fails
   And "4 Edit Default and Mapping Names" is "Disabled" 
   And input mappings are
	| Inputs         | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output | Output Alias | Recordset Name      |
	And "Save" is "Disabled"

   




































