@WorkflowExecution
Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Background: Setup for workflow execution
			Given Debug events are reset
			And All environments disconnected
			And Debug states are cleared

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
	Given I have a workflow "TestAssignWithRemote"
	 And "TestAssignWithRemote" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	And "TestAssignWithRemote" contains "WorkflowUsedBySpecs" from server "Remote Connection Integration" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable      |
	| input            | [[inputData]] | output              | [[output]]       |
	|                  |               | values(*).upper     | [[values().up]]  |
	|                  |               | values(*).lower     | [[values().low]] |
	  When "TestAssignWithRemote" is executed
	  Then the workflow execution has "NO" error
	   And the 'AssignData' in WorkFlow 'TestAssignWithRemote' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the 'AssignData' in Workflow 'TestAssignWithRemote' debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the 'WorkflowUsedBySpecs' in WorkFlow 'TestAssignWithRemote' debug inputs as
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
	  And the 'WorkflowUsedBySpecs' in Workflow 'TestAssignWithRemote' debug outputs as
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
#
#Bug-12216
#Scenario: Workflow with Assign Base Convert and Case Convert passing invalid variables through execution
#	  Given I have a workflow "WorkflowWithAssignBaseConvertandCaseconvert"
#	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains an Assign "Assign1" as
#	  | variable       | value    |
#	  | [[rec(1).a]]   | Warewolf |
#	  | [[rec(2).a]]   | test     |
#	  | [[index(1).a]] | a        |
#	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains case convert "Case to Convert" as
#	  | Variable                  | Type  |
#	  | [[rec([[index(1).a]]).a]] | UPPER |
#	  And "WorkflowWithAssignBaseConvertandCaseconvert" contains Base convert "Base to Convert" as
#	  | Variable                  | From | To      |
#	  | [[rec([[index(1).a]]).a]] | Text | Base 64 |
#	  When "WorkflowWithAssignBaseConvertandCaseconvert" is executed
#	  Then the workflow execution has "AN" error
#	  And the 'Assign1' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
#	  | # | Variable         | New Value |
#	  | 1 | [[rec(1).a]] =   | Warewolf  |
#	  | 2 | [[rec(2).a]] =   | test      |
#	  | 3 | [[index(1).a]] = | a         |
#	   And the 'Assign1' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
#	  | # |                          |
#	  | 1 | [[rec(1).a]] =  Warewolf |
#	  | 2 | [[rec(2).a]] =  test     |
#	  | 3 | [[index(1).a]] =  a      |
#	  And the 'Case to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
#	  | # | Convert                     | To    |
#	  | 1 | [[rec([[index(1).a]]).a]] = | UPPER |
#	  And the 'Case to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
#	  | # |                     |
#	  And the 'Base to Convert' in WorkFlow 'WorkflowWithAssignBaseConvertandCaseconvert' debug inputs as
#	  | # | Convert                     | From | To      |
#	  | 1 | [[rec([[index(1).a]]).a]] = | Text | Base 64 |
#      And the 'Base to Convert' in Workflow 'WorkflowWithAssignBaseConvertandCaseconvert' debug outputs as  
#	  | # |                     |
#	

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
	   | [[res1]] =  |
	  And the 'Delete' in WorkFlow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug inputs as
	  | Input Path                | Username | Password |
	  | [[rec(1).a]] = create.txt |   Username =       |    Password =      |
	  And the 'Delete' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as    
	  |                    |
	  | [[res1]] =  |

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

Scenario: Workflow with Assign recordset calculate field
      Given I have a workflow "WFWithAssignHasCalculate"
	  And "WFWithAssignHasCalculate" contains an Assign "values1" as
      | variable     | value                      |
      | [[a]]        | 1                          |
      | [[b]]        | 2                          |
      | [[rec(1).a]] | [[a]]                      |
      | [[rec(1).b]] | [[b]]                      |
      | [[rec(1).c]] | =[[rec(1).a]]+[[rec(1).b]] |
	  When "WFWithAssignHasCalculate" is executed
	  Then the workflow execution has "NO" error
	  And the 'values1' in WorkFlow 'WFWithAssignHasCalculate' debug inputs as 
	  | # | Variable       | New Value                        |
	  | 1 | [[a]] =        | 1                                |
	  | 2 | [[b]] =        | 2                                |
	  | 3 | [[rec(1).a]] = | [[a]] = 1                        |
	  | 4 | [[rec(1).b]] = | [[b]] = 2                        |
	  | 5 | [[rec(1).c]] = | =[[rec(1).a]]+[[rec(1).b]] ==1+2 |
	  And the 'values1' in Workflow 'WFWithAssignHasCalculate' debug outputs as   
	  | # |                  |
	  | 1 | [[a]] = 1       |
	  | 2 | [[b]] = 2       |
	  | 3 | [[rec(1).a]] = 1 |
	  | 4 | [[rec(1).b]] = 2 |
	  | 5 | [[rec(1).c]] = 3 |

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
      | fx =                                  |
      | [[rec([[index(1).a]]).a]]+[[a]] = 2+1 |       
      And the 'Calculate1' in Workflow 'WFWithAssignCalculateindexrecordset' debug outputs as  
	  |                |
	  | [[result]] = 3 |

Scenario: Workflow with Assign Calculate multiple recursion
      Given I have a workflow "WFAssignCalculateRecursion"
	  And "WFAssignCalculateRecursion" contains an Assign "values1" as
      | variable     | value    |
      | [[b]]        | rec(1).b |
      | [[rec(1).a]] | b        |
      | [[rec(1).b]] | 1        |
	  And "WFAssignCalculateRecursion" contains Calculate "Calculate1" with formula "[[[[[[rec(1).a]]]]]]+1" into "[[result]]"
	  When "WFAssignCalculateRecursion" is executed
	  Then the workflow execution has "NO" error
	  And the 'values1' in WorkFlow 'WFAssignCalculateRecursion' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[b]] =        | rec(1).b  |
	  | 2 | [[rec(1).a]] = | b         |
	  | 3 | [[rec(1).b]] = | 1         |
	 And the 'values1' in Workflow 'WFAssignCalculateRecursion' debug outputs as   
	  | # |                           |
	  | 1 | [[b]]         =  rec(1).b |
	  | 2 | [[rec(1).a]]  =  b        |
	  | 3 | [[rec(1).b]]   = 1        |
	  And the 'Calculate1' in WorkFlow 'WFAssignCalculateRecursion' debug inputs as 
      | fx =                         |
      | [[[[[[rec(1).a]]]]]]+1 = 1+1 |       
      And the 'Calculate1' in Workflow 'WFAssignCalculateRecursion' debug outputs as  
	  |                |
	  | [[result]] = 2 |



Scenario: Workflow with Assign and Calculate
      Given I have a workflow "WFAssign&Calculate"
	  And "WFAssign&Calculate" contains an Assign "values1" as
      | variable       | value |
      | [[Honda().a1]] | 1     |
      | [[Honda().a2]] | 2     |
      | [[Honda().a3]] | 3     |
      | [[Benz().a1]]  | 10    |
      | [[Benz().a2]]  | 20    |
      | [[Benz().a3]]  | 30    |
	  And "WFAssign&Calculate" contains Calculate "Calculate1" with formula "sum([[Benz(*)]])+sum([[Honda(*)]])" into "[[result]]"
	  When "WFAssign&Calculate" is executed
	  Then the workflow execution has "NO" error
	  And the 'values1' in WorkFlow 'WFAssign&Calculate' debug inputs as 
	  | # | Variable         | New Value |
	  | 1 | [[Honda().a1]] = | 1         |
	  | 2 | [[Honda().a2]] = | 2         |
	  | 3 | [[Honda().a3]] = | 3         |
	  | 4 | [[Benz().a1]]  = | 10        |
	  | 5 | [[Benz().a2]]  = | 20        |
	  | 6 | [[Benz().a3]]  = | 30        |
	 And the 'values1' in Workflow 'WFAssign&Calculate' debug outputs as   
	  | # |                      |
	  | 1 | [[Honda(1).a1]] =  1  |
	  | 2 | [[Honda(1).a2]] =  2  |
	  | 3 | [[Honda(1).a3]] =  3  |
	  | 4 | [[Benz(1).a1]]  =  10 |
	  | 5 | [[Benz(1).a2]]  =  20 |
	  | 6 | [[Benz(1).a3]]  =  30 |
	  And the 'Calculate1' in WorkFlow 'WFAssign&Calculate' debug inputs as 
      | fx =                                                          |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(10,20,30)+sum(1,2,3) |       
      And the 'Calculate1' in Workflow 'WFAssign&Calculate' debug outputs as  
	  |                 |
	  | [[result]] = 66 |

#This Scenario should be passed after the bug 11714 is fixed
Scenario: Workflow with Assign and ForEach
     Given I have a workflow "WFWithForEach"
	 And "WFWithForEach" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[Warewolf]] | bob   |
     And "WFWithForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "3"
	 And "ForEachTest" contains workflow "11714Nested" with mapping as
	 | Input to Service | From Variable | Output from Service | To Variable |
	 | a                | [[Warewolf]]      |                     |             |
	 When "WFWithForEach" is executed
	 Then the workflow execution has "NO" error
	 And the 'ForEachTest' in WorkFlow 'WFWithForEach' debug inputs as 
	    |                 | Number |
	    | No. of Executes | 3      |
	 And the 'ForEachTest' in WorkFlow 'WFWithForEach' has  "3" nested children 
	 And each "11714Nested" contains debug outputs for "Assign (1)" as
      | variable | value    |
      | [[a]]    | warewolf |  
        
 #Bug - 12160       
Scenario: Workflow with ForEach which contains assign
      Given I have a workflow "WFWithForEachContainingAssign"
	  And "WFWithForEachContainingAssign" contains an Assign "Rec To Convert" as
	    | variable    | value |
	    | [[Warewolf]] | bob   |
	  And "WFWithForEachContainingAssign" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	  And "ForEachTest" contains an Assign "MyAssign" as
	    | variable    | value |
	    | [[rec().a]] | Test  |
      When "WFWithForEachContainingAssign" is executed
	  Then the workflow execution has "NO" error
	  And the 'ForEachTest' in WorkFlow 'WFWithForEachContainingAssign' debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the 'ForEachTest' in WorkFlow 'WFWithForEachContainingAssign' has  "2" nested children 
	  And the 'MyAssign' in step 1 for 'ForEachTest' debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the 'MyAssign' in step 1 for 'ForEachTest' debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = Test |
	  And the 'MyAssign' in step 2 for 'ForEachTest' debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the 'MyAssign' in step 2 for 'ForEachTest' debug outputs as
		| # |                     |
		| 1 | [[rec(2).a]] = Test |


##Bug - 12160  
Scenario: Workflow with ForEach which contains Sequence
      Given I have a workflow "WorkflowWithForEachContainingSequence"
	  And "WorkflowWithForEachContainingSequence" contains an Assign "RecVal" as
	  | variable     | value |
	  | [[rec(1).a]] | 123   |
	  | [[rec(1).b]] | 456   |
	  And "WorkflowWithForEachContainingSequence" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "2"
	  And "ForEachTest1" contains a Sequence "Seq1" as
	  And "Seq1" in 'ForEachTest1' contains Data Merge "Data Merge" into "[[rec(1).c]]" as
	  | Variable     | Type | Using | Padding | Alignment |
	  | [[rec(1).a]] | None |       |         | Left      |
	  | [[rec(1).b]] | None |       |         | Left      |
	   And "Seq1" in 'ForEachTest1' contains Gather System Info "System info" as
	  | Variable     | Selected    |
	  | [[rec(1).d]] | Date & Time |
	  When "WorkflowWithForEachContainingSequence" is executed
	  Then the workflow execution has "NO" error
	  And the 'RecVal' in WorkFlow 'WorkflowWithForEachContainingSequence' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  | 2 | [[rec(1).b]] = | 456       |
	  And the 'RecVal' in Workflow 'WorkflowWithForEachContainingSequence' debug outputs as 
	  | # |                      |
	  | 1 | [[rec(1).a]]  =  123 |
	  | 2 | [[rec(1).b]]  =  456 |
	   And the 'ForEachTest1' in WorkFlow 'WorkflowWithForEachContainingSequence' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
      And the 'ForEachTest1' in WorkFlow 'WorkflowWithForEachContainingSequence' has  "2" nested children 
	  And the 'Data Merge' in "Seq1" in step 1 for 'ForEachTest1' debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	   And the 'Data Merge' in "Seq1" in step 1 for 'ForEachTest1' debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
	  And the 'System info' in "Seq1" in step 1 for 'ForEachTest1' debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	  And the 'System info' in "Seq1" in step 1 for 'ForEachTest1' debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String | 
	  And the 'Data Merge' in "Seq1" in step 2 for 'ForEachTest1' debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	   And the 'Data Merge' in "Seq1" in step 2 for 'ForEachTest1' debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
	  And the 'System info' in "Seq1" in step 2 for 'ForEachTest1' debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	  And the 'System info' in "Seq1" in step 2 for 'ForEachTest1' debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String |	

