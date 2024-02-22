Feature: DatabaseWorkflowExecution
	In order to execute a workflow
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
			Given Debug events are reset
			And Debug states are cleared

@DatabaseWorkflowExecution
Scenario: Database PostgreSql Database service inputs and outputs
     Given I depend on a valid PostgreSQL server
	 And I have a workflow "PostgreSqlGetCountries"
	 And "PostgreSqlGetCountries" contains a postgre tool using "get_countries" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable           |
	  | countrynamecontains           | s             | Id                  | [[countries(*).Id]]   |
	  |                  |               | Name                | [[countries(*).Name]] |
      When "PostgreSqlGetCountries" is executed
     Then the workflow execution has "NO" error
	 And the "get_countries" in Workflow "PostgreSqlGetCountries" debug outputs as
	  |                                       |
	  | [[countries(1).Id]] = 1               |
	  | [[countries(2).Id]] = 3               |
	  | [[countries(1).Name]] = United States |
	  | [[countries(2).Name]] = South Africa  |

@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service using * indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithMySqlStarIndex | MySqlEmail  | [[rec(*).name]] | [[rec(*).email]] | NO           |

@DatabaseWorkflowExecution
Scenario: Database MySqlDB Database service using char in param name
     Given I have a workflow "TestMySqlWFWithMySqlCharParamName"
	 And "TestMySqlWFWithMySqlCharParamName" contains a mysql database service "procWithCharNoOutput" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  | id               | 445           |                     |             |
	  | val              | bart01        |                     |             |
      When "TestMySqlWFWithMySqlCharParamName" is executed
     Then the workflow execution has "NO" error


@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service using int indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs is
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                 | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithMySqlIntIndex | MySqlEmail  | [[rec(1).name]] | [[rec(1).email]] | NO           |

@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service last  indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs is
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestMySqlWFWithMySqlLastIndex | MySqlEmail  | [[rec().name]] | [[rec().email]] | NO           |

@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                |
	  | [[name]] = Monk                |
	  | [[email]] = dora@explorers.com |
Examples: 
    | WorkflowName               | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestMySqlWFWithMySqlScalar | MySqlEmail  | [[name]]     | [[email]]     | NO           |

@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service Error outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
    | WorkflowName                                 | ServiceName | nameVariable         | emailVariable | errorOccured |
    | TestMySqlWFWithMySqlMailsInvalidIndex        | MySqlEmail  | [[rec(-1).name]]     | [[email]]     | YES          |
    | TestMySqlWFWithMySqlMailsInvalidVar          | MySqlEmail  | [[123]]              | [[email]]     | YES          |
    | TestMySqlWFWithMySqlMailsInvalidVarWithIndex | MySqlEmail  | [[rec(-1).name.bob]] | [[email]]     | YES          |

@DatabaseWorkflowExecution
Scenario Outline: Database MySqlDB Database service inputs and outputs
	Given I depend on a valid MySQL server
    And I have a workflow "<WorkflowName>"
	And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	 | Input to Service | From Variable | Output from Service | To Variable     |
	 | name             | afg%          | countryid           | <nameVariable>  |
	 |                  |               | description         | <emailVariable> |
    When "<WorkflowName>" is executed
    Then the workflow execution has "<errorOccured>" error
	And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	 |                                            |
	 | [[countries(1).id]] = 1                    |
	 | [[countries(2).id]] = 1                    |
	 | [[countries(1).description]] = Afghanistan |
	 | [[countries(2).description]] = Afghanistan |
Examples: 
    | WorkflowName                  | ServiceName           | nameVariable        | emailVariable                | errorOccured |
    | TestMySqlWFWithMySqlCountries | Pr_CitiesGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

@MSSql
Scenario Outline: Database SqlDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  | Prefix           | afg           | countryid           | <nameVariable>  |
	  |                  |               | description         | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
Examples: 
    | WorkflowName                    | ServiceName               | nameVariable        | emailVariable                | errorOccured |
    | TestSqlWFWithSqlServerCountries | dbo.Pr_CitiesGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service DBErrors
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
     | WorkflowName                      | ServiceName     | nameVariable | emailVariable | errorOccured |
     | TestWFWithDBSqlServerErrorProcSql | dbo.willalwayserror | [[name]]     | [[email]]     | YES          |

@MSSql
Scenario Outline: Database SqlDB  service using int indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestWFWithDBSqlServerIntIndex | dbo.SQLEmail    | [[rec(1).name]] | [[rec(1).email]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service using last indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName              | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestWFWithDBSqlServerLastIndex | dbo.SQLEmail    | [[rec().name]] | [[rec().email]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service using scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                  |
	  | [[name]] = dora                  |
	  | [[email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName                | ServiceName  | nameVariable | emailVariable | errorOccured |
    | TestWFWithDBSqlServerScalar | dbo.SQLEmail | [[name]]     | [[email]]     | NO           |

@DatabaseWorkflowExecution
Scenario: Executing unsaved workflow should execute by ID
	Given I create a new unsaved workflow with name "Unsaved 1"
	And "Unsaved 1" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec(1).a]] | yes   |
	  | [[rec(2).a]] | no    |	 
	  When '1' unsaved WF "Unsaved 1" is executed
	  Then the workflow execution has "NO" error
	  And the "Rec To Convert" in Workflow "Unsaved 1" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  Then I create a new unsaved workflow with name "Unsaved 1"
	  And "Unsaved 1" contains an Assign "Assign 1" as
	  | variable    | value |
	  | [[rec(1).a]] | 1   |
	  | [[rec(2).a]] | 2    |	 
	  When '2' unsaved WF "Unsaved 1" is executed	 
	  And the "Assign 1" in Workflow "Unsaved 1" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = 1 |
	  | 2 | [[rec(2).a]] = 2  |