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
	 Given I have a workflow "TestWFWithDBService"
	 And "TestWFWithDBService" contains a "database" service "Fetch" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  |                  |               | dbo_proc_SmallFetch(*).Value | [[dbo_proc_SmallFetch().Value]] |
	 And "TestWFWithDBService" contains Count Record "Count" on "[[dbo_proc_SmallFetch()]]" into "[[count]]"
	  When "TestWFWithDBService" is executed
	  Then the workflow execution has "NO" error
	  And the 'Fetch' in WorkFlow 'TestWFWithDBService' debug inputs as
	  |  |
	  |  |
	  And the 'Fetch' in Workflow 'TestWFWithDBService' debug outputs as
	  |                      |
	  | [[dbo_proc_SmallFetch(9).Value]] = 5 |
	  And the 'Count' in WorkFlow 'TestWFWithDBService' debug inputs as
	  | Recordset            |
	  | [[dbo_proc_SmallFetch(1).Value]] = 1 |
	  | [[dbo_proc_SmallFetch(2).Value]] = 2 |
	  | [[dbo_proc_SmallFetch(3).Value]] = 1 |
	  | [[dbo_proc_SmallFetch(4).Value]] = 2 |
	  | [[dbo_proc_SmallFetch(5).Value]] = 1 |
	  | [[dbo_proc_SmallFetch(6).Value]] = 2 |
	  | [[dbo_proc_SmallFetch(7).Value]] = 1 |
	  | [[dbo_proc_SmallFetch(8).Value]] = 2 |
	  | [[dbo_proc_SmallFetch(9).Value]] = 5 |
	 And the 'Count' in Workflow 'TestWFWithDBService' debug outputs as    
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
	And "TestAssignAndRemote" contains "WorkflowUsedBySpecs" from server "Integration Connection" with mapping as
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
#
# This Test should be passed after the bug 11612 is fixed
# This test relies upon the fact that the remote workflow is real and loaded into the remote environment
#Scenario: Remote Workflow with an remote workflow
#	  Given I have server a "Remote Connection" with workflow "Bug11612_Outer"
#	  When "Remote Connection" is the active environment used to execute "Bug11612_Outer"
#	  Then the workflow execution has "NO" error
#	  And the 'BUGS\Bug11612_Inner' in WorkFlow 'Bug11612_Outer' debug inputs as
#	  |                                 |
#	  | 2                               | 
#	  | Execute workflow asynchronously: False                               | 	 
#	 And the 'BUGS\Bug11612_Inner' in Workflow 'Bug11612_Outer' debug outputs as
#	  |                  |
#	  | [[result]] = 3 |

	  
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
	  | [[rec(1).a]] | Text | Base 64 |
	  When "WorkflowWithAssignBaseConvertandCaseconvert" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign1' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  | 2 | [[rec().a]] = | test      |
	  | 3 | [[rec().a]] = | 100       |
	   And the 'Assign1' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                      |
	  | 1 | [[rec(1).a]] =  50   |
	  | 2 | [[rec(2).a]] =  test |
	  | 3 | [[rec(3).a]] =  100  |
	  And the 'Case to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Convert             | To    |
	  | 1 | [[rec(2).a]] = test | UPPER |
	  And the 'Case to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(2).a]] = TEST |
	  And the 'Base to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
	  | # | Convert           | From | To     |
	  | 1 | [[rec(1).a]] = 50 | Text | Base 64 |
      And the 'Base to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = NTA= |
	  

# This issue should be resolved as part of bug 12112
#Scenario: Workflow with Assign and 2 Delete tools executing against the server
#	  Given I have a workflow "WorkflowWithAssignand2Deletetools"
#	  And "WorkflowWithAssignand2Deletetools" contains an Assign "Assign to delete" as
#	  | variable    | value |
#	  | [[rec().a]] | 50    |
#	  And "WorkflowWithAssignand2Deletetools" contains Delete "Delet1" as
#	  | Variable   | result      |
#	  | [[rec(1)]] | [[result1]] |
#      And "WorkflowWithAssignand2Deletetools" contains Delete "Delet2" as
#	   | Variable   | result      |
#	   | [[rec(1)]] | [[result2]] |
#	  When "WorkflowWithAssignand2Deletetools" is executed
#      Then the workflow execution has "NO" error
#	  And the 'Assign to delete' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | 50        |
#	  And the 'Assign to delete' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
#	  | # |                   |
#	  | 1 | [[rec(1).a]] = 50 |
#	  And the 'Delet1' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
#	  | Records          |
#	  | [[rec(1).a]] = 50 |
#	  And the 'Delet1' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
#	  |                       |
#	  | [[result1]] = Success |
#	  And the 'Delet2' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
#	   | Records        |	  
#	  And the 'Delet2' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
#	  |                       |
#	  | [[result2]] = Failure |

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
Scenario: Workflow with Assign and Date and Time Difference tools executing against the server
	  Given I have a workflow "WorkflowWithAssignAndDateTimeDifferencetools1"
	  And "WorkflowWithAssignAndDateTimeDifferencetools1" contains an Assign "InputDates" as
	  | variable | value |
	  | [[a]]    | 2014  |
	  | [[b]]    | 10.    |
	  And "WorkflowWithAssignAndDateTimeDifferencetools1" contains Date and Time Difference "DateAndTime" as	
	  | Input1        | Input2     | Input Format | Output In | Result     |
	  | 2020/[[b]]/01 | 2030/01/01 | yyyy/mm/dd   | Years     | [[result]] |  
	  When "WorkflowWithAssignAndDateTimeDifferencetools1" is executed
	  Then the workflow execution has "AN" error
	  And the 'InputDates' in WorkFlow 'WorkflowWithAssignAndDateTimeDifferencetools1' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | 2014      |
	  | 2 | [[b]] =  | 10.       |
	  And the 'InputDates' in Workflow 'WorkflowWithAssignAndDateTimeDifferencetools1' debug outputs as  
	  | # |              |
	  | 1 | [[a]] = 2014 |
	  | 2 | [[b]] = 10.  |
	  And the 'DateAndTime' in WorkFlow 'WorkflowWithAssignAndDateTimeDifferencetools1' debug inputs as
	  | Input 1       | Input 2    | Input Format | Output In |
	  | 2020/[[b]]/01 = 2020/10./01 | 2030/01/01 | yyyy/mm/dd   | Years     |
	  And the 'DateAndTime' in Workflow 'WorkflowWithAssignAndDateTimeDifferencetools1' debug outputs as 
	  |               |
	  | [[result]] = |


Scenario: Workflow with Assigns DataMerge and DataSplit executing against the server
      Given I have a workflow "WorkflowWithAssignDataMergeAndDataSplittools"
	  And "WorkflowWithAssignDataMergeAndDataSplittools" contains an Assign "Assign To merge" as
      | variable      | value    |
      | [[a]]         | Test     |
      | [[b]]         | Warewolf |
      | [[split().a]] | Workflow |
	  And "WorkflowWithAssignDataMergeAndDataSplittools" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable | Type  | Using | Padding | Alignment |
	  | [[a]]    | Index | 4     |         | Left      |
	  | [[b]]    | Index | 8     |         | Left      |
	  And "WorkflowWithAssignDataMergeAndDataSplittools" contains Data Split "Data Split" as
	  | String                  | Variable     | Type  | At | Include    | Escape |
	  | [[result]][[split().a]] | [[rec().b]] | Index | 4  | Unselected |        |
	  |                         | [[rec().b]] | Index | 8  | Unselected |        |
	  When "WorkflowWithAssignDataMergeAndDataSplittools" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign To merge' in WorkFlow 'WorkflowWithAssignDataMergeAndDataSplittools' debug inputs as 
	  | # | Variable        | New Value |
	  | 1 | [[a]] =         | Test      |
	  | 2 | [[b]] =         | Warewolf  |
	  | 3 | [[split().a]] = | Workflow  |
	 And the 'Assign To merge' in Workflow 'WorkflowWithAssignDataMergeAndDataSplittools' debug outputs as   
	  | # |                           |
	  | 1 | [[a]]         =  Test     |
	  | 2 | [[b]]         =  Warewolf |
	  | 3 | [[split(1).a]] =  Workflow |
	  And the 'Data Merge' in WorkFlow 'WorkflowWithAssignDataMergeAndDataSplittools' debug inputs as 
	  | # |                   | With  | Using | Pad | Align |
	  | 1 | [[a]] = Test     | Index | "4"   | ""  | Left  |
	  | 2 | [[b]] = Warewolf | Index | "8"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowWithAssignDataMergeAndDataSplittools' debug outputs as  
	  |                           |
	  | [[result]] = TestWarewolf |
	  And the 'Data Split' in WorkFlow 'WorkflowWithAssignDataMergeAndDataSplittools' debug inputs as 
	  | String to Split                                 | Process Direction | Skip blank rows | # |                | With  | Using | Include | Escape |
	  | [[result]][[split(1).a]] = TestWarewolfWorkflow | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  |                                                 |                   |                 | 2 | [[rec().b]] = | Index | 8     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithAssignDataMergeAndDataSplittools' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).b]] = Test     |
	  |   | [[rec(2).b]] = Warewolf |
	  |   | [[rec(3).b]] = Work     |
	  |   | [[rec(4).b]] = flow     |
	  | 2 | [[rec(1).b]] = Test     |
	  |   | [[rec(2).b]] = Warewolf |
	  |   | [[rec(3).b]] = Work     |
	  |   | [[rec(4).b]] = flow     |

#This test is going to pass after the issue 11804 is fixed
Scenario: Workflow with Assigns and DataSplit executing against the server
      Given I have a workflow "WorkflowWithAssignandDataSplittools"
	  And "WorkflowWithAssignandDataSplittools" contains an Assign "splitvalues1" as
      | variable    | value |
      | [[a]]       | b     |
      | [[b]]       | 2     |
      | [[rs(1).a]] | test  |
	   And "WorkflowWithAssignandDataSplittools" contains an Assign "splitvalues2" as
      | variable | value    |
      | [[test]] | warewolf |
	  And "WorkflowWithAssignandDataSplittools" contains Data Split "DataSpliting" as
	  | String          | Variable     | Type  | At        | Include    | Escape |
	  | [[[[rs(1).a]]]] | [[rec(1).a]] | Index | [[[[a]]]] | Unselected |        |
	  When "WorkflowWithAssignandDataSplittools" is executed
	  Then the workflow execution has "NO" error
	  And the 'splitvalues1' in WorkFlow 'WorkflowWithAssignandDataSplittools' debug inputs as 
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | 2         |
	  | 3 | [[rs(1).a]] = | test      |
	 And the 'splitvalues1' in Workflow 'WorkflowWithAssignandDataSplittools' debug outputs as   
	  | # |                       |
	  | 1 | [[a]]         =  b    |
	  | 2 | [[b]]         =  2    |
	  | 3 | [[rs(1).a]]   =  test |
	 And the 'splitvalues2' in WorkFlow 'WorkflowWithAssignandDataSplittools' debug inputs as 
	  | # | Variable   | New Value |
	  | 1 | [[test]] = | warewolf  | 
	 And the 'splitvalues2' in Workflow 'WorkflowWithAssignandDataSplittools' debug outputs as   
	  | # |                      |
	  | 1 | [[test]] =  warewolf |
	  And the 'DataSpliting' in WorkFlow 'WorkflowWithAssignandDataSplittools' debug inputs as 
	  | String to Split            | Process Direction | Skip blank rows | # |                | With  | Using         | Include | Escape |
	  | [[[[rs(1).a]]]] = warewolf | Forward           | No              | 1 | [[rec(1).a]] = | Index | [[[[a]]]] = 2 | No      |        |
	  And the 'DataSpliting' in Workflow 'WorkflowWithAssignandDataSplittools' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = lf |
	
	  
#@ignore 	  
#Scenario Outline: Workflow with Assign Base Convert and Decision tools executing against the server
#	  Given I have a workflow "WorkflowWithAssignBaseConvertandDecision"
#	  And "WorkflowWithAssignBaseConvertandDecision" contains an Assign "Assign1" as
#	  | variable    | value     |
#	  | [[rec().a]] | '<value>' |
#	  And "WorkflowWithAssignBaseConvertandDecision" contains Base convert "BaseConvert" as
#	  | Variable     | From     | To     |
#	  | [[rec(1).a]] | '<from>' | '<to>' |
#	  And "WorkflowWithAssignBaseConvertandDecision" contains Decision "Decision" as
#	  |       |          |
#	  | [[a]] | '<cond>' |
#	  When "WorkflowWithAssignBaseConvertandDecision" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Assign1' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
#	  | # | Variable       | New Value |
#	  | 1 | [[rec(1).a]] = | <value>   |
#	   And the 'Assign1' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
#	  | # |                |         |
#	  | 1 | [[rec(1).a]] = | <value> |
#	  And the 'BaseConvert' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
#	  | 1 | [[rec(1).a]] = warewolf | <text>  |<to> |
#      And the 'BaseConvert' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
#	  | # |                         |
#	  | 1 | [[rec(1).a]] = <result> |
#	  And the 'Decision' in WorkFlow 'WorkflowWithAssignBaseConvertandDecision' debug inputs as
#	  |  | Statement | Require All decisions to be True |
#	  |  | String    | YES                              |
#	  And the 'Decision' in Workflow 'WorkflowWithAssignBaseConvertandDecision' debug outputs as  
#	  |          |
#	  | <output> |
#Examples: 
#     | no | Value    | from | to     | result       | cond      | output |
#     | 1  | warewolf | Text | Base64 | d2FyZxdvbGY= | Is Base64 | YES    |
#     | 2  | a        | Text | Binary | 01100001     | Is Binary | YES    |
#     | 3  | a        | Text | Hex    | 0x61         | Is Hex    | YES    |
#     | 4  | 2013/01  | Text | Text   | 2013/01      | Is Date   | YES    |