##Bug - 12160  
Scenario: Executing ForEach in Rec with star which contains Sequence
      Given I have a workflow "WorkFWithForEachwithRecContainingSequence"
	  And "WorkFWithForEachwithRecContainingSequence" contains an Assign "RecVal" as
	  | variable     | value    |
	  | [[rec(1).a]] | 123      |
	  | [[rec(1).b]] | 456      |
	  | [[rec(2).a]] | Test     |
	  | [[rec(2).b]] | Warewolf |
	  And "WorkFWithForEachwithRecContainingSequence" contains a Foreach "ForEachTest1" as "InRecordset" executions "[[rec(*)]]"
	  And "ForEachTest1" contains a Sequence "Seq1" as
	  And "Seq1" in 'ForEachTest1' contains Data Merge "Data Merge" into "[[rec(*).c]]" as
	  | Variable     | Type | Using | Padding | Alignment |
	  | [[rec(*).a]] | None |       |         | Left      |
	  | [[rec(*).b]] | None |       |         | Left      |
	  And "Seq1" in 'ForEachTest1' contains Gather System Info "System info" as
	  | Variable     | Selected    |
	  | [[rec(*).d]] | Date & Time |
	  When "WorkFWithForEachwithRecContainingSequence" is executed
	  Then the workflow execution has "NO" error
	  And the 'RecVal' in WorkFlow 'WorkFWithForEachwithRecContainingSequence' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  | 2 | [[rec(1).b]] = | 456       |
	  | 3 | [[rec(2).a]] = | Test      |
	  | 4 | [[rec(2).b]] = | Warewolf  |
	  And the 'RecVal' in Workflow 'WorkFWithForEachwithRecContainingSequence' debug outputs as 
	  | # |                          |
	  | 1 | [[rec(1).a]]  =  123     |
	  | 2 | [[rec(1).b]]  =  456     |
	  | 3 | [[rec(2).a]] =  Test     |
	  | 4 | [[rec(2).b]] =  Warewolf |
	  And the 'ForEachTest1' in WorkFlow 'WorkFWithForEachwithRecContainingSequence' debug inputs as 
	  |                | Recordset               |
	  | * in Recordset |                         |
	  |                | [[rec(1).a]] = 123      |
	  |                | [[rec(1).b]] = 456      |
	  |                | [[rec(1).c]] =          |
	  |                | [[rec(1).d]] =          |
	  |                | [[rec(2).a]] = Test     |
	  |                | [[rec(2).b]] = Warewolf |
	  |                | [[rec(2).c]] =          |
	  |                | [[rec(2).d]]  =         |
      And the 'ForEachTest1' in WorkFlow 'WorkFWithForEachwithRecContainingSequence' has  "2" nested children
	  And the 'Data Merge' in "Seq1" in step 1 for 'ForEachTest1' debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	  And the 'Data Merge' in "Seq1" in step 1 for 'ForEachTest1' debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
       And the 'System info' in "Seq1" in step 1 for 'ForEachTest1' debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	   And the 'System info' in "Seq1" in step 1 for 'ForEachTest1' debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String |
	  And the 'Data Merge' in "Seq1" in step 2 for 'ForEachTest1' debug inputs as
	  | # |                         | With | Using | Pad | Align |
	  | 1 | [[rec(2).a]] = Test     | None | ""    | ""  | Left  |
	  | 2 | [[rec(2).b]] = Warewolf | None | ""    | ""  | Left  |
	  And the 'Data Merge' in "Seq1" in step 2 for 'ForEachTest1' debug outputs as
	  |                             |
	  | [[rec(2).c]] = TestWarewolf |
      And the 'System info' in "Seq1" in step 2 for 'ForEachTest1' debug inputs as
	  | # |                |             |
	  | 1 | [[rec(2).d]] = | Date & Time |
	   And the 'System info' in "Seq1" in step 2 for 'ForEachTest1' debug outputs as
	  | # |                       |
	  | 1 | [[rec(2).d]] = String |

##Bug - 12160  
Scenario: Executing 2 ForEach's inside a ForEach which contains Assign only
      Given I have a workflow "WFContainsForEachInsideforEach"
	  And "WFContainsForEachInsideforEach" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "2"
	  And "ForEachTest1" contains a Foreach "ForEachTest2" as "NumOfExecution" executions "2"
	  And "ForEachTest2" contains a Foreach "ForEachTest3" as "NumOfExecution" executions "2"
	  And "ForEachTest3" contains an Assign "Testingoutput" as
	  | variable     | value    |
	  | [[rec(1).a]] | 123      |
	  When "WFContainsForEachInsideforEach" is executed
	  Then the workflow execution has "NO" error
	  And the 'ForEachTest1' in WorkFlow 'WFContainsForEachInsideforEach' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
	  And the 'ForEachTest1' in WorkFlow 'WFContainsForEachInsideforEach' has  "2" nested children
      And the 'ForEachTest2' in step 1 for 'ForEachTest1' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
      And the 'ForEachTest2' in WorkFlow 'ForEachTest1' has  "2" nested children
	  And the 'ForEachTest3' in step 1 for 'ForEachTest2' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
	  And the 'ForEachTest3' in WorkFlow 'ForEachTest2' has  "2" nested children
	  And the 'Testingoutput' in step 1 for 'ForEachTest3' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  And the 'Testingoutput' in step 1 for 'ForEachTest3' debug outputs as
	  | # |                          |
	  | 1 | [[rec(1).a]]  =  123     |
	  And the 'Testingoutput' in step 2 for 'ForEachTest3' debug inputs as
	  | # | Variable           | New Value |
	  | 1 | [[rec(1).a]] = 123 | 123       |
	  And the 'Testingoutput' in step 2 for 'ForEachTest3' debug outputs as
	  | # |                    |
	  | 1 | [[rec(1).a]] = 123 |	  		
		
  Scenario: Executing 2 ForEach's inside a ForEach which contains Assign only Large Execution
      Given I have a workflow "WFForEachInsideforEachLargeTenFifty"
	  And "WFForEachInsideforEachLargeTenFifty" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "10"
	  And "ForEachTest1" contains a Foreach "ForEachTest2" as "NumOfExecution" executions "50"
	  And "ForEachTest2" contains an Assign "Testingoutput" as
	  | variable    | value         |
	  | [[rec().a]] | 123asda       |
	  | [[rec().b]] | aaaaa         |
	  | [[rec().c]] | rrrrrrr       |
	  | [[rec().d]] | 123asda       |
	  | [[rec().e]] | sdfsdrf45456  |
	  | [[rec().f]] | cvbcb1123     |
	  | [[rec().g]] | aasdww2323    |
	  | [[rec().h]] | oooooo9999    |
	  | [[rec().i]] | sdfsdf3434    |
	  | [[rec().j]] | asda123123    |
	  | [[rec().k]] | sssdff444     |
	  | [[rec().l]] | asdvvvbbg3333 |
	  | [[rec().m]] | aasdasd       |
	  | [[rec().n]] | aasdd222      |
	  | [[rec().o]] | 22323asda     |
	  And I get the server memory
	  When "WFForEachInsideforEachLargeTenFifty" is executed
	  Then the workflow execution has "NO" error
	  And the server CPU usage is less than 10%
	  And the server memory difference is less than 100 mb
	  And the 'ForEachTest1' in WorkFlow 'WFForEachInsideforEachLargeTenFifty' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 10      |
	  And the 'ForEachTest1' in WorkFlow 'WFForEachInsideforEachLargeTenFifty' has  "10" nested children
      And the 'ForEachTest2' in step 1 for 'ForEachTest1' debug inputs as 
	  |                 | Number |
	  | No. of Executes | 50      |
      And the 'ForEachTest2' in WorkFlow 'ForEachTest1' has  "50" nested children	 
	  And the 'Testingoutput' in step 50 for 'ForEachTest2' debug inputs as
	  | #  | Variable      | New Value     |
	  | 1  | [[rec().a]] = | 123asda       |
	  | 2  | [[rec().b]] = | aaaaa         |
	  | 3  | [[rec().c]] = | rrrrrrr       |
	  | 4  | [[rec().d]] = | 123asda       |
	  | 5  | [[rec().e]] = | sdfsdrf45456  |
	  | 6  | [[rec().f]] = | cvbcb1123     |
	  | 7  | [[rec().g]] = | aasdww2323    |
	  | 8  | [[rec().h]] = | oooooo9999    |
	  | 9  | [[rec().i]] = | sdfsdf3434    |
	  | 10 | [[rec().j]] = | asda123123    |
	  | 11 | [[rec().k]] = | sssdff444     |
	  | 12 | [[rec().l]] = | asdvvvbbg3333 |
	  | 13 | [[rec().m]] = | aasdasd       |
	  | 14 | [[rec().n]] = | aasdd222      |
	  | 15 | [[rec().o]] = | 22323asda     |
	  And the 'Testingoutput' in step 50 for 'ForEachTest2' debug outputs as
	  | #  |                                |
	  | 1  | [[rec(50).a]] = 123asda       |
	  | 2  | [[rec(50).b]] = aaaaa         |
	  | 3  | [[rec(50).c]] = rrrrrrr       |
	  | 4  | [[rec(50).d]] = 123asda       |
	  | 5  | [[rec(50).e]] = sdfsdrf45456  |
	  | 6  | [[rec(50).f]] = cvbcb1123     |
	  | 7  | [[rec(50).g]] = aasdww2323    |
	  | 8  | [[rec(50).h]] = oooooo9999    |
	  | 9  | [[rec(50).i]] = sdfsdf3434    |
	  | 10 | [[rec(50).j]] = asda123123    |
	  | 11 | [[rec(50).k]] = sssdff444     |
	  | 12 | [[rec(50).l]] = asdvvvbbg3333 |
	  | 13 | [[rec(50).m]] = aasdasd       |
	  | 14 | [[rec(50).n]] = aasdd222      |
	  | 15 | [[rec(50).o]] = 22323asda     |
		
  
  #Bug 12159
