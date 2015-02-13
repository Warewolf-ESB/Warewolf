Feature: DB Service
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DB Service
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



Scenario: Editing DB Service
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And "" is focused
   And "1 Data Source" is "Enabled"
  When I select "DemoDB" as data source
 
   And "2 Select Action" is "Enabled"





   And "3 Test Connector and Calculate Outputs" is "Enabled" 




   And "4 Edit Dfault and Mapping Names" is "Enabled" 






   And "Save" is "Disabled"







   When I select "DemoDB" as data source
   Then "2 Select Action" is "Enabled"
   And I Select "dbo.ConverToint" as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "Test" is "Enabled"
   And "3 Test Connector" Inputs looks like
   | charValue |
   | 1         |	
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































