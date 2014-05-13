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
      Then the workflow execution has "NO" error
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
	  Then the workflow execution has "NO" error
	  And the 'Assigntool1' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | rec(1).a  |
	  And the 'Assigntool1' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = rec(1).a |
	  And the 'Assigntool2' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable                | New Value |
	  | 1 | [[test]] =              | rec(1).a  |
	  | 2 | [[rec(1).a]] = rec(1).a | Warewolf  |
	  And the 'Assigntool2' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                         |
	  | 1 | [[test]] = rec(1).a     |
	  | 2 | [[rec(1).a]] = Warewolf |
	   And the 'Assigntool3' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
	  | # | Variable  | New Value               |
	  | 1 | [[new]] = | [[[[test]]]] = Warewolf |
	  And the 'Assigntool3' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
	  | # |                    |
	  | 1 | [[new]] = Warewolf |

#This test is going to pass after the issue 11785 is fixed
#@Ignore 
#Scenario: Workflow with Assign and Date and Time Difference tools executing against the server
#	  Given I have a workflow "WorkflowWithAssignAndDateTimeDifferencetools"
#	  And "WorkflowWithAssignAndDateTimeDifferencetools" contains an Assign "InputDates" as
#	  | variable | value |
#	  | [[a]]    | 2014  |
#	  | [[b]]    | 10    |
#	  And "WorkflowWithAssignAndDateTimeDifferencetools" contains Date and Time Difference "Date&Time" as	
#	  | Input1        | Input2     | Input Format | Output In | Result     |
#	  | 2020/[[b]]/01 | 2030/01/01 | yyyy/mm/dd   | Years     | [[result]] |  
#	  When "WorkflowWithAssignAndDateTimeDifferencetools" is executed
#	  Then the execution has "AN" error
#	  And the 'InputDates' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | 2014      |
#	  | 2 | [[b]] =  | 01.       |
#	  And the 'InputDates' in Workflow 'WorkflowWith3Assigntools' debug outputs as  
#	  | # |              |
#	  | 1 | [[a]] = 2014 |
#	  | 2 | [[b]] = 01.  |
#	  And the 'Date&Time' in WorkFlow 'WorkflowWith3Assigntools' debug inputs as
#	  | Input 1       | Input 2    | Input Format | Output In |
#	  | 2014/[[b]]/01 | 2030/01/01 | yyyy/mm/dd   | Years     |
#	  And the 'Date&Time' in Workflow 'WorkflowWith3Assigntools' debug outputs as 
#	  |               |
#	  | [[result1]] = |



Scenario: Workflow with Assigns DataMerge and DataSplit executing against the server
      Given I have a workflow "WorkflowWithAssignDataMergeandDataSplittools""
	  And "WorkflowWithAssignDataMergeandDataSplittools" contains an Assign "Assign To merge" as
      | variable      | value    |
      | [[a]]         | Test     |
      | [[b]]         | Warewolf |
      | [[split().a]] | Workflow |
	  And "WorkflowWithAssignDataMergeandDataSplittools" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable | Type  | Using | Padding | Alignment |
	  | [[a]]    | Index | 4     |         | Left      |
	  | [[b]]    | Index | 8     |         | Left      |
	  And "WorkflowWithAssignDataMergeandDataSplittools" contains Data Split "Data Split" as
	  | String                  | Variable     | Type  | At | Include    | Escape |
	  | [[result]][[split().a]] | [[rec(1).b]] | Index | 8  | Unselected |        |
	  |                         | [[rec(2).b]] | Index | 8  | Unselected |        |
	  When "WorkflowWithAssignDataMergeandDataSplittools" is executed
	  Then the execution has "NO" error
	  And the 'Assign To merge' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
	  | # | Variable        | New Value |
	  | 1 | [[a]] =         | Test      |
	  | 2 | [[b]] =         | Warewolf  |
	  | 3 | [[split().a]] = | Workflow  |
	 And the 'Assign To merge' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as   
	  | # |                           |
	  | 1 | [[a]]         =  Test     |
	  | 2 | [[b]]         =  Warewolf |
	  | 3 | [[split().a]] =  Workflow |
	  And the 'Data Merge' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
	  | # |                   | With  | Using | Pad | Align |
	  | 1 | [[a]] =  Test     | Index | "4"   | ""  | Left  |
	  | 2 | [[b]] =  warewolf | Index | "8"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as  
	  |                           |
	  | [[result]] = Testwarewolf |
	  And the 'Data Split' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
	  | String to Split                                 | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	  | [[result]][[split(1).a]] = TestWarewolfWorkflow | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | 4     | No      |        |
	  |                                                 |                   |                 | 2 | [[rec(2).b]] = nothing | Index | 8     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = Test     |
	  | 2 | [[rec(2).a]] = Warewolf |
	  | 3 | [[rec(3).a]] = Workflow |