#  Before you look into this spec plz connect to "Integration Connection" server and search for Unique data workflow and debug.
#   Observe the output.	
#Scenario: Executing remote workflow which contains For Each with assign
#	      Given I have a workflow "TestAssignAndRemote"
#          And "TestAssignAndRemote" contains "Unique data" from server "Integration Connection" with mapping as
#          When "TestAssignAndRemote" is executed
#	      Then the workflow execution has "NO" error
#		  And the 'Assign for sequence' in WorkFlow 'workflowithAssignandsequence' debug inputs as
#	      | # | Variable                         | New Value |
#	      | 1 | [[Student().firstname]] =        | Murali    |
#	      | 2 | [[Student().lastname]]  =        | naidu     |
#	      | 3 | [[Student().firstname]] =        | Rak       |
#	      | 4 | [[Student().lastname]]  =        | m         |
#	      | 5 | [[Student().firstname]] =        | Murali    |
#	      | 6 |
#	      |   | [[Student(1).lastname]] =  naidu |
#	      |   | [[Student(2).lastname]] =  m     |
#	      |   | [[Student(3).lastname]] =        |
#	       And the 'Assign for sequence' in Workflow 'workflowithAssignandsequence' debug outputs as    
#	      | # |                                   |
#	      | 1 | [[Student(1).firstname]] =  Murali |
#	      | 2 | [[Student(1).lastname]]  =  naidu  |
#	      | 3 | [[Student(2).firstname]] =  Rak    |
#	      | 4 | [[Student().lastname]]  =  m      |
#	      | 5 | [[Student().firstname]] =  Murali |
#	      | 6 | [[Student(1).lastname]] =  naidu  |
#	      |   | [[Student(2).lastname]] =  naidu  |
#	      |   | [[Student(3).lastname]] =  naidu  |
#		  And the 'ForEachTest1' in WorkFlow 'WFWithForEachInsideforEach' debug inputs as 
#	       | Recordset                         |
#	       | [[Student(1).firstname]] = Murali |
#	       | [[Student(1).lastname]] = naidu   |
#	       | [[Student(1).fullname]] =         |
#	       | [[Student(1).date]] =             |
#	       | [[Student(2).firstname]] = Rak    |
#	       | [[Student(2).lastname]] = naidu   |
#	       | [[Student(2).fullname]] =         |
#	       | [[Student(2).date]] =             |
#	       | [[Student(3).firstname]] = Murali |
#	       | [[Student(3).lastname]] = naidu   |
#	       | [[Student(3).fullname]] =         |
#	       | [[Student(3).date]] =             |
#	       And the 'ForEachTest1' in WorkFlow 'WFWithForEachInsideforEach' has  "3" nested children
#		        And the 'Data Merge' child 1,1 in WorkFlow 'Sequence' debug inputs as
#	            | # |                                   | With | Using | Pad | Align |
#	            | 1 | [[Student(1).firstname]] = Murali | None | ""    | ""  | Left  |
#	            | 2 | [[Student(1).lastname]] = naidu   | None | ""    | ""  | Left  |
#	            And the 'Data Merge' child 1,1 in Workflow 'Sequence' debug outputs as
#	            |                                      |
#	            | [[Student(1).fullname]]= Muralinaidu |
#	            And the 'System info' child 1,2 in WorkFlow 'Sequence' debug inputs as
#	            | # |                       |             |
#	            | 1 | [[Student(1).date]] = | Date & Time |
#	            And the 'System info' child 1,2 in Workflow 'Sequence' debug outputs as   
#	            | # |                             |
#	            | 1 | [[Student(1).date]]= String |
#				And the 'Data Merge' child 2,1 in WorkFlow 'Sequence' debug inputs as
#	            | # |                                 | With | Using | Pad | Align |
#	            | 1 | [[Student(2).firstname]] = Rak  | None | ""    | ""  | Left  |
#	            | 2 | [[Student(2).lastname]] = naidu | None | ""    | ""  | Left  |
#	            And the 'Data Merge' child 2,1 in Workflow 'Sequence' debug outputs as
#	            |                                   |
#	            | [[Student(2).fullname]]= Raknaidu |
#	            And the 'System info' child 2,2 in WorkFlow 'Sequence' debug inputs as
#	            | # |                       |             |
#	            | 1 | [[Student(2).date]] = | Date & Time |
#	            And the 'System info' child 2,2 in Workflow 'Sequence' debug outputs as   
#	            | # |                             |
#	            | 1 | [[Student(2).date]]= String |
#				And the 'Data Merge' child 3,1 in WorkFlow 'Sequence' debug inputs as
#	            | # |                                   | With | Using | Pad | Align |
#	            | 1 | [[Student(3).firstname]] = Murali | None | ""    | ""  | Left  |
#	            | 2 | [[Student(3).lastname]] = naidu   | None | ""    | ""  | Left  |
#	            And the 'Data Merge' child 3,1 in Workflow 'Sequence' debug outputs as
#	            |                                      |
#	            | [[Student(3).fullname]]= Muralinaidu |
#	            And the 'System info' child 3,2 in WorkFlow 'Sequence' debug inputs as
#	            | # |                       |             |
#	            | 1 | [[Student(3).date]] = | Date & Time |
#	            And the 'System info' child 3,2 in Workflow 'Sequence' debug outputs as   
#	            | # |                             |
#	            | 1 | [[Student(3).date]]= String |
#          And the 'Unique rec' in WorkFlow 'workflowithAssignandUnique' debug inputs as
#            | #           |                                       | Return Fields |
#            | In Field(s) | [[Student(1).fullname]] = Muralinaidu |               |
#            |             | [[Student(2).fullname]] = Raknaidu    |               |
#            |             | [[Student(3).fullname]] = Muralinaidu |               |
#            |             |                                       | [[rs().row]]  |
#           And the 'Unique rec' in Workflow 'workflowithAssignandUnique' debug outputs as  
#            | # |                                    |
#            | 1 | [[list(1).students]] = Muralinaidu |
#            |   | [[list(2).students]] = Raknaidu    |
#             



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
 Scenario: Workflow with Assign and Replace by using recordset star
 Given I have a workflow "workflowithAssignandreplaces"
      And "workflowithAssignandreplaces" contains an Assign "Assignee" as
      | variable    | value |
      | [[rec().a]] | a     |
      | [[rec().a]] | b     | 
	  And "WorkflowWithAssignandReplaces" contains Replace "Rep" into "[[rec().a]]" as	
	  | In Fields    | Find         | Replace With |
	  | [[rec(*).a]] | [[rec(*).a]] | Warewolf     |
	  When "workflowithAssignandreplaces" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assignee' in WorkFlow 'workflowithAssignandreplaces' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | a         |
	  | 2 | [[rec().a]] = | b         |
	  And the 'Assignee' in Workflow 'workflowithAssignandreplaces' debug outputs as    
	  | # |                  |
	  | 1 | [[rec(1).a]] = a |
	  | 2 | [[rec(2).a]] = b |
	  And the 'Rep' in WorkFlow 'workflowithAssignandreplaces' debug inputs as 
	  | In Field(s)      | Find             | Replace With |
	  | [[rec(1).a]] = a |                  |              |
	  | [[rec(2).a]] = b |                  |              |
	  |                  | [[rec(1).a]] = a |              |
	  |                  | [[rec(2).a]] = b |              |
	  |                  |                  | Warewolf     |
	  And the 'Rep' in Workflow 'workflowithAssignandreplaces' debug outputs as
	  |                         |
	  | [[rec(1).a]] = Warewolf |
	  | [[rec(2).a]] = Warewolf |
	  | [[rec(3).a]] = 2        |

#The below 12 scenario should be passed after the bug 11994 is fixe
Scenario: Workflow with Assign and Find Record index
      Given I have a workflow "WFWithAssignandFindRecordindex"
	  And "WFWithAssignandFindRecordindex" contains an Assign "Record" as
      | # | variable     | value    |
      | # | [[rec(1).a]] | Warewolf |
	  And "WFWithAssignandFindRecordindex" contains Find Record Index "FindRecord" into result as "[[a]][[b]]"
      | # | In Field     | #        | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().a]] | 1        | =          | Warewolf | YES                            | NO |
	  When "WFWithAssignandFindRecordindex" is executed
	  Then the workflow execution has "AN" error
	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | Warewolf  | 
	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as   
	  | # |                                  |
	  | 1 | [[rec(1).a]]         =  Warewolf |
	  And the 'FindRecord' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
	  | #           |                         | # |   |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	  | In Field(s) | [[rec(1).a]] = Warewolf | 1 | = | Warewolf |  |     | YES                         | NO                             |
	  And the 'FindRecord' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as
	  ||   
#

#Bug 12180, 
#Scenario: Workflow contains Assign and Find Record index executing with invalid result variable
#      Given I have a workflow "WFWithAssignandFindRecordindex"
#	  And "WFWithAssignandFindRecordindex" contains an Assign "Record" as
#      | # | variable     | value    |
#      | # | [[rec(1).a]] | Warewolf |
#	  And "WFWithAssignandFindRecordindex" contains Find Record Index "FindRecord" into result as "[[a]]*]]"
#      | # | In Field     | #        | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
#      | # | [[rec().a]] | 1        | =          | Warewolf | YES                            | NO |
#	  When "WFWithAssignandFindRecordindex" is executed
#	  Then the workflow execution has "AN" error
#	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
#	  | # | Variable       | New Value |
#	  | 1 | [[rec(1).a]] = | Warewolf  | 
#	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as   
#	  | # |                                  |
#	  | 1 | [[rec(1).a]]         =  Warewolf |
#	  And the 'FindRecord' in WorkFlow 'WFWithAssignandFindRecordindex' debug inputs as 
#	  | #           |                         | # |   |          |  | And | Require All Fields To Match | Require All Matches To Be True |
#	  | In Field(s) | [[rec(1).a]] = Warewolf | 1 | = | Warewolf |  |     | YES                         | NO                             |
#	  And the 'FindRecord' in Workflow 'WFWithAssignandFindRecordindex' debug outputs as   
#	  |            |
#	  

	
#Scenario Outline: Testing Count with two variables in Result field
#      Given I have a workflow "WorkflowforCount"
#      And "WorkflowforCount" contains an Assign "Rec To Convert" as
#	  | variable    | value |
#	  | [[rec().a]] | 1213  |
#	  | [[rec().a]] | 4561  |
#	  And "WorkflowforCount" contains Count Record "CountRec" on "[[rec()]]" into '<Variable>'
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
#Examples: 
#       | No    | Variable       |
#       | 1     | [[Count]][[a]] |
# #12180 | 2     | [[count]]*]]   |
##
# 12180 remove this as per previous step
Scenario: Testing Count with two variables in Result field
      Given I have a workflow "WorkflowforCount"
      And "WorkflowforCount" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | 1213  |
	  | [[rec().a]] | 4561  |
	  And "WorkflowforCount" contains Count Record "CountRec" on "[[rec()]]" into "[[count]][[a]]"
	  When "WorkflowforCount" is executed
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforCount' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1213      |
	  | 2 | [[rec().a]] = | 4561      |
	  And the 'Rec To Convert' in Workflow 'WorkflowforCount' debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = 1213 |
	  | 2 | [[rec(2).a]] = 4561 |
	  And the 'CountRec' in WorkFlow 'WorkflowforCount' debug inputs as
	  | Recordset            |
	  | [[rec(1).a]] = 1213 |
	  | [[rec(2).a]] = 4561 |
	  And the 'CountRec' in Workflow 'WorkflowforCount' debug outputs as    
	  |               |


#Scenario Outline: Testing Length with two variables in Result field
#      Given I have a workflow "WorkflowforLength"
#      And "WorkflowforLength" contains an Assign "Rec To Convert" as
#	  | variable    | value |
#	  | [[rec().a]] | 1213  |
#	  | [[rec().a]] | 4561  |
#	  And "WorkflowforLength" contains Length "Len" on "[[rec(*)]]" into '<Variable>'
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
#	  |                |
#Examples:
#      | No    | Variable       |
#      | 1     | [[length]][[a]] |
#12180 
 #     | 2  | [[a]]*]]               |
 #     | 3  | [[var@]]               |
 #     | 4  | [[var]]00]]            |
 #     | 5  | [[(1var)]]             |
 #     | 6  | [[var[[a]]]]           |
 #     | 7  | [[var.a]]              |
 #     | 8  | [[@var]]               |
 #     | 9  | [[var 1]]              |
 #     | 10 | [[rec(1).[[rec().1]]]] |
 #     | 11 | [[rec(@).a]]           |
 #     | 12 | [[rec"()".a]]          |
 #     | 13 | [[rec([[[[b]]]]).a]]   |


##12180  -- remove this when previous is passing
 #Scenario: Testing Length with two variables in Result field
 #     Given I have a workflow "WorkflowforLength"
 #     And "WorkflowforLength" contains an Assign "Rec To Convert" as
	#  | variable    | value |
	#  | [[rec().a]] | 1213  |
	#  | [[rec().a]] | 4561  |
	#  And "WorkflowforLength" contains Length "Len" on "[[rec(*)]]" into "[[length]][[a]]"
	#  When "WorkflowforLength" is executed
	#  Then the workflow execution has "AN" error	
 #     And the 'Rec To Convert' in WorkFlow 'WorkflowforLength' debug inputs as
	#  | # | Variable      | New Value |
	#  | 1 | [[rec().a]] = | 1213      |
	#  | 2 | [[rec().a]] = | 4561      |
	#  And the 'Rec To Convert' in Workflow 'WorkflowforLength' debug outputs as    
	#  | # |                     |
	#  | 1 | [[rec(1).a]] = 1213 |
	#  | 2 | [[rec(2).a]] = 4561 |
	#  And the 'Len' in WorkFlow 'WorkflowforLength' debug inputs as
	#  | Recordset           |
	#  | [[rec(1).a]] = 1213 |
	#  | [[rec(2).a]] = 4561 |
	#  And the 'Len' in Workflow 'WorkflowforLength' debug outputs as    
	#  |                |
	#  |                |


@ignore
Scenario Outline: Testing Find Index with two variables in Result field
      Given I have a workflow "WorkflowforFI"
      And "WorkflowforFI" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | 141   |
	  | [[rec().a]] | 4561  |
	  And "WorkflowforFI" contains Find Index "Index" into '<Variable>' as
	  | In Fields    | Index         | Character | Direction     |
	  | [[rec(*).a]] | All Occurence | 1         | Left to Right |	
	  When "WorkflowforFI" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforFI' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 141       |
	  | 2 | [[rec().a]] = | 4561      |
	  And the 'Rec To Convert' in Workflow 'WorkflowforFI' debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = 141  |
	  | 2 | [[rec(2).a]] = 4561 |
	  And the 'Index' in WorkFlow 'WorkflowforFI' debug inputs as
	  | In Field            | Index         | Characters | Direction     |
	  | [[rec(1).a]] = 141  |               |            |               |
	  | [[rec(2).a]] = 4561 | All Occurence | 1          | Left to Right |
	  And the 'Index' in Workflow 'WorkflowforFI' debug outputs as
	  |                   |
Examples: 
      | No    | Variable       |
      | 1     | [[a]][[indexResult]] |
##12180 
# #     | 2  | [[a]]*]]               |
# #     | 3  | [[var@]]               |
# #     | 4  | [[var]]00]]            |
# #     | 5  | [[(1var)]]             |
# #     | 6  | [[var[[a]]]]           |
# #     | 7  | [[var.a]]              |
# #     | 8  | [[@var]]               |
# #     | 9  | [[var 1]]              |
# #     | 10 | [[rec(1).[[rec().1]]]] |
# #     | 11 | [[rec(@).a]]           |
# #     | 12 | [[rec"()".a]]          |
# #     | 13 | [[rec([[[[b]]]]).a]]   |
##

