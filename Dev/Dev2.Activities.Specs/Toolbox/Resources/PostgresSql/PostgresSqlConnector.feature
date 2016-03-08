Feature: PostgresSqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Creating PostgresSql Server Connector
	Given I drag a PostgresSql Server database connector
	When I select "DemoPostgres" as the source
	When I select "getemployees" as the action
	Then Inputs is Enabled 
	#And Inputs appear as 
	#| Input     | Value | Empty is Null |
	#| EID		|       | false         |
	#And Validate is Enable
	#When I click Generate Outputs
	#Then Then the Inputs Window will open
	#And Test Inputs appear.
	#| fname |
	#| null  |
	#Then I Enter a name value
	#| fname |
	#| Bill  |
	#When I click Test
	#Then Test Connector and Calculate Outputs outputs appear as
	#| name | salary | age |
	#| Bill | 4200   | 45  |
	#When I click Done
	#Then Outputs appear as
	#| Mapped From | Mapped To                     | 
	#| Column1     | [[getemployees().name]] | 

	#And Recordset Name equals "getemployees"	