#This test is going to pass after the issue 11804 is fixed
#@Ignore 
#Scenario: Workflow with Assigns Calculate and DataSplit executing against the server
#      Given I have a workflow "WorkflowWithAssignCalculateandDataSplittools""
#	  And "WorkflowWithAssignCalculateandDataSplittools" contains an Assign "splitvalues1" as
#      | variable  | value    |
#      | [[a]]     | 1        |
#      | [[b]]     | 2        |
#      | [[c]]     | test     |
#      | [[split]] | warewolf |
#	  And "WorkflowWithAssignCalculateandDataSplittools" contains an Assign "splitvalues2" as
#      | variable | value  |
#      | [[test]] | result |
#	  And "WorkflowWithAssignCalculateandDataSplittools" contains Calculate "Calculate" with formula "[[a]]+[[b]]" into "[[result]]"
#	  And "WorkflowWithAssignCalculateandDataSplittools" contains Data Split "Data Spliting" as
#	  | String    | Variable     | Type  | At        | Include    | Escape |
#	  | [[split]] | [[rec(1).b]] | Index | [[[[c]]]] | Unselected |        |
#	  When "WorkflowWithAssignCalculateandDataSplittools" is executed
#	  Then the execution has "NO" error
#	  And the 'splitvalues1' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
#	  | # | Variable        | New Value |
#	  | 1 | [[a]] =         | 1         |
#	  | 2 | [[b]] =         | 2         |
#	  | 3 | [[c]] =         | test      |
#	  | 4 | [[split]] = | workflow  |
#	 And the 'splitvalues1' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as   
#	  | # |                       |
#	  | 1 | [[a]]         =  1    |
#	  | 2 | [[b]]         =  2    |
#	  | 3 | [[c]]         =  test |
#	  | 4 | [[split]] = workflow  |
#	  And the 'splitvalues2' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
#      | fx =              |
#      | [[a]]+[[b]] = 1+2 |          
#      And the 'splitvalues12' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as  
#	  |                |
#	  | [[result]] = 3 |
#	  And the 'Data Spliting' in WorkFlow 'WorkflowWithAssignDataMergeandDataSplittools' debug inputs as 
#	  | String to Split      | Process Direction | Skip blank rows | # |                        | With  | Using     | Include | Escape |
#	  | [[split]] = workflow | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | [[[[c]]]] | No      |        |
#	  And the 'Data Spliting' in Workflow 'WorkflowWithAssignDataMergeandDataSplittools' debug outputs as  
#	  | # |                   |
#	  | 1 | [[rec(1).a]] = lf |
#	
	  
Scenario Outline: Workflow with Assign Base Convert and Decision tools executing against the server
	  Given I have a workflow "WorkflowWithAssignBaseConvertandDecision"
	  And "WorkflowWithAssignBaseConvertandDecision" contains an Assign "Assign1" as
	  | variable    | value     |
	  | [[rec().a]] | '<value>' |
	  And "WorkflowWithAssignBaseConvertandDecision" contains Base convert "BaseConvert" as
	  | Variable     | From     | To     |
	  | [[rec(1).a]] | '<from>' | '<to>' |
	  And "WorkflowWithAssignBaseConvertandDecision" contains Decision "Decision" as
	  |       |          |
	  | [[a]] | '<cond>' |
	  When "WorkflowWithAssignBaseConvertandDecision" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign1' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | <value>   |
	   And the 'Assign1' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
	  | # |                |         |
	  | 1 | [[rec(1).a]] = | <value> |
	  And the 'BaseConvert' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
	  | 1 | [[rec(1).a]] = warewolf | <text>  |<to> |
      And the 'BaseConvert' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = <result> |
	  And the 'Decision' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
	  |  | Statement | Require All decisions to be True |
	  |  | String    | YES                              |
	  And the 'Decision' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
	  |          |
	  | <output> |
Examples: 
     | no | Value    | from | to     | result       | cond      | output |
     | 1  | warewolf | Text | Base64 | d2FyZxdvbGY= | Is Base64 | YES    |
     | 2  | a        | Text | Binary | 01100001     | Is Binary | YES    |
     | 3  | a        | Text | Hex    | 0x61         | Is Hex    | YES    |
     | 4  | 2013/01  | Text | Text   | 2013/01      | Is Date   | YES    |