@ignore
Scenario Outline: Testing Data Merge with two variables in Result field
      Given I have a workflow "WorkflowforDataMerge"
      And "WorkflowforDataMerge" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | Test     |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowforDataMerge" contains Data Merge "Data Merge" into '<Variable>' as	
	  | Variable     | Type | Using | Padding | Alignment |
	  | [[rec(1).a]] | None |       |         | Left      |
	  | [[rec(2).a]] | None |       |         | Left      |
	  When "WorkflowforDataMerge" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforDataMerge' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	  And the 'Rec To Convert' in Workflow 'WorkflowforDataMerge' debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = Test     |
	  | 2 | [[rec(2).a]] = Warewolf |
	 And the 'Data Merge' in WorkFlow 'WorkflowforDataMerge' debug inputs as 
	  | # |                         | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = Test     | None | ""    | ""  | Left  |
	  | 2 | [[rec(2).a]] = Warewolf | None | ""    | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowforDataMerge' debug outputs as  
	  |                   |
	  | [[result]][[a]] = |  
Examples: 
      | No    | Variable       |
      | 1     | [[a]][[Result]] |
##12180 
# #     | 2  | [[a]]*]]               |
# #     | 3  | [[var@]]               |
# #     | 4  | [[var]]00]]            |
# #     | 5  | [[(1var)]]             |
# #     | 6  | [[var[[a]]]]           |
# #     | 7  | [[var.a]]              |
# #     | 8  | [[@var]]               |
# #     | 9  | [[var 1]]              |
# #     | 10 | [[rec(1).[[rec().1]]]] |
# #     | 11 | [[rec(@).a]]           |
# #     | 12 | [[rec"()".a]]          |
# #     | 13 | [[rec([[[[b]]]]).a]]   |


Scenario: Testing Data Split with two variables in Result field
      Given I have a workflow "WorkflowforDatasplit"
      And "WorkflowforDatasplit" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | Test     |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowforDatasplit" contains Data Split "Data Split" as
	  | String       | Variable        | Type  | At | Include    | Escape |
	  | [[rec(1).a]] | [[fr().a]][[a]] | Index | 2  | Unselected |        |
	  |              | [[fr().b]][[b]] | Index | 2  | Unselected |        |
	  When "WorkflowforDatasplit" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforDatasplit' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	  And the 'Rec To Convert' in Workflow 'WorkflowforDatasplit' debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = Test     |
	  | 2 | [[rec(2).a]] = Warewolf |
	 And the 'Data Split' in WorkFlow 'WorkflowforDatasplit' debug inputs as 
	  | String to Split     | Process Direction | Skip blank rows | # |                   | With  | Using | Include | Escape |
	  | [[rec(1).a]] = Test | Forward           | No              | 1 | [[fr().a]][[a]] = | Index | 2     | No      |        |
	  |                     |                   |                 | 2 | [[fr().b]][[b]] = | Index | 2     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowforDatasplit' debug outputs as  
	  | # |                    |
	  | 1 | [[fr(1).aa]] = |
	  | 2 | [[fr(1).bb]] = |
	
@ignore
Scenario Outline: Testing Replace with two variables in Result field
      Given I have a workflow "WorkflowforReplace"
      And "WorkflowforReplace" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | Test     |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowforReplace" contains Replace "Replac" into '<Variable>' as	
	  | In Fields    | Find | Replace With |
	  | [[rec(*).a]] | Test | rocks        |
	  When "WorkflowforReplace" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforReplace' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	  And the 'Rec To Convert' in Workflow 'WorkflowforReplace' debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = Test     |
	  | 2 | [[rec(2).a]] = Warewolf |
	  And the 'Replac' in WorkFlow 'WorkflowforReplace' debug inputs as 	
	  | In Field(s)             | Find | Replace With |
	  | [[rec(1).a]] = Test     |      |              |
	  | [[rec(2).a]] = Warewolf | Test | rocks        |
	  And the 'Replac' in Workflow 'WorkflowforReplace' debug outputs as 
	  |                   |
	  | [[a]][[b]][[c]] = |
Examples: 
      | No    | Variable       |
      | 1     | [[a]][[Result]] |
#12180 
 #     | 2  | [[a]]*]]               |
 #     | 3  | [[var@]]               |
 #     | 4  | [[var]]00]]            |
 #     | 5  | [[(1var)]]             |
 #     | 6  | [[var[[a]]]]           |
 #     | 7  | [[var.a]]              |
 #     | 8  | [[@var]]               |
 #     | 9  | [[var 1]]              |
 #     | 10 | [[rec(1).[[rec().1]]]] |
 #     | 11 | [[rec(@).a]]           |
 #     | 12 | [[rec"()".a]]          |
 #     | 13 | [[rec([[[[b]]]]).a]]   |

	
@ignore
Scenario Outline: Testing Calculate with two variables in Result field
      Given I have a workflow "WorkflowforCals"
      And "WorkflowforCals" contains an Assign "Values" as
	  | variable | value |
	  | [[a]]    | 1     |
	  | [[b]]    | 2     |
	 And "WorkflowforCal" contains Calculate "Calculate1" with formula "[[a]]+[[b]]" into '<Variable>'	 
	  When "WorkflowforCals" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Values' in WorkFlow 'WorkflowforCals' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | 1         |
	  | 2 | [[b]] =  | 2         |
	  And the 'Values' in Workflow 'WorkflowforCals' debug outputs as    
	  | # |           |
	  | 1 | [[a]] = 1 |
	  | 2 | [[b]] = 2 |
	  And the 'Calculate1' in WorkFlow 'WorkflowforCals' debug inputs as 
      | fx =                  |
      | [[a]]+[[b]] = 1+2 = 2 |           
      And the 'Calculate1' in Workflow 'WorkflowforCals' debug outputs as  
	  |              |
	  | '<Variable>' |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |
      #| 2  | [[a]]*]]               |
      #| 3  | [[var@]]               |
      #| 4  | [[var]]00]]            |
      #| 5  | [[(1var)]]             |
      #| 6  | [[var[[a]]]]           |
      #| 7  | [[var.a]]              |
      #| 8  | [[@var]]               |
      #| 9  | [[var 1]]              |
      #| 10 | [[rec(1).[[rec().1]]]] |
      #| 11 | [[rec(@).a]]           |
      #| 12 | [[rec"()".a]]          |
      #| 13 | [[rec([[[[b]]]]).a]]   |


Scenario Outline: Testing Format Numbers with two variables in Result
      Given I have a workflow "Workflowforfn"
	  And "Workflowforfn" contains an Assign "Values" as
	  | variable | value |
	  | [[a]]    | 1     |
	  | [[b]]    | 2     |
	  And "Workflowforfn" contains Format Number "Fnumber" as 
	  | Number  | Rounding Selected | Rounding To | Decimal to show | Result       |
	  | 123.568 | Up                | 2           | 2               | '<Variable>' |
	  When "Workflowforfn" is executed  	  
	  Then the workflow execution has "AN" error
	  And the 'Fnumber' in WorkFlow 'Workflowforfn' debug inputs as 	
	  | Number  | Rounding | Rounding Value | Decimals to show |
	  | 123.568 | Up       | 2              | 2                |
	  And the 'Fnumber' in Workflow 'Workflowforfn' debug outputs as 
	  |                |
	  | '<Variable>' = |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |
      #| 2  | [[a]]*]]               |
      #| 3  | [[var@]]               |
      #| 4  | [[var]]00]]            |
      #| 5  | [[(1var)]]             |
      #| 6  | [[var[[a]]]]           |
      #| 7  | [[var.a]]              |
      #| 8  | [[@var]]               |
      #| 9  | [[var 1]]              |
      #| 10 | [[rec(1).[[rec().1]]]] |
      #| 11 | [[rec(@).a]]           |
      #| 12 | [[rec"()".a]]          |
      #| 13 | [[rec([[[[b]]]]).a]]   |


#Scenario Outline: Testing Random Numbers with two variables in Result
#      Given I have a workflow "Workflowforrandom123"
#	  And "Workflowforrandom123" contains an Assign "Values" as
#	  | variable | value |
#	  | [[a]]    | 1     |
#	  | [[b]]    | 10     |
#	  And "Workflowforrandom123" contains Random "Randoms" as
#	  | Type    | From | To | Result       |
#	  | Numbers | 1    | 10 | '<Variable>' |
#	  When "Workflowforrandom123" is executed  	  
#	  Then the workflow execution has "AN" error
#	   And the 'Values' in WorkFlow 'Workflowforrandom123' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]] =  | 1         |
#	  | 2 | [[b]] =  | 10        |
#	  And the 'Values' in Workflow 'Workflowforrandom123' debug outputs as    
#	  | # |             |
#	  | 1 | [[a]] =  1  |
#	  | 2 | [[b]] =  10 |
#	  And the 'Randoms' in WorkFlow 'Workflowforrandom123' debug inputs as 
#	    | Random  | From | To |
#	    | Numbers | 1    | 10  |
#      And the 'Randoms' in Workflow 'Workflowforrandom123' debug outputs as
#	  |              |
#	  | '<Variable>' |
#Examples: 
#      | No | Variable               |
#      | 1  | [[a]][[Result]]        |
#      | 2  | [[a]]*]]               |
#      | 3  | [[var@]]               |
#      | 4  | [[var]]00]]            |
#      | 5  | [[(1var)]]             |
#      | 6  | [[var[[a]]]]           |
#      | 7  | [[var.a]]              |
#      | 8  | [[@var]]               |
#      | 9  | [[var 1]]              |
#      | 10 | [[rec(1).[[rec().1]]]] |
#      | 11 | [[rec(@).a]]           |
#      | 12 | [[rec"()".a]]          |
#      | 13 | [[rec([[[[b]]]]).a]]   |
#	  							
#
#	 
#Scenario Outline: Testing Date and Time with two variables in Result field
#      Given I have a workflow "WorkflowforDT"
#      And "WorkflowforDT" contains an Assign "Convert2" as
#	  | variable    | value      |
#	  | [[rec().a]] | 12/01/2001 |
#	  And "WorkflowforDT" contains Date and Time "AddDates" as
#      | Input       | Input Format | Add Time | Output Format | Result       |
#      | [[rec().a]] | dd/mm/yyyy   | 1        | dd/mm/yyyy    | '<Variable>' |	
#	  When "WorkflowforDT" is executed  	  
#	  Then the workflow execution has "AN" error	
#      And the 'Convert2' in WorkFlow 'WorkflowforDT' debug inputs as
#	  | # | Variable      | New Value  |
#	  | 1 | [[rec().a]] = | 12/01/2001 |
#	  And the 'Convert2' in Workflow 'WorkflowforDT' debug outputs as    
#	  | # |                           |
#	  | 1 | [[rec(1).a]] = 12/01/2001 |
#	  And the 'AddDates' in WorkFlow 'WorkflowforDT' debug inputs as
#	   | Input                     | Input Format | Add Time |   | Output Format |
#	   | [[rec(1).a]] = 12/01/2001 | dd/mm/yyyy   | Years    | 1 | dd/mm/yyyy    |	
#	  And the 'AddDates' in Workflow 'WorkflowforDT' debug outputs as   
#	   |              |
#	   | '<Variable>' |
#Examples: 
#      | No | Variable               |
#      | 1  | [[a]][[Result]]        |
#      | 2  | [[a]]*]]               |
#      | 3  | [[var@]]               |
#      | 4  | [[var]]00]]            |
#      | 5  | [[(1var)]]             |
#      | 6  | [[var[[a]]]]           |
#      | 7  | [[var.a]]              |
#      | 8  | [[@var]]               |
#      | 9  | [[var 1]]              |
#      | 10 | [[rec(1).[[rec().1]]]] |
#      | 11 | [[rec(@).a]]           |
#      | 12 | [[rec"()".a]]          |
#      | 13 | [[rec([[[[b]]]]).a]]   |


#   
#Scenario Outline: Testing Date Time Diff with two variables in Result field
#      Given I have a workflow "WorkflowforDateTimeDiff"
#      And "WorkflowforDateTimeDiff" contains an Assign "Values" as
#	  | variable    | value      |
#	  | [[rec().a]] | 01/01/2001 |
#	  | [[rec().a]] | 01/01/2010 |
#	  And "WorkflowforDateTimeDiff" contains Date and Time Difference "DateAndTime" as	
#	  | Input1       | Input2       | Input Format | Output In | Result               |
#	  | [[rec(1).a]] | [[rec(2).a]] | dd/mm/yyyy   | Years     | '<Variable>' |	   
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
#	  |  |
#Examples: 
#      | No | Variable               |
#      | 1  | [[a]][[Result]]        |
#      | 2  | [[a]]*]]               |
#      | 3  | [[var@]]               |
#      | 4  | [[var]]00]]            |
#      | 5  | [[(1var)]]             |
#      | 6  | [[var[[a]]]]           |
#      | 7  | [[var.a]]              |
#      | 8  | [[@var]]               |
#      | 9  | [[var 1]]              |
#      | 10 | [[rec(1).[[rec().1]]]] |
#      | 11 | [[rec(@).a]]           |
#      | 12 | [[rec"()".a]]          |
#      | 13 | [[rec([[[[b]]]]).a]]   |
#

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

