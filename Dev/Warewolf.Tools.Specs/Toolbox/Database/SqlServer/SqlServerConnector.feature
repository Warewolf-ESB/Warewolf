Feature: SqlServerConnector
    In order to manage my database services

    As a Warewolf User

    I want to be shown the database service setup

@Database
Scenario: Passing Timeouts to SQL Server
    Given I have a workflow "Test Sql With Timeouts"
	And "Test Sql With Timeouts" contains "TestSqlExecutesWithTimeouts" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter | Empty is Null |
	When "Test Sql With Timeouts" is executed
    Then the workflow containing the Sql Server connector has "An" execution error

    And The Sql Server step "TestSqlExecutesWithTimeouts" in Workflow "Test Sql With Timeouts" debug outputs appear as
	  |  |
	  |  |

@Database
Scenario: Opening Saved workflow with SQL Server tool
   Given I open workflow with database connector

    And Sql Server Source is Enabled
    And Sql Server Source is "testingDBSrc"	
	And Sql Server Action is "dbo.Pr_CitiesGetCountries"
	And Sql Server Inputs Are Enabled

    And Sql Server Inputs appear as
	| Input | Value       | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate Sql Server is Enabled
    Then Sql Server Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Sql Server Recordset Name equals "dbo_Pr_CitiesGetCountries"

@Database
Scenario: Change SQL Server Source on Existing tool
    Given I open workflow with database connector

    And Sql Server Source is Enabled
    And Sql Server Source is "testingDBSrc"
	And Sql Server Action is "dbo.Pr_CitiesGetCountries"
	And Sql Server Inputs Are Enabled

    And Sql Server Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate Sql Server is Enabled
    Then Sql Server Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Sql Server Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Source is changed from to "GreenPoint"
	Then Sql Server Action is Enabled
    And Sql Server Inputs Are Enabled
    And Validate Sql Server is Enabled

@Database
Scenario: Changing SQL Server Actions
    Given I open workflow with database connector
    And Sql Server Source is Enabled
    And Sql Server Source is "testingDBSrc"
	And Sql Server Action is "dbo.Pr_CitiesGetCountries"
	And Sql Server Inputs Are Enabled

    And Sql Server Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate Sql Server is Enabled
    Then Sql Server Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Sql Server Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Action is changed from to "dbo.ImportOrder"
	And Sql Server Inputs Are Enabled
    And Sql Server Inputs appear as
	| Input     | Value         | Empty is Null |
	| ProductId | [[ProductId]] | false         |
	And Validate Sql Server is Enabled

@Database
Scenario: Change SQL Server Recordset Name
    Given I open workflow with database connector
    And Sql Server Source is Enabled
    And Sql Server Source is "testingDBSrc"
	And Sql Server Action is "dbo.Pr_CitiesGetCountries"
	And Sql Server Inputs Are Enabled
    And Sql Server Inputs appear as
	| Input | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate Sql Server is Enabled
    Then Sql Server Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Sql Server Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Recordset Name is changed to "Pr_Cities"
	Then Sql Server Outputs appear as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |

@Database
Scenario: No SQL Server Action to be loaded Error
    Given I have a workflow "NoStoredProceedureToLoad"
	And "NoStoredProceedureToLoad" contains "Testing/SQL/NoSqlStoredProceedure" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter | Empty is Null |
	When "NoStoredProceedureToLoad" is executed
    Then the workflow containing the Sql Server connector has "An" execution error
    And The Sql Server step "Testing/SQL/NoSqlStoredProceedure" in Workflow "NoStoredProceedureToLoad" debug outputs appear as
	  |                                                                  |
	  | Error: The selected database does not contain actions to perform |

@Database
Scenario: Passing Null Input values to SQL Server
    Given I have a workflow "PassingNullInputValue"
	And "PassingNullInputValue" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullInputValue" is executed
    Then the workflow containing the Sql Server connector has "An" execution error
    And The Sql Server step "Acceptance Testing Resources/GreenPoint" in Workflow "PassingNullInputValue" debug outputs appear as
	  |                                       |
	  | Error: Scalar value { value } is NULL |

@Database
Scenario: Mapped To Recordsets incorrect
    Given I have a workflow "BadSqlParameterName"
	And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter | Empty is Null |
	     |                            | a         | True          |
	And And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with Mapping To as
	| Mapped From      | Mapped To                                |
	| id               | [[dbo_leon bob proc().id]]               |
	| some column Name | [[dbo_leon bob proc().some column Name]] |
	When "BadSqlParameterName" is executed
    Then the workflow containing the Sql Server connector has "An" execution error
    And The Sql Server step "Acceptance Testing Resources/GreenPoint" in Workflow "BadSqlParameterName" debug outputs appear as
	  |                               |
	  | Error: Sql Error: parse error |

@Database
Scenario: Parameter not found in the collection
    Given I have a workflow "BadMySqlParameterName"
	And "BadMySqlParameterName" contains "Testing/MySql/MySqlParameters" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadMySqlParameterName" is executed
    Then the workflow containing the Sql Server connector has "An" execution error

    And The Sql Server step "Testing/MySql/MySqlParameters" in Workflow "BadMySqlParameterName" debug outputs appear as
	  |                                                      |
	  | Parameter "p_startswith" not found in the collection |

@Database
Scenario: Recordset has invalid character
    Given I have a workflow "MappingHasIncorrectCharacter"
	And "MappingHasIncorrectCharacter" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or[[Variable]] | Parameter | Empty is Null |
	     | 1                          | charValue | True          |
	When "MappingHasIncorrectCharacter" is executed
    Then the workflow containing the Sql Server connector has "An" execution error
    And The Sql Server step "Acceptance Testing Resources/GreenPoint" in Workflow "MappingHasIncorrectCharacter" debug outputs appear as
	  |                                                                    |
	  | [[dbo_ConvertTo, Int().result]] : Recordset name has invalid format |

@Database
Scenario: SqlServer backward Compatiblity
    Given I have a workflow "DataMigration"
	And "DataMigration" contains "DataCon" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      | [[ProductId]]    | productId     | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "DataMigration" is executed
    Then the workflow "DataMigration" execution has "NO" error
	
@SqlDatabaseBroker
Scenario: Execute Sql Server With Timeout
	Given I have workflow "SqlServerWorkflowForTimeout" with "SqlServerActivity" SqlServer database connector
	And Sql Server Source is Enabled
	And I Select "NewSqlServerSource" as SqlServer Source for "SqlServerActivity"
	And I Select "dbo.Pr_CitiesGetCountries_Delayed" as Server Action for "SqlServerActivity"
	And Sql Command Timeout is "60" milliseconds for "SqlServerActivity"
	And Validate Sql Server is Enabled
	And I click Sql Generate Outputs
	And I click Test
	And Sql Command Timeout is "1" milliseconds for "SqlServerActivity"
	When Sql Workflow "SqlServerWorkflowForTimeout" containing dbTool is executed
	And the workflow "SqlServerWorkflowForTimeout" execution has "AN" error "SQL Error: Execution Timeout Expired"
	And the workflow "SqlServerWorkflowForTimeout" error does not contain "NewLine"