Scenario: Workflow with Assign and Sequence(Assign, Datamerge, Data Split, Find Index and Replace) executing against the server
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
	  And the 'Data Merge' in WorkFlow 'Test1' debug inputs as
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = test     | Index | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = warewolf | Index | "8"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'Test1' debug outputs as
	  |                           |
	  | [[result]] = testwarewolf |
	  And the 'Data Split' in WorkFlow 'Test1' debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | 4     | No      |        |
	  |                 |                   |                 | 2 | [[rec(2).b]] = nothing | Index | 8     | No      |        |
	  And the 'Data Split' in Workflow 'Test1' debug outputs as
	  | # |                         |
	  | 1 | [[rec(1).b]] = test     |
	  | 2 | [[rec(2).b]] = warewolf |
      And the 'Index' in WorkFlow 'Test1' debug inputs as
	  | In Field                | Index           | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurence | e          | Left to Right |
	  And the 'Index' in Workflow 'Test1' debug outputs as
	  |                     |
	  | [[indexResult]] = 4 |
	  And the 'Replacing' in WorkFlow 'Test1' debug inputs as 
	  | In Field(s)             | Find | Replace With |
	  | [[rec(1).a]] = test     |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the 'Replacing' in Workflow 'Test1' debug outputs as
	  |                                |
	  | [[rec(1).a]] = tREPLACEDst     |
	  | [[rec(1).b]] = tREPLACEDst     |
	  | [[rec(2).a]] = warREPLACEDwolf |
	  | [[rec(2).b]] = warREPLACEDwolf |
	  | [[replaceResult]] = 4          |

Scenario: Workflow with Assign Create and Delete folder tools executing against the server
	  Given I have a workflow "WorkflowWithAssignCreateandDeleteRecord"
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Assign "Assign to create" as
	  | variable    | value           |
	  | [[rec().a]] | C:\copied00.txt |
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Create "Create1" as
	  | File or Folder | If it exits | Username | Password | Result   |
	  | [[rec().a]]    | True        |          |          | [[res1]] |
	  And "WorkflowWithAssignCreateandDeleteRecord" contains an Delete Folder "DeleteFolder" as
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
	  | [[rec(1).a]] = C:\copied00.txt | True      | ""       | ""       |  
	   And the 'Create1' in Workflow 'WorkflowWithAssignCreateandDeleteRecord' debug outputs as    
	   |                    |
	   | [[res1]] = Success |
	  And the 'DeleteFolder' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecord' debug inputs as
	  | Input Path                    | Username | Password |
	  | [[rec(1).a]] = C:\copied00.txt | ""       | ""       |
	  And the 'DeleteFolder' in Workflow 'WorkflowWithAssignCreateandDeleteRecord' debug outputs as    
	  |                    |
	  | [[res2]] = Success |

#This Test Scenario should be passed after the Bug 11815 is fixed
Scenario: Workflow with Assign Create and Delete Record tools with incorrect input path executing against the server
	  Given I have a workflow "WorkflowWithAssignCreateDeleteRecordNoneExist1"
	  And "WorkflowWithAssignCreateDeleteRecordNoneExist1" contains an Assign "Assign to create" as
	  | variable    | value         |
	  | [[rec().a]] | create.txt |
	  And "WorkflowWithAssignCreateDeleteRecordNoneExist1" contains an Create "Create1" as
	  | File or Folder | If it exits | Username | Password | Result   |
	  | [[rec().a]]    | True        |          |          | [[res1]] |
	  And "WorkflowWithAssignCreateDeleteRecordNoneExist1" contains an Delete "Delete" as
	  | File Or Folder | Result   |
	  | [[rec().a]]  | [[res1]] |
	  When "WorkflowWithAssignCreateDeleteRecordNoneExist1" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign to create' in WorkFlow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug inputs as
	  | # | Variable      | New Value   |
	  | 1 | [[rec().a]] = | create.txt |
	  And the 'Assign to create' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as     
	  | # |                            |
	  | 1 | [[rec(1).a]] = create.txt |
	  And the 'Create1' in WorkFlow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug inputs as
	  | File or Folder            | Overwrite | Username   | Password   |
	  | [[rec(1).a]] = create.txt | True      | Username = | Password = |
	  And the 'Create1' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as    
	   |                    |
	   | [[res1]] = Failure |
	  And the 'Delete' in WorkFlow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug inputs as
	  | Input Path                | Username | Password |
	  | [[rec(1).a]] = create.txt |   Username =       |    Password =      |
	  And the 'Delete' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as    
	  |                    |
	  | [[res1]] = Failure |

Scenario: Workflow with 2 Assign tools executing against the server
	  Given I have a workflow "WorkflowWith2Assigntools"
	  And "WorkflowWith3Assigntools" contains an Assign "tool1" as
	  | variable | value    |
	  | [[a]]    | b        |
	  | [[b]]    | test     |
	  | [[test]] | warewolf |
	  And "WorkflowWith2Assigntools" contains an Assign "tool2" as
	  | variable  | value         |
	  | [[[[a]]]] | [[[[[[a]]]]]] |
	  When "WorkflowWith2Assigntools" is executed
	  Then the workflow execution has "NO" error
	  And the 'tool1' in WorkFlow 'WorkflowWith2Assigntools' debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[a]] =    | b         |
	  | 2 | [[b]] =    | test      |
	  | 3 | [[test]] = | warewolf  |
	  And the 'tool1' in Workflow 'WorkflowWith2Assigntools' debug outputs as  
	  | # |                     |
	  | 1 | [[a]] = b           |
	  | 2 | [[b]] = test        |
	  | 3 | [[test]] = warewolf |
	  And the 'tool2' in WorkFlow 'WorkflowWith2Assigntools' debug inputs as
	  | # | Variable         | New Value                |
	  | 1 | [[[[a]]]] = test | [[[[[[a]]]]]] = warewolf |
	  And the 'tool2' in Workflow 'WorkflowWith2Assigntools' debug outputs as  
	  | # |                   |
	  | 1 | [[b]] =  warewolf |


#This Test Scenario should be passed after the issue 11834 is fixed	  
Scenario: Workflow with 2 Assign tools by using recordsets in fields executing against the server
	  Given I have a workflow "WorkflowWith2Assigntoolswithrecordsets"
	  And "WorkflowWith2Assigntoolswithrecordsets" contains an Assign "rec1" as
	  | variable     | value    |
	  | [[rec().a]]  | rec(2).a |
	  | [[rec(2).a]] | test     |
	  And "WorkflowWith2Assigntoolswithrecordsets" contains an Assign "rec2" as
	  | variable         | value    |
	  | [[[[rec(1).a]]]] | warewolf |
	  When "WorkflowWith2Assigntoolswithrecordsets" is executed
	  Then the workflow execution has "NO" error
	  And the 'rec1' in WorkFlow 'WorkflowWith2Assigntoolswithrecordsets' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec().a]] =  | rec(2).a  |
	  | 2 | [[rec(2).a]] = | test      |
	  And the 'rec1' in Workflow 'WorkflowWith2Assigntoolswithrecordsets' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = rec(2).a |
	  | 2 | [[rec(2).a]] = test     |
	  And the 'rec2' in WorkFlow 'WorkflowWith2Assigntoolswithrecordsets' debug inputs as
	  | # | Variable                | New Value |
	  | 1 | [[[[rec(1).a]]]] = rec(2).a | warewolf  |
	  And the 'rec2' in Workflow 'WorkflowWith2Assigntoolswithrecordsets' debug outputs as  
	  | # |                          |
	  | 1 | [[rec(2).a]] =  warewolf |

Scenario: Workflow with 2 Assign tools by using Scalars as variables executing against the server
	  Given I have a workflow "WorkflowWith2Assigntoolswithrscalars"
	  And "WorkflowWith2Assigntoolswithscalars" contains an Assign "scl1" as
	  | variable | value |
	  | [[a]]    | b     |
	  | [[b]]    | test  |
	  And "WorkflowWith2Assigntoolswithrscalars" contains an Assign "scl2" as
	  | variable  | value    |
	  | [[[[a]]]] | warewolf |
	  When "WorkflowWith2Assigntoolswithrscalars" is executed
	  Then the workflow execution has "NO" error
	  And the 'scl1' in WorkFlow 'WorkflowWith2Assigntoolswithrscalars' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the 'scl1' in Workflow 'WorkflowWith2Assigntoolswithrscalars' debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the 'scl2' in WorkFlow 'WorkflowWith2Assigntoolswithrscalars' debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[[[a]]]] = test | warewolf  |
	  And the 'scl2' in Workflow 'WorkflowWith2Assigntoolswithrscalars' debug outputs as  
	  | # |                   |
	  | 1 | [[b]] =  warewolf |

#This test scenario should be passed after the bug 11818 is fixed
#Scenario: Workflow with Assign and Gather System Information
#      Given I have a workflow "workflowithAssignandGatherSystemInformation"
#	  And "workflowithAssignandGatherSystemInformation" contains an Assign "Assign for sys" as
#	  | variable | value |
#	  | [[b]]    | 1     |    
#	   And "workflowithAssignandGatherSystemInformation" contains Gather System Info "System info" as
#	  | Variable      | Selected    |
#	  | [[test[[b]]]] | Date & Time |
#	  When "workflowithAssignandGatherSystemInformation" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Assign for sys' in WorkFlow 'workflowithAssignandGatherSystemInformation' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[b]] =  | 1         |
#	  And the 'Assign for sys' in Workflow 'workflowithAssignandGatherSystemInformation' debug outputs as    
#	  | # |          |
#	  | 1 | [b]] = 1 |
#	  And the 'System info' in WorkFlow 'workflowithAssignandGatherSystemInformation' debug inputs as
#	  | # |                 |             |
#	  | 1 | [[test[[b]]]] = | Date & Time |
#	  And the 'System info' in Workflow 'workflowithAssignandGatherSystemInformation' debug outputs as   
#	  | # |                        |
#	  | 1 | [[test[[1]]]] = String |

#This test should be passed after the bug 11837 is fixed
#Scenario: Workflow with assign and webservice with incorrect output variables
#	 Given I have a workflow "TestService"
#	 And "TestService" contains an Assign "Inputsvar" as
#	  | variable | value |
#	  | [[test]] | a&    |
#	  | [[a]]    | d     |
#	 And "TestService" contains a "webservice" service "InternalCountriesServiceTest" with mappings
#	  | Input to Service | From Variable | Output from Service      | To Variable                 |
#	  | extension        | json          | Countries(*).CountryID   | [[Countries().CountryID]]   |
#	  | prefix           | [[[[test]]]]  | Countries(*).Description | [[Countries().Description]] |
#	  When "TestService" is executed
#	  Then the workflow execution has "AN" error
#	   And the 'Inputsvar' in WorkFlow 'TestService' debug inputs as 
#	  | # | Variable   | New Value |
#	  | 1 | [[test]] = | a         |
#	  | 2 | [[a]]    = | d         |
#	  And the 'Inputsvar' in Workflow 'TestService' debug outputs as    
#	  | # |              |
#	  | 1 | [[test]] = a |
#	  | 2 | [[a]] = d    |
#	  And the 'InternalCountriesServiceTest' in WorkFlow 'TestService' debug inputs as
#	  |                       |
#	  | json                  |
#	  | [[[[test]]]] = [[d&]] |
#	  And the 'InternalCountriesServiceTest' in Workflow 'TestService' debug outputs as
#	  |                                 |
#	  | [[Countries(10).CountryID]] =   |
#	  | [[Countries(10).Description]] = |


Scenario: Workflow with Assign Count Data Merge and 2 Delete  tools executing against the server
	  Given I have a workflow "WorkflowWithAssignCountDataMerge&2Delete"
	  And "WorkflowWithAssignCountDataMerge&2Delete" contains an Assign "countrecordval1" as
	  | variable    | value |
	  | [[rec().a]] | 21    |
	  | [[rec().a]] | 22    |
	  | [[rec().a]] |       |
	  And "WorkflowWithAssignCountDataMerge&2Delete" contains Count Record "Cnt1" on "[[rec()]]" into "[[result1]]"
	  And "WorkflowWithAssignCountDataMerge&2Delete" contains Delete "Delrec" as
	  | Variable  | result      |
	  | [[rec()]] | [[result2]] |
	  And "WorkflowWithAssignCountDataMerge&2Delete" contains Data Merge "DataMerge1" into "[[rec().a]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 2     |         | Left      |
	  | [[rec(2).a]] | Index | 2     |         | Left      |
	  And "WorkflowWithAssignCountDataMerge&2Delete" contains Count Record "Cnt2" on "[[rec()]]" into "[[result3]]"
	  When "WorkflowWithAssignCountDataMerge&2Delete" is executed
	  Then the workflow execution has "NO" error
	  And the 'countrecordval1' in WorkFlow 'WorkflowWithAssignCountDataMerge&2Delete' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec().a]] = | 21        |
	  | 2 | [[rec().a]] = | 22        |
	  | 3 | [[rec().a]] = | ""        |
	  And the 'countrecordval1' in Workflow 'WorkflowWithAssignCountDataMerge&2Delete' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 21 |
	  | 2 | [[rec(2).a]] = 22 |
	  | 3 | [[rec(3).a]] =    |
	  And the 'Cnt1' in WorkFlow 'WorkflowWithAssignCountDataMerge&2Delete' debug inputs as 
	  | Recordset         |
	  | [[rec(1).a]] = 21 |
	  | [[rec(2).a]] = 22 |
	  | [[rec(3).a]] =    |
	  And the 'Cnt1' in Workflow 'WorkflowWithAssignCountDataMerge&2Delete' debug outputs as 
	  |                 |
	  | [[result1]] = 3 |
	  And the 'Delrec' in WorkFlow 'WorkflowWithAssignCountDataMerge&2Delete' debug inputs as
	  | Records        |
	  | [[rec(3).a]] = |
	  And the 'Delrec' in Workflow 'WorkflowWithAssignCountDataMerge&2Delete' debug outputs as  
	  |                       |
	  | [[result2]] = Success |	
	  And the 'DataMerge1' in WorkFlow 'WorkflowWithAssignCountDataMerge&2Delete' debug inputs as
	  | # |                   | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 21 | Index | "2"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = 22 | Index | "2"   | ""  | Left  |
	  And the 'DataMerge1' in Workflow 'WorkflowWithAssignCountDataMerge&2Delete' debug outputs as 
	  |                     |
	  | [[rec(3).a]] = 2122 |
	   And the 'Cnt2' in WorkFlow 'WorkflowWithAssignCountDataMerge&2Delete' debug inputs as 
	  | Recordset           |
	  | [[rec(1).a]] = 21   |
	  | [[rec(2).a]] = 22   |
	  | [[rec(3).a]] = 2122 |
	  And the 'Cnt2' in Workflow 'WorkflowWithAssignCountDataMerge&2Delete' debug outputs as 
	  |                 |
	  | [[result3]] = 3 |

