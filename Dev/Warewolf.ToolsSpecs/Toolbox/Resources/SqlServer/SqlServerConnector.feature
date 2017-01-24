Feature: SqlServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Opening Saved workflow with SQL Server tool
   Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"	
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change SQL Server Source on Existing tool
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Source is changed from to "GreenPoint"
	Then Action is Enabled
	And Inputs is Enabled 
	And Validate is Enabled 

@ChangingSqlServerFunctions
Scenario: Changing SQL Server Actions
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	#And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Action is changed from to "dbo.ImportOrder"
	And Inputs is Enabled
	And Inputs appear as
	| Input     | Value         | Empty is Null |
	| ProductId | [[ProductId]] | false         |
	And Validate is Enabled	

Scenario: Change SQL Server Recordset Name
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	#And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Recordset Name is changed to "Pr_Cities"
	Then Outputs appear as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |

Scenario: No SQL Server Action to be loaded Error
	Given I have a workflow "NoStoredProceedureToLoad"
	And "NoStoredProceedureToLoad" contains "Testing/SQL/NoSqlStoredProceedure" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "NoStoredProceedureToLoad" is executed
	Then the sqlsERVER workflow execution has "An" error
	And The sqlsERVER "Testing/SQL/NoSqlStoredProceedure" in Workflow "NoStoredProceedureToLoad" debug outputs as
	  |                                                                  |
	  | Error: The selected database does not contain actions to perform |

Scenario: Passing Null Input values to SQL Server
	Given I have a workflow "PassingNullInputValue"
	And "PassingNullInputValue" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullInputValue" is executed
	Then the sqlsERVER workflow execution has "An" error
	And The sqlsERVER "Acceptance Testing Resources/GreenPoint" in Workflow "PassingNullInputValue" debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |

Scenario: Mapped To Recordsets incorrect
	Given I have a workflow "BadSqlParameterName"
	And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     |                            | a         | True          |
	And And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with Mapping To as
	| Mapped From      | Mapped To                                |
	| id               | [[dbo_leon bob proc().id]]               |
	| some column Name | [[dbo_leon bob proc().some column Name]] |
	When "BadSqlParameterName" is executed
	Then the sqlsERVER workflow execution has "An" error
	And The sqlsERVER "Acceptance Testing Resources/GreenPoint" in Workflow "BadSqlParameterName" debug outputs as
	  |                               |
	  | Error: Sql Error: parse error |


#Needs Work
Scenario: Parameter not found in the collection
	Given I have a workflow "BadMySqlParameterName"
	And "BadMySqlParameterName" contains "Testing/MySql/MySqlParameters" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadMySqlParameterName" is executed
	Then the sqlsERVER workflow execution has "An" error
	And The sqlsERVER "Testing/MySql/MySqlParameters" in Workflow "BadMySqlParameterName" debug outputs as
	  |                                                      |
	  | Parameter "p_startswith" not found in the collection |


Scenario: Recordset has invalid character
	Given I have a workflow "MappingHasIncorrectCharacter"
	And "MappingHasIncorrectCharacter" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | 1                          | charValue | True          |
	When "MappingHasIncorrectCharacter" is executed
	Then the sqlsERVER workflow execution has "An" error
	And The sqlsERVER "Acceptance Testing Resources/GreenPoint" in Workflow "MappingHasIncorrectCharacter" debug outputs as
	  |                                                                    |
	  | [[dbo_ConvertTo,Int().result]] : Recordset name has invalid format |
	  
#Wolf-1262
Scenario: SqlServer backward Compatiblity
	Given I have a workflow "DataMigration"
	And "DataMigration" contains "DataCon" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      | [[ProductId]]    | productId     | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "DataMigration" is executed
	Then the workflow execution has "NO" error
