﻿Feature: DB Service
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers



Scenario: Creating DB Service
	Given I click "New Data Base Service Connector"
	Then "New DB Service" tab is opened
	And "Data Source" is focused
	And "1 Data Source" is "Enabled"
	And "2 Select Action" is "Disabled"
	And "3 Test Connector and Calculate Outputs" is "Disabled" 
	And "4 Edit Dfault and Mapping Names" is "Disabled" 
	And "Save" is "Disabled"
	When I select "DemoDB" as data source
	Then "2 Select Action" is "Enabled"
	And I Select "dbo.ConverToint" as the action
	Then "3 Test Connector and Calculate Outputs" is "Enabled" 
	And "Test" is "Enabled"
	And "3 Test Connector" Inputs looks like
	| charValue |
	| 1         |	
	And "4 Edit Dfault and Mapping Names" is "Disabled" 
	When I test the action
	Then "3 Test Connector" Outputs looks like
	| Recordset Name     | Result |
	| dbo_ConverToInt(1) | 1      |
	And "4 Edit Dfault and Mapping Names" is "Enabled" 
    And "Save" is "Enabled"
	And "4 Mappings" Inputs looks like
	| charValue | Required Field | Empty is Null |
	|           |                |               |
	And "4 Mappings" Output looks like
	| result | Recordset Name     |
	|        | dbo_ConverToInt |
	When I "Save"
	Then Save Dialog is opened 



Scenario: Opening Saved DB Service
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And "" is focused
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "3 Test Connector" Inputs looks like
   | fname | Iname | usernam | password | lastAccessDate |
   | Test  | Ware  | wolf    | Dev      | 12/1/1990      |
   And "Test" is "Enabled"
   Then "3 Test Connector" Outputs looks like
	| Recordset Name         | UserID |
	| ddo_InsertDummyUser(1) | 14378  |
   And "4 Edit Dfault and Mapping Names" is "Enabled" 
   And "4 Mappings" Inputs looks like
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               |                |               |
	  | iname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
	And "4 Mappings" Output looks like
	| Output | Output Alias | Recordset Name      |
	| UserID | UserID       | dbo_InsertDummyUser |
   And "Save" is "Disabled"
   
  

Scenario: Editing Saved DB Service By selecting Source
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And "" is focused
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "Test" is "Enabled"
   And "4 Edit Dfault and Mapping Names" is "Enabled" 
   And "Save" is "Disabled"
   When I select "DemoDB" as data source  
   And "2 Select Action" is "Disabled"
   And "3 Test Connector and Calculate Outputs" is "Disabled" 
   And "Test" is "Disabled"
   And "4 Edit Dfault and Mapping Names" is "Disabled" 
   And "Save" is "Disabled"
   Then "2 Select Action" is "Enabled"
   And I Select "dbo.ConverToint" as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "Test" is "Enabled"
   And "3 Test Connector" Inputs looks like
   | charValue |
   | 1         |	
   And "4 Edit Dfault and Mapping Names" is "Disabled" 
   When I test the action
   Then "3 Test Connector" Outputs looks like
   | Recordset Name     | Result |
   | dbo_ConverToInt(1) | 1      |
   And "4 Edit Dfault and Mapping Names" is "Enabled" 
   And "Save" is "Enabled"
   And "4 Mappings" Inputs looks like
   | charValue | Required Field | Empty is Null |
   |           |                |               |
   And "4 Mappings" Output looks like
   | result | Recordset Name     |
   |        | dbo_ConverToInt |
   When I "Save"
   Then "InsertDummyUser" is saved
   And Save Dialog is not opened 


Scenario: Creating DB Service Mappings
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And "" is focused
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "Save" is "Disabled"
   When I edit "3 Test Connector" Inputs 
      | fname  | Iname | usernam | password | lastAccessDate |
      | Change | Test  | wolf    | Dev      | 10/1/1990      |
   And "Test" is "Enabled"
   When I test the action
   And "Save" is "Enabled"
   Then "3 Test Connector" Outputs looks like
	  | Recordset Name         | UserID |
	  | ddo_InsertDummyUser(1) | 14378  |
   And "4 Edit Dfault and Mapping Names" is "Enabled" 
   When I edit "4 Mappings" Inputs 
	  | Inputs         | Default Value | Required Field | Empty is Null |
	  | fname          |               | Yes            |               |
	  | iname          |               |                |               |
	  | username       |               |                |               |
	  | password       |               |                |               |
	  | lastAccessDate |               |                |               |
   And "Save" is "Enabled"
   And "4 Mappings" Output looks like
	  | Output | Output Alias | Recordset Name      |
	  | UserID | UserID       | dbo_InsertDummyUser |
   When I "Save"
   Then "InsertDummyUser" is saved
   And Save Dialog is not opened 



   






   




