#Below 2 scenarios should be passed after the issue 11866 is fixed
# Note the using database "xxx" MUST be loaded into DBSource directory else execution will fail ;)
# And it must be in the server resources too ;(
Scenario: Workflow with multiple tools Assign and SQL Bulk Insert executing against the server
	  Given I have a workflow "WorkflowWithAssignAndSQLBulkInsert"
	  And "WorkflowWithAssignAndSQLBulkInsert" contains an Assign "InsertData" as
	  | variable    | value    |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowWithAssignAndSQLBulkInsert" contains an SQL Bulk Insert "BulkInsert" using database "testingDBSrc" and table "dbo.MailingList" and KeepIdentity set "false" and Result set "[[result]]" as
	  | Column | Mapping             | IsNullable | DataTypeName | MaxLength | IsAutoIncrement |
	  | Id     |                     | false      | int          |           | true            |
	  | Name   | [[rec().a]]         | false      | varchar      | 50        | false           |
	  | Email  | Warewolf@dev2.co.za | false      | varchar      | 50        | false           |
	  When "WorkflowWithAssignAndSQLBulkInsert" is executed
	  Then the workflow execution has "NO" error
	  And the 'InsertData' in WorkFlow 'WorkflowWithAssignAndSQLBulkInsert' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Warewolf  |
	  And the 'InsertData' in Workflow 'WorkflowWithAssignAndSQLBulkInsert' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = Warewolf |
	  And the 'BulkInsert' in WorkFlow 'WorkflowWithAssignAndSQLBulkInsert' debug inputs as
	  | # |                         | To Field | Type         | Batch Size | Timeout | Check Constraints | Keep Table Lock | Fire Triggers | Keep Identity | Use Internal Transaction | Skip Blank Rows |
	  | 1 | [[rec(1).a]] = Warewolf | Name     | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  | 2 | Warewolf@dev2.co.za     | Email    | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  |   |                         |          |              | 0          | 0       | NO                | NO              | NO            | NO            | NO                       | YES             |
	  And the 'BulkInsert' in Workflow 'WorkflowWithAssignAndSQLBulkInsert' debug outputs as
	  |                      |
	  | [[result]] = Success |

Scenario: Workflow with multiple tools Assign and SQL Bulk Insert with negative Recordset Index executing against the server
	  Given I have a workflow "WorkflowWithAssignAndSQLBulk"
	  And "WorkflowWithAssignAndSQLBulk" contains an Assign "InsertData" as
	  | variable    | value |
	  | [[rec().a]] | Warewolf     |
	  And "WorkflowWithAssignAndSQLBulk" contains an SQL Bulk Insert "BulkInsert" using database "testingDBSrc" and table "dbo.MailingList" and KeepIdentity set "false" and Result set "[[result]]" as
	  | Column | Mapping             | IsNullable | DataTypeName | MaxLength | IsAutoIncrement |
	  | Id     |                     | false      | int          |           | true            |
	  | Name   | [[rec(-1).a]]       | false      | varchar      | 50        | false           |
	  | Email  | Warewolf@dev2.co.za | false      | varchar      | 50        | false           |
	  When "WorkflowWithAssignAndSQLBulk" is executed
	  Then the workflow execution has "AN" error
	  And the 'InsertData' in WorkFlow 'WorkflowWithAssignAndSQLBulk' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Warewolf  |
	  And the 'InsertData' in Workflow 'WorkflowWithAssignAndSQLBulk' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = Warewolf |
	  And the 'BulkInsert' in WorkFlow 'WorkflowWithAssignAndSQLBulk' debug inputs as
	  | # |                     | To Field | Type         | Batch Size | Timeout | Check Constraints | Keep Table Lock | Fire Triggers | Keep Identity | Use Internal Transaction | Skip Blank Rows |
	  | 1 | [[rec(-1).a]] =     | Name     | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  | 2 | Warewolf@dev2.co.za | Email    | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  And the 'BulkInsert' in Workflow 'WorkflowWithAssignAndSQLBulk' debug outputs as
	  |                      |
	  | [[result]] = Failure |

