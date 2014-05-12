Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssign' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssign' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  
Scenario: Workflow with multiple tools executing against the server
	  Given I have a workflow "WorkflowWithAssignAndCount"
	  And "WorkflowWithAssignAndCount" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |
	  And "WorkflowWithAssignAndCount" contains Count Record "CountRec" on "[[rec()]]" into "[[count]]"
	  When "WorkflowWithAssignAndCount" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssignAndCount' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssignAndCount' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  And the 'CountRec' in WorkFlow 'WorkflowWithAssignAndCount' debug inputs as
	  | Recordset            |
	  | [[rec(1).a]] = yes |
	  | [[rec(2).a]] = no |
	  And the 'CountRec' in Workflow 'WorkflowWithAssignAndCount' debug outputs as    
	  |               |
	  | [[count]] = 2 |
	
Scenario: Simple workflow executing against the server with a database service
	 Given I have a workflow "TestDbServiceWF"
	 And "TestDbServiceWF" contains a "database" service "Fetch" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  |                  |               | dbo_proc_SmallFetch(*).Value | [[rec().fetch]] |
	 And "TestDbServiceWF" contains Count Record "Count" on "[[rec()]]" into "[[count]]"
	  When "TestDbServiceWF" is executed
	  Then the workflow execution has "NO" error
	  And the 'Fetch' in WorkFlow 'TestDbServiceWF' debug inputs as
	  |  |
	  |  |
	  And the 'Fetch' in Workflow 'TestDbServiceWF' debug outputs as
	  |                      |
	  | [[rec(9).fetch]] = 5 |
	  And the 'Count' in WorkFlow 'TestDbServiceWF' debug inputs as
	  | Recordset            |
	  | [[rec(1).fetch]] = 1 |
	  | [[rec(2).fetch]] = 2 |
	  | [[rec(3).fetch]] = 1 |
	  | [[rec(4).fetch]] = 2 |
	  | [[rec(5).fetch]] = 1 |
	  | [[rec(6).fetch]] = 2 |
	  | [[rec(7).fetch]] = 1 |
	  | [[rec(8).fetch]] = 2 |
	  | [[rec(9).fetch]] = 5 |
	 And the 'Count' in Workflow 'TestDbServiceWF' debug outputs as    
	 |               |
	 | [[count]] = 9 |

Scenario: Workflow with an assign and webservice
	 Given I have a workflow "TestWebServiceWF"
	 And "TestWebServiceWF" contains an Assign "Inputs" as
	  | variable   | value |
	  | [[ext]]    | json  |
	  | [[prefix]] | a     |
	 And "TestWebServiceWF" contains a "webservice" service "InternalCountriesServiceTest" with mappings
	  | Input to Service | From Variable | Output from Service      | To Variable                 |
	  | extension        | [[ext]]       | Countries(*).CountryID   | [[Countries().CountryID]]   |
	  | prefix           | [[prefix]]    | Countries(*).Description | [[Countries().Description]] |
	  When "TestWebServiceWF" is executed
	  Then the workflow execution has "NO" error
	   And the 'Inputs' in WorkFlow 'TestWebServiceWF' debug inputs as
	  | # | Variable     | New Value |
	  | 1 | [[ext]] =    | json      |
	  | 2 | [[prefix]] = | a         |
	  And the 'Inputs' in Workflow 'TestWebServiceWF' debug outputs as    
	  | # |                |
	  | 1 | [[ext]] = json |
	  | 2 | [[prefix]] = a |
	  And the 'InternalCountriesServiceTest' in WorkFlow 'TestWebServiceWF' debug inputs as
	  |  |
	  | [[ext]] = json |
	  | [[prefix]] = a |
	  And the 'InternalCountriesServiceTest' in Workflow 'TestWebServiceWF' debug outputs as
	  |                                            |
	  | [[Countries(10).CountryID]] = 10           |
	  | [[Countries(10).Description]] = Azerbaijan |

	