Scenario: Workflow with Assign and Sequence(Assign, Datamerge, Data Split, Find Index and Replace)
      Given I have a workflow "workflowithAssignandsequence"
       And "workflowithAssignandsequence" contains an Assign "Assign for sequence" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
      And "workflowithAssignandsequence" contains a Sequence "Test1" as
	  And "Test1" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4     |         | Left      |
	  | [[rec(2).a]] | Index | 8     |         | Left      |
	  And "Test1" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	  And "Test1" contains Find Index "Index" into "[[indexResult]]" as
	  | In Fields    | Index           | Character | Direction     |
	  | [[rec().a]] | First Occurence | e         | Left to Right |
	  And "Test1" contains Replace "Replacing" into "[[replaceResult]]" as	
	  | In Fields  | Find | Replace With |
	  | [[rec(*)]] | e    | REPLACED     |
	  When "workflowithAssignandsequence" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign for sequence' in WorkFlow 'workflowithAssignandsequence' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().b]] = | nothing   |
	  | 3 | [[rec().a]] = | warewolf  |
	  | 4 | [[rec().b]] = | nothing   |
	   And the 'Assign for sequence' in Workflow 'workflowithAssignandsequence' debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = test     |
	  | 2 | [[rec(1).b]] = nothing  |
	  | 3 | [[rec(2).a]] = warewolf |
	  | 4 | [[rec(2).b]] = nothing  |
	  And the "Data Merge" debug inputs as  
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = test     | Index | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = warewolf | Index | "8"   | ""  | Left  |
	  And the "Data Merge" debug outputs as 
	  |                           |
	  | [[result]] = testwarewolf |
	  And the "Data Split" debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | 4     | No      |        |
	  |                 |                   |                 | 2 | [[rec(2).b]] = nothing | Index | 8     | No      |        |
	  And the "Data Split" debug outputs as
	  | # |                         |
	  | 1 | [[rec(1).b]] = test     |
	  | 2 | [[rec(2).b]] = warewolf |
      And the "Index" debug inputs as
	  | In Field                | Index           | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurence | e          | Left to Right |
	  And the "Index" debug outputs as
	  |                     |
	  | [[indexResult]] = 4 |
	  And the "Replacing" debug inputs as 
	  | In Field(s)             | Find | Replace With |
	  | [[rec(1).a]] = test     |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the "Replacing" debug outputs as 
	  |                                |
	  | [[rec(1).a]] = tREPLACEDst     |
	  | [[rec(1).b]] = tREPLACEDst     |
	  | [[rec(2).a]] = warREPLACEDwolf |
	  | [[rec(2).b]] = warREPLACEDwolf |
	  | [[replaceResult]] = 4          |

Scenario: Workflow with Assign Create and Delete Record tools executing against the server
	  Given I have a workflow "WorkflowWithAssignCreateandDeleteRecord"
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Assign "Assign to create" as
	  | variable    | value           |
	  | [[rec().a]] | C:\copied00.txt |
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Create "Create1" as
	  | File or Folder | If it exits | Username | Password | Result   |
	  | [[rec().a]]    | True        |          |          | [[res1]] |
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Delete "DeleteFolder" as
	  | Recordset   | Result   |
	  | [[rec().a]] | [[res2]] |
	  When "WorkflowWithAssignCreateandDeleteRecord" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign to create' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecord' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | C:\copied00.txt       |
	  And the 'Assign to create' in Workflow 'WorkflowWithAssignCreateandDeleteRecord' debug outputs as     
	  | # |                         |
	  | 1 | [[rec(1).a]] = C:\copied00.txt      |
	 And the 'Create1' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecord' debug inputs as
	  | File or Folder                | Overwrite | Username | Password |
	  | [[rec().a]] = C:\copied00.txt | True      |          |          |  
	   And the 'Create1' in Workflow 'WorkflowWithAssignCreateandDeleteRecord' debug outputs as    
	   |                    |
	   | [[res1]] = Success |
	  And the 'DeleteFolder' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecord' debug inputs as
	  | Input Path                    | Username | Password |
	  | [[rec().a]] = C:\copied00.txt |          |          |
	  And the 'DeleteFolder' in Workflow 'WorkflowWithAssignCreateandDeleteRecord' debug outputs as    
	  |                    |
	  | [[res2]] = Success |