Scenario: Workflow with Assign and Unique Tool, Infields rec without star
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
	  | # | Variable         | New Value |
	  | 1 | [[rs(1).row]] =  | 10        |
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


Scenario: Convert an recordset to Upper by using index as scalar
	Given I have a workflow "ConvertUsingScalarInRecursiveEvalution"
	And "ConvertUsingScalarInRecursiveEvalution" contains an Assign "Records" as
	  | variable     | value    |
	  | [[rs().row]] | warewolf |
	  | [[a]]        | 1        |
	And "ConvertUsingScalarInRecursiveEvalution" contains case convert "Case to Convert" as
	  | Variable          | Type  |
	  | [[rs([[a]]).row]] | UPPER |
	When "ConvertUsingScalarInRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the 'Records' in WorkFlow 'ConvertUsingScalarInRecursiveEvalution' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rs().row]] = | warewolf  |
	  | 2 | [[a]] =        | 1         |
	And the 'Records' in Workflow 'ConvertUsingScalarInRecursiveEvalution' debug outputs as  
	  | # |                           |
	  | 1 | [[rs(1).row]] =  warewolf |
	  | 2 | [[a]] =  1                |
	And the 'Case to Convert' in WorkFlow 'ConvertUsingScalarInRecursiveEvalution' debug inputs as
	  | # | Convert                  | To    |
	  | 1 | [[rs(1).row]] = warewolf | UPPER |
	And the 'Case to Convert' in Workflow 'ConvertUsingScalarInRecursiveEvalution' debug outputs as
	  | # |                          |
	  | 1 | [[rs(1).row]] = WAREWOLF |

Scenario: Convert an recordset to Upper by using index as recordset
	Given I have a workflow "ConvertUsingRecSetInRecursiveEvalution"
	And "ConvertUsingRecSetInRecursiveEvalution" contains an Assign "Records" as
	  | variable       | value    |
	  | [[rs().row]]   | warewolf |
	  | [[rs().index]] | 1        |
	And "ConvertUsingRecSetInRecursiveEvalution" contains case convert "Case to Convert" as
	  | Variable                    | Type  |
	  | [[rs([[rs(1).index]]).row]] | UPPER |
	When "ConvertUsingRecSetInRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the 'Records' in WorkFlow 'ConvertUsingRecSetInRecursiveEvalution' debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[rs().row]] =   | warewolf  |
	  | 2 | [[rs().index]] = | 1         |
	And the 'Records' in Workflow 'ConvertUsingRecSetInRecursiveEvalution' debug outputs as  
	  | # |                          |
	  | 1 | [[rs(1).row]] = warewolf |
	  | 2 | [[rs(1).index]] = 1      |
	And the 'Case to Convert' in WorkFlow 'ConvertUsingRecSetInRecursiveEvalution' debug inputs as
	  | # | Convert                  | To    |
	  | 1 | [[rs(1).row]] = warewolf | UPPER |
	And the 'Case to Convert' in Workflow 'ConvertUsingRecSetInRecursiveEvalution' debug outputs as
	  | # |                          |
	  | 1 | [[rs(1).row]] = WAREWOLF |

#Bug 11840
Scenario: Base Convert two varibles on one row 
	Given I have a workflow "BaseConvertUsingRecSetInRecursiveEvalution"
	And "BaseConvertUsingRecSetInRecursiveEvalution" contains an Assign "Records" as
	  | variable    | value |
	  | [[rs().a]]  | 1     |
	  | [[rec().a]] | 2     |
	And "BaseConvertUsingRecSetInRecursiveEvalution" contains Base convert "Base to Convert" as
	  | Variable               | From | To      |
	  | [[rec([[rs(1).a]]).a]] | Text | Base 64 |
	When "BaseConvertUsingRecSetInRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the 'Records' in WorkFlow 'BaseConvertUsingRecSetInRecursiveEvalution' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | 1         |
	  | 2 | [[rec().a]] = | 2         |
	And the 'Records' in Workflow 'BaseConvertUsingRecSetInRecursiveEvalution' debug outputs as  
	  | # |                  |
	  | 1 | [[rs(1).a]] = 1  |
	  | 2 | [[rec(1).a]] = 2 |
	And the 'Base to Convert' in WorkFlow 'BaseConvertUsingRecSetInRecursiveEvalution' debug inputs as
	  | # | Convert           | From | To     |
	  | 1 | [[rec(1).a]] = 2 | Text | Base 64 |
    And the 'Base to Convert' in Workflow 'BaseConvertUsingRecSetInRecursiveEvalution' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = Mg== |

#This should be passed with the bug 12160
Scenario: Workflow by using For Each with Raandom in it
      Given I have a workflow "WFWithForEachContainsRandom"
	  And "WFWithForEachContainsRandom" contains a Foreach "ForEachTest123" as "NumOfExecution" executions "5"
	  And "ForEachTest123" contains Random "Random" as
	    | Type    | From | To | Result       |
	    | Numbers | 1    | 5  | [[rec(*).a]] |
      When "WFWithForEachContainsRandom" is executed
	  Then the workflow execution has "NO" error
	  And the 'ForEachTest123' in WorkFlow 'WFWithForEachContainsRandom' debug inputs as 
	    |                 | Number |
	    | No. of Executes | 5      |
      And the 'ForEachTest123' in WorkFlow 'WFWithForEachContainsRandom' has  "5" nested children 
	   And the 'Random' in step 1 for 'ForEachTest123' debug inputs as
	    | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the 'Random' in step 1 for 'ForEachTest123' debug outputs as
        |                      |
	    | [[rec(1).a]] = Int32 |
	  And the 'Random' in step 2 for 'ForEachTest123' debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the 'Random' in step 2 for 'ForEachTest123' debug outputs as
        |                      |
	    | [[rec(1).a]] = Int32 |
       And the 'Random' in step 3 for 'ForEachTest123' debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the 'Random' in step 3 for 'ForEachTest123' debug outputs as
         |                      |
	    | [[rec(1).a]] = Int32 |
      And the 'Random' in step 4 for 'ForEachTest123' debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the 'Random' in step 4 for 'ForEachTest123' debug outputs as
       |                      |
	    | [[rec(1).a]] = Int32 |
       And the 'Random' in step 5 for 'ForEachTest123' debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	And the 'Random' in step 5 for 'ForEachTest123' debug outputs as
         |                      |
         | [[rec(1).a]] = Int32 |


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

#bug 12470
Scenario: Workflow with Assign and foreach contains calculate. 
      Given I have a workflow "Workflowwithforeachcontainscalculates"
	  And "Workflowwithforeachcontainscalculates" contains an Assign "Assignval1" as
      | variable   | value |
      | [[rs().a]] | 1     |
      | [[rs().a]] | 2     |
      | [[rs().a]] | 3     |
	  And "Workflowwithforeachcontainscalculates" contains a Foreach "ForEachTesting" as "InRecordset" executions "[[rs()]]"
	  And "ForEachTesting" contains Calculate "Cal" with formula "[[rs(*).a]]+1" into "[[result]]"
	  When "Workflowwithforeachcontainscalculates" is executed
	  Then the workflow execution has "NO" error
      And the 'Assignval1' in WorkFlow 'Workflowwithforeachcontainscalculates' debug inputs as
      | # | Variable     | New Value |
      | 1 | [[rs().a]] = | 1         |
      | 2 | [[rs().a]] = | 2         |
      | 3 | [[rs().a]] = | 3         |
      And the 'Assignval1' in Workflow 'Workflowwithforeachcontainscalculates' debug outputs as 
      | # |                 |
      | 1 | [[rs(1).a]] = 1 |
      | 2 | [[rs(2).a]] = 2 |
      | 3 | [[rs(3).a]] = 3 |   
      And the 'ForEachTesting' in WorkFlow 'Workflowwithforeachcontainscalculates' has  "3" nested children
	  And the 'Cal' in step 1 for 'ForEachTesting' debug inputs as
      | fx =                |
      | [[rs(1).a]]+1 = 1+1 |           
      And the 'Cal' in step 1 for 'ForEachTesting' debug outputs as
	  |                |
	  | [[result]] = 2 |
	 And the 'Cal' in step 2 for 'ForEachTesting' debug inputs as 
      | fx =                |
      | [[rs(2).a]]+1 = 2+1 |           
       And the 'Cal' in step 2 for 'ForEachTesting' debug outputs as  
	  |                |
	  | [[result]] = 3 |
	   And the 'Cal' in step 3 for 'ForEachTesting' debug inputs as
      | fx =                |
      | [[rs(3).a]]+1 = 3+1 |           
       And the 'Cal' in step 3 for 'ForEachTesting' debug outputs as
	  |                |
	  | [[result]] = 4 |
##
##bug 12470
Scenario: Workflow with Assign and foreach with invalid rec and it contains calculate in it
      Given I have a workflow "WorkflowDwithforeachcontainscalinvalid"
	  And "WorkflowDwithforeachcontainscalinvalid" contains an Assign "Assigl" as
      | variable   | value |
      | [[rs().a]] | 1     |
      | [[rs().a]] | 2     |
      | [[rs().a]] | 3     |
	  And "WorkflowDwithforeachcontainscalinvalid" contains a Foreach "ForEachTes" as "InRecordset" executions "[[rs()]]+1"
	  And "ForEachTes" contains Calculate "Cal" with formula "[[rs(*).a]]+1" into "[[result]]"
	  When "WorkflowDwithforeachcontainscalinvalid" is executed
	  Then the workflow execution has "NO" error
      And the 'Assigl' in WorkFlow 'WorkflowDwithforeachcontainscalinvalid' debug inputs as
      | # | Variable     | New Value |
      | 1 | [[rs().a]] = | 1         |
      | 2 | [[rs().a]] = | 2         |
      | 3 | [[rs().a]] = | 3         |
      And the 'Assigl' in Workflow 'WorkflowDwithforeachcontainscalinvalid' debug outputs as 
      | # |                 |
      | 1 | [[rs(1).a]] = 1 |
      | 2 | [[rs(2).a]] = 2 |
      | 3 | [[rs(3).a]] = 3 |   
	   And the 'ForEachTes' in WorkFlow 'WorkflowDwithforeachcontainscalinvalid' has  "1" nested children


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

#This Test should be passed after the bug 12119 is fixed
Scenario: Workflow with Assign and Unique Tool to find unique names in diff rows
      Given I have a workflow "WorkflowUniqueWithNames"
      And "WorkflowUniqueWithNames" contains an Assign "Records1" as
	  | variable            | value    |
	  | [[emp().firstname]] | Smith    |
	  | [[emp().lastname]]  | Gordan   |
	  | [[emp().firstname]] | Nicholas |
	  | [[emp().lastname]]  | Cage     |
	  | [[emp().firstname]] | Cage     |
	  | [[emp().lastname]]  | Nicholas |
	  And "WorkflowUniqueWithNames" contains an Unique "Unique" as
	  | In Field(s)                              | Return Fields       | Result         |
	  | [[emp(*).firstname]],[[emp(*).lastname]] | [[emp().firstname]] | [[emp(*).uni]] |
	  When "WorkflowUniqueWithNames" is executed
	  Then the workflow execution has "NO" error
	  And the 'Records1' in WorkFlow 'WorkflowUniqueWithNames' debug inputs as
	  | # | Variable             | New Value |
	  | 1 | [[emp().firstname]] = | Smith     |
	  | 2 | [[emp().lastname]] = | Gordan    |
	  | 3 | [[emp().firstname]] = | Nicholas  |
	  | 4 | [[emp().lastname]] = | Cage      |
	  | 5 | [[emp().firstname]] = | Cage      |
	  | 6 | [[emp().lastname]] = | Nicholas  |
	  And the 'Records1' in Workflow 'WorkflowUniqueWithNames' debug outputs as  
	  | # |                                |
	  | 1 | [[emp(1).firstname]] =  Smith    |
	  | 2 | [[emp(1).lastname]] =  Gordan   |
	  | 3 | [[emp(2).firstname]] =  Nicholas |
	  | 4 | [[emp(2).lastname]] =  Cage     |
	  | 5 | [[emp(3).firstname]] =  Cage     |
	  | 6 | [[emp(3).lastname]] =  Nicholas |
	  And the 'Unique' in WorkFlow 'WorkflowUniqueWithNames' debug inputs as
       | #           |                                 | Return Fields         |
       | In Field(s) | [[emp(1).firstname]] = Smith    |                       |
       |             | [[emp(2).firstname]] = Nicholas |                       |
       |             | [[emp(3).firstname]] = Cage     |                       |
       |             | [[emp(1).lastname]] = Gordan    |                       |
       |             | [[emp(2).lastname]] = Cage      |                       |
       |             | [[emp(3).lastname]] = Nicholas  |                       |
       |             |                                 | [[emp().firstname]] = |     
      And the 'Unique' in Workflow 'WorkflowUniqueWithNames' debug outputs as  
       | # |                           |
       | 1 | [[emp(1).uni]]  = Smith    |
       |   | [[emp(2).uni]]  = Nicholas |
       |   | [[emp(3).uni]]  = Cage     |
   