Scenario: Workflow with an assign and remote workflow
	Given I have a workflow "TestAssignAndRemote"
	 And "TestAssignAndRemote" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	And "TestAssignAndRemote" contains "WorkflowUsedBySpecs" from server "Remote Connection" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable      |
	| input            | [[inputData]] | output              | [[output]]       |
	|                  |               | values(*).upper     | [[values().up]]  |
	|                  |               | values(*).lower     | [[values().low]] |
	  When "TestAssignAndRemote" is executed
	  Then the workflow execution has "NO" error
	   And the 'AssignData' in WorkFlow 'TestAssignAndRemote' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the 'AssignData' in Workflow 'TestAssignAndRemote' debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the 'WorkflowUsedBySpecs' in WorkFlow 'TestAssignAndRemote' debug inputs as
	  |                       |
	  | [[inputData]] = hello |
	  And the 'Setup Assign (1)' in Workflow 'WorkflowUsedBySpecs' debug outputs as
	  | # |                |
	  | 1 | [[in]] = hello |
	  And the 'Convert Case (1)' in Workflow 'WorkflowUsedBySpecs' debug outputs as
	  | # |                |
	  | 1 | [[in]] = HELLO |
	  And the 'Final Assign (3)' in Workflow 'WorkflowUsedBySpecs' debug outputs as
	  | # |                             |
	  | 1 | [[output]] = HELLO          |
	  | 2 | [[values(1).upper]] = HELLO |
	  | 3 | [[values(1).lower]] = hello |	  	 
	  And the 'WorkflowUsedBySpecs' in Workflow 'TestAssignAndRemote' debug outputs as
	  |                           |
	  | [[output]] = HELLO        |
	  | [[values(1).up]] = HELLO  |
	  | [[values(1).low]] = hello |

	  
Scenario: Workflow with Assign Base Convert and Case Convert tools executing against the server
	  Given I have a workflow "WorkflowWithAssignBaseConvertandCaseconvert"
	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains an Assign "Assign1" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  | [[rec().a]] | test  |
	  | [[rec().a]] | 100   |
	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains case convert "Case to Convert" as
	  | Variable     | Type  |
	  | [[rec(2).a]] | UPPER |
	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains Base convert "Base to Convert" as
	  | Variable     | From | To     |
	  | [[rec(1).a]] | Text | Base64 |
	  When "WorkflowWithAssignBaseConvertandCaseconvert" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 50        |
	  | 2 | [[rec(2).a]] = | test      |
	  | 3 | [[rec(3).a]] = | 100       |
	   And the 'Rec To Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                |      |
	  | 1 | [[rec(1).a]] = | 50   |
	  | 2 | [[rec(2).a]] = | test |
	  | 3 | [[rec(3).a]] = | 100  |
	  And the 'Case to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Convert             | To    |
	  | 1 | [[rec(2).a]] = test | UPPER |
	  And the 'Case to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(2).a]] = TEST |
	  And the 'Base to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | 1 | [[rec(1).a]] = 50 | Text  | Base64 |
      And the 'Base to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = NTA= |

Scenario: Workflow with Assign and 2 Delete tools executing against the server
	  Given I have a workflow "WorkflowWithAssignand2Deletetools"
	  And "WorkflowWithAssignand2Deletetools" contains an Assign "Assign to delete" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "WorkflowWithAssignand2Deletetools" contains Delete "Delet1" as
	  | Variable   | result      |
	  | [[rec(1)]] | [[result1]] |
      And "WorkflowWithAssignand2Deletetools" contains Delete "Delet2" as
	   | variable   | value        |
	   | [[rec(1)]] | [[result2]]] |
	  When "WorkflowWithAssignBaseConvertandCaseconvert" is executed
      Then the execution has "AN" error
	  And the 'Assign to delete' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the 'Assign to delete' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the 'Delet1' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | Records          |
	  | [[rec(1).a]] = 50 |
	  And the 'Delet1' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  |                       |
	  | [[result1]] = Success |
	  And the 'Delet2' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = |           |
	 And the 'Delet2' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                       |
	  | 1 | [[result2]] = Failure |

Scenario: Workflow with 3 Assigns tools executing against the server
	  Given I have a workflow "WorkflowWith3Assigntools"
	  And "WorkflowWith3Assigntools" contains an Assign "Assigntool1" as
	  | variable    | value    |
	  | [[rec().a]] | rec(1).a |
	   And "WorkflowWith3Assigntools" contains an Assign "Assigntool2" as
	  | variable     | value    |
	  | [[test]]     | rec(1).a |
	  | [[rec(1).a]] | Warewolf |
	   And "WorkflowWith3Assigntools" contains an Assign "Assigntool3" as
	  | variable | value        |
	  | [[new]]  | [[[[test]]]] |
	  When "WorkflowWith3Assigntools" is executed
	  Then the execution has "NO" error
	  And the 'Assigntool1' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | rec(1).a  |
	  And the 'Assigntool1' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                        |
	  | 1 | [[rec().a]] = rec(1).a |
	  And the 'Assigntool2' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[test]] =    | rec(1).a  |
	  | 2 | [[rec(1).a]]= | Warewolf  |
	  And the 'Assigntool2' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                         |
	  | 1 | [[rec().a]] = rec(1).a  |
	  | 2 | [[rec(1).a]] = Warewolf |
	   And the 'Assigntool3' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable  | New Value               |
	  | 1 | [[new]] = | [[[[test]]]] = Warewolf |
	  And the 'Assigntool3' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                    |
	  | 1 | [[new]] = Warewolf |