Scenario: Simple workflow with Assign and Base Convert(Evaluating scalar variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandBase"
	 And "WorkflowWithAssignandBase" contains an Assign "Base Var" as
	  | variable | value |
	  | [[a]]    | b     |
	  | [[b]]    | 12    |	
	   And "WorkflowWithAssignandBase" contains Base convert "Base" as
	  | Variable  | From | To      |
	  | [[[[a]]]] | Text | Base 64 |
	  When "WorkflowWithAssignandBase" is executed
	  Then the workflow execution has "NO" error
	  And the 'Base Var' in WorkFlow 'WorkflowWithAssignandBase' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12        |
	  And the 'Base Var' in Workflow 'WorkflowWithAssignandBase' debug outputs as  
	  | # |            |
	  | 1 | [[a]] = b  |
	  | 2 | [[b]] = 12 |
	   And the 'Base' in WorkFlow 'WorkflowWithAssignandBase' debug inputs as
	  | # | Convert        | From | To      |
	  | 1 | [[[[a]]]] = 12 | Text | Base 64 |
      And the 'Base' in Workflow 'WorkflowWithAssignandBase' debug outputs as  
	  | # |              |
	  | 1 | [[b]] = MTI= |

#This scenario should pass after the bug 11872 is fixed	  
Scenario: Simple workflow with Assign and Base Convert(Evaluating Recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandBasec"
	 And "WorkflowWithAssignandBasec" contains an Assign "BaseVar" as
	  | variable    | value    |
	  | [[rs().a]]  | rec(1).a |
	  | [[rec().a]] | 12       |	
	   And "WorkflowWithAssignandBasec" contains Base convert "Base" as
	  | Variable       | From | To      |
	  | [[[[rs().a]]]] | Text | Base 64 |
	  When "WorkflowWithAssignandBasec" is executed
	  Then the workflow execution has "NO" error
	  And the 'BaseVar' in WorkFlow 'WorkflowWithAssignandBasec' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | rec(1).a  |
	  | 2 | [[rec().a]] = | 12        |
	  And the 'BaseVar' in Workflow 'WorkflowWithAssignandBasec' debug outputs as  
	  | # |                        |
	  | 1 | [[rs(1).a]] = rec(1).a |
	  | 2 | [[rec(1).a]] = 12      |
	   And the 'Base' in WorkFlow 'WorkflowWithAssignandBasec' debug inputs as
	  | # | Convert              | From | To      |
	  | 1 | [[[[rs(1).a]]]] = 12 | Text | Base 64 |
      And the 'Base' in Workflow 'WorkflowWithAssignandBasec' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = MTI= |
	  
#The below 2 scenarios should be passed after the bug 11873 is fixed
Scenario: Simple workflow with Assign and Case Convert(Evaluating scalar variable inside variable)executing against the server.
	 Given I have a workflow "WorkflowWithAssignandcCse"
	 And "WorkflowWithAssignandcCse" contains an Assign "Case Var" as
	  | variable | value    |
	  | [[a]]    | b        |
	  | [[b]]    | warewolf |	
	   And "WorkflowWithAssignandcCse" contains case convert "CaseConvert" as
	  | Variable  | Type  |
	  | [[[[a]]]] | UPPER |
	  When "WorkflowWithAssignandcCse" is executed
	  Then the workflow execution has "NO" error
	  And the 'Case Var' in WorkFlow 'WorkflowWithAssignandcCse' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | warewolf  |
	  And the 'Case Var' in Workflow 'WorkflowWithAssignandcCse' debug outputs as  
	  | # |                  |
	  | 1 | [[a]] = b        |
	  | 2 | [[b]] = warewolf |
	 And the 'CaseConvert' in WorkFlow 'WorkflowWithAssignandcCse' debug inputs as
	  | # | Convert              | To    |
	  | 1 | [[[[a]]]] = warewolf | UPPER |
	  And the 'CaseConvert' in Workflow 'WorkflowWithAssignandcCse' debug outputs as  
	  | # |                  |
	  | 1 | [[b]] = WAREWOLF |

Scenario: Simple workflow with Assign and Case Convert(Evaluating Recordset variable inside variable)executing against the server.
	 Given I have a workflow "WorkflowWithAssignandcCase"
	 And "WorkflowWithAssignandcCase" contains an Assign "Case Var" as
	   | variable    | value    |
	   | [[rs().a]]  | rec(1).a |
	   | [[rec().a]] | warewolf |	
	   And "WorkflowWithAssignandcCase" contains case convert "CaseConvert" as
	  | Variable        | Type  |
	  | [[[[rs(1).a]]]] | UPPER |
	  When "WorkflowWithAssignandcCase" is executed
	  Then the workflow execution has "NO" error
	  And the 'Case Var' in WorkFlow 'WorkflowWithAssignandcCase' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | rec(1).a  |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the 'Case Var' in Workflow 'WorkflowWithAssignandcCase' debug outputs as  
	  | # |                         |
	  | 1 | [[rs(1).a]] = rec(1).a  |
	  | 2 | [[rec(1).a]] = warewolf |
	 And the 'CaseConvert' in WorkFlow 'WorkflowWithAssignandcCase' debug inputs as
	  | # | Convert                    | To    |
	  | 1 | [[[[rs(1).a]]]] = warewolf | UPPER |
	  And the 'CaseConvert' in Workflow 'WorkflowWithAssignandcCase' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = WAREWOLF |

#This Test Scenario should be Passed after the bug 11874 is fixed.
Scenario: Simple workflow with Assign and Data Merge (Evaluating variables inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandData"
	 And "WorkflowWithAssignandData" contains an Assign "Datam" as
	  | variable    | value    |
	  | [[a]]       | b        |
	  | [[b]]       | warewolf |
	  | [[rs().a]]  | rec(1).a |
	  | [[rec().a]] | test     |
     And "WorkflowWithAssignandData" contains Data Merge "Datamerge" into "[[result]]" as	
	  | Variable       | Type  | Using | Padding | Alignment |
	  | [[[[a]]]]      | Index | 8     |         | Left      |
	  | [[[[rs().a]]]] | Index | 4     |         | Left      |
	 When "WorkflowWithAssignandData" is executed
	 Then the workflow execution has "NO" error
	 And the 'Datam' in WorkFlow 'WorkflowWithAssignandData' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | warewolf  |
	  | 3 | [[rs().a]] =  | rec(1).a  |
	  | 4 | [[rec().a]] = | test      |
	 And the 'Datam' in Workflow 'WorkflowWithAssignandData' debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = b              |
	  | 2 | [[b]] = warewolf       |
	  | 3 | [[rs(1).a]] = rec(1).a |
	  | 4 | [[rec(1).a]] = test    |
	 And the 'Datamerge' in WorkFlow 'WorkflowWithAssignandData' debug inputs as
	  | # |                        | With  | Using | Pad | Align |
	  | 1 | [[[[a]]]] = warewolf   | Index | "8"   | ""  | Left  |
	  | 2 | [[[[rs(1).a]]]] = test | Index | "4"   | ""  | Left  |
	  And the 'Datamerge' in Workflow 'WorkflowWithAssignandData' debug outputs as  
	  | # |                           |
	  | 1 | [[result]] = warewolftest |

Scenario: Simple workflow with Assign and Find Index(Evaluating scalar variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandFindIndex"
	 And "WorkflowWithAssignandFindIndex" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[a]]    | b     |
	  | [[b]]    | test  |	 	  
     And "WorkflowWithAssignandFindIndex" contains Find Index "Indexchar" into "[[indexResult]]" as
	  | In Fields | Index           | Character | Direction     |
	  | [[[[a]]]] | First Occurence | s         | Left to Right |
	  When "WorkflowWithAssignandFindIndex" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignandFindIndex' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignandFindIndex' debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	   And the 'Indexchar' in WorkFlow 'WorkflowWithAssignandFindIndex' debug inputs as 	
	  | In Field         | Index           | Characters | Direction     |
	  | [[[[a]]]] = test | First Occurence | s          | Left to Right |
	  And the 'Indexchar' in Workflow 'WorkflowWithAssignandFindIndex' debug outputs as 
	  |                     |
	  | [[indexResult]] = 3 |

# This Scenario should pass after the issue 11878 is fixed
Scenario: Simple workflow with Assign and Find Index(Evaluating recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandFindIndex1"
	 And "WorkflowWithAssignandFindIndex1" contains an Assign "Index Val" as
	  | variable    | value   |
	  | [[rec().a]] | new().a |
	  | [[new().a]] | test    |	 	  
     And "WorkflowWithAssignandFindIndex1" contains Find Index "Index char" into "[[indexResult]]" as
	  | In Fields       | Index           | Character | Direction     |
	  | [[[[rec().a]]]] | First Occurence | s         | Left to Right |
	  When "WorkflowWithAssignandFindIndex1" is executed
	  Then the workflow execution has "NO" error
	  And the 'Index Val' in WorkFlow 'WorkflowWithAssignandFindIndex1' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | test      |
	  And the 'Index Val' in Workflow 'WorkflowWithAssignandFindIndex1' debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = test    |
	   And the 'Index char' in WorkFlow 'WorkflowWithAssignandFindIndex1' debug inputs as 	
	  | In Field               | Index           | Characters | Direction     |
	  | [[[[rec(1).a]]]] = test | First Occurence | s          | Left to Right |
	  And the 'Index char' in Workflow 'WorkflowWithAssignandFindIndex1' debug outputs as 
	  |                     |
	  | [[indexResult]] = 3 |

Scenario: Simple workflow with Assign and Replace(Evaluating scalar variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandReplace"
	 And "WorkflowWithAssignandReplace" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[a]]    | b     |
	  | [[b]]    | test  |	 	  
      And "WorkflowWithAssignandReplace" contains Replace "Replac" into "[[replaceResult]]" as	
	  | In Fields | Find | Replace With |
	  | [[[[a]]]] | s    | REPLACE      |
	  When "WorkflowWithAssignandReplace" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignandReplace' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignandReplace' debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the 'Replac' in WorkFlow 'WorkflowWithAssignandReplace' debug inputs as 	
	 | In Field(s)         | Find | Replace With |
	 | [[[[a]]]] = test | s    | REPLACE      |
	    And the 'Replac' in Workflow 'WorkflowWithAssignandReplace' debug outputs as 
	  |                       |
	  | [[b]] = teREPLACEt    |
	  | [[replaceResult]] = 1 |

#This Scenario should be passed after the bug 11879 is fixed
Scenario: Simple workflow with Assign and Replace(Evaluating Recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandReplacebyrec"
	 And "WorkflowWithAssignandReplacebyrec" contains an Assign "Vals" as
	  | variable    | value   |
	  | [[rec().a]] | new().a |
	  | [[new().a]] | test    | 
      And "WorkflowWithAssignandReplacebyrec" contains Replace "Rep" into "[[replaceResult]]" as	
	  | In Fields        | Find | Replace With |
	  | [[[[rec(1).a]]]] | s    | REPLACE      |
	  When "WorkflowWithAssignandReplacebyrec" is executed
	  Then the workflow execution has "NO" error
	  And the 'Vals' in WorkFlow 'WorkflowWithAssignandReplacebyrec' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | test      |
	  And the 'Vals' in Workflow 'WorkflowWithAssignandReplacebyrec' debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = test    |
	  And the 'Rep' in WorkFlow 'WorkflowWithAssignandReplacebyrec' debug inputs as 	
	  | In Field(s)             | Find | Replace With |
	  | [[[[rec(1).a]]]] = test | s    | REPLACE      |
	    And the 'Rep' in Workflow 'WorkflowWithAssignandReplacebyrec' debug outputs as 
	  |                           |
	  | [[new(1).a]] = teREPLACEt |
	  | [[replaceResult]] = 1     |



Scenario: Simple workflow with Assign and Format Numbers(Evaluating scalar variable inside variable)executing against the server
	  Given I have a workflow "WorkflowWithAssignandFormat"
	  And "WorkflowWithAssignandFormat" contains an Assign "IndexVal" as
	  | variable | value   |
	  | [[a]]    | b       |
	  | [[b]]    | 12.3412 |	 	  
      And "WorkflowWithAssignandFormat" contains Format Number "Fnumber" as 
	  | Number    | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[[[a]]]] | Up                | 3           | 3               | [[fresult]] |
	  When "WorkflowWithAssignandFormat" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignandFormat' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12.3412   |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignandFormat' debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  | 2 | [[b]] = 12.3412 |
	  And the 'Fnumber' in WorkFlow 'WorkflowWithAssignandFormat' debug inputs as 	
	  | Number              | Rounding | Rounding Value | Decimals to show |
	  | [[[[a]]]] = 12.3412 | Up       | 3              | 3                |
	  And the 'Fnumber' in Workflow 'WorkflowWithAssignandFormat' debug outputs as 
	  |                      |
	  | [[fresult]] = 12.342 |

#This test is should be passed after the bug 11884
Scenario: Simple workflow with Assign and Format Numbers(Evaluating Recordset variable inside variable)executing against the server
	  Given I have a workflow "WorkflowWithAssignandFormatn"
	  And "WorkflowWithAssignandFormatn" contains an Assign "IndVal" as
	  | variable    | value   |
	  | [[rec().a]] | new().a |
	  | [[new().a]] | 12.3412 |	 	  
      And "WorkflowWithAssignandFormatn" contains Format Number "Fnumb" as 
	  | Number          | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[[[rec().a]]]] | Up                | 3           | 3               | [[fresult]] |
	  When "WorkflowWithAssignandFormatn" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndVal' in WorkFlow 'WorkflowWithAssignandFormatn' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | 12.3412   |
	  And the 'IndVal' in Workflow 'WorkflowWithAssignandFormatn' debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = 12.3412 |
	  And the 'Fnumb' in WorkFlow 'WorkflowWithAssignandFormatn' debug inputs as 	
	  | Number                    | Rounding | Rounding Value | Decimals to show |
	  | [[[[rec(1).a]]]] = 12.3412 | Up       | 3              | 3                |
	  And the 'Fnumb' in Workflow 'WorkflowWithAssignandFormatn' debug outputs as 
	  |                      |
	  | [[fresult]] = 12.342 |


#This scenario should be passed after the bug 11887 is fixed
Scenario: Simple workflow with Assign and Random(Evaluating recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandRandom"
	 And "WorkflowWithAssignandRandom" contains an Assign "Valforrandno" as
	  | variable    | value   |
	  | [[a]]       | b       |
	  | [[b]]       | 10      |
	  | [[rec().a]] | new().a |
	  | [[new().a]] | 20      |	 	  
	   And "WorkflowWithAssignandRandom" contains Random "Rand" as
	  | Type    | From      | To              | Result        |
	  | Numbers | [[[[a]]]] | [[[[rec().a]]]] | [[ranresult]] |
	  When "WorkflowWithAssignandRandom" is executed
	  Then the workflow execution has "NO" error
	  And the 'Valforrandno' in WorkFlow 'WorkflowWithAssignandRandom' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | 10        |
	  | 3 | [[rec().a]] = | new().a   |
	  | 4 | [[new().a]] = | 20        |
	  And the 'Valforrandno' in Workflow 'WorkflowWithAssignandRandom' debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = b              |
	  | 2 | [[b]] = 10             |
	  | 3 | [[rec(1).a]] = new().a |
	  | 4 | [[new(1).a]] = 20      |
	  And the 'Rand' in WorkFlow 'WorkflowWithAssignandRandom' debug inputs as 
	  | Random  | From           | To                   |
	  | Numbers | [[[[a]]]] = 10 | [[[[rec(1).a]]]] = 20 |
	  And the 'Rand' in Workflow 'WorkflowWithAssignandRandom' debug outputs as
	  |                       |
	  | [[ranresult]] = Int32 |

#This test scenario should be passed after the bug 11888 is fixed
Scenario: Simple workflow with Assign and Date and Time(Evaluating recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandDateTimetool"
	 And "WorkflowWithAssignandDateTimetool" contains an Assign "Dateandtime" as
	  | variable    | value      |
	  | [[a]]       | b          |
	  | [[b]]       | 01/02/2014 |
	  | [[rec().a]] | new().a    |
	  | [[new().a]] | dd/mm/yyyy |	 	  
	  And "WorkflowWithAssignandDateTimetool" contains Date and Time "AddDate" as
      | Input     | Input Format    | Add Time | Output Format | Result  |
      | [[[[a]]]] | [[[[rec(1).a]]]] | 1        | dd/mm/yyyy    | [[res]] |
	  When "WorkflowWithAssignandDateTimetool" is executed
	  Then the workflow execution has "NO" error
	  And the 'Dateandtime' in WorkFlow 'WorkflowWithAssignandDateTimetool' debug inputs as
	  | # | Variable      | New Value  |
	  | 1 | [[a]] =       | b          |
	  | 2 | [[b]] =       | 01/02/2014 |
	  | 3 | [[rec().a]] = | new().a    |
	  | 4 | [[new().a]] = |  dd/mm/yyyy|
	   And the 'Dateandtime' in Workflow 'WorkflowWithAssignandDateTimetool' debug outputs as  
	   | # |                            |
	   | 1 | [[a]] = b                  |
	   | 2 | [[b]] = 01/02/2014         |
	   | 3 | [[rec(1).a]] = new().a     |
	   | 4 | [[new(1).a]] =  dd/mm/yyyy |
	   And the 'AddDate' in WorkFlow 'WorkflowWithAssignandDateTimetool' debug inputs as
	   | Input                  | Input Format                  | Add Time |   | Output Format |
	   | [[[[a]]]] = 01/02/2014 | [[[[rec(1).a]]]] = dd/mm/yyyy | Years    | 1 | dd/mm/yyyy    |	
	   And the 'AddDate' in Workflow 'WorkflowWithAssignandDateTimetool' debug outputs as   
	   |                      |
	   | [[res]] = 01/02/2015 |
#
#  
#This test scenario should be passed after the bug 11888 is fixed
Scenario: Simple workflow with Assign and DateTimeDiff(Evaluating recordset variable inside variable)executing against the server
	  Given I have a workflow "WorkflowWithAssignandDateTimeDiff"
	  And "WorkflowWithAssignandDateTimeDiff" contains an Assign "Dateandtime" as
	   | variable    | value      |
	   | [[a]]       | b          |
	   | [[b]]       | 01/02/2016 |
	   | [[rec().a]] | new().a    |
	   | [[new().a]] | 01/02/2014 |	 	  
	  And "WorkflowWithAssignandDateTimeDiff" contains Date and Time Difference "DateTimedif" as
       | Input1           | Input2    | Input Format | Output In | Result     |
       | [[[[rec(1).a]]]] | [[[[a]]]] | dd/mm/yyyy   | Years     | [[result]] |  
	   When "WorkflowWithAssignandDateTimeDiff" is executed
	   Then the workflow execution has "NO" error
	   And the 'Dateandtime' in WorkFlow 'WorkflowWithAssignandDateTimeDiff' debug inputs as
	   | # | Variable      | New Value  |
	   | 1 | [[a]] =       | b          |
	   | 2 | [[b]] =       | 01/02/2016 |
	   | 3 | [[rec().a]] = | new().a    |
	   | 4 | [[new().a]] = | 01/02/2014 |
	   And the 'Dateandtime' in Workflow 'WorkflowWithAssignandDateTimeDiff' debug outputs as  
	   | # |                           |
	   | 1 | [[a]] = b                 |
	   | 2 | [[b]] = 01/02/2016        |
	   | 3 | [[rec(1).a]] = new().a    |
	   | 4 | [[new(1).a]] = 01/02/2014 |
	   And the 'DateTimedif' in WorkFlow 'WorkflowWithAssignandDateTimeDiff' debug inputs as
	   | Input 1                       | Input 2                | Input Format | Output In |
	   | [[[[rec(1).a]]]] = 01/02/2014 | [[[[a]]]] = 01/02/2016 | dd/mm/yyyy   | Years     |
	   And the 'DateTimedif' in Workflow 'WorkflowWithAssignandDateTimeDiff' debug outputs as   
	   |                |
	   | [[result]] = 2 |

Scenario: Simple workflow with Assign and Replace(Evaluating variable inside a varable)executing against the server
	 Given I have a workflow "WorkflowWithAssignReplace"
	 And "WorkflowWithAssignReplace" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[a]]    | b     |
	  | [[b]]    | test  |	 	  
      And "WorkflowWithAssignReplace" contains Replace "Replac" into "[[replaceResult]]" as	
	  | In Fields | Find | Replace With |
	  | [[[[a]]]] | s    | REPLACE      |
	  When "WorkflowWithAssignReplace" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignReplace' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignReplace' debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the 'Replac' in WorkFlow 'WorkflowWithAssignReplace' debug inputs as 	
	 | In Field(s)      | Find | Replace With |
	 | [[[[a]]]] = test | s    | REPLACE      |
	    And the 'Replac' in Workflow 'WorkflowWithAssignReplace' debug outputs as 
	  |                       |
	  | [[b]] = teREPLACEt    |
	  | [[replaceResult]] = 1 |

Scenario: Simple workflow with Assign and Format Numbers(Evaluating variable inside variable in format number tool)executing against the server
      Given I have a workflow "WorkflowAssignandFormat"
	  And "WorkflowAssignandFormat" contains an Assign "IndexVal1" as
	  | variable | value   |
	  | [[a]]    | b       |
	  | [[b]]    | 12.3412 |	 	  
      And "WorkflowAssignandFormat" contains Format Number "Fnumber1" as 
	  | Number    | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[[[a]]]] | Up                | 3           | 3               | [[fresult]] |
	  When "WorkflowAssignandFormat" is executed
	  Then the workflow execution has "NO" error
	  And the 'IndexVal1' in WorkFlow 'WorkflowAssignandFormat' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12.3412   |
	  And the 'IndexVal1' in Workflow 'WorkflowAssignandFormat' debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  | 2 | [[b]] = 12.3412 |
	  And the 'Fnumber1' in WorkFlow 'WorkflowAssignandFormat' debug inputs as 	
	  | Number              | Rounding | Rounding Value | Decimals to show |
	  | [[[[a]]]] = 12.3412 | Up       | 3              | 3                |
	  And the 'Fnumber1' in Workflow 'WorkflowAssignandFormat' debug outputs as 
	  |                      |
	  | [[fresult]] = 12.342 |

Scenario: Simple workflow with Assign DataMerge and DataSplit(Evaluating recordset variable as index variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignDatamergeandSplit"
	 And "WorkflowWithAssignDatamergeandSplit" contains an Assign "Data" as
	  | variable     | value    |
	  | [[a]]        | 1        |
	  | [[b]]        | 2        |
	  | [[rec(1).a]] | warewolf |
	  | [[rec(2).a]] | test     |	
      And "WorkflowWithAssignDatamergeandSplit" contains Data Merge "Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 8     |         | Left      |
	  | [[a]]        | Index | 4     |         | Left      |
	  And "WorkflowWithAssignDatamergeandSplit" contains Data Split "DataSplit" as
	  | String       | Variable | Type  | At | Include    | Escape |
	  | [[rec(1).a]] | [[d]]    | Index | 4  | Unselected |        |
	  |              | [[c]]    | Index | 4  | Unselected |        |
	  When "WorkflowWithAssignDatamergeandSplit" is executed
	  Then the workflow execution has "NO" error
	  And the 'Data' in WorkFlow 'WorkflowWithAssignDatamergeandSplit' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[a]] =        | 1         |
	  | 2 | [[b]] =        | 2         |
	  | 3 | [[rec(1).a]] = | warewolf  |
	  | 4 | [[rec(2).a]] = | test      |
	  And the 'Data' in Workflow 'WorkflowWithAssignDatamergeandSplit' debug outputs as 
	  | # |                         |
	  | 1 | [[a]] = 1               |
	  | 2 | [[b]] = 2               |
	  | 3 | [[rec(1).a]] = warewolf |
	  | 4 | [[rec(2).a]] = test     |  	
      And the 'Merge' in WorkFlow 'WorkflowWithAssignDatamergeandSplit' debug inputs as
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = warewolf | Index | "8"   | ""  | Left  |
	  | 2 | [[a]] = 1               | Index | "4"   | ""  | Left  |
	  And the 'Merge' in Workflow 'WorkflowWithAssignDatamergeandSplit' debug outputs as
	  |                        |
	  | [[result]] = warewolf1 |
	  And the 'DataSplit' in WorkFlow 'WorkflowWithAssignDatamergeandSplit' debug inputs as  
	  | String to Split         | Process Direction | Skip blank rows | # |         | With  | Using | Include | Escape |
	  | [[rec(1).a]] = warewolf | Forward           | No              | 1 | [[d]] = | Index | 4     | No      |        |
	  |                         |                   |                 | 2 | [[c]] = | Index | 4     | No      |        |
	  And the 'DataSplit' in Workflow 'WorkflowWithAssignDatamergeandSplit' debug outputs as
	  | # |              |
	  | 1 | [[d]] = ware |
	  | 2 | [[c]] = wolf |


#This Test Scenario should be passed after the bug 11889 is fixed
# It will not since there is a bug in the Debug Input generation logic OF THE ACCEPTANCE TESTING when using DataMerge and recursive evaluation.
# The server seems to generate exactly what it should
# There might be a bug in studio rendering as well given item 3 never appears in the studio output
 #Scenario: Simple workflow with Assign DataMerge and DataSplit(Evaluating index recordset variable)executing against the server
	# Given I have a workflow "WorkflowWithAssignamergeandSplit"
	# And "WorkflowWithAssignamergeandSplit" contains an Assign "Data" as
	#  | variable       | value    |
	#  | [[a]]          | 1        |
	#  | [[b]]          | 2        |
	#  | [[rec(1).a]]   | warewolf |
	#  | [[rec(2).a]]   | test     |
	#  | [[index(1).a]] | 1        |
	#  | [[index(2).a]] | 3        |	
 #     And "WorkflowWithAssignamergeandSplit" contains Data Merge "Merge" into "[[result]]" as	
	#  | Variable                             | Type  | Using | Padding | Alignment |
	#  | [[rec([[index(1).a]]).a]] = warewolf | Index | 8     |         | Left      |
	#  | [[a]]                                | Index | 4     |         | Left      |
	#  And "WorkflowWithAssignamergeandSplit" contains Data Split "DataSplit" as
	#  | String       | Variable                  | Type  | At | Include    | Escape |
	#  | [[rec(1).a]] | [[d]]                     | Index | 4  | Unselected |        |
	#  |              | [[rec([[index(2).a]]).a]] | Index | 4  | Unselected |        |
	#  When "WorkflowWithAssignamergeandSplit" is executed
	#  Then the workflow execution has "NO" error
	#  And the 'Data' in WorkFlow 'WorkflowWithAssignamergeandSplit' debug inputs as
	#  | # | Variable         | New Value |
	#  | 1 | [[a]] =          | 1         |
	#  | 2 | [[b]] =          | 2         |
	#  | 3 | [[rec(1).a]] =   | warewolf  |
	#  | 4 | [[rec(2).a]] =   | test      |
	#  | 5 | [[index(1).a]] = | 1         |
	#  | 6 | [[index(2).a]] = | 3         |
	#  And the 'Data' in Workflow 'WorkflowWithAssignamergeandSplit' debug outputs as 
	#  | # |                         |
	#  | 1 | [[a]] = 1               |
	#  | 2 | [[b]] = 2               |
	#  | 3 | [[rec(1).a]] = warewolf |
	#  | 4 | [[rec(2).a]] = test     |
	#  | 5 | [[index(1).a]] = 1      |
	#  | 6 | [[index(2).a]] = 3      |  	
 #     And the 'Merge' in WorkFlow 'WorkflowWithAssignamergeandSplit' debug inputs as
	#  | # |                                      | With  | Using | Pad | Align |
	#  | 1 | [[rec([[index(1).a]]).a]] = warewolf | Index | "8"   | ""  | Left  |
	#  | 2 | [[a]] = 1                            | Index | "4"   | ""  | Left  |
	#  And the 'Merge' in Workflow 'WorkflowWithAssignamergeandSplit' debug outputs as
	#  |                        |
	#  | [[result]] = warewolf1 |
	#  And the 'DataSplit' in WorkFlow 'WorkflowWithAssignamergeandSplit' debug inputs as  
	#  | String to Split         | Process Direction | Skip blank rows | # |                                          | With  | Using | Include | Escape |
	#  | [[rec(1).a]] = warewolf | Forward           | No              | 1 | [[d]] =                                  | Index | 4     | No      |        |
	#  |                         |                   |                 | 2 | [[rec([[index(2).a]]).a]] = [[rec(2).a]] | Index | 4     | No      |        |
	#  And the 'DataSplit' in Workflow 'WorkflowWithAssignamergeandSplit' debug outputs as
	#  | # |                     |
	#  | 1 | [[d]] = ware        |
	#  | 2 | [[rec(2).a]] = wolf |

#This scenario should be passed after the bug 11890 is resolved.
Scenario: Simple workflow with 2 Assign tools evaluating recordset index variables.
	 Given I have a workflow "WorkflowWithAssignandAssign"
	 And "WorkflowWithAssignandAssign" contains an Assign "Data1" as
	  | variable       | value |
	  | [[a]]          | 1     |
	  | [[rec(1).a]]   | 2     |
	  | [[index(1).a]] | 2     |
	   And "WorkflowWithAssignandAssign" contains an Assign "Data2" as
	  | variable             | value    |
	  | [[new([[a]]).a]]     | test     |
	  | [[rec([[index(1).a]]).a]] | warewolf |
	  When "WorkflowWithAssignandAssign" is executed
	  Then the workflow execution has "NO" error
	  And the 'Data1' in WorkFlow 'WorkflowWithAssignandAssign' debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | 2         |
	  | 3 | [[index(1).a]] = | 2         |
	  And the 'Data1' in Workflow 'WorkflowWithAssignandAssign' debug outputs as 
	  | # |                        |
	  | 1 | [[a]] = 1              |
	  | 2 | [[rec(1).a]] = 2   |
	  | 3 | [[index(1).a]] = 2 |
	   And the 'Data2' in WorkFlow 'WorkflowWithAssignandAssign' debug inputs as
	  | # | Variable                  | New Value |
	  | 1 | [[new(1).a]] =        | test      |
	  | 2 | [[rec(2).a]] = | warewolf  |
	  And the 'Data2' in Workflow 'WorkflowWithAssignandAssign' debug outputs as 
	  | # |                         |
	  | 1 | [[new(1).a]] = test     |
	  | 2 | [[rec(2).a]] = warewolf |

Scenario: Workflow with Assign Calculate
      Given I have a workflow "WFWithAssignCalculateindexrecordset"
	  And "WFWithAssignCalculateindexrecordset" contains an Assign "values1" as
      | variable       | value |
      | [[a]]          | 1     |
      | [[rec(1).a]]   | 2     |
      | [[index(1).a]] | 1     |
      | [[rec(2).a]]   | 6     |
	  And "WFWithAssignCalculateindexrecordset" contains Calculate "Calculate1" with formula "[[rec([[index(1).a]]).a]]+[[a]]" into "[[result]]"
	  When "WFWithAssignCalculateindexrecordset" is executed
	  Then the workflow execution has "NO" error
	  And the 'values1' in WorkFlow 'WFWithAssignCalculateindexrecordset' debug inputs as 
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | 2         |
	  | 3 | [[index(1).a]] = | 1         |
	  | 4 | [[rec(2).a]] =   | 6         |
	 And the 'values1' in Workflow 'WFWithAssignCalculateindexrecordset' debug outputs as   
	  | # |                    |
	  | 1 | [[a]]         =  1 |
	  | 2 | [[rec(1).a]]  =  2 |
	  | 3 | [[index(1).a]] = 1 |
	  | 4 | [[rec(2).a]]   = 6 |
	  And the 'Calculate1' in WorkFlow 'WFWithAssignCalculateindexrecordset' debug inputs as 
      | fx =                                 |
      | [[rec([[index(1).a]]).a]]+[[a]] = [[rec(1).a]]+1 = 2+1 |           
      And the 'Calculate1' in Workflow 'WFWithAssignCalculateindexrecordset' debug outputs as  
	  |                |
	  | [[result]] = 3 |

#This Scenario should be passed after the bug 11714 is fixed
#Scenario: Workflow with Assign and ForEach
#     Given I have a workflow "WFWithForEach"
#     And I have a ForEach "ForEachTest" as "NumOfExecution" executions "3"
#	 And "ForEachTest" contains workflow "Test" with mapping as
#	 | Input to Service | From Variable | Output from Service | To Variable |
#	 | a                | Warewolf      |                     |             |
#	 When "WFWithForEach" is executed
#	 Then the workflow execution has "NO" error
#	 And the 'ForEachTest' in WorkFlow 'WFWithForEach' debug inputs as 
#	    |                 | Number |
#	    | No. of Executes | 2      |
#	    And the 'Test' in WorkFlow 'WFWithForEach' debug inputs as     
#	        | # | Variable | New Value | inputs   | Execute workflow asynchronously |
#	        | 1 | [[a]] =  | Warewolf  | Warewolf | False                           |
#	    And the 'Test' in Workflow 'WFWithForEach' debug outputs as
#	        | # |                   | Number of steps |
#	        | 1 | [[a]] =  Warewolf | 1               |
#        And the 'Test' in WorkFlow 'WFWithForEach' debug inputs as     
#	        | # | Variable | New Value | inputs   | Execute workflow asynchronously |
#	        | 1 | [[a]] =  | Warewolf  | Warewolf | False                           |
#	    And the 'Test' in Workflow 'WFWithForEach' debug outputs as
#	        | # |                   | Number of steps |
#	        | 1 | [[a]] =  Warewolf | 2               |

#This Test should pass after the bug 11539 is fixed
#Scenario: Test Mappings for Assign and Calculate Workflow 
#      Given I have a workflow "TestMappings"
#	  And "TestMappings" contains an Assign "values1" as
#      | variable       | value |
#      | [[rec(1).a]]   | 1     |
#      | [[rec(1).b]]   | 2     |
#	  And "TestMappings" contains Calculate "Calculate1" with formula "[[rec(1).a]]+[[rec(1).b]]" into "[[rec(1).c]]"
#	  And "WorkflowWithAssignBaseConvertandCaseconvert" inputs
#	  | Inputs |
#	  |        |
#	  And "WorkflowWithAssignBaseConvertandCaseconvert" Outputs
#	  | Outputs |
#	  | rec().a |
#	  | rec().b |
#	  When "TestMappings" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'values1' in WorkFlow 'TestMappings' debug inputs as 
#	  | # | Variable       | New Value |
#	  | 1 | [[rec(1).a]] = | 1         |
#	  | 2 | [[rec(1).b]] = | 2         |
#	  And the 'values1' in Workflow 'TestMappings' debug outputs as   
#	  | # |                           |
#	  | 1 | [[rec(1).a]]         =  1 |
#	  | 2 | [[rec(1).b]]  =  2        |
#	  And the 'Calculate1' in WorkFlow 'TestMappings' debug inputs as 
#      | fx =                                 |
#      | [[rec(1).a]]+[[rec(1).b]] = 1+2 |           
#      And the 'Calculate1' in Workflow 'TestMappings' debug outputs as  
#	  |                  |
#	  | [[rec(1).c]] = 3 |
#	  And the 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as
#	  |                  |
#	  | [[rec(1).a]] = 1 |
#	  | [[rec(1).b]] = 2 |
#	  | [[rec(1).c]] = 3 |



#This Test scenario should be passed after the bug 12016 is fixed
# Scenario: Workflow with Assign and Replace by using recordset star
# Given I have a workflow "workflowithAssignandreplaces"
#      And "workflowithAssignandreplaces" contains an Assign "Assignee" as
#      | variable    | value |
#      | [[rec().a]] | a     |
#      | [[rec().a]] | b     | 
#	  And "WorkflowWithAssignandReplaces" contains Replace "Rep" into "[[rec().a]]" as	
#	  | In Fields    | Find         | Replace With |
#	  | [[rec(*).a]] | [[rec(*).a]] | Warewolf     |
#	  When "workflowithAssignandreplaces" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Assignee' in WorkFlow 'workflowithAssignandreplaces' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | a         |
#	  | 2 | [[rec().a]] = | b         |
#	  And the 'Assignee' in Workflow 'workflowithAssignandreplaces' debug outputs as    
#	  | # |                  |
#	  | 1 | [[rec(1).a]] = a |
#	  | 2 | [[rec(2).a]] = b |
#	  And the 'Rep' in WorkFlow 'Test1' debug inputs as 
#	  | In Field(s)      | Find             | Replace With |
#	  | [[rec(1).a]] = a | [[rec(1).a]] = a |              |
#	  | [[rec(2).a]] = b | [[rec(2).a]] = b | Warewolf     |
#	  And the 'Rep' in Workflow 'Test1' debug outputs as
#	  |                         |
#	  | [[rec(1).a]] = Warewolf |
#	  | [[rec(2).a]] = Warewolf |
#	  | [[rec(3).a]] = 2        |


#The below 12 scenario should be passed after the bug 11994 is fixed

#Scenario: Workflow with Assign and Find Record index
#      Given I have a workflow "WFWithAssignandFindRecordindex"
#	  And "WFWithAssignandFindRecordindex" contains an Assign "Record" as
#      | # | variable     | value    |
#      | # | [[rec(1).a]] | Warewolf |
#	  And "WFWithAssignandFindRecordindex" contains Find Record Index "FindRecord" into result as "[[a]][[b]]"
#      | # | In Field     | #        | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
#      | # | [[rec(*).a]] | 1        | =          | Warewolf | YES                            | NO |
#	  When "WFWithAssignandFindRecordindex" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
#	  | # | Variable       | New Value |
#	  | 1 | [[rec(1).a]] = | Warewolf  | 
#	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as   
#	  | # |                                  |
#	  | 1 | [[rec(1).a]]         =  Warewolf |
#	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
#	  | #           |                         | # |   |          |  | And | Require All Fields To Match | Require All Matches To Be True |
#	  | In Field(s) | [[rec(1).a]] = Warewolf | 1 | = | Warewolf |  |     | YES                         | NO                             |
#	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as   
#	  |           |
#	  | [[a]] = 1 |
#	  | [[b]] = 1 |
#	
#Scenario: Testing Count with two variables in Result field
#      Given I have a workflow "WorkflowforCount"
#      And "WorkflowforCount" contains an Assign "Rec To Convert" as
#	  | variable    | value |
#	  | [[rec().a]] | 1213  |
#	  | [[rec().a]] | 4561  |
#	  And "WorkflowforCount" contains Count Record "CountRec" on "[[rec(*)]]" into "[[count]][[a]]"
#	  When "WorkflowforCount" is executed
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforCount' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | 1213      |
#	  | 2 | [[rec().a]] = | 4561      |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforCount' debug outputs as    
#	  | # |                     |
#	  | 1 | [[rec(1).a]] = 1213 |
#	  | 2 | [[rec(2).a]] = 4561 |
#	  And the 'CountRec' in WorkFlow 'WorkflowforCount' debug inputs as
#	  | Recordset            |
#	  | [[rec(1).a]] = 1213 |
#	  | [[rec(2).a]] = 4561 |
#	  And the 'CountRec' in Workflow 'WorkflowforCount' debug outputs as    
#	  |               |
#	  | [[count]] = 2 |
#	  | [[a]] =       |
#
#Scenario: Testing Length with two variables in Result field
#      Given I have a workflow "WorkflowforLength"
#      And "WorkflowforLength" contains an Assign "Rec To Convert" as
#	  | variable    | value |
#	  | [[rec().a]] | 1213  |
#	  | [[rec().a]] | 4561  |
#	  And "WorkflowforLength" contains Length "Len" on "[[rec(*)]]" into "[[length]][[a]]"
#	  When "WorkflowforLength" is executed
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforLength' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | 1213      |
#	  | 2 | [[rec().a]] = | 4561      |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforLength' debug outputs as    
#	  | # |                     |
#	  | 1 | [[rec(1).a]] = 1213 |
#	  | 2 | [[rec(2).a]] = 4561 |
#	  And the 'Len' in WorkFlow 'WorkflowforLength' debug inputs as
#	  | Recordset           |
#	  | [[rec(1).a]] = 1213 |
#	  | [[rec(2).a]] = 4561 |
#	  And the 'Len' in Workflow 'WorkflowforLength' debug outputs as    
#	  |                |
#	  | [[length]] = 2 |
#	  | [[a]] =        |
#
#Scenario: Testing Find Index with two variables in Result field
#      Given I have a workflow "WorkflowforFI"
#      And "WorkflowforFI" contains an Assign "Rec To Convert" as
#	  | variable    | value |
#	  | [[rec().a]] | 141   |
#	  | [[rec().a]] | 4561  |
#	  And "WorkflowforFI" contains Find Index "Index" into "[[a]][[indexResult]]" as
#	  | In Fields    | Index         | Character | Direction     |
#	  | [[rec(*).a]] | All Occurence | 1         | Left to Right |	
#	  When "WorkflowforFI" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforFI' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | 141       |
#	  | 2 | [[rec().a]] = | 4561      |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforFI' debug outputs as    
#	  | # |                     |
#	  | 1 | [[rec(1).a]] = 141  |
#	  | 2 | [[rec(2).a]] = 4561 |
#	  And the 'Index' in WorkFlow 'WorkflowforFI' debug inputs as
#	  | In Field            | Index         | Characters | Direction     |
#	  | [[rec(1).a]] = 141  |               |            |               |
#	  | [[rec(2).a]] = 4561 | All Occurence | 1          | Left to Right |
#	  And the 'Index' in Workflow 'WorkflowforFI' debug outputs as
#	  |                   |
#	  | [[a]]  = 1,3,4    |
#	  | [[indexResult]] = |
#
#Scenario: Testing Data Merge with two variables in Result field
#      Given I have a workflow "WorkflowforDataMerge"
#      And "WorkflowforDataMerge" contains an Assign "Rec To Convert" as
#	  | variable    | value    |
#	  | [[rec().a]] | Test     |
#	  | [[rec().a]] | Warewolf |
#	  And "WorkflowforDataMerge" contains Data Merge "Data Merge" into "[[result]][[a]]" as	
#	  | Variable     | Type | Using | Padding | Alignment |
#	  | [[rec(1).a]] | None |       |         | Left      |
#	  | [[rec(2).a]] | None |       |         | Left      |
#	  When "WorkflowforDataMerge" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforDataMerge' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | Test      |
#	  | 2 | [[rec().a]] = | Warewolf  |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforDataMerge' debug outputs as    
#	  | # |                         |
#	  | 1 | [[rec(1).a]] = Test     |
#	  | 2 | [[rec(2).a]] = Warewolf |
#	 And the 'Data Merge' in WorkFlow 'WorkflowforDataMerge' debug inputs as 
#	  | # |                         | With | Using | Pad | Align |
#	  | 1 | [[rec(1).a]] = Test     | None | ""    | ""  | Left  |
#	  | 2 | [[rec(2).a]] = Warewolf | None | ""    | ""  | Left  |
#	  And the 'Data Merge' in Workflow 'WorkflowforDataMerge' debug outputs as  
#	  |                           |
#	  | [[result]] = TestWarewolf |
#	  | [[a]] =                   |
#
#Scenario: Testing Data Split with two variables in Result field
#      Given I have a workflow "WorkflowforDatasplit"
#      And "WorkflowforDatasplit" contains an Assign "Rec To Convert" as
#	  | variable    | value    |
#	  | [[rec().a]] | Test     |
#	  | [[rec().a]] | Warewolf |
#	  And "WorkflowforDatasplit" contains Data Split "Data Split" as
#	  | String       | Variable        | Type  | At | Include    | Escape |
#	  | [[rec(1).a]] | [[fr().a]][[a]] | Index | 2  | Unselected |        |
#	  |              | [[fr().b]][[b]] | Index | 2  | Unselected |        |
#	  When "WorkflowforDatasplit" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforDatasplit' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | Test      |
#	  | 2 | [[rec().a]] = | Warewolf  |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforDatasplit' debug outputs as    
#	  | # |                         |
#	  | 1 | [[rec(1).a]] = Test     |
#	  | 2 | [[rec(2).a]] = Warewolf |
#	 And the 'Data Split' in WorkFlow 'WorkflowforDatasplit' debug inputs as 
#	  | String to Split     | Process Direction | Skip blank rows | # |                   | With  | Using | Include | Escape |
#	  | [[rec(1).a]] = Test | Forward           | No              | 1 | [[fr().a]][[a]] = | Index | 2     | No      |        |
#	  |                     |                   |                 | 2 | [[fr().b]][[b]] = | Index | 2     | No      |        |
#	  And the 'Data Split' in Workflow 'WorkflowforDatasplit' debug outputs as  
#	  | # |                    |
#	  | 1 | [[fr(1).a]][[a]] = |
#	  | 2 | [[fr(1).b]][[b]] = |
#	
#Scenario: Testing Replace with two variables in Result field
#      Given I have a workflow "WorkflowforReplace"
#      And "WorkflowforReplace" contains an Assign "Rec To Convert" as
#	  | variable    | value    |
#	  | [[rec().a]] | Test     |
#	  | [[rec().a]] | Warewolf |
#	  And "WorkflowforReplace" contains Replace "Replac" into "[[a]][[b]][[c]]" as	
#	  | In Fields    | Find | Replace With |
#	  | [[rec(*).a]] | Test | rocks        |
#	  When "WorkflowforReplace" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforReplace' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | Test      |
#	  | 2 | [[rec().a]] = | Warewolf  |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforReplace' debug outputs as    
#	  | # |                         |
#	  | 1 | [[rec(1).a]] = Test     |
#	  | 2 | [[rec(2).a]] = Warewolf |
#	  And the 'Replac' in WorkFlow 'WorkflowforReplace' debug inputs as 	
#	  | In Field(s)             | Find | Replace With |
#	  | [[rec(1).a]] = Test     |      |              |
#	  | [[rec(2).a]] = Warewolf | Test | rocks        |
#	  And the 'Replac' in Workflow 'WorkflowforReplace' debug outputs as 
#	  |                      |
#	  | [[rec(1).a]] = rocks |
#	  | [[a]] = 1            |
#	  | [[b]] =              |
#	  | [[c]] =              |
#	
#Scenario: Testing Calculate with two variables in Result field
#      Given I have a workflow "WorkflowforCal"
#      And "WorkflowforCal" contains an Assign "Values" as
#	  | variable | value |
#	  | [[a]]    | 1     |
#	  | [[b]]    | 2     |
#	 And "WorkflowforCal" contains Calculate "Calculate1" with formula "[[a]]+[[b]]" into "[[result]][[c]]"	 
#	  When "WorkflowforCal" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Values' in WorkFlow 'WorkflowforCal' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | 1         |
#	  | 2 | [[b]] =  | 2         |
#	  And the 'Values' in Workflow 'WorkflowforCal' debug outputs as    
#	  | # |           |
#	  | 1 | [[a]] = 1 |
#	  | 2 | [[b]] = 2 |
#	  And the 'Calculate1' in WorkFlow 'WorkflowforCal' debug inputs as 
#      | fx =                  |
#      | [[a]]+[[b]] = 1+2 = 2 |           
#      And the 'Calculate1' in Workflow 'WorkflowforCal' debug outputs as  
#	  |                |
#	  | [[result]] = 3 |
#	  | [[c]] =        |
#
#Scenario: Testing Format Numbers with two variables in Result field
#      Given I have a workflow "Workflowforfn"
#	  And "Workflowforfn" contains Format Number "Fnumber" as 
#	  | Number  | Rounding Selected | Rounding To | Decimal to show | Result                |
#	  | 123.568 | Up                | 2           | 2               | [[fresult]][[a]][[b]] |
#	  When "Workflowforfn" is executed  	  
#	  Then the workflow execution has "AN" error
#	  And the 'Fnumber' in WorkFlow 'Workflowforfn' debug inputs as 	
#	  | Number  | Rounding | Rounding Value | Decimals to show |
#	  | 123.568 | Up       | 2              | 2                |
#	  And the 'Fnumber' in Workflow 'Workflowforfn' debug outputs as 
#	  |                      |
#	  | [[fresult]] = 123.57 |
#	  | [[a]] =              |
#	  | [[b]] =              |
#
# Scenario: Testing Random Numbers with two variables in Result field
#      Given I have a workflow "Workflowforrandom"
#	  And "Workflowforrandom" contains Random "Rand1" as
#	  | Type    | From | To | Result               |
#	  | Numbers | 1    | 10 | [[result]][[a]][[b]] |
#	  When "Workflowforrandom" is executed  	  
#	  Then the workflow execution has "AN" error
#	  And the 'Rand1' in WorkFlow 'Workflowforrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 10 |
#      And the 'Rand1' in Workflow 'Workflowforrandom' debug outputs as
#	  |                    |
#	  | [[result]] = Int32 |
#	  | [[a]] =            |
#	  | [[b]] =            |
#
#Scenario: Testing Date and Time with two variables in Result field
#      Given I have a workflow "WorkflowforDT"
#      And "WorkflowforDT" contains an Assign "Rec To Convert" as
#	  | variable    | value      |
#	  | [[rec().a]] | 12/01/2001 |
#	  And "WorkflowforDT" contains Date and Time "AddDate" as
#      | Input       | Input Format | Add Time | Output Format | Result            |
#      | [[rec().a]] | dd/mm/yyyy   | 1        | dd/mm/yyyy    | [[res]][[a]][[b]] |	
#	  When "WorkflowforDT" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Rec To Convert' in WorkFlow 'WorkflowforDT' debug inputs as
#	  | # | Variable      | New Value  |
#	  | 1 | [[rec().a]] = | 12/01/2001 |
#	  And the 'Rec To Convert' in Workflow 'WorkflowforDT' debug outputs as    
#	  | # |                           |
#	  | 1 | [[rec(1).a]] = 12/01/2001 |
#	  And the 'AddDate' in WorkFlow 'WorkflowforDT' debug inputs as
#	   | Input                     | Input Format | Add Time |   | Output Format |
#	   | [[rec(1).a]] = 12/01/2001 | dd/mm/yyyy   | Years    | 1 | dd/mm/yyyy    |	
#	  And the 'AddDate' in Workflow 'WorkflowforDT' debug outputs as   
#	   |                      |
#	   | [[res]] = 12/01/2002 |
#	   | [[a]] =              |
#	   | [[b]]] =             |
#	   
#Scenario: Testing Date Time Diff with two variables in Result field
#      Given I have a workflow "WorkflowforDateTimeDiff"
#      And "WorkflowforDateTimeDiff" contains an Assign "Values" as
#	  | variable    | value      |
#	  | [[rec().a]] | 01/01/2001 |
#	  | [[rec().a]] | 01/01/2010 |
#	  And "WorkflowforDateTimeDiff" contains Date and Time Difference "DateAndTime" as	
#	  | Input1       | Input2       | Input Format | Output In | Result               |
#	  | [[rec(1).a]] | [[rec(2).a]] | dd/mm/yyyy   | Years     | [[result]][[a]][[b]] |	   
#	  When "WorkflowforDateTimeDiff" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Values' in WorkFlow 'WorkflowforDateTimeDiff' debug inputs as
#	  | # | Variable      | New Value  |
#	  | 1 | [[rec().a]] = | 01/01/2001 |
#	  | 2 | [[rec().a]] = | 01/01/2010 |
#	  And the 'Values' in Workflow 'WorkflowforDateTimeDiff' debug outputs as    
#	  | # |                           |
#	  | 1 | [[rec(1).a]] = 01/01/2001 |
#	  | 2 | [[rec(2).a]] = 01/01/2010 |
#	  And the 'DateAndTime' in WorkFlow 'WorkflowforDateTimeDiff' debug inputs as
#	  | Input 1                   | Input 2                   | Input Format | Output In |
#	  | [[rec(1).a]] = 01/01/2001 | [[rec(2).a]] = 01/01/2010 | dd/mm/yyyy   | Years     |
#	  And the 'DateAndTime' in Workflow 'WorkflowforDateTimeDiff' debug outputs as 
#	  |                 |
#	  | [[result1]] = 9 |
#	  | [[a]] =         |
#	  | [[b]] =         |   

#This Test scenario should be passed after the bug 12016 is fixed
# Scenario: Workflow with Assign and Replace by using recordset star
# Given I have a workflow "workflowithAssignandreplaces"
#      And "workflowithAssignandreplaces" contains an Assign "Assignee" as
#      | variable    | value |
#      | [[rec().a]] | a     |
#      | [[rec().a]] | b     | 
#	  And "WorkflowWithAssignandReplaces" contains Replace "Rep" into "[[rec().a]]" as	
#	  | In Fields    | Find         | Replace With |
#	  | [[rec(*).a]] | [[rec(*).a]] | Warewolf     |
#	  When "workflowithAssignandreplaces" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Assignee' in WorkFlow 'workflowithAssignandreplaces' debug inputs as
#	  | # | Variable      | New Value |
#	  | 1 | [[rec().a]] = | a         |
#	  | 2 | [[rec().a]] = | b         |
#	  And the 'Assignee' in Workflow 'workflowithAssignandreplaces' debug outputs as    
#	  | # |                  |
#	  | 1 | [[rec(1).a]] = a |
#	  | 2 | [[rec(2).a]] = b |
#	  And the 'Rep' in WorkFlow 'Test1' debug inputs as 
#	  | In Field(s)      | Find             | Replace With |
#	  | [[rec(1).a]] = a | [[rec(1).a]] = a |              |
#	  | [[rec(2).a]] = b | [[rec(2).a]] = b | Warewolf     |
#	  And the 'Rep' in Workflow 'Test1' debug outputs as
#	  |                         |
#	  | [[rec(1).a]] = Warewolf |
#	  | [[rec(1).a]] = Warewolf |
#	  | [[rec(3).a]] = 2        |

Scenario: Workflow with Assign and Sort Forward to test gaps
      Given I have a workflow "workflowithAssignandsortrec"
      And "workflowithAssignandsortrec" contains an Assign "sortval" as
	  | variable    | value |
	  | [[rs(1).a]] | 30    |
	  | [[rs(5).a]] | 20    |
	  | [[rs(7).a]] | 10    |
	  | [[rs(2).b]] | 6     |
	  | [[rs(4).b]] | 4     |
	  | [[rs(6).b]] | 2     |
	  And "workflowithAssignandsortrec" contains an Sort "sortRec" as
	  | Sort Field | Sort Order |
	  | [[rs(*).a | Forward    |
	  When "workflowithAssignandsortrec" is executed
	  Then the workflow execution has "NO" error
	  And the 'sortval' in WorkFlow 'workflowithAssignandsortrec' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 30        |
	  | 2 | [[rs(5).a]] = | 20        |
	  | 3 | [[rs(7).a]] = | 10        |
	  | 4 | [[rs(2).b]] = | 6         |
	  | 5 | [[rs(4).b]] = | 4         |
	  | 6 | [[rs(6).b]] = | 2         |
	  And the 'sortval' in Workflow 'workflowithAssignandsortrec' debug outputs as    
	  | # |                  |
	  | 1 | [[rs(1).a]] = 30 |
	  | 2 | [[rs(5).a]] = 20 |
	  | 3 | [[rs(7).a]] = 10 |
	  | 4 | [[rs(2).b]] = 6  |
	  | 5 | [[rs(4).b]] = 4  |
	  | 6 | [[rs(6).b]] = 2  |
	  And the 'sortRec' in WorkFlow 'workflowithAssignandsortrec' debug inputs as
	  | Sort Field       | Sort Order |
	  | [[rs(1).a]] = 30 |            |
	  | [[rs(2).a]] =    |            |
	  | [[rs(4).a]] =    |            |
	  | [[rs(5).a]] = 20 |            |
	  | [[rs(6).a]] =    |            |
	  | [[rs(7).a]] = 10 | Forward    |
	  And the 'sortRec' in Workflow 'workflowithAssignandsortrec' debug outputs as
	  |                  |
	  | [[rs(1).a]] =  |
	  | [[rs(2).a]] =  |
	  | [[rs(4).a]] =  |
	  | [[rs(5).a]] = 10 |
	  | [[rs(6).a]] = 20 |
	  | [[rs(7).a]] = 30 |

Scenario: Workflow with Assign and Sort Backward to test gaps
      Given I have a workflow "workflowithAssignandsortrecBack"
      And "workflowithAssignandsortrecBack" contains an Assign "sortval" as
	  | variable    | value |
	  | [[rs(1).a]] | 10    |
	  | [[rs(5).a]] | 20    |
	  | [[rs(7).a]] | 30    |
	  | [[rs(2).b]] | 6     |
	  | [[rs(4).b]] | 4     |
	  | [[rs(6).b]] | 2     |
	  And "workflowithAssignandsortrecBack" contains an Sort "sortRec" as
	  | Sort Field | Sort Order |
	  | [[rs(*).a]] | Backwards   |
	  When "workflowithAssignandsortrecBack" is executed
	  Then the workflow execution has "NO" error
	  And the 'sortval' in WorkFlow 'workflowithAssignandsortrecBack' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 10        |
	  | 2 | [[rs(5).a]] = | 20        |
	  | 3 | [[rs(7).a]] = | 30        |
	  | 4 | [[rs(2).b]] = | 6         |
	  | 5 | [[rs(4).b]] = | 4         |
	  | 6 | [[rs(6).b]] = | 2         |
	  And the 'sortval' in Workflow 'workflowithAssignandsortrecBack' debug outputs as    
	  | # |                  |
	  | 1 | [[rs(1).a]] = 10 |
	  | 2 | [[rs(5).a]] = 20 |
	  | 3 | [[rs(7).a]] = 30 |
	  | 4 | [[rs(2).b]] = 6  |
	  | 5 | [[rs(4).b]] = 4  |
	  | 6 | [[rs(6).b]] = 2  |
	  And the 'sortRec' in WorkFlow 'workflowithAssignandsortrecBack' debug inputs as
	  | Sort Field       | Sort Order |
	  | [[rs(1).a]] = 10 |            |
	  | [[rs(2).a]] =    |            |
	  | [[rs(4).a]] =    |            |
	  | [[rs(5).a]] = 20 |            |
	  | [[rs(6).a]] =    |            |
	  | [[rs(7).a]] = 30 | Backwards    |
	  And the 'sortRec' in Workflow 'workflowithAssignandsortrecBack' debug outputs as
	  |                  |
	  | [[rs(1).a]] = 30 |
	  | [[rs(2).a]] = 20 |
	  | [[rs(4).a]] = 10 |
	  | [[rs(5).a]] =    |
	  | [[rs(6).a]] =    |
	  | [[rs(7).a]] =    |


#Scenario: Workflow #FIXED ON 11782
#Scenario: Workflow with Assign and Unique Tool, finding unique data from multiple rows 
#      Given I have a workflow "workflowithAssignandUnique"
#      And "workflowithAssignandUnique" contains an Assign "Records" as
#	  | variable      | value |
#	  | [[rs().row]]  | 10    |
#	  | [[rs().data]] | 10    |
#	  | [[rs().row]]  | 40    |
#	  | [[rs().data]] | 20    |
#	  | [[rs().row]]  | 20    |
#	  | [[rs().data]] | 20    |
#	  | [[rs().row]]  | 30    |
#	  | [[rs().data]] | 40    |
#	  And "workflowithAssignandUnique" contains an Unique "Unique rec" as
#	  | In Field(s)                  | Return Fields | Result           |
#	  | [[rs(*).row]],[[rs(*).data]] | [[rs().row]]  | [[rec().unique]] |
#	  When "workflowithAssignandUnique" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Records' in WorkFlow 'workflowithAssignandUnique' debug inputs as
#	  | # | Variable        | New Value |
#	  | 1 | [[rs().row]] =  | 10        |
#	  | 2 | [[rs().data]] = | 10        |
#	  | 3 | [[rs().row]] =  | 40        |
#	  | 4 | [[rs().data]] = | 20        |
#	  | 5 | [[rs().row]] =  | 20        |
#	  | 6 | [[rs().data]] = | 20        |
#	  | 7 | [[rs().row]] =  | 30        |
#	  | 8 | [[rs().data]] = | 40        |
#	  And the 'Records' in Workflow 'workflowithAssignandUnique' debug outputs as  
#	  | # |                     |
#	  | 1 | [[rs(1).row]] =  10 |
#	  | 2 | [[rs(1).data]] =  10 |
#	  | 3 | [[rs(2).row]] =  40  |
#	  | 4 | [[rs(2).data]] =  20 |
#	  | 5 | [[rs(3).row]] =  20  |
#	  | 6 | [[rs(3).data]] =  20 |
#	  | 7 | [[rs(4).row]] =  30  |
#	  | 8 | [[rs(4).data]] =  40 |
#	  And the 'Unique rec' in WorkFlow 'workflowithAssignandUnique' debug inputs as
#       | #           |                     | Return Fields |
#       | In Field(s) | [[rs(1).row]] = 10  |               |
#       |             | [[rs(2).row]] = 40  |               |
#       |             | [[rs(3).row]] = 20  |               |
#       |             | [[rs(4).row]] = 30  |               |
#       |             | [[rs(1).data]] = 10 |               |
#       |             | [[rs(2).data]] = 20 |               |
#       |             | [[rs(3).data]] = 20 |               |
#       |             | [[rs(4).data]] = 40 |               |
#       |             |                     | [[rs().row]]  |
#      And the 'Unique rec' in Workflow 'workflowithAssignandUnique' debug outputs as  
#       | # |                        |
#       | 1 | [[rec(1).unique]] = 10 |
#       |   | [[rec(2).unique]] = 40 |
#       |   | [[rec(3).unique]] = 20 |
#       |   | [[rec(4).unique]] = 30 |
#with Assign and Unique Tool, Infields rec without star
      Given I have a workflow "workflowithAssignandUniqueToolc"
      And "workflowithAssignandUniqueToolc" contains an Assign "Records" as
	  | variable       | value |
	  | [[rs(1).row]]  | 10    |
	  | [[rs(1).data]] | 10    |
	  | [[rs(2).row]]  | 40    |
	  | [[rs(2).data]] | 20    |
	  | [[rs(3).row]]  | 20    |
	  | [[rs(3).data]] | 20    |
	  | [[rs(4).row]]  | 30    |
	  | [[rs(4).data]] | 40    |
	  And "workflowithAssignandUniqueToolc" contains an Unique "Unique rec" as
	  | In Field(s)                | Return Fields | Result           |
	  | [[rs().row]],[[rs().data]] | [[rs().row]]  | [[rec().unique]] |
	  When "workflowithAssignandUniqueToolc" is executed
	  Then the workflow execution has "No" error
	  And the 'Records' in WorkFlow 'workflowithAssignandUniqueToolc' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[rs(1).row]] = | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  | 3 | [[rs(2).row]] =  | 40        |
	  | 4 | [[rs(2).data]] = | 20        |
	  | 5 | [[rs(3).row]] =  | 20        |
	  | 6 | [[rs(3).data]] = | 20        |
	  | 7 | [[rs(4).row]] =  | 30        |
	  | 8 | [[rs(4).data]] = | 40        |
	  And the 'Records' in Workflow 'workflowithAssignandUniqueToolc' debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the 'Unique rec' in WorkFlow 'workflowithAssignandUniqueToolc' debug inputs as
       | #           |                     | Return Fields |
       | In Field(s) | [[rs(4).row]] = 30  |               |
       |             | [[rs(4).data]] = 40 | [[rs().row]] =  |
      And the 'Unique rec' in Workflow 'workflowithAssignandUniqueToolc' debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 10 |
       |   | [[rec(2).unique]] = 40 |
       |   | [[rec(3).unique]] = 20 |
       |   | [[rec(4).unique]] = 30 |
#
Scenario: Workflow with Assign and Unique Tool, Result rec with star
      Given I have a workflow "workflowithAssignandUniqueTools"
      And "workflowithAssignandUniqueTools" contains an Assign "Records" as
	  | variable       | value |
	  | [[rs(1).row]]  | 10    |
	  | [[rs(1).data]] | 10    |
	  | [[rs(2).row]]  | 40    |
	  | [[rs(2).data]] | 20    |
	  | [[rs(3).row]]  | 20    |
	  | [[rs(3).data]] | 20    |
	  | [[rs(4).row]]  | 30    |
	  | [[rs(4).data]] | 40    |
	  And "workflowithAssignandUniqueTools" contains an Unique "Unique rec" as
	  | In Field(s)                | Return Fields | Result           |
	  | [[rs().row]],[[rs().data]] | [[rs().row]]  | [[rec(*).unique]] |
	  When "workflowithAssignandUniqueTools" is executed
	  Then the workflow execution has "NO" error
	  And the 'Records' in WorkFlow 'workflowithAssignandUniqueTools' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rs(1).row]] = | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  | 3 | [[rs(2).row]] =  | 40        |
	  | 4 | [[rs(2).data]] = | 20        |
	  | 5 | [[rs(3).row]] =  | 20        |
	  | 6 | [[rs(3).data]] = | 20        |
	  | 7 | [[rs(4).row]] =  | 30        |
	  | 8 | [[rs(4).data]] = | 40        |
	  And the 'Records' in Workflow 'workflowithAssignandUniqueTools' debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the 'Unique rec' in WorkFlow 'workflowithAssignandUniqueTools' debug inputs as
       | #           |                     | Return Fields |
       | In Field(s) | [[rs(4).row]] = 30  |               |
       |             | [[rs(4).data]] = 40 | [[rs().row]] =  |
      And the 'Unique rec' in Workflow 'workflowithAssignandUniqueTools' debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 10 |
       |   | [[rec(2).unique]] = 40 |
       |   | [[rec(3).unique]] = 20 |
       |   | [[rec(4).unique]] = 30 |

#This should be passed with the bug 12093
#Scenario: Workflow by using For Each with Raandom in it
#      Given I have a workflow "workflowithforeachcontainsrandom"
#      And "workflowithforeachcontainsrandom" contains a ForEach "ForEachTest" as "NumOfExecution" executiom "5"
#	  And "ForEachTest" contains Random "Random" as
#	  | Type    | From | To | Result       |
#	  | Numbers | 1    | 5  | [[rec(*).a]] |
#	  When "workflowithforeachcontainsrandom" is executed
#	  Then the workflow execution has "No" error
#	  And the 'ForEachTest' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as
#	  |                 | Number |
#	  | No. of Executes | 5      | 
#	  And the 'Random' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 5  |
#	  And the 'Random' in Workflow 'workflowithforeachcontainsrandom' debug outputs as
#	  |                      |
#	  | [[rec(1).a]] = Int32 |
#	    And the 'Random' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 5  |
#	  And the 'Random' in Workflow 'workflowithforeachcontainsrandom' debug outputs as
#	  |                      |
#	  | [[rec(1).a]] = Int32 |
#	    And the 'Random' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 5  |
#	  And the 'Random' in Workflow 'workflowithforeachcontainsrandom' debug outputs as
#	  |                      |
#	  | [[rec(1).a]] = Int32 |
#	    And the 'Random' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 5  |
#	  And the 'Random' in Workflow 'workflowithforeachcontainsrandom' debug outputs as
#	  |                      |
#	  | [[rec(1).a]] = Int32 |
#	    And the 'Random' in WorkFlow 'workflowithforeachcontainsrandom' debug inputs as 
#	  | Random  | From | To |
#	  | Numbers | 1    | 5  |
#	  And the 'Random' in Workflow 'workflowithforeachcontainsrandom' debug outputs as
#	  |                      |
#	  | [[rec(1).a]] = Int32 |

#This should be passed after the bug 12021 is fixed (RECURSIVE EVALUATION)
#Scenario: Workflow with Assigns DataSplit executing against the server
#      Given I have a workflow "WorkflowDataSplit"
#	  And "WorkflowDataSplit" contains an Assign "Assignval" as
#      | variable | value   |
#      | [[a]]    | rec().a |
#	    And "WorkflowDataSplit" contains Data Split "DataSplit" as
#	  | String | Variable  | Type  | At | Include    | Escape |
#	  | abcd   | [[[[a]]]] | Index | 4  | Unselected |        | 
#	  When "WorkflowDataSplit" is executed
#	  Then the workflow execution has "No" error
#	  And the 'Assignval' in WorkFlow 'WorkflowDataSplit' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | rec().a   |
#	  And the 'Assignval' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                  |
#	  | 1 | [[a]] =  rec().a |
#	  And the 'DataSplit' in WorkFlow 'WorkflowDataSplit' debug inputs as 
#	  | String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	  | abcd            | Forward           | No              | 1 | [[[[a]]]] = [[rec().a]] | Index | 4     | No      |        |
#	  And the 'DataSplit' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                     |
#	  | 1 | [[rec(1).b]] = abcd |
#
#Scenario: Workflow with Assign and foreach contains calculate. 
#      Given I have a workflow "WorkflowDwithforeachcontainscalculate"
#	  And "WorkflowDwithforeachcontainscalculate" contains an Assign "Assignval" as
#      | variable   | value |
#      | [[rs().a]] | 1     |
#      | [[rs().a]] | 2     |
#      | [[rs().a]] | 3     |
#	  And "WorkflowDwithforeachcontainscalculate" contains a ForEach "ForEachTest" as "NumOfExecution" and used "[[rs()]]"
#	  And "ForEachTest" contains Calculate "Cal" with formula "[[rs(*).a]]+1" into "[[result]]"
#	  When "WorkflowDwithforeachcontainscalculate" is executed
#	  Then the workflow execution has "No" error
#      And the 'Assignval' in WorkFlow 'WorkflowDwithforeachcontainscalculate' debug inputs as
#      | # | Variable   | New Value |
#      | 1 | [[rs().a]] | 1         |
#      | 2 | [[rs().a]] | 2         |
#      | 3 | [[rs().a]] | 3         |
#      And the 'Assignval' in Workflow 'WorkflowDwithforeachcontainscalculate' debug outputs as 
#      | # |                 |
#      | 1 | [[rs(1).a]] = 1 |
#      | 2 | [[rs(2).a]] = 2 |
#      | 3 | [[rs(3).a]] = 3 |   
#	  And the 'ForEachTest' in WorkFlow 'WorkflowDwithforeachcontainscalculate' debug inputs a
#	  |                | Recordset       |
#	  | * in Recordset | [[rs(1).a]] = 1 |
#	  |                | [[rs(2).a]] = 2 |
#	  |                | [[rs(3).a]] = 3 |     
#	   And the 'Calculate1' in WorkFlow 'ForEachTest' debug inputs as 
#      | fx =                |
#      | [[rs(1).a]]+1 = 1+1 |           
#      And the 'Calculate1' in Workflow 'ForEachTest' debug outputs as  
#	  |                |
#	  | [[result]] = 2 |
#	   And the 'Calculate1' in WorkFlow 'ForEachTest' debug inputs as 
#      | fx =                |
#      | [[rs(2).a]]+1 = 2+1 |           
#      And the 'Calculate1' in Workflow 'ForEachTest' debug outputs as  
#	  |                |
#	  | [[result]] = 3 |
#	   And the 'Calculate1' in WorkFlow 'ForEachTest' debug inputs as 
#      | fx =                |
#      | [[rs(3).a]]+1 = 3+1 |           
#      And the 'Calculate1' in Workflow 'ForEachTest' debug outputs as  
#	  |                |
#	  | [[result]] = 4 |
#
#Scenario: Workflow with Assign and foreach with invalid rec and it contains calculate in it. 
#      Given I have a workflow "WorkflowDwithforeachcontainscal"
#	  And "WorkflowDwithforeachcontainscal" contains an Assign "Assigl" as
#      | variable   | value |
#      | [[rs().a]] | 1     |
#      | [[rs().a]] | 2     |
#      | [[rs().a]] | 3     |
#	  And "WorkflowDwithforeachcontainscal" contains a ForEach "ForEachTest" as "NumOfExecution" and used "[[rs()]]+1"
#	  And "ForEachTest" contains Calculate "Cal" with formula "[[rs(*).a]]+1" into "[[result]]"
#	  When "WorkflowDwithforeachcontainscal" is executed
#	  Then the workflow execution has "AN" error
#      And the 'Assigl' in WorkFlow 'WorkflowDwithforeachcontainscal' debug inputs as
#      | # | Variable   | New Value |
#      | 1 | [[rs().a]] | 1         |
#      | 2 | [[rs().a]] | 2         |
#      | 3 | [[rs().a]] | 3         |
#      And the 'Assigl' in Workflow 'WorkflowDwithforeachcontainscal' debug outputs as 
#      | # |                 |
#      | 1 | [[rs(1).a]] = 1 |
#      | 2 | [[rs(2).a]] = 2 |
#      | 3 | [[rs(3).a]] = 3 |   
#	   And the 'ForEachTest' in WorkFlow 'WorkflowDwithforeachcontainscalculate' debug inputs a
#	  |                | Recordset       |
#	  | * in Recordset | [[rs(1).a]]+1 = |
#	  |                | [[rs(2).a]]+1 = |
#	  |                | [[rs(3).a]]+1 = |  
#
#
#


#This should be passed after the bug 12021 is fixed
#Scenario: Workflow with Assigns DataSplit executing against the server
#      Given I have a workflow "WorkflowDataSplit"
#	  And "WorkflowDataSplit" contains an Assign "Assignval" as
#      | variable | value   |
#      | [[a]]    | rec().a |
#	    And "WorkflowDataSplit" contains Data Split "DataSplit" as
#	  | String | Variable  | Type  | At | Include    | Escape |
#	  | abcd   | [[[[a]]]] | Index | 4  | Unselected |        | 
#	  When "WorkflowDataSplit" is executed
#	  Then the workflow execution has "No" error
#	  And the 'Assignval' in WorkFlow 'WorkflowDataSplit' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | rec().a   |
#	  And the 'Assignval' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                  |
#	  | 1 | [[a]] =  rec().a |
#	  And the 'DataSplit' in WorkFlow 'WorkflowDataSplit' debug inputs as 
#	  | String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	  | abcd            | Forward           | No              | 1 | [[[[a]]]] = [[rec().a]] | Index | 4     | No      |        |
#	  And the 'DataSplit' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                     |
#	  | 1 | [[rec(1).b]] = abcd |


#This should be passed after the bug 12021 is fixed (RECURSIVE EVALUATION)
#Scenario: Workflow with Assigns DataSplit executing against the server 2
#      Given I have a workflow "WorkflowDataSplit"
#	  And "WorkflowDataSplit" contains an Assign "Assignval" as
#      | variable | value   |
#      | [[a]]    | rec().a |
#	    And "WorkflowDataSplit" contains Data Split "DataSplit" as
#	  | String | Variable  | Type  | At | Include    | Escape |
#	  | abcd   | [[[[a]]]] | Index | 4  | Unselected |        | 
#	  When "WorkflowDataSplit" is executed
#	  Then the workflow execution has "No" error
#	  And the 'Assignval' in WorkFlow 'WorkflowDataSplit' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | rec().a   |
#	  And the 'Assignval' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                  |
#	  | 1 | [[a]] =  rec().a |
#	  And the 'DataSplit' in WorkFlow 'WorkflowDataSplit' debug inputs as 
#	  | String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	  | abcd            | Forward           | No              | 1 | [[[[a]]]] = [[rec().a]] | Index | 4     | No      |        |
#	  And the 'DataSplit' in Workflow 'WorkflowDataSplit' debug outputs as  
#	  | # |                     |
#	  | 1 | [[rec(1).b]] = abcd |

#
#This Test should be passed after the bug 12119 is fixed
#Scenario: Workflow with Assign and Unique Tool
#      Given I have a workflow "workfllowAssingunique"
#      And "workfllowAssingunique" contains an Assign "Records" as
#	  | variable    | value |
#	  | [[rs(1).a]] | 19    |
#	  | [[rs(2).a]] | 20    |
#	  | [[rs(3).a]] | 40    |
#	  | [[rs(4).a]] | 50    |
#	  | [[rs(1).b]] | 19    |
#	  | [[rs(2).b]] | 20    |
#	  | [[rs(3).b]] | 30    |
#	  | [[rs(4).b]] | 80    |
#	  And "workfllowAssingunique" contains an Unique "Unique rec" as
#	  | In Field(s)           | Return Fields | Result           |
#	  | [[rs().a]],[[rs().b]] | [[rs().a]]    | [[rec().unique]] |
#	  When "workfllowAssingunique" is executed
#	  Then the workflow execution has "NO" error
#	  And the 'Records' in WorkFlow 'workfllowAssingunique' debug inputs as
#	  | # | Variable     | New Value |
#	  | 1 | [[rs(1).a]]= | 19        |
#	  | 2 | [[rs(2).a]]= | 20        |
#	  | 3 | [[rs(3).a]]= | 40        |
#	  | 4 | [[rs(4).a]]= | 50        |
#	  | 5 | [[rs(1).b]]= | 19        |
#	  | 6 | [[rs(2).b]]= | 20        |
#	  | 7 | [[rs(3).b]]= | 30        |
#	  | 8 | [[rs(4).b]]= | 80        |
#	  And the 'Records' in Workflow 'workfllowAssingunique' debug outputs as  
#	  | # |                   |
#	  | 1 | [[rs(1).a]] =  19 |
#	  | 2 | [[rs(2).a]] =  20 |
#	  | 3 | [[rs(3).a]] =  40 |
#	  | 4 | [[rs(4).a]] =  50 |
#	  | 5 | [[rs(1).b]] =  19 |
#	  | 6 | [[rs(2).b]] =  20 |
#	  | 7 | [[rs(3).b]] =  30 |
#	  | 8 | [[rs(4).b]] =  80 |
#	  And the 'Unique r ec' in WorkFlow 'workfllowAssingunique' debug inputs as
#       | #           |                  | Return Fields |
#       | In Field(s) | [[rs(1).a]] = 19 |               |
#       |             | [[rs(2).a]] = 20 |               |
#       |             | [[rs(3).a]] = 40 |               |
#       |             | [[rs(4).a]] = 50 |               |
#       |             | [[rs(1).b]] = 19 |               |
#       |             | [[rs(2).b]] = 20 |               |
#       |             | [[rs(3).b]] = 30 |               |
#       |             | [[rs(4).b]] = 80 |               |
#       |             |                  | [[rs().a]]    |
#      And the 'Unique rec' in Workflow 'workfllowAssingunique' debug outputs as  
#       | # |                        |
#       | 1 | [[rec(1).unique]] = 19 |
#       |   | [[rec(2).unique]] = 20 |
#       |   | [[rec(3).unique]] = 40 |
#       |   | [[rec(4).unique]] = 80 |
#
#

