#This Test should be passed after the bug 12119 is fixed
Scenario: Workflow with Assign and Unique to return unique data  
      Given I have a workflow "UniqueNamesTest"
      And "UniqueNamesTest" contains an Assign "Records1" as
	  | variable            | value    |
	  | [[emp().firstname]] | Smith    |
	  | [[emp().lastname]]  | Gordan   |
	  | [[emp().firstname]] | Nicholas |
	  | [[emp().lastname]]  | Cage     |
	  | [[emp().firstname]] | Cage     |
	  | [[emp().lastname]]  | Nicholas |
	  | [[emp().firstname]] | Cage     |
	  | [[emp().lastname]]  | Nicholas |
	  And "UniqueNamesTest" contains an Unique "Unique" as
	  | In Field(s)                              | Return Fields      | Result         |
	  | [[emp(*).firstname]],[[emp(*).lastname]] | [[emp().lastname]] | [[emp(*).uni]] |
	  When "UniqueNamesTest" is executed
	  Then the workflow execution has "NO" error
	  And the 'Records1' in WorkFlow 'UniqueNamesTest' debug inputs as
	  | # | Variable              | New Value |
	  | 1 | [[emp().firstname]] = | Smith     |
	  | 2 | [[emp().lastname]] =  | Gordan    |
	  | 3 | [[emp().firstname]] = | Nicholas  |
	  | 4 | [[emp().lastname]] =  | Cage      |
	  | 5 | [[emp().firstname]] = | Cage      |
	  | 6 | [[emp().lastname]] =  | Nicholas  |
	  | 7 | [[emp().firstname]] = | Cage      |
	  | 8 | [[emp().lastname]] =  | Nicholas  |
	  And the 'Records1' in Workflow 'UniqueNamesTest' debug outputs as  
	  | # |                                  |
	  | 1 | [[emp(1).firstname]] =  Smith    |
	  | 2 | [[emp(1).lastname]] =  Gordan    |
	  | 3 | [[emp(2).firstname]] =  Nicholas |
	  | 4 | [[emp(2).lastname]] =  Cage      |
	  | 5 | [[emp(3).firstname]] =  Cage     |
	  | 6 | [[emp(3).lastname]] =  Nicholas  |
	  | 7 | [[emp(4).firstname]] =  Cage     |
	  | 8 | [[emp(4).lastname]] =  Nicholas  |
	  And the 'Unique' in WorkFlow 'UniqueNamesTest' debug inputs as
       | #           |                                 | Return Fields        |
       | In Field(s) | [[emp(1).firstname]] = Smith    |                      |
       |             | [[emp(2).firstname]] = Nicholas |                      |
       |             | [[emp(3).firstname]] = Cage     |                      |
       |             | [[emp(4).firstname]] = Cage     |                      |
       |             | [[emp(1).lastname]] = Gordan    |                      |
       |             | [[emp(2).lastname]] = Cage      |                      |
       |             | [[emp(3).lastname]] = Nicholas  |                      |
       |             | [[emp(4).lastname]] = Nicholas  |                      |
       |             |                                 | [[emp().lastname]] = |    
      And the 'Unique' in Workflow 'UniqueNamesTest' debug outputs as  
       | # |                            |
       | 1 | [[emp(1).uni]]  = Gordan   |
       |   | [[emp(2).uni]]  = Cage     |
       |   | [[emp(3).uni]]  = Nicholas |
 
Scenario: Workflow with Assign and Unique Tool
      Given I have a workflow "WorkflowAssingUnique"
      And "WorkflowAssingUnique" contains an Assign "Records" as
	  | variable    | value |
	  | [[rs(1).a]] | 19    |
	  | [[rs(2).a]] | 20    |
	  | [[rs(3).a]] | 40    |
	  | [[rs(4).a]] | 50    |
	  | [[rs(1).b]] | 19    |
	  | [[rs(2).b]] | 20    |
	  | [[rs(3).b]] | 30    |
	  | [[rs(4).b]] | 80    |
	  And "WorkflowAssingUnique" contains an Unique "Unique rec" as
	  | In Field(s)             | Return Fields | Result           |
	  | [[rs(*).a]],[[rs(*).b]] | [[rs().a]]    | [[rec().unique]] |
	  When "WorkflowAssingUnique" is executed
	  Then the workflow execution has "NO" error
	  And the 'Records' in WorkFlow 'WorkflowAssingUnique' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 19        |
	  | 2 | [[rs(2).a]] = | 20        |
	  | 3 | [[rs(3).a]] = | 40        |
	  | 4 | [[rs(4).a]] = | 50        |
	  | 5 | [[rs(1).b]] = | 19        |
	  | 6 | [[rs(2).b]] = | 20        |
	  | 7 | [[rs(3).b]] = | 30        |
	  | 8 | [[rs(4).b]] = | 80        |
	  And the 'Records' in Workflow 'WorkflowAssingUnique' debug outputs as  
	  | # |                   |
	  | 1 | [[rs(1).a]] =  19 |
	  | 2 | [[rs(2).a]] =  20 |
	  | 3 | [[rs(3).a]] =  40 |
	  | 4 | [[rs(4).a]] =  50 |
	  | 5 | [[rs(1).b]] =  19 |
	  | 6 | [[rs(2).b]] =  20 |
	  | 7 | [[rs(3).b]] =  30 |
	  | 8 | [[rs(4).b]] =  80 |
	  And the 'Unique rec' in WorkFlow 'WorkflowAssingUnique' debug inputs as
       | #           |                  | Return Fields |
       | In Field(s) | [[rs(1).a]] = 19 |               |
       |             | [[rs(2).a]] = 20 |               |
       |             | [[rs(3).a]] = 40 |               |
       |             | [[rs(4).a]] = 50 |               |
       |             | [[rs(1).b]] = 19 |               |
       |             | [[rs(2).b]] = 20 |               |
       |             | [[rs(3).b]] = 30 |               |
       |             | [[rs(4).b]] = 80 |               |
       |             |                  | [[rs().a]] =  |
      And the 'Unique rec' in Workflow 'WorkflowAssingUnique' debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 19 |
       |   | [[rec(2).unique]] = 20 |
       |   | [[rec(3).unique]] = 40 |
       |   | [[rec(4).unique]] = 50 |

#Bug 12142
Scenario: Workflow with Calculation using Star notation
      Given I have a workflow "WorkflowWithAssignCalculationUsingStar"
      And "WorkflowWithAssignCalculationUsingStar" contains an Assign "Records" as
	  | variable    | value |
	  | [[rs(1).a]] | 19    |
	  | [[rs(2).a]] | 20    |
	  | [[rs(3).a]] | 40    |	 
	  And "WorkflowWithAssignCalculationUsingStar" contains an Assign "Calculation" as
	  | variable      | value          |
	  | [[rec().sum]] | =[[rs(*).a]]+1 |
	  When "WorkflowWithAssignCalculationUsingStar" is executed
	  Then the workflow execution has "AN" error
	  And the 'Records' in WorkFlow 'WorkflowWithAssignCalculationUsingStar' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 19        |
	  | 2 | [[rs(2).a]] = | 20        |
	  | 3 | [[rs(3).a]] = | 40        |
	  And the 'Records' in Workflow 'WorkflowWithAssignCalculationUsingStar' debug outputs as  
	  | # |                   |
	  | 1 | [[rs(1).a]] =  19 |
	  | 2 | [[rs(2).a]] =  20 |
	  | 3 | [[rs(3).a]] =  40 |
	   And the 'Calculation' in WorkFlow 'WorkflowWithAssignCalculationUsingStar' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[rec().sum]] = |           |
	  And the 'Calculation' in Workflow 'WorkflowWithAssignCalculationUsingStar' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).sum]] =  |

# This Test should be passed after the bug 12236 is fixed    
#Scenario: Workflow with Assign and Gather System Information executing with incorrect variable
#      Given I have a workflow "workfloforGatherSystemInformationtool"
#	  And "workfloforGatherSystemInformationtool" contains an Assign "Assign for sys" as
#	  | variable | value |
#	  | [[a]]    | 123   |
#	   And "workfloforGatherSystemInformationtool" contains Gather System Info "System info" as
#	  | Variable         | Selected    |
#	  | [[rec[[a]]().a]] | Date & Time |
#	  When "workfloforGatherSystemInformationtool" is executed
#	  Then the workflow execution has "AN" error
#	  And the 'Assign for sys' in WorkFlow 'workfloforGatherSystemInformationtool' debug inputs as
#	  | # | Variable | New Value |
#	  | 1 | [[a]]  = | 123       |
#	  And the 'Assign for sys' in Workflow 'workfloforGatherSystemInformationtool' debug outputs as    
#	 | #                  |             |
#	 | 1                  | [[a]] = 123 |
#	  And the 'System info' in WorkFlow 'workfloforGatherSystemInformationtool' debug inputs as
#	  | # |                    |             |
#	  | 1 | [[rec[[a]]().a]] = | Date & Time |
#	  And the 'System info' in Workflow 'workfloforGatherSystemInformationtool' debug outputs as   
#	  |                        |
##	  
 
#  Bug 12341 
Scenario: Workflow with Assign Unique to check debug outputs
      Given I have a workflow "workflowithAssignUniquedebugoutputs"
      And "workflowithAssignUniquedebugoutputs" contains an Assign "Recordset" as
	  | variable          | value |
	  | [[team(1).Names]] | test  |
	  | [[team(1).Id]]    | 23    |
	  | [[team(2).Names]] | test  |
	  | [[team(2).Id]]    | 23    |
	  And "workflowithAssignUniquedebugoutputs" contains an Unique "Uni" as
	  | In Field(s)       | Return Fields    | Result           |
	  | [[team(*).Names]] | [[team().Names]] | [[List(*).Name]] |
	  When "workflowithAssignUniquedebugoutputs" is executed
	  Then the workflow execution has "NO" error
	  And the 'Recordset' in WorkFlow 'workflowithAssignUniquedebugoutputs' debug inputs as
	  | # | Variable             | New Value |
	  | 1 | [[team(1).Names]]  = | test      |
	  | 2 | [[team(1).Id]]     = | 23        |
	  | 3 | [[team(2).Names]]  = | test      |
	  | 4 | [[team(2).Id]]     = | 23        |
	  And the 'Recordset' in Workflow 'workflowithAssignUniquedebugoutputs' debug outputs as  
	  | # |                            |
	  | 1 | [[team(1).Names]] =   test |
	  | 2 | [[team(1).Id]]    =  23    |
	  | 3 | [[team(2).Names]] =  test  |
	  | 4 | [[team(2).Id]]    =  23    |
	  And the 'Uni' in WorkFlow 'workflowithAssignUniquedebugoutputs' debug inputs as
       | #           |                          | Return Fields      |
       | In Field(s) | [[team(1).Names]] = test |                    |
       |             | [[team(2).Names]] = test | [[team().Names]] = |
      And the 'Uni' in Workflow 'workflowithAssignUniquedebugoutputs' debug outputs as  
       | # |                         |
       | 1 | [[List(1).Name]] = test |
       

#12326
Scenario: Workflow Saving with Different Versions 
	 Given I have a workflow "WorkflowWithVersionAssignTest"
	 And "WorkflowWithVersionAssignTest" contains an Assign "VarsAssign" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |	 
	  When workflow "WorkflowWithVersionAssignTest" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignTest" has "0" Versions in explorer
	  When workflow "WorkflowWithVersionAssignTest" is saved "2" time
	  Then workflow "WorkflowWithVersionAssignTest" has "2" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.2 DateTime        |
	  | v.1 DateTime        |
	  When workflow "WorkflowWithVersionAssignTest" is saved "3" time
	  Then workflow "WorkflowWithVersionAssignTest" has "5" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.5 DateTime Save   |
	  | v.4 DateTime Save   |
	  | v.3 DateTime Save   |
	  | v.2 DateTime Save   |
	  | v.1 DateTime Save   |