#This Test Scenario should be passed after the Bug 11815 is fixed
#Scenario Outline: Workflow with Assign Create and Delete Record tools with incorrect input path executing against the server
#	  Given I have a workflow "WorkflowWithAssignCreateDeleteRecord"
#	  And "WorkflowWithAssignCreateDeleteRecord" contains an Assign "Assign to create" as
#	  | variable    | value         |
#	  | [[rec().a]] | \create.txt |
#	  And "WorkflowWithAssignCreateDeleteRecord" contains an Create "Create1" as
#	  | File or Folder | If it exits | Username | Password | Result   |
#	  | [[rec().a]]    | True        |          |          | [[res1]] |
#	  And "WorkflowWithAssignCreateDeleteRecord" contains an Delete "DeleteFolder" as
#	  | Recordset   | Result   |
#	  | [[rec().a]] | [[res2]] |
#	  When "WorkflowWithAssignCreateDeleteRecord" is executed
#	  Then the workflow execution has "AN" error
#	  And the 'Assign to create' in WorkFlow 'WorkflowWithAssignCreateDeleteRecord' debug inputs as
#	  | # | Variable      | New Value   |
#	  | 1 | [[rec().a]] = | \create.txt |
#	  And the 'Assign to create' in Workflow 'WorkflowWithAssignCreateDeleteRecord' debug outputs as     
#	  | # |                            |
#	  | 1 | [[rec(1).a]] = \create.txt |
#	  And the 'Create1' in WorkFlow 'WorkflowWithAssignCreateDeleteRecord' debug inputs as
#	  | File or Folder            | Overwrite | Username | Password |
#	  | [[rec().a]] = \create.txt | True      |          |          | 
#	  And the 'Create1' in Workflow 'WorkflowWithAssignCreateDeleteRecord' debug outputs as    
#	   |                    |
#	   | [[res1]] = Failure |
#	  And the 'DeleteFolder' in WorkFlow 'WorkflowWithAssignCreateDeleteRecord' debug inputs as
#	  | Input Path                | Username | Password |
#	  | [[rec().a]] = \create.txt |          |          |
#	  And the 'DeleteFolder' in Workflow 'WorkflowWithAssignCreateDeleteRecord' debug outputs as    
#	  |                    |
#	  | [[res2]] = Failure |





  


Scenario: Workflow with Assign and Sequence(Assign, Datamerge, Data Split, Find Index and Replace)
      Given I have a workflow "workflowithAssignandsequence"
       And "workflowithAssignandsequence" contains an Assign "Assign for sequence" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
      And "workflowithAssignandsequence" contains a Sequence "Test1" as
	  And "Test1" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4     |         | Left      |
	  | [[rec(2).a]] | Index | 8     |         | Left      |
	  And "Test1" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	  And "Test1" contains Find Index "Index" into "[[indexResult]]" as
	  | In Fields    | Index           | Character | Direction     |
	  | [[rec().a]] | First Occurence | e         | Left to Right |
	  And "Test1" contains Replace "Replacing" into "[[replaceResult]]" as	
	  | In Fields  | Find | Replace With |
	  | [[rec(*)]] | e    | REPLACED     |
	  When "workflowithAssignandsequence" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign for sequence' in WorkFlow 'workflowithAssignandsequence' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().b]] = | nothing   |
	  | 3 | [[rec().a]] = | warewolf  |
	  | 4 | [[rec().b]] = | nothing   |
	   And the 'Assign for sequence' in Workflow 'workflowithAssignandsequence' debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = test     |
	  | 2 | [[rec(1).b]] = nothing  |
	  | 3 | [[rec(2).a]] = warewolf |
	  | 4 | [[rec(2).b]] = nothing  |
	  And the "Data Merge" debug inputs as  
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = test     | Index | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = warewolf | Index | "8"   | ""  | Left  |
	  And the "Data Merge" debug outputs as 
	  |                           |
	  | [[result]] = testwarewolf |
	  And the "Data Split" debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | 4     | No      |        |
	  |                 |                   |                 | 2 | [[rec(2).b]] = nothing | Index | 8     | No      |        |
	  And the "Data Split" debug outputs as
	  | # |                         |
	  | 1 | [[rec(1).b]] = test     |
	  | 2 | [[rec(2).b]] = warewolf |
      And the "Index" debug inputs as
	  | In Field                | Index           | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurence | e          | Left to Right |
	  And the "Index" debug outputs as
	  |                     |
	  | [[indexResult]] = 4 |
	  And the "Replacing" debug inputs as 
	  | In Field(s)             | Find | Replace With |
	  | [[rec(1).a]] = test     |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the "Replacing" debug outputs as 
	  |                                |
	  | [[rec(1).a]] = tREPLACEDst     |
	  | [[rec(1).b]] = tREPLACEDst     |
	  | [[rec(2).a]] = warREPLACEDwolf |
	  | [[rec(2).b]] = warREPLACEDwolf |
	  | [[replaceResult]] = 4          |


