Scenario: Executing workflow of different versions
	 Given I have a workflow "WorkflowWithVersionAssignExecuted2"
	 And "WorkflowWithVersionAssignExecuted2" contains an Assign "VarsAssign" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |	 
	  When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "0" Versions in explorer
	  When "WorkflowWithVersionAssignExecuted2" is executed without saving
	  Then the workflow execution has "NO" error
	  And the 'VarsAssign' in WorkFlow 'WorkflowWithVersionAssignExecuted2' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | New       |
	  | 2 | [[rec().a]] = | Test      |
	  And the 'VarsAssign' in Workflow 'WorkflowWithVersionAssignExecuted2' debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = New  |
	  | 2 | [[rec(2).a]] = Test | 
	  When workflow "WorkflowWithVersionAssignExecuted2" is saved "2" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "2" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.2 DateTime        |
	  | v.1 DateTime        |
	 And "WorkflowWithVersionAssignExecuted2" contains an Assign "VarsAssign2" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |
	  | [[rec().a]] | V1    |
	 When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	 When "WorkflowWithVersionAssignExecuted2" is executed without saving
	 Then the workflow execution has "NO" error
	 And the 'VarsAssign2' in WorkFlow 'WorkflowWithVersionAssignExecuted2' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | New       |
	  | 2 | [[rec().a]] = | Test      |
	  | 3 | [[rec().a]] = | V1        |	  
	 When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "4" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.4 DateTime        |
	  | v.3 DateTime        |
	  | v.2 DateTime        |
	  | v.1 DateTime        |	
	  When I rollback "WorkflowWithVersionAssignExecuted" to version "1"
	  When "WorkflowWithVersionAssignExecuted2" is executed without saving
	  Then the workflow execution has "NO" error
	  And the 'VarsAssign' in Workflow 'WorkflowWithVersionAssignExecuted2' debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = New  |
	  | 2 | [[rec(2).a]] = Test |
	  And the 'VarsAssign' in Workflow 'WorkflowWithVersionAssignExecuted2' debug outputs does not exist|



#Bug 12050
Scenario: Workflow with 2 Assigns testing variable that hasn't been assigned
      Given I have a workflow "WFTOTestBlanvValues"
	  And "WFTOTestBlanvValues" contains an Assign "Record1" as
      | # | variable  | value    |
      | # | [[Value]] | Warewolf | 
	  And "WFTOTestBlanvValues" contains an Assign "Record2" as
      | # | variable    | value      |
      | # | [[rec().a]] | [[Value1]] |
	  When "WFTOTestBlanvValues" is executed
	  Then the workflow execution has "AN" error
	  And the 'Record1' in WorkFlow 'WFTOTestBlanvValues' debug inputs as 
	  | # | Variable    | New Value |
	  | 1 | [[Value]] = | Warewolf  | 
	  And the 'Record1' in Workflow 'WFTOTestBlanvValues' debug outputs as   
	  | # |                          |
	  | 1 | [[Value]]    =  Warewolf |
	  And the 'Record2' in WorkFlow 'WFTOTestBlanvValues' debug inputs as 
	  | # | Variable      | New Value    |
	  | 1 | [[rec().a]] = | [[Value1]] = |
	  And the 'Record2' in Workflow 'WFTOTestBlanvValues' debug outputs as   
	  | # |                |
	  | 1 | [[rec(1).a]] = |


Scenario: Workflow with Assign Base Convert and Case Convert testing variable that hasn't been assigned
	  Given I have a workflow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues"
	  And "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" contains an Assign "Assign1" as
	  | variable    | value |
	  | [[res]] | 1    |
	  And "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" contains case convert "Case to Convert" as
	  | Variable     | Type  |
	  | [[res12]] | UPPER |
	  And "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" contains Base convert "Base to Convert" as
	  | Variable  | From | To      |
	  | [[res12]] | Text | Base 64 |
	  When "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign1' in WorkFlow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | 1         |
	   And the 'Assign1' in Workflow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug outputs as  
	  | # |              |
	  | 1 | [[res]] =  1 |
	  And the 'Case to Convert' in WorkFlow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug inputs as
	  | # | Convert    | To    |
	  | 1 | [[res12]] = | UPPER |
	  And the 'Case to Convert' in Workflow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug outputs as  
	  | # |             |
	  | 1 | [[res12]] = |
	  And the 'Base to Convert' in WorkFlow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug inputs as
	  | # | Convert     | From | To      |
	  | 1 | [[res12]] = | Text | Base 64 |
      And the 'Base to Convert' in Workflow 'WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues' debug outputs as  
	  | # |             |


Scenario: Workflow with Assigns DataMerge and DataSplit and testing variables that hasn't been assigned
      Given I have a workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues"
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" contains an Assign "Assign To merge" as
      | variable | value |
      | [[res]]  | Test  |
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable  | Type  | Using | Padding | Alignment |
	  | [[Value]] | Index | 4     |         | Left      |
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" contains Data Split "Data Split" as
	  | String      | Variable    | Type  | At | Include    | Escape |
	  | [[Value12]] | [[rec().b]] | Index | 4  | Unselected |        |
	  When "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign To merge' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug inputs as 
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | Test      |
	 And the 'Assign To merge' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug outputs as   
	  | # |                          |
	  | 1 | [[res]]          =  Test |
	  And the 'Data Merge' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug inputs as 
	  | # |             | With  | Using | Pad | Align |
	  | 1 | "" =   | Index | "4"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug outputs as  
	  |             |
	  | [[result]] = |
	  And the 'Data Split' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[Value12]]  =  | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug outputs as  
	  | # |               |
	  | 1 | [[rec(1).b]] = |

Scenario: Workflow with Assigns Replace and testing variables that hasn't been assigned
      Given I have a workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues"
       And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains an Assign "Assign34" as
      | variable | value |
      | [[Val]]  | test  |
	  And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains Replace "Replacing" into "[[replac]]" as	
	  | In Fields  | Find     | Replace With |
	  | [[rec()]] | [[Val1]] | [[Val2]]     |
	  When "workflowithAssignandReplaceTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign34' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | test      |
	   And the 'Assign34' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug outputs as    
	  | # |                |
	  | 1 | [[Val]] = test |
	  And the 'Replacing' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug inputs as 
	  | In Field(s) | Find | Replace With |
	  | [[rec()]] = |      |              |
	  And the 'Replacing' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug outputs as
	  |              |
	  | [[replac]] = |
	 

Scenario: Workflow with Assign Format Numbers and testing variables that hasn't been assigned
	  Given I have a workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues"
	  And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[val]]  | 1     | 	  
      And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" contains Format Number "Fnumber" as 
	  | Number   | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[val1]] | Up                | [[val1]]    | [[val1]]        | [[fresult]] |
	  When "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "AN" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues' debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 1         |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues' debug outputs as  
	  | # |              |
	  | 1 | [[val]]  = 1 |   
	  And the 'Fnumber' in WorkFlow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues' debug inputs as 	
	  | Number     | Rounding | Rounding Value | Decimals to show |
	  | [[val1]] = | Up       | [[val1]] =     | [[val1]]  =      |
	  And the 'Fnumber' in Workflow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues' debug outputs as 
	  |             |
	  | [[fresult]]  = |


Scenario: Workflow with Assign Create Delete folder and testing variable values that hasn't been assigned
	  Given I have a workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues"
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" contains an Assign "AssignT" as
	  | variable    | value           |
	  | [[rec().a]] | C:\copied00.txt |
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" contains an Create "Create12" as
	  | File or Folder | If it exits | Username | Password | Result   |
	  | [[NoValue]]    | True        |          |          | [[res1]] |
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" contains an Delete Folder "DeleteFolder1" as
	  | Recordset   | Result   |
	  | [[NoValue]] | [[res2]] |
	  When "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "AN" error
	  And the 'AssignT' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug inputs as
	  | # | Variable      | New Value       |
	  | 1 | [[rec().a]] = | C:\copied00.txt |
	  And the 'AssignT' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug outputs as     
	  | # |                              |
	  | 1 | [[rec(1).a]] = C:\copied00.txt |
	 And the 'Create12' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug inputs as
	  | File or Folder | Overwrite | Username | Password |
	  | [[NoValue]] =  | True      | ""       | ""       |  
	   And the 'Create12' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug outputs as    
	   |            |
	   | [[res1]] = |
	  And the 'DeleteFolder1' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug inputs as
	  | Input Path    | Username | Password |
	  | [[NoValue]] = | ""       | ""       |
	  And the 'DeleteFolder1' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug outputs as    
	  |            |
	  | [[res2]] = |


Scenario: Calculate testing variable values that hasn't been assigned
      Given I have a workflow "WorkflowforCalTestingUnassignedvariablevalue"
      And "WorkflowforCalTestingUnassignedvariablevalue" contains an Assign "Values34" as
	  | variable | value |
	  | [[Val]]    | 1     |
	 And "WorkflowforCalTestingUnassignedvariablevalue" contains Calculate "Calculate1" with formula "[[Val1]]+1" into "[[res]]"
	  When "WorkflowforCalTestingUnassignedvariablevalue" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Values34' in WorkFlow 'WorkflowforCalTestingUnassignedvariablevalue' debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | 1         |
	  And the 'Values34' in Workflow 'WorkflowforCalTestingUnassignedvariablevalue' debug outputs as    
	  | # |             |
	  | 1 | [[Val]] = 1 |
	  And the 'Calculate1' in WorkFlow 'WorkflowforCalTestingUnassignedvariablevalue' debug inputs as 
      | fx =                    |
      | [[Val1]]+1 = [[Val1]]+1 |           
      And the 'Calculate1' in Workflow 'WorkflowforCalTestingUnassignedvariablevalue' debug outputs as  
	  |           |
	  | [[res]] = |

Scenario: Workflow with Assign and Random and testing variable values that hasn't been assigned
	 Given I have a workflow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue"
	 And "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" contains an Assign "Valforrandno" as
	  | variable    | value   |
	  | [[a]]       | 1       |	  
	   And "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" contains Random "Rand" as
	  | Type    | From     | To       | Result        |
	  | Numbers | [[val1]] | [[val2]] | [[ranresult]] |
	  When "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" is executed
	  Then the workflow execution has "AN" error
	  And the 'Valforrandno' in WorkFlow 'WorkflowWithAssignandRandomTestingUnassignedvariablevalue' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | 1         |
	  And the 'Valforrandno' in Workflow 'WorkflowWithAssignandRandomTestingUnassignedvariablevalue' debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = 1              |
	  And the 'Rand' in WorkFlow 'WorkflowWithAssignandRandomTestingUnassignedvariablevalue' debug inputs as 
	  | Random  | From        | To         |
	  | Numbers | [[val1]]  = | [[val2]] = |
	  And the 'Rand' in Workflow 'WorkflowWithAssignandRandomTestingUnassignedvariablevalue' debug outputs as
	  |                 |
	  | [[ranresult]] = |



Scenario: Workflow with Assign, Date Time Difference tools and testing variable values that hasn't been assigned
	  Given I have a workflow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue"
	  And "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" contains an Assign "InputDates" as
	  | variable | value |
	  | [[val]]  | 2014  |
	  And "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" contains Date and Time Difference "DateAndTime" as	
	  | Input1   | Input2   | Input Format | Output In | Result     |
	  | [[val1]] | [[val2]] | [[val3]]     | Years     | [[result]] |  
	  When "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" is executed
	  Then the workflow execution has "AN" error
	  And the 'InputDates' in WorkFlow 'WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue' debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 2014      |
	  And the 'InputDates' in Workflow 'WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue' debug outputs as  
	  | # |                 |
	  | 1 | [[val]]  = 2014 |
	  And the 'DateAndTime' in WorkFlow 'WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue' debug inputs as
	  | Input 1    | Input 2 | Input Format | Output In |
	  | [[val1]] = | [[val2]] =      | [[val3]]  =    | Years     |
	  And the 'DateAndTime' in Workflow 'WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue' debug outputs as 
	  |              |

#Bug 12050
#This spec is passing but not working in studio
Scenario: Workflow with Assign  Delete and testing variables that hasn't been assigned
	  Given I have a workflow "WorkflowWithAssignDelete12"
	  And "WorkflowWithAssignDelete12" contains an Assign "DelRec" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "WorkflowWithAssignDelete12" contains Delete "Delet12" as
	  | Variable   | result      |
	  | [[Del(1)]] | [[result1]] |
	  When "WorkflowWithAssignDelete12" is executed
      Then the workflow execution has "AN" error
	  And the 'DelRec' in WorkFlow 'WorkflowWithAssignDelete12' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the 'DelRec' in Workflow 'WorkflowWithAssignDelete12' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the 'Delet12' in WorkFlow 'WorkflowWithAssignDelete12' debug inputs as
	  | Records      |
	  | [[Del(1)]] = |
	  And the 'Delet12' in Workflow 'WorkflowWithAssignDelete12' debug outputs as  
	  |               |
	  | [[result1]] = |


#This spec is passing but not working as expected in studio
Scenario: Workflow with Assign Sort and testing variables that hasn't been assigned
      Given I have a workflow "workflowithAssignandsortrec12"
      And "workflowithAssignandsortrec12" contains an Assign "sortval" as
	  | variable    | value |
	  | [[rs(1).a]] | 10    |
	  | [[rs(5).a]] | 20    |
	  | [[rs(7).a]] | 30    |
	  | [[rs(2).b]] | 6     |
	  | [[rs(4).b]] | 4     |
	  | [[rs(6).b]] | 2     |
	  And "workflowithAssignandsortrec12" contains an Sort "sortRec" as
	  | Sort Field  | Sort Order |
	  | [[xs(*).a]] | Backwards  |
	  When "workflowithAssignandsortrec12" is executed
	  Then the workflow execution has "AN" error
	  And the 'sortval' in WorkFlow 'workflowithAssignandsortrec12' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 10        |
	  | 2 | [[rs(5).a]] = | 20        |
	  | 3 | [[rs(7).a]] = | 30        |
	  | 4 | [[rs(2).b]] = | 6         |
	  | 5 | [[rs(4).b]] = | 4         |
	  | 6 | [[rs(6).b]] = | 2         |
	  And the 'sortval' in Workflow 'workflowithAssignandsortrec12' debug outputs as    
	  | # |                  |
	  | 1 | [[rs(1).a]] = 10 |
	  | 2 | [[rs(5).a]] = 20 |
	  | 3 | [[rs(7).a]] = 30 |
	  | 4 | [[rs(2).b]] = 6  |
	  | 5 | [[rs(4).b]] = 4  |
	  | 6 | [[rs(6).b]] = 2  |
	  And the 'sortRec' in WorkFlow 'workflowithAssignandsortrec12' debug inputs as
	  | Sort Field   | Sort Order |
	  | [[xs(*).a]] = | Backwards  |
	  And the 'sortRec' in Workflow 'workflowithAssignandsortrec12' debug outputs as
	  |                  |
	 

Scenario: Workflow with Assign Unique Tool and testing variables in Returnfield hasn't been assigned
      Given I have a workflow "workflowithAssignUni"
      And "workflowithAssignUni" contains an Assign "Records1" as
	  | variable       | value |
	  | [[rs(1).row]]  | 10    |
	  | [[rs(1).data]] | 10    |
	  And "workflowithAssignUni" contains an Unique "Unrec" as
	  | In Field(s)   | Return Fields | Result           |
	  | [[rs(1).row]] | [[new().row]] | [[rec().unique]] |
	  When "workflowithAssignUni" is executed
	  Then the workflow execution has "AN" error
	  And the 'Records1' in WorkFlow 'workflowithAssignUni' debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[rs(1).row]] =  | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  And the 'Records1' in Workflow 'workflowithAssignUni' debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  And the 'Unrec' in WorkFlow 'workflowithAssignUni' debug inputs as
      | #           |                    | Return Fields   |
      | In Field(s) | [[rs(1).row]] = 10 |                 |
      |             |                    | [[new().row]] = |
      And the 'Unrec' in Workflow 'workflowithAssignUni' debug outputs as  
       |                     |
       | [[rec(1).unique]] = |
      

Scenario: Executing Utility - Format Number example workflow
	  Given I have a workflow "Utility - Format Number Test"
	  And "Utility - Format Number Test" contains "Utility - Format Number" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Format Number Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Format Number' in WorkFlow 'Utility - Format Number' debug inputs as
	  | Number  | Rounding | Rounding Value | Decimals to show |
	  | 123.446 | Normal   | 2              | 2                |
	  And the 'Format Number' in Workflow 'Utility - Format Number' debug outputs as    
	  |                    |
	  | [[Price]] = 123.45 |
	   And the 'Format Number' in WorkFlow 'Utility - Format Number' debug inputs as
	  | Number | Rounding | Rounding Value | Decimals to show |
	  | 14649  | Up       | 2              | -3               |
	  And the 'Format Number' in Workflow 'Utility - Format Number' debug outputs as    
	  |                   |
	  | [[PriceInK]] = 14 |
	 
Scenario: Executing Utility - Random example workflow
	  Given I have a workflow "Utility - Random Test"
	  And "Utility - Random Test" contains "Utility - Random" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Random Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Format Number' in WorkFlow 'Utility - Random' debug inputs as
	  | Random  | From | To |
	  | Numbers | 1    | 6  |
	  And the 'Format Number' in Workflow 'Utility - Random' debug outputs as    
	  |                      |
	  | [[DiceRoll]] = Int32 |
	    And the 'Format Number' in WorkFlow 'Utility - Random' debug inputs as
	  | Random  | Length |
	  | Letters | 7      |
	  And the 'Format Number' in Workflow 'Utility - Random' debug outputs as    
	  |                      |
	  | [[Scrabble]] = Int32 |
	     And the 'Format Number' in WorkFlow 'Utility - Random' debug inputs as
	  | Random |
	  | GUID   | 
	  And the 'Format Number' in Workflow 'Utility - Random' debug outputs as    
	  |                     |
	  | [[License]] = Int32 |


Scenario: Executing Utility - Date and Time example workflow
	  Given I have a workflow "Utility - Date and Time Test"
	  And "Utility - Date and Time Test" contains "Utility - Date and Time" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Date and Time Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Date and Time(1)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input            | =        | Input Format            | =                      | Add Time |    | Output Format           | =                      |
	  | System Date Time | DateTime | System Date Time Format | MM/dd/yyyy hh:mm:ss tt | ""       | "" | System Date Time Format | MM/dd/yyyy hh:mm:ss tt |
	  And the 'Date and Time(1)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                       |
	  | [[nowish]] = DateTime |   
	 And the 'Date and Time(2)' in WorkFlow 'Utility - Date and Time' debug inputs as
	 | Input                 | Input Format            | =                      | Add Time |    | Output Format          |
	 | [[nowish]] = DateTime | System Date Time Format | MM/dd/yyyy hh:mm:ss tt | ""       | "" | mm/dd/yy 12h:min am/pm |
	  And the 'Date and Time(2)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                       |
	  | [[nowish]] = DateTime |  
	  And the 'Date and Time(3)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input              | Input Format | Add Time |       | Output Format            |
	  | Sunday, 23 July 78 | DW, dd MM yy | Minutes  | 46664 | mm/dd/yyyy 12h:min am/pm |	
	  And the 'Date and Time(3)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                             |
	  | [[SomeTimeBack]] = DateTime |  
	 And the 'Date and Time(4)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input | Input Format | Add Time |    | Output Format                  |
	  | am    | am/pm        | ""       | "" | mm/dd/yyyy 12h:min:ss.sp am/pm |
	  And the 'Date and Time(4)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                               |
	  | [[TheDefaultDate]] = DateTime |  
	  And the 'Date and Time(5)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input            | =        | Input Format            | =                      | Add Time |    | Output Format                                 |
	  | System Date Time | DateTime | System Date Time Format | MM/dd/yyyy hh:mm:ss tt | ""       | "" | 'Date format yyyy MM dd yields : ' yyyy MM dd |

Scenario: Executing Utility - Gather System Information example workflow
	  Given I have a workflow "Utility - System Information Test"
	  And "Utility - System Information Test" contains "Utility - System Information" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - System Information Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Gather System Information (17)' in WorkFlow 'Utility - System Information' debug inputs as
	 | #  |                     |                     |
	 | 1  | [[DateTime]] =      | Date & Time         |
	 | 2  | [[OpSystem]]  =     | Operating System    |
	 | 3  | [[SP]] =            | Service Pack        |
	 | 4  | [[Bit]] =           | 32/64 Bit           |
	 | 5  | [[DatTimeFormat]] = | Date & Time Format  |
	 | 6  | [[DiskAvailable]] = | Disk Available (GB) |
	 | 7  | [[DiskTotal]]  =    | Disk Total (GB)     |
	 | 8  | [[RAMAvailable]] =  | RAM Available (MB)  |
	 | 9  | [[RAMTotal]]  =     | RAM Total (MB)      |
	 | 10 | [[CPUAvailable]] =  | CPU Available       |
	 | 11 | [[CPUTotal]]  =     | CPU Total           |
	 | 12 | [[Language]] =      | Language            |
	 | 13 | [[Region]] =        | Region              |
	 | 14 | [[UserRoles]] =     | User Roles          |
	 | 15 | [[UserName]] =      | User Name           |
	 | 16 | [[Domain]] =        | Domain              |
	 | 17 | [[Agents]] =        | Warewolf Agents     |
	 And the 'Gather System Information (17)' in Workflow 'Utility - System Information' debug outputs as    
	   | #  |                               |
	   | 1  | [[DateTime]]      =    String |
	   | 2  | [[OpSystem]]      =    String |
	   | 3  | [[SP]]            =    String |
	   | 4  | [[Bit]]           =    String |
	   | 5  | [[DatTimeFormat]] =    String |
	   | 6  | [[DiskAvailable]] =    String |
	   | 7  | [[DiskTotal]]     =    String |
	   | 8  | [[RAMAvailable]]  =    String |
	   | 9  | [[RAMTotal]]      =    String |
	   | 10 | [[CPUAvailable]]  =    String |
	   | 11 | [[CPUTotal]]      =    String |
	   | 12 | [[Language]]      =    String |
	   | 13 | [[Region]]        =    String |
	   | 14 | [[UserRoles]]     =    String |
	   | 15 | [[UserName]]      =    String |
	   | 16 | [[Domain]]        =    String |
	   | 17 | [[Agents]]        =    String |


Scenario: Executing Utility - Web Request example workflow
	  Given I have a workflow "Utility - Web Request Test"
	  And "Utility - Web Request Test" contains "Utility - Web Request" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Web Request Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Web Request(1)' in WorkFlow 'Utility - Web Request' debug inputs as
	  | URL                     | Header |
	  | https://www.google.com/ |        |
	  And the 'Web Request(1)' in Workflow 'Utility - Web Request' debug outputs as    
	  |                         |
	  | [[GoogleHome]] = String |
	  And the 'Web Request(2)' in WorkFlow 'Utility - Web Request' debug inputs as
	  | URL                                                                                            | Header |
	  | http://maps.googleapis.com/maps/api/geocode/xml?address=[[BartsAddress]]&sensor=false = String |        |
	  And the 'Web Request(2)' in Workflow 'Utility - Web Request' debug outputs as    
	  |                             |
	  | [[GecodedAddress]] = String |

Scenario: Executing Utility - Assign example workflow
	  Given I have a workflow "Utility - Assign Test"
	  And "Utility - Assign Test" contains "Utility - Assign" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable        |
	  |                  |               | [[rec(*).set]]      | [[rec().set]]      |
	  |                  |               | [[hero(*).pushups]] | [[hero().pushups]] |
	  |                  |               | [[hero(*).name]]    | [[hero().name]]    |
	  When "Utility - Assign Test" is executed
	  Then the workflow execution has "NO" error
	 # And the 'Assign (3)' in WorkFlow 'Utility - Assign Test' debug inputs as
	  And the 'Utility - Assign' in Workflow 'Utility - Assign Test' debug outputs as    
	  |                                                                   |
	  | [[rec(1).set]] =    Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	  | [[hero(1).pushups]] = All of them.                                 |
	  | [[hero(1).name]] =                                                |












	#| # | Variable      | New Value                       |
	#| 1 | [[Name]] =    | Bart                            |
	#| 2 | [[Surname]] = | Simpson                         |
	#| 2 | [[Info]] =    | I WILL NOT INSTIGATE REVOLUTION |
	#And the 'Assign (3)' in Workflow 'Utility - Assign' debug outputs as    
	#| # |                                               |
	#| 1 | [[Name]] =    Bart                            |
	#| 2 | [[Surname]] = Simpson                         |
	#| 3 | [[Info]] =    I WILL NOT INSTIGATE REVOLUTION |
	#And the 'Assign (2)' in WorkFlow 'Utility - Assign' debug inputs as
	#| # | Variable         | New Value                                                                      |
	#| 1 | [[rec(1).set]] = | [[Name]] [[Surname]]: [[Info]] = Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	#And the 'Assign (2)' in Workflow 'Utility - Assign' debug outputs as    
	#| # |                                                                |
	#| 1 | [[rec(1).set]] = Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	# And the 'Assign Sum (1)' in WorkFlow 'Utility - Assign' debug inputs as
	#| # | Variable  | New Value |  
	#| 1 | [[sum]] = | =23+19    |
	#And the 'Assign Sum (1)' in Workflow 'Utility - Assign' debug outputs as    
	#| # |              |
	#| 1 | [[sum]] = 42 |
	# And the 'Assign Records (2)' in WorkFlow 'Utility - Assign' debug inputs as
	#| # | Variable             | New Value    |
	#| 1 | [[hero().name]] =    | Chuck Norris |
	#| 2 | [[hero().pushups]] = | All of them. |
	#And the 'Assign Records (2)' in Workflow 'Utility - Assign' debug outputs as    
	#| # |                                    |
	#| 1 | [[hero().name]] = Chuck Norris     | 
	#| 2 | [[hero().pushups]] =  All of them. |


Scenario: Executing Data - Base Conversion example workflow
	  Given I have a workflow "Data - Base Conversion Test"
	  And "Data - Base Conversion Test" contains "Data - Base Conversion" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Data - Base Conversion Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Base Conversion (1)' in WorkFlow 'Data - Base Conversion' debug inputs as
	  | # | Convert                                                                                                             | From   | To   |
	  | 1 | [[Blob]] = 01001001001000000111011101100001011100110010000001101101011000010110111001100111011011000110010101100100 | Binary | Text |
	  And the 'Base Conversion (1)' in Workflow 'Data - Base Conversion' debug outputs as    
	   | # |                          |
	   | 1 | [[Blob]] = I was mangled |
	 



	 






