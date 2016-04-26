﻿@WorkflowExecution
Feature: WorkflowExecution
	In order to execute a workflow on the server
	As a Warewolf user
	I want to be able to build workflows and execute them against the server

Background: Setup for workflow execution
			Given Debug events are reset
			And All environments disconnected
			And Debug states are cleared

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
	 And "TestWFWithDBService" contains a "sqlserver database" service "Fetch" with mappings
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
	  | variable      | value |
	  | [[extension]] | json  |
	  | [[prefix]]    | a     |
	 And "TestWebServiceWF" contains a "webservice" service "InternalCountriesServiceTest" with mappings
	  | Input to Service | From Variable | Output from Service      | To Variable                 |
	  | extension        | [[extension]] | Countries(*).CountryID   | [[Countries().CountryID]]   |
	  | prefix           | [[prefix]]    | Countries(*).Description | [[Countries().Description]] |
	  When "TestWebServiceWF" is executed
	  Then the workflow execution has "NO" error
	   And the 'Inputs' in WorkFlow 'TestWebServiceWF' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[extension]] = | json      |
	  | 2 | [[prefix]] =    | a         |
	  And the 'Inputs' in Workflow 'TestWebServiceWF' debug outputs as    
	  | # |                      |
	  | 1 | [[extension]] = json |
	  | 2 | [[prefix]] = a       |
	   And the 'InternalCountriesServiceTest' in WorkFlow 'TestWebServiceWF' debug inputs as
	  | #            |                                                  |
	  | URL          | "" = http://rsaklfsvrtfsbld/IntegrationTestSite/ |
	  | Query String | "" = GetCountries.ashx?extension=json&prefix=a   |
	  | Headers      |                                                  |
	  And the 'InternalCountriesServiceTest' in Workflow 'TestWebServiceWF' debug outputs as
	  |                                            |
	  | [[Countries(10).CountryID]] = 10           |
	  | [[Countries(10).Description]] = Azerbaijan |

Scenario: Workflow with an assign and remote workflow
	Given I have a workflow "TestAssignWithRemoteNoError1"
	 And "TestAssignWithRemoteNoError1" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	And "TestAssignWithRemoteNoError1" contains "WorkflowUsedBySpecs" from server "Remote Connection Integration" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable      |
	| inputData            | [[inputData]] | output              | [[output]]       |
	|                  |               | values(*).up     | [[values().up]]  |
	|                  |               | values(*).low     | [[values().low]] |
	  When "TestAssignWithRemoteNoError1" is executed
	  Then the workflow execution has "NO" error
	   And the 'AssignData' in WorkFlow 'TestAssignWithRemoteNoError1' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the 'AssignData' in Workflow 'TestAssignWithRemoteNoError1' debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the 'WorkflowUsedBySpecs' in WorkFlow 'TestAssignWithRemoteNoError1' debug inputs as
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
	  | 2 | [[values(1).up]] = HELLO |
	  | 3 | [[values(1).low]] = hello |	  	 
	  And the 'WorkflowUsedBySpecs' in Workflow 'TestAssignWithRemoteNoError1' debug outputs as
	  |                           |
	  | [[values(1).up]] = HELLO  |
	  | [[values(1).low]] = hello |
	  | [[output]] = HELLO        |
	  
	  And the 'WorkflowUsedBySpecs' in Workflow 'TestAssignWithRemoteNoError1' has a debug Server Name of ""Remote Connection Integration""

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


Scenario: Workflow with Assign and 2 Delete tools executing against the server
	  Given I have a workflow "WorkflowWithAssignand2Deletetools"
	  And "WorkflowWithAssignand2Deletetools" contains an Assign "Assign to delete" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "WorkflowWithAssignand2Deletetools" contains Delete "Delet1" as
	  | Variable   | result      |
	  | [[rec(1)]] | [[result1]] |
      And "WorkflowWithAssignand2Deletetools" contains Delete "Delet2" as
	   | Variable   | result      |
	   | [[rec(1)]] | [[result2]] |
	  When "WorkflowWithAssignand2Deletetools" is executed
      Then the workflow execution has "NO" error
	  And the 'Assign to delete' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the 'Assign to delete' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the 'Delet1' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
	  | Records          |
	  | [[rec(1).a]] = 50 |
	  And the 'Delet1' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
	  |                       |
	  | [[result1]] = Success |
	  And the 'Delet2' in WorkFlow 'WorkflowWithAssignand2Deletetools' debug inputs as
	   | Records      |
	   | [[rec(1)]] = |	  
	  And the 'Delet2' in Workflow 'WorkflowWithAssignand2Deletetools' debug outputs as  
	  |                       |
	  | [[result2]] = Failure |

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
	  | 2 | [[b]] = 10.0 |
	  And the 'DateAndTime' in WorkFlow 'WorkflowWithAssignAndDateTimeDifferencetools1' debug inputs as
	  | Input 1       | Input 2    | Input Format | Output In |
	  | 2020/[[b]]/01 = 2020/10.0/01 | 2030/01/01 | yyyy/mm/dd   | Years     |
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
	  | String                  | Variable    | Type  | At | Include    | Escape |
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
	  | String to Split                                | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[result]][[split().a]] = TestWarewolfWorkflow | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  |                                                |                   |                 | 2 | [[rec().b]] = | Index | 8     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithAssignDataMergeAndDataSplittools' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).b]] = Test     |
	  |   | [[rec(2).b]] = Warewolf |
	  |   | [[rec(3).b]] = Work     |
	  |   | [[rec(4).b]] = flow     |

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
	  | String to Split     | Process Direction | Skip blank rows | # |                | With  | Using     | Include | Escape |
	  | [[test]] = warewolf | Forward           | No              | 1 | [[rec(1).a]] = | Index | [[b]] = 2 | No      |        |
	  And the 'DataSpliting' in Workflow 'WorkflowWithAssignandDataSplittools' debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = lf |

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
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the 'Replacing' in Workflow 'Test1' debug outputs as
	  |                                |
	  | [[rec(1).a]] = tREPLACEDst     |
	  | [[rec(2).a]] = warREPLACEDwolf |
	  | [[rec(1).b]] = tREPLACEDst     |
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
	  | File or Folder            | Overwrite | Username | Password |
	  | [[rec(1).a]] = create.txt | True      | " "      | " "      |
	  And the 'Create1' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as    
	   |                    |
	   | [[res1]] = Failure  |
	  And the 'Delete' in WorkFlow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug inputs as
	  | Input Path                | Username   | Password   |
	  | [[rec(1).a]] = create.txt | " " | " " |
	  And the 'Delete' in Workflow 'WorkflowWithAssignCreateDeleteRecordNoneExist1' debug outputs as    
	  |                    |
	  | [[res1]] = Failure |

Scenario: Workflow with 2 Assign tools executing against the server
	  Given I have a workflow "WorkflowWith2Assigntools"
	  And "WorkflowWith2Assigntools" contains an Assign "tool1" as
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
	  | 1 | [[[[a]]]] = test | [[test]] = warewolf |

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
	  | # | Variable            | New Value |
	  | 1 | [[rec(2).a]] = test | warewolf  |
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
	  | 1 | [[b]] = test | warewolf  |
	  And the 'scl2' in Workflow 'WorkflowWith2Assigntoolswithrscalars' debug outputs as  
	  | # |                   |
	  | 1 | [[b]] =  warewolf |

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
	  | 1 | [[b]] = 12 | Text | Base 64 |
      And the 'Base' in Workflow 'WorkflowWithAssignandBase' debug outputs as  
	  | # |              |
	  | 1 | [[b]] = MTI= |

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
	  | 1 | [[rec(1).a]] = 12 | Text | Base 64 |
      And the 'Base' in Workflow 'WorkflowWithAssignandBasec' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = MTI= |
	  
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
	  | 1 | [[b]] = warewolf | UPPER |
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
	  | 1 | [[rec(1).a]] = warewolf | UPPER |
	  And the 'CaseConvert' in Workflow 'WorkflowWithAssignandcCase' debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = WAREWOLF |

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
	  | # |                     | With  | Using | Pad | Align |
	  | 1 | [[b]] = warewolf    | Index | "8"   | ""  | Left  |
	  | 2 | [[rec(1).a]] = test | Index | "4"   | ""  | Left  |
	  And the 'Datamerge' in Workflow 'WorkflowWithAssignandData' debug outputs as  
	  |                           |
	  | [[result]] = warewolftest |

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
	  | [[b]] = test | First Occurence | s          | Left to Right |
	  And the 'Indexchar' in Workflow 'WorkflowWithAssignandFindIndex' debug outputs as 
	  |                     |
	  | [[indexResult]] = 3 |

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
	  | [[new(1).a]] = test | First Occurence | s          | Left to Right |
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
	 | [[b]] = test | s    | REPLACE      |
	    And the 'Replac' in Workflow 'WorkflowWithAssignandReplace' debug outputs as 
	  |                       |
	  | [[b]] = teREPLACEt    |
	  | [[replaceResult]] = 1 |

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
	  | [[new(1).a]] = test | s    | REPLACE      |
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
	  | [[b]] = 12.3412 | Up       | 3              | 3                |
	  And the 'Fnumber' in Workflow 'WorkflowWithAssignandFormat' debug outputs as 
	  |                      |
	  | [[fresult]] = 12.342 |

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
	  | [[new(1).a]] = 12.3412 | Up       | 3              | 3                |
	  And the 'Fnumb' in Workflow 'WorkflowWithAssignandFormatn' debug outputs as 
	  |                      |
	  | [[fresult]] = 12.342 |

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
	  | Numbers | [[b]] = 10 | [[new(1).a]] = 20 |
	  And the 'Rand' in Workflow 'WorkflowWithAssignandRandom' debug outputs as
	  |                       |
	  | [[ranresult]] = Int32 |

Scenario: Simple workflow with Assign and Date and Time(Evaluating recordset variable inside variable)executing against the server
	 Given I have a workflow "WorkflowWithAssignandDateTimetool"
	 And "WorkflowWithAssignandDateTimetool" contains an Assign "Dateandtime" as
	  | variable    | value      |
	  | [[a]]       | b          |
	  | [[b]]       | 01/02/2014 |
	  | [[rec().a]] | new().a    |
	  | [[new().a]] | dd/mm/yyyy |	 	  
	  And "WorkflowWithAssignandDateTimetool" contains Date and Time "AddDate" as
      | Input     | Input Format     | Add Time | Output Format | Result  |
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
	   | [[b]] = 01/02/2014 | [[new(1).a]] = dd/mm/yyyy | Years    | 1 | dd/mm/yyyy    |	
	   And the 'AddDate' in Workflow 'WorkflowWithAssignandDateTimetool' debug outputs as   
	   |                      |
	   | [[res]] = 01/02/2015 |
 
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
	   | Input 1                   | Input 2            | Input Format | Output In |
	   | [[new(1).a]] = 01/02/2014 | [[b]] = 01/02/2016 | dd/mm/yyyy   | Years     |
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
	 | [[b]] = test | s    | REPLACE      |
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
	  | [[b]] = 12.3412 | Up       | 3              | 3                |
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



Scenario: Simple workflow with Assign DataMerge and DataSplit(Evaluating index recordset variable)executing against the server
 Given I have a workflow "WorkflowWithAssignMergeandSplit"
 And "WorkflowWithAssignMergeandSplit" contains an Assign "Data" as
  | variable       | value    |
  | [[a]]          | 1        |
  | [[b]]          | 2        |
  | [[rec(1).a]]   | warewolf |
  | [[rec(2).a]]   | test     |
  | [[index(1).a]] | 1        |
  | [[index(2).a]] | 3        |	
  And "WorkflowWithAssignMergeandSplit" contains Data Merge "Merge" into "[[result]]" as	
	 | Variable                  | Type  | Using | Padding | Alignment |
	 | [[rec([[index(1).a]]).a]] | Index | 8     |         | Left      |
	 | [[a]]                     | Index | 4     |         | Left      |
	 And "WorkflowWithAssignMergeandSplit" contains Data Split "DataSplit" as
	 | String       | Variable                  | Type  | At | Include    | Escape |
	 | [[rec(1).a]] | [[d]]                     | Index | 4  | Unselected |        |
	 |              | [[rec([[index(2).a]]).a]] | Index | 4  | Unselected |        |
	 When "WorkflowWithAssignMergeandSplit" is executed
	 Then the workflow execution has "NO" error
	 And the 'Data' in WorkFlow 'WorkflowWithAssignMergeandSplit' debug inputs as
	 | # | Variable         | New Value |
	 | 1 | [[a]] =          | 1         |
	 | 2 | [[b]] =          | 2         |
	 | 3 | [[rec(1).a]] =   | warewolf  |
	 | 4 | [[rec(2).a]] =   | test      |
	 | 5 | [[index(1).a]] = | 1         |
	 | 6 | [[index(2).a]] = | 3         |
	 And the 'Data' in Workflow 'WorkflowWithAssignMergeandSplit' debug outputs as 
	 | # |                         |
	 | 1 | [[a]] = 1               |
	 | 2 | [[b]] = 2               |
	 | 3 | [[rec(1).a]] = warewolf |
	 | 4 | [[rec(2).a]] = test     |
	 | 5 | [[index(1).a]] = 1      |
	 | 6 | [[index(2).a]] = 3      |  	
    And the 'Merge' in WorkFlow 'WorkflowWithAssignMergeandSplit' debug inputs as
	 | # |                                      | With  | Using | Pad | Align |
	 | 1 | [[rec(1).a]] = warewolf | Index | "8"   | ""  | Left  |
	 | 2 | [[a]] = 1                            | Index | "4"   | ""  | Left  |
	 And the 'Merge' in Workflow 'WorkflowWithAssignMergeandSplit' debug outputs as
	 |                        |
	 | [[result]] = warewolf1 |
	 And the 'DataSplit' in WorkFlow 'WorkflowWithAssignMergeandSplit' debug inputs as  
	 | String to Split         | Process Direction | Skip blank rows | # |                | With  | Using | Include | Escape |
	 | [[rec(1).a]] = warewolf | Forward           | No              | 1 | [[d]] =        | Index | 4     | No      |        |
	 |                         |                   |                 | 2 | [[rec(3).a]] = | Index | 4     | No      |        |
	  And the 'DataSplit' in Workflow 'WorkflowWithAssignMergeandSplit' debug outputs as
	  | # |                     |
	  | 1 | [[d]] = ware        |
	  | 2 | [[rec(3).a]] = wolf |

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
	  | # | Variable       | New Value                       |
	  | 1 | [[a]] =        | 1                               |
	  | 2 | [[b]] =        | 2                               |
	  | 3 | [[rec(1).a]] = | [[a]] = 1                       |
	  | 4 | [[rec(1).b]] = | [[b]] = 2                       |
	  | 5 | [[rec(1).c]] = | [[rec(1).a]]+[[rec(1).b]] = 1+2 |
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
      | [[rec(1).b]]+1 = 1+1 |       
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

Scenario: Workflow with Assign and ForEach
     Given I have a workflow "WFWithAssignForEach"
	 And "WFWithAssignForEach" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[Warewolf]] | bob   |
     And "WFWithAssignForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "3"
	 And "ForEachTest" contains workflow "11714Nested" with mapping as
	 | Input to Service | From Variable | Output from Service | To Variable |
	 | a                | [[Warewolf]]  |                     |             |
	 When "WFWithAssignForEach" is executed
	 Then the workflow execution has "NO" error
	 And the 'ForEachTest' in WorkFlow 'WFWithAssignForEach' debug inputs as 
	    |                 | Number |
	    | No. of Executes | 3      |
	 And the 'ForEachTest' in WorkFlow 'WFWithAssignForEach' has  "3" nested children 
	 And each "11714Nested" contains debug outputs for "Assign (1)" as
      | variable | value    |
      | [[a]]    | warewolf |  
        
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
	  |                | Recordset      |
	  | * in Recordset |                |
	  |                | [[rec(1)]] = |
	  |                | [[rec(2)]] = |
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
	  And the server CPU usage is less than 15%
	  And the server memory difference is less than 150 mb
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
	  | [[rec(2).a]] = b        |
	  | [[rec(1).a]] = Warewolf |
	  | [[rec(2).a]] = Warewolf |
	  | [[rec(3).a]] = 3        |

Scenario: Workflow Assign and Find Record index tool with two variables in reult field expect error
      Given I have a workflow "WFWithAssignandFindRecordindexy"
	  And "WFWithAssignandFindRecordindexy" contains an Assign "Record" as
      | # | variable     | value    |
      | # | [[rec(1).a]] | Warewolf |
	  And "WFWithAssignandFindRecordindexy" contains Find Record Index "FindRecord0" into result as "[[a]][[b]]"
      | # | In Field    | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexy" is executed
	  Then the workflow execution has "AN" error
	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindexy' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | Warewolf  | 
	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindexy' debug outputs as   
	  | # |                                  |
	  | 1 | [[rec(1).a]]         =  Warewolf |


Scenario: Workflow Assign and Find Record index
      Given I have a workflow "WFWithAssignandFindRecordindexTool"
	  And "WFWithAssignandFindRecordindexTool" contains an Assign "Record" as
      | # | variable     | value    |
      | # | [[rec(1).a]] | Warewolf |
	  And "WFWithAssignandFindRecordindexTool" contains Find Record Index "FindRecord0" into result as "[[a]]*]]"
      | # | In Field    | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexTool" is executed
	  Then the workflow execution has "AN" error
	  And the 'Record' in WorkFlow 'WFWithAssignandFindRecordindexTool' debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | Warewolf  | 
	  And the 'Record' in Workflow 'WFWithAssignandFindRecordindexTool' debug outputs as   
	  | # |                                  |
	  | 1 | [[rec(1).a]]         =  Warewolf |	 	 
		 
Scenario Outline: Testing Length with two variables in Result field
      Given I have a workflow "WorkflowforLength"
      And "WorkflowforLength" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | 1213  |
	  | [[rec().a]] | 4561  |
	  And "WorkflowforLength" contains Length "Len" on "[[rec(*)]]" into '<Variable>'
	  When "WorkflowforLength" is executed
	  Then the workflow execution has "AN" error	
      And the 'Rec To Convert' in WorkFlow 'WorkflowforLength' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1213      |
	  | 2 | [[rec().a]] = | 4561      |
	  And the 'Rec To Convert' in Workflow 'WorkflowforLength' debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = 1213 |
	  | 2 | [[rec(2).a]] = 4561 |
	  And the 'Len' in WorkFlow 'WorkflowforLength' debug inputs as
	  | Recordset           |
	  | [[rec(1).a]] = 1213 |
	  | [[rec(2).a]] = 4561 |
	  And the 'Len' in Workflow 'WorkflowforLength' debug outputs as    
	  |                |
	  |                |
Examples: 
      | No    | Variable       |
      | 1     | [[length]][[a]] |
      | 2  | [[a]]*]]               |
      | 3  | [[var@]]               |
      | 4  | [[var]]00]]            |
      | 5  | [[(1var)]]             |
      | 6  | [[var[[a]]]]           |
      | 7  | [[var.a]]              |
      | 8  | [[@var]]               |
      | 9  | [[var 1]]              |
      | 10 | [[rec(1).[[rec().1]]]] |
      | 11 | [[rec(@).a]]           |
      | 12 | [[rec"()".a]]          |
      | 13 | [[rec([[[[b]]]]).a]]   |


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

Scenario Outline: Testing Random Numbers with two variables in Result
      Given I have a workflow "Workflowforrandom123"
	  And "Workflowforrandom123" contains an Assign "Values" as
	  | variable | value |
	  | [[a]]    | 1     |
	  | [[b]]    | 10     |
	  And "Workflowforrandom123" contains Random "Randoms" as
	  | Type    | From | To | Result       |
	  | Numbers | 1    | 10 | '<Variable>' |
	  When "Workflowforrandom123" is executed  	  
	  Then the workflow execution has "AN" error
	   And the 'Values' in WorkFlow 'Workflowforrandom123' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | 1         |
	  | 2 | [[b]] =  | 10        |
	  And the 'Values' in Workflow 'Workflowforrandom123' debug outputs as    
	  | # |             |
	  | 1 | [[a]] =  1  |
	  | 2 | [[b]] =  10 |
	  And the 'Randoms' in WorkFlow 'Workflowforrandom123' debug inputs as 
	    | Random  | From | To |
	    | Numbers | 1    | 10  |
      And the 'Randoms' in Workflow 'Workflowforrandom123' debug outputs as
	  |                |
	  | '<Variable>' = |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |

Scenario Outline: Testing Date and Time with two variables in Result field
      Given I have a workflow "WorkflowforDT"
      And "WorkflowforDT" contains an Assign "Convert2" as
	  | variable    | value      |
	  | [[rec().a]] | 12/01/2001 |
	  And "WorkflowforDT" contains Date and Time "AddDates" as
      | Input       | Input Format | Add Time | Output Format | Result       |
      | [[rec().a]] | dd/mm/yyyy   | 1        | dd/mm/yyyy    | '<Variable>' |	
	  When "WorkflowforDT" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Convert2' in WorkFlow 'WorkflowforDT' debug inputs as
	  | # | Variable      | New Value  |
	  | 1 | [[rec().a]] = | 12/01/2001 |
	  And the 'Convert2' in Workflow 'WorkflowforDT' debug outputs as    
	  | # |                           |
	  | 1 | [[rec(1).a]] = 12/01/2001 |
	  And the 'AddDates' in WorkFlow 'WorkflowforDT' debug inputs as
	   | Input                     | Input Format | Add Time |   | Output Format |
	   | [[rec(1).a]] = 12/01/2001 | dd/mm/yyyy   | Years    | 1 | dd/mm/yyyy    |	
	  And the 'AddDates' in Workflow 'WorkflowforDT' debug outputs as   
	   |                |
	   | '<Variable>' = |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |      


Scenario: Workflow with Assign and Unique Tool, finding unique data from multiple rows 
      Given I have a workflow "workflowithAssignandUnique"
      And "workflowithAssignandUnique" contains an Assign "Records" as
	  | variable      | value |
	  | [[rs().row]]  | 10    |
	  | [[rs().data]] | 10    |
	  | [[rs().row]]  | 40    |
	  | [[rs().data]] | 20    |
	  | [[rs().row]]  | 20    |
	  | [[rs().data]] | 20    |
	  | [[rs().row]]  | 30    |
	  | [[rs().data]] | 40    |
	  And "workflowithAssignandUnique" contains an Unique "Unique rec" as
	  | In Field(s)                  | Return Fields | Result           |
	  | [[rs(*).row]],[[rs(*).data]] | [[rs().row]]  | [[rec().unique]] |
	  When "workflowithAssignandUnique" is executed
	  Then the workflow execution has "NO" error
	  And the 'Records' in WorkFlow 'workflowithAssignandUnique' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[rs().row]] =  | 10        |
	  | 2 | [[rs().data]] = | 10        |
	  | 3 | [[rs().row]] =  | 40        |
	  | 4 | [[rs().data]] = | 20        |
	  | 5 | [[rs().row]] =  | 20        |
	  | 6 | [[rs().data]] = | 20        |
	  | 7 | [[rs().row]] =  | 30        |
	  | 8 | [[rs().data]] = | 40        |
	  And the 'Records' in Workflow 'workflowithAssignandUnique' debug outputs as  
	  | # |                     |
	  | 1 | [[rs(1).row]] =  10 |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the 'Unique rec' in WorkFlow 'workflowithAssignandUnique' debug inputs as
       | #           |                     | Return Fields  |
       | In Field(s) | [[rs(1).row]] = 10  |                |
       |             | [[rs(2).row]] = 40  |                |
       |             | [[rs(3).row]] = 20  |                |
       |             | [[rs(4).row]] = 30  |                |
       |             | [[rs(1).data]] = 10 |                |
       |             | [[rs(2).data]] = 20 |                |
       |             | [[rs(3).data]] = 20 |                |
       |             | [[rs(4).data]] = 40 |                |
       |             |                     | [[rs().row]] = |
      And the 'Unique rec' in Workflow 'workflowithAssignandUnique' debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 10 |
       |   | [[rec(2).unique]] = 40 |
       |   | [[rec(3).unique]] = 20 |
       |   | [[rec(4).unique]] = 30 |

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
	Given I have a workflow "ConvertUsingScalarWithRecursiveEvalution"
	And "ConvertUsingScalarWithRecursiveEvalution" contains an Assign "Records" as
	  | variable     | value    |
	  | [[rs().row]] | warewolf |
	  | [[a]]        | 1        |
	And "ConvertUsingScalarWithRecursiveEvalution" contains case convert "Case to Convert" as
	  | Variable          | Type  |
	  | [[rs([[a]]).row]] | UPPER |
	When "ConvertUsingScalarWithRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the 'Records' in WorkFlow 'ConvertUsingScalarWithRecursiveEvalution' debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rs().row]] = | warewolf  |
	  | 2 | [[a]] =        | 1         |
	And the 'Records' in Workflow 'ConvertUsingScalarWithRecursiveEvalution' debug outputs as  
	  | # |                           |
	  | 1 | [[rs(1).row]] =  warewolf |
	  | 2 | [[a]] =  1                |
	And the 'Case to Convert' in WorkFlow 'ConvertUsingScalarWithRecursiveEvalution' debug inputs as
	  | # | Convert                  | To    |
	  | 1 | [[rs(1).row]] = warewolf | UPPER |
	And the 'Case to Convert' in Workflow 'ConvertUsingScalarWithRecursiveEvalution' debug outputs as
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

Scenario: Workflow with Assigns DataSplit executing against the server
      Given I have a workflow "WorkflowDataSplit"
	  And "WorkflowDataSplit" contains an Assign "Assignval" as
      | variable | value   |
      | [[a]]    | rec().a |
	    And "WorkflowDataSplit" contains Data Split "DataSplit" as
	  | String | Variable  | Type  | At | Include    | Escape |
	  | abcd   | [[[[a]]]] | Index | 4  | Unselected |        | 
	  When "WorkflowDataSplit" is executed
	  Then the workflow execution has "No" error
	  And the 'Assignval' in WorkFlow 'WorkflowDataSplit' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | rec().a   |
	  And the 'Assignval' in Workflow 'WorkflowDataSplit' debug outputs as  
	  | # |                  |
	  | 1 | [[a]] =  rec().a |
	  And the 'DataSplit' in WorkFlow 'WorkflowDataSplit' debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	  | abcd            | Forward           | No              | 1 |  [[rec().a]] = | Index | 4     | No      |        |
	  And the 'DataSplit' in Workflow 'WorkflowDataSplit' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = abcd |

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

Scenario: Workflow with Assign and foreach with invalid rec and it contains calculate in it
      Given I have a workflow "WorkflowDwithforeachcontainscalinvalid"
	  And "WorkflowDwithforeachcontainscalinvalid" contains an Assign "Assigl" as
      | variable   | value |
      | [[rs().a]] | 1     |
      | [[rs().a]] | 2     |
      | [[rs().a]] | 3     |
	  And "WorkflowDwithforeachcontainscalinvalid" contains a Foreach "ForEachTes" as "InRecordset" executions "[[rs()]]"
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


Scenario: Workflow with Assigns DataSplit executing against the server 2
      Given I have a workflow "WorkflowDataSplit"
	  And "WorkflowDataSplit" contains an Assign "Assignval" as
      | variable | value   |
      | [[a]]    | rec().a |
	    And "WorkflowDataSplit" contains Data Split "DataSplit" as
	  | String | Variable  | Type  | At | Include    | Escape |
	  | abcd   | [[[[a]]]] | Index | 4  | Unselected |        | 
	  When "WorkflowDataSplit" is executed
	  Then the workflow execution has "No" error
	  And the 'Assignval' in WorkFlow 'WorkflowDataSplit' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | rec().a   |
	  And the 'Assignval' in Workflow 'WorkflowDataSplit' debug outputs as  
	  | # |                  |
	  | 1 | [[a]] =  rec().a |
	  And the 'DataSplit' in WorkFlow 'WorkflowDataSplit' debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | abcd            | Forward           | No              | 1 | [[rec().a]] = | Index | 4     | No      |        |
	  And the 'DataSplit' in Workflow 'WorkflowDataSplit' debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = abcd |

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
       |   | [[emp(4).uni]]  =          |
 
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
	  And the 'Calculation' in Workflow 'WorkflowWithAssignCalculationUsingStar' debug outputs as  
	  | # |                   |

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
	  | 1 | [[Value]] =   | Index | "4"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug outputs as  
	  |             |
	  | [[result]] = |
	  And the 'Data Split' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[Value12]]  =  | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues' debug outputs as  
	  | # |               |

Scenario: Workflow with Assigns DataMerge and DataSplit and testing variables that hasn't been assigned2
      Given I have a workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2"
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" contains an Assign "Assign To merge" as
      | variable | value |
      | [[res]]  | Test  |
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable      | Type  | Using | Padding | Alignment |
	  | [[Value]]Test | Index | 4     |         | Left      |
	  And "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" contains Data Split "Data Split" as
	  | String          | Variable    | Type  | At | Include    | Escape |
	  | [[Value12]]Test | [[rec().b]] | Index | 4  | Unselected |        |
	  When "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign To merge' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug inputs as 
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | Test      |
	 And the 'Assign To merge' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug outputs as   
	  | # |                 |
	  | 1 | [[res]] =  Test |
	  And the 'Data Merge' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug inputs as 
	  | # |                 | With  | Using | Pad | Align |
	  | 1 | [[Value]]Test = | Index | "4"   | ""  | Left  |
	  And the 'Data Merge' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug outputs as  
	  |              |
	  | [[result]] = |
	  And the 'Data Split' in WorkFlow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug inputs as 
	  | String to Split   | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[Value12]]Test = | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  And the 'Data Split' in Workflow 'WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2' debug outputs as  
	  | # |                |

Scenario: Workflow with Assigns Replace and testing variables that hasn't been assigned
      Given I have a workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues"
       And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains an Assign "Assign34" as
      | variable | value |
      | [[Val]]  | test  |
	  And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains Replace "Replacing" into "[[replac]]" as	
	  | In Fields  | Find     | Replace With |
	  | [[rec()]] | [[Val1]] | [[Val2]]     |
	  When "workflowithAssignandReplaceTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "No" error
	  And the 'Assign34' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | test      |
	   And the 'Assign34' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug outputs as    
	  | # |                |
	  | 1 | [[Val]] = test |
	  And the 'Replacing' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug inputs as 
	  | In Field(s) | Find       | Replace With |
	  | [[rec()]] = | [[Val1]] = | [[Val2]] =   |
#	  And the 'Replacing' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues' debug outputs as
#	  |              |
#	  | [[replac]] = |

Scenario: Workflow with Assigns Replace and testing variables that hasn't been assigned2
      Given I have a workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues2"
      And "workflowithAssignandReplaceTestingUnassignedvariablevalues2" contains an Assign "Assign34" as
      | variable    | value    |
      | [[Val]]     | test     |
      | [[rec().a]] | Warewolf |
	  And "workflowithAssignandReplaceTestingUnassignedvariablevalues2" contains Replace "Replacing" into "[[replac]]" as	
	  | In Fields | Find         | Replace With |
	  | [[rec()]] | [[Val1]]Test | [[Val]]      |
	  When "workflowithAssignandReplaceTestingUnassignedvariablevalues2" is executed
	  Then the workflow execution has "NO" error
	  And the 'Assign34' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues2' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[Val]] =     | test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	   And the 'Assign34' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues2' debug outputs as    
	  | # |                         |
	  | 1 | [[Val]] = test          |
	  | 2 | [[rec(1).a]] = Warewolf |
	  And the 'Replacing' in WorkFlow 'workflowithAssignandReplaceTestingUnassignedvariablevalues2' debug inputs as 
	  | In Field(s)             | Find                | Replace With   |
	  | [[rec(1).a]] = Warewolf | [[Val1]]Test =  | [[Val]] = test |
#	  And the 'Replacing' in Workflow 'workflowithAssignandReplaceTestingUnassignedvariablevalues2' debug outputs as
#	  |                |
#	  | [[replac]] =  |	 

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

Scenario: Workflow with Assign Format Numbers and testing variables that hasn't been assigned2
	  Given I have a workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2"
	  And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[val]]  | 1     | 	  
     And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" contains Format Number "Fnumber" as 
	  | Number      | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[val1]]234 | Up                | [[val]]     | [[val]]         | [[fresult]] |
	  When "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" is executed
	  Then the workflow execution has "AN" error
	  And the 'IndexVal' in WorkFlow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues2' debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 1         |
	  And the 'IndexVal' in Workflow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues2' debug outputs as  
	  | # |              |
	  | 1 | [[val]]  = 1 |   
	  And the 'Fnumber' in WorkFlow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues2' debug inputs as 	
	  | Number            | Rounding | Rounding Value | Decimals to show |
	  | [[val1]]234 =  | Up       | [[val]] = 1    | [[val]]  = 1     |
	  And the 'Fnumber' in Workflow 'WorkflowWithAssignandFormatTestingUnassignedvariablevalues2' debug outputs as 
	  |                      |
	  | [[fresult]]  =  |

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
	   And the 'Create12' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug outputs as    
	   |                    |
	  And the 'DeleteFolder1' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug inputs as
	  | Input Path    | Username | Password |
	  And the 'DeleteFolder1' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues' debug outputs as    
	  |                    |

Scenario: Workflow with Assign Create Delete folder and testing variable values that hasn't been assigned2
	  Given I have a workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2"
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" contains an Assign "AssignT" as
	  | variable    | value           |
	  | [[rec().a]] | C:\copied00.txt |
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" contains an Create "Create12" as
	  | File or Folder           | If it exits | Username | Password | Result   |
	  | [[NoValue]]\copied00.txt | True        |          |          | [[res1]] |
	  And "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" contains an Delete Folder "DeleteFolder1" as
	  | Recordset                | Result   |
	  | [[NoValue]]\copied00.txt | [[res2]] |
	  When "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" is executed
	  Then the workflow execution has "AN" error
	  And the 'AssignT' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug inputs as
	  | # | Variable      | New Value       |
	  | 1 | [[rec().a]] = | C:\copied00.txt |
	  And the 'AssignT' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug outputs as     
	  | # |                              |
	  | 1 | [[rec(1).a]] = C:\copied00.txt |
	 And the 'Create12' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug inputs as
	  | File or Folder                           | Overwrite | Username | Password |
	   And the 'Create12' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug outputs as    
	   |                    |
	  And the 'DeleteFolder1' in WorkFlow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug inputs as
	  | Input Path                               | Username | Password |
	  And the 'DeleteFolder1' in Workflow 'WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2' debug outputs as    
	  |                    |


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
      | fx =         |
      | [[Val1]]+1 = |           
      And the 'Calculate1' in Workflow 'WorkflowforCalTestingUnassignedvariablevalue' debug outputs as  
	  |           |
	  | [[res]] = |


Scenario: Calculate testing variable values that hasn't been assigned2
      Given I have a workflow "WorkflowforCalTestingUnassignedvariablevalue2"
      And "WorkflowforCalTestingUnassignedvariablevalue2" contains an Assign "Values34" as
	  | variable | value |
	  | [[Val]]  | 1     |
	 And "WorkflowforCalTestingUnassignedvariablevalue2" contains Calculate "Calculate1" with formula "[[Val1]]23+1" into "[[res]]"
	  When "WorkflowforCalTestingUnassignedvariablevalue2" is executed  	  
	  Then the workflow execution has "AN" error	
      And the 'Values34' in WorkFlow 'WorkflowforCalTestingUnassignedvariablevalue2' debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | 1         |
	  And the 'Values34' in Workflow 'WorkflowforCalTestingUnassignedvariablevalue2' debug outputs as    
	  | # |             |
	  | 1 | [[Val]] = 1 |
	  And the 'Calculate1' in WorkFlow 'WorkflowforCalTestingUnassignedvariablevalue2' debug inputs as 
      | fx =           |
      | [[Val1]]23+1 = |           
      And the 'Calculate1' in Workflow 'WorkflowforCalTestingUnassignedvariablevalue2' debug outputs as  
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

Scenario: Workflow with Assign, Date Time Difference tools and testing variable values that hasn't been assigned2
	  Given I have a workflow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2"
	  And "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" contains an Assign "InputDates2" as
	  | variable | value |
	  | [[val]]  | 2014    |
	  And "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" contains Date and Time Difference "DateTime4" as	
	  | Input1                                 | Input2     | Input Format | Output In | Result     |
	  | 10/01/1991  [[val1]]/[[val1]]/[[val1]] | 10/01/1991 | dd/mm/yyyy   | Years     | [[result]] |  
	  When "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" is executed
	  Then the workflow execution has "AN" error
	  And the 'InputDates2' in WorkFlow 'WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2' debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 2014      |
	  And the 'InputDates2' in Workflow 'WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2' debug outputs as  
	  | # |                 |
	  | 1 | [[val]]  = 2014 |
	  And the 'DateTime4' in WorkFlow 'WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2' debug inputs as
	  | Input 1                                                | Input 2    | Input Format | Output In |
	  | 10/01/1991  [[val1]]/[[val1]]/[[val1]] =  | 10/01/1991 | dd/mm/yyyy   | Years     |
	  And the 'DateTime4' in Workflow 'WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2' debug outputs as 
	  |                |
	  |  |

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
	  |                       |
	  | [[result1]] = Failure |


Scenario: Workflow with Assign Sort and testing variables that hasn't been assigned
      Given I have a workflow "workflowithAssignandsortingrec12"
      And "workflowithAssignandsortingrec12" contains an Assign "sortval5" as
	  | variable    | value |
	  | [[rs(1).a]] | 10    |
	  | [[rs(5).a]] | 20    |
	  | [[rs(7).a]] | 30    |
	  | [[rs(2).b]] | 6     |
	  | [[rs(4).b]] | 4     |
	  | [[rs(6).b]] | 2     |
	  And "workflowithAssignandsortingrec12" contains an Sort "sortRec1" as
	  | Sort Field  | Sort Order |
	  | [[xs(*).a]] | Backwards  |
	  When "workflowithAssignandsortingrec12" is executed
	  Then the workflow execution has "AN" error
	  And the 'sortval5' in WorkFlow 'workflowithAssignandsortingrec12' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 10        |
	  | 2 | [[rs(5).a]] = | 20        |
	  | 3 | [[rs(7).a]] = | 30        |
	  | 4 | [[rs(2).b]] = | 6         |
	  | 5 | [[rs(4).b]] = | 4         |
	  | 6 | [[rs(6).b]] = | 2         |
	  And the 'sortval5' in Workflow 'workflowithAssignandsortingrec12' debug outputs as    
	  | # |                  |
	  | 1 | [[rs(1).a]] = 10 |
	  | 2 | [[rs(5).a]] = 20 |
	  | 3 | [[rs(7).a]] = 30 |
	  | 4 | [[rs(2).b]] = 6  |
	  | 5 | [[rs(4).b]] = 4  |
	  | 6 | [[rs(6).b]] = 2  |
	  And the 'sortRec1' in WorkFlow 'workflowithAssignandsortingrec12' debug inputs as
	  | Sort Field    | Sort Order |
	  | [[xs(*).a]] = | Backwards  |
	  And the 'sortRec1' in Workflow 'workflowithAssignandsortingrec12' debug outputs as
	  |               |
	  | [[xs(*).a]] = |
	 
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
       | [[rec(*).unique]] = |
      
      
Scenario: Example Executing Utility - Format Number example workflow
	  Given I have a workflow "Utility - Format Number Test"
	  And "Utility - Format Number Test" contains "Utility - Format Number" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Format Number Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Format Number1' in WorkFlow 'Utility - Format Number' debug inputs as
	  | Number  | Rounding | Rounding Value | Decimals to show |
	  | 123.446 | Normal   | 2              | 2                |
	  And the 'Format Number1' in Workflow 'Utility - Format Number' debug outputs as    
	  |                    |
	  | [[Price]] = 123.45 |
	   And the 'Format Number2' in WorkFlow 'Utility - Format Number' debug inputs as
	  | Number | Rounding | Rounding Value | Decimals to show |
	  | 14649  | Up       | -2             | -3               |
	  And the 'Format Number2' in Workflow 'Utility - Format Number' debug outputs as    
	  |                   |
	  | [[PriceInK]] = 14 |

 
Scenario: Example Executing Utility - Random example workflow
	  Given I have a workflow "Utility - Random Test"
	  And "Utility - Random Test" contains "Utility - Random" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Random Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Random1' in WorkFlow 'Utility - Random' debug inputs as
	  | Random  | From | To |
	  | Numbers | 1    | 6  |
	  And the 'Random1' in Workflow 'Utility - Random' debug outputs as    
	  |                      |
	  | [[DiceRoll]] = Int32 |
	    And the 'Random2' in WorkFlow 'Utility - Random' debug inputs as
	  | Random  | Length |
	  | Letters | 7      |
	  And the 'Random2' in Workflow 'Utility - Random' debug outputs as    
	  |                       |
	  | [[Scrabble]] = String |
	     And the 'Random3' in WorkFlow 'Utility - Random' debug inputs as
	  | Random |
	  | GUID   | 
	  And the 'Random3' in Workflow 'Utility - Random' debug outputs as    
	  |                      |
	  | [[License]] = String |


Scenario: Example Executing Utility - Date and Time example workflow
	  Given I have a workflow "Utility - Date and Time Test"
	  And "Utility - Date and Time Test" contains "Utility - Date and Time" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Utility - Date and Time Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Date and Time(1)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input            | =        | Input Format            | =                      | Add Time |  | Output Format           | =                      |
	  | System Date Time | DateTime | System Date Time Format | yyyy/MM/dd hh:mm:ss tt | ""       |  | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |
	  And the 'Date and Time(1)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                       |
	  | [[nowish]] = DateTime |   
	 And the 'Date and Time(2)' in WorkFlow 'Utility - Date and Time' debug inputs as
	 | Input                 | Input Format            | =                      | Add Time |  | Output Format          |
	 | [[nowish]] = DateTime | System Date Time Format | yyyy/MM/dd hh:mm:ss tt | ""       |  | mm/dd/yy 12h:min am/pm |
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
	  | Input | Input Format | Add Time |  | Output Format                  |
	  | am    | am/pm        | ""       |  | mm/dd/yyyy 12h:min:ss.sp am/pm |
	  And the 'Date and Time(4)' in Workflow 'Utility - Date and Time' debug outputs as    
	  |                               |
	  | [[TheDefaultDate]] = DateTime |  
	  And the 'Date and Time(5)' in WorkFlow 'Utility - Date and Time' debug inputs as
	  | Input            | =        | Input Format            | =                      | Add Time |  | Output Format                                 |
	  | System Date Time | DateTime | System Date Time Format | yyyy/MM/dd hh:mm:ss tt | ""       |  | 'Date format yyyy MM dd yields : ' yyyy MM dd |

Scenario: Example Executing Utility - Gather System Information example workflow
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


Scenario: Example Executing Utility - Web Request example workflow
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

Scenario: Example Executing Utility - Assign example workflows
	  Given I have a workflow "Test Example Utility - Assign"
	  And "Test Example Utility - Assign" contains "Utility - Assign" from server "localhost" with mapping as
	 | Input to Service | From Variable | Output from Service | To Variable        |
	 |                  |               | rec(*).set      | [[rec().set]]      |
	 |                  |               | hero(*).pushups | [[hero().pushups]] |
	 |                  |               | hero(*).name    | [[hero().name]]    |
	  When "Test Example Utility - Assign" is executed
	  Then the workflow execution has "NO" error
	  And the 'Utility - Assign' in Workflow 'Test Example Utility - Assign' debug outputs as    
	  |                                                                |
	  | [[rec(1).set]] = Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	  | [[hero(1).pushups]] = All of them.                             |
	  | [[hero(1).name]] = Chuck Norris                                |

	

Scenario: Example Executing Data - Base Conversion example workflow
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


Scenario: Example Executing Data - Case Conversion example workflow
	  Given I have a workflow "Data - Case Conversion Test"
	  And "Data - Case Conversion Test" contains "Data - Case Conversion" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Data - Case Conversion Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Case Conversion1 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                                            | To    |
	  | 1 | [[sometext]] = gET reaDy FoR sOme Text CONVersionS | UPPER | 
	  And the 'Case Conversion1 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                                                    |
	   | 1 | [[sometext]] = GET READY FOR SOME TEXT CONVERSIONS |
	   And the 'Case Conversion2 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                                            | To    |
	  | 1 | [[sometext]] = GET READY FOR SOME TEXT CONVERSIONS | lower | 
	  And the 'Case Conversion2 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                                                    |
	   | 1 | [[sometext]] = get ready for some text conversions |
	   And the 'Case Conversion3 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                                            | To       |
	  | 1 | [[sometext]] = get ready for some text conversions | Sentence | 
	  And the 'Case Conversion3 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                                                    |
	   | 1 | [[sometext]] = Get ready for some text conversions |
	     And the 'Case Conversion4 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                                            | To         |
	  | 1 | [[sometext]] = Get ready for some text conversions | Title Case | 
	  And the 'Case Conversion4 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                                                    |
	   | 1 | [[sometext]] = Get Ready For Some Text Conversions |
	     And the 'Case Conversion5 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                       | To    |
	  | 1 | [[sometext]] = 1Mixed up 5om3 | UPPER | 
	  And the 'Case Conversion5 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                               |
	   | 1 | [[sometext]] = 1MIXED UP 5OM3 |
	     And the 'Case Conversion6 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                       | To       |
	  | 1 | [[sometext]] = 1MIXED UP 5OM3 | lower | 
	  And the 'Case Conversion6 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                                                    |
	   | 1 | [[sometext]] = 1mixed up 5om3 |
	     And the 'Case Conversion7 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                       | To       |
	  | 1 | [[sometext]] = 1mixed up 5om3 | Sentence | 
	  And the 'Case Conversion7 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                               |
	   | 1 | [[sometext]] = 1mixed up 5om3 |
	     And the 'Case Conversion8 (1)' in WorkFlow 'Data - Case Conversion' debug inputs as
	  | # | Convert                       | To         |
	  | 1 | [[sometext]] = 1mixed up 5om3 | Title Case | 
	  And the 'Case Conversion8 (1)' in Workflow 'Data - Case Conversion' debug outputs as    
	   | # |                               |
	  | 1 | [[sometext]] = 1Mixed Up 5Om3 |


Scenario: Example Executing Data - Data Merge example workflow
	  Given I have a workflow "Data - Data Merge Test"
	  And "Data - Data Merge Test" contains "Data - Data Merge" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | FileContent         | [[FileContent]] |
	  When "Data - Data Merge Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Data - Data Merge' in Workflow 'Data - Data Merge' debug outputs as    
	    |                                                                |
	    | [[FileContent]] = String |
	   
Scenario: Example Executing Data - Find Index example workflow
	  Given I have a workflow "Utility - Find Index Test"
	  And "Utility - Find Index Test" contains "Utility - Find Index" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Utility - Find Index Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Find Index1' in WorkFlow 'Utility - Find Index' debug inputs as
	  | In Field | Index            | Characters | Direction     |
	  | abc      | First Occurrence | b          | Left to Right |
	  And the 'Find Index1' in Workflow 'Utility - Find Index' debug outputs as  
	  |                  |
	  | [[WhereIsB]] = 2 |
	  And the 'Find Index2' in WorkFlow 'Utility - Find Index' debug inputs as
	  | In Field           | Index           | Characters | Direction     |
	  | abcbdefghibjklmnop | All Occurrences | b          | Left to Right |
	  And the 'Find Index2' in Workflow 'Utility - Find Index' debug outputs as  
	  |                            |
	  | [[WhereAreTheBs]] = 2,4,11 |

Scenario: Example Excuting File and Folder - Copy
	  Given I have a workflow "File and Folder - Copy Test"
	  And "File and Folder - Copy Test" contains "File and Folder - Copy" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Copy Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Copy' in Workflow 'File and Folder - Copy' debug outputs as 
	  |                  |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Create
	  Given I have a workflow "File and Folder - Create Test"
	  And "File and Folder - Create Test" contains "File and Folder - Create" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Create Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Create' in Workflow 'File and Folder - Create' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Delete
	  Given I have a workflow "File and Folder - Delete Test"
	  And "File and Folder - Delete Test" contains "File and Folder - Delete" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Delete Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Delete' in Workflow 'File and Folder - Delete' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Move
	  Given I have a workflow "File and Folder - Move Test"
	  And "File and Folder - Move Test" contains "File and Folder - Move" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Move Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Move' in Workflow 'File and Folder - Move' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario:Example Excuting File and Folder - Read File
	  Given I have a workflow "File and Folder - Read File Test"
	  And "File and Folder - Read File Test" contains "File and Folder - Read File" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Read File Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Read File' in Workflow 'File and Folder - Read File' debug outputs as 
	  |                                         |
	  | [[Logs]] = the contents of the log file |

Scenario: Example Excuting File and Folder - Read Folder
	  Given I have a workflow "File and Folder - Read Folder Test"
	  And "File and Folder - Read Folder Test" contains "File and Folder - Read Folder" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Read Folder Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Read Folder1' in Workflow 'File and Folder - Read Folder' debug outputs as 
	  |                                                     |
	  | [[users]] = c:\temp\WarewolfExamples\ReadFolder\sub |
	  And the 'Read Folder 2' in Workflow 'File and Folder - Read Folder' debug outputs as 
	  |                                                               |
	  | [[server(1).users]] = c:\temp\WarewolfExamples\ReadFolder\sub |
	  And the 'Read Folder 3' in Workflow 'File and Folder - Read Folder' debug outputs as 
	  |                                                                           |
	  | [[server(1).userfolders]] = c:\temp\WarewolfExamples\ReadFolder\sub\inner |

Scenario: Example Excuting File and Folder - Rename
	  Given I have a workflow "File and Folder - Rename Test"
	  And "File and Folder - Rename Test" contains "File and Folder - Rename" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Rename Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Rename' in Workflow 'File and Folder - Rename' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Unzip
	  Given I have a workflow "File and Folder - Unzip Test"
	  And "File and Folder - Unzip Test" contains "File and Folder - Unzip" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Unzip Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'UnZip' in Workflow 'File and Folder - Unzip' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Write File
	  Given I have a workflow "File and Folder - Write File Test"
	  And "File and Folder - Write File Test" contains "File and Folder - Write File" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Write File Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Write' in Workflow 'File and Folder - Write File' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Excuting File and Folder - Zip
	  Given I have a workflow "File and Folder - Zip Test"
	  And "File and Folder - Zip Test" contains "File and Folder - Zip" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "File and Folder - Zip Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Zip' in Workflow 'File and Folder - Zip' debug outputs as 
	  |                        |
	  | [[Complete]] = Success |

Scenario: Example Executing Utility - Replace example workflow
	  Given I have a workflow "Utility - Replace Test"
	  And "Utility - Replace Test" contains "Utility - Replace" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Utility - Replace Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Replace1' in WorkFlow 'Utility - Replace' debug inputs as
	 | In Field(s)            | Find | Replace With |
	 | [[SomeText]] = Replace | ac   | icat         |
	  And the 'Replace1' in Workflow 'Utility - Replace' debug outputs as  
	  |                                                                               |
	  | [[SomeText]] = Replicate                                                      |
	  | [[count]] = 1                                                                 |
	  And the 'Replace2' in Workflow 'Utility - Replace' debug outputs as  
	  |                                       |
	  | [[rec(1).homeNumber]]   = +1553122442 |
	  | [[rec(2).homeNumber]]   = +1554682136 |
	  | [[rec(1).mobileNumber]] = +1655985781 |
	  | [[rec(2).mobileNumber]] = +1985623145 |
	  | [[recount]] = 3                       |

Scenario: Example Executing Recordset - Count Records example workflow
	  Given I have a workflow "Recordset - Count Records Test"
	  And "Recordset - Count Records Test" contains "Recordset - Count Records" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Count Records Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Count Records' in WorkFlow 'Recordset - Count Records' debug inputs as
	  | Recordset                  |
	  | [[rec(1).set]] = Warewolf |
	  | [[rec(2).set]] = SOA |
	  | [[rec(3).set]] = Recordset |
	  And the 'Count Records' in Workflow 'Recordset - Count Records' debug outputs as  
	  |               |
	  | [[count]] = 3 |

Scenario: Example Executing Recordset - Records Length example workflow
	  Given I have a workflow "Recordset - Records Length Test"
	  And "Recordset - Records Length Test" contains "Recordset - Records Length" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Records Length Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Length1' in WorkFlow 'Recordset - Records Length' debug inputs as
	  | Recordset                  |
	  | [[rec(1).set]] = Warewolf  |
	  | [[rec(5).set]] = Recordset |
	  And the 'Length1' in Workflow 'Recordset - Records Length' debug outputs as  
	  |                |
	  | [[length]] = 5 |
	  And the 'Length2' in WorkFlow 'Recordset - Records Length' debug inputs as
	  | Recordset                  |
	  | [[rec(5).set]] = Recordset |
	  And the 'Length2' in Workflow 'Recordset - Records Length' debug outputs as  
	  |                |
	  | [[length]] = 5 |

Scenario: Example Executing Recordset - Delete Records example workflow
	  Given I have a workflow "Recordset - Delete Records Test"
	  And "Recordset - Delete Records Test" contains "Recordset - Delete Records" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Delete Records Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Delete Record1' in WorkFlow 'Recordset - Delete Records' debug inputs as
	  | Records          |
	  | [[rec(2).set]] = 2 |
	  And the 'Delete Record1' in Workflow 'Recordset - Delete Records' debug outputs as  
	  |                      |
	  | [[result]] = Success |
	  And the 'Delete Record2' in WorkFlow 'Recordset - Delete Records' debug inputs as
	  | Records          |
	  | [[rec(1).set]] = 1 |
	  | [[rec(3).set]] = 3 |
	  And the 'Delete Record2' in Workflow 'Recordset - Delete Records' debug outputs as  
	  |                      |
	  | [[result]] = Success |

Scenario: Example Executing Recordset - Find Records example workflow
	  Given I have a workflow "Recordset - Find Records Test"
	  And "Recordset - Find Records Test" contains "Recordset - Find Records" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Find Records Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Find Record Index (2)' in Workflow 'Recordset - Find Records' debug outputs as  
	  |                   |
	  | [[Results]] = 6,8 |
	  
Scenario:Example Executing Recordset - Sort Records example workflow
	  Given I have a workflow "Recordset - Sort Records Test"
	  And "Recordset - Sort Records Test" contains "Recordset - Sort Records" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Sort Records Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Sort Records1' in WorkFlow 'Recordset - Sort Records' debug inputs as
	  | Sort Field         | Sort Order |
	  | [[rec(1).set]] = 2 |            |
	  | [[rec(2).set]] = x |            |
	  | [[rec(3).set]] = 3 |            |
	  | [[rec(4).set]] = c |            |
	  | [[rec(5).set]] = y |            |
	  | [[rec(6).set]] = 0 |            |
	  | [[rec(7).set]] = a |            |
	  | [[rec(8).set]] = b |            |
	  | [[rec(9).set]] = 1 |            |
	  | [[rec(10).set]] =z | Backwards  |
	  And the 'Sort Records1' in Workflow 'Recordset - Sort Records' debug outputs as  
	  |                    |
	  | [[rec(1).set]] = z |
	  | [[rec(2).set]] = y |
	  | [[rec(3).set]] = x |
	  | [[rec(4).set]] = c |
	  | [[rec(5).set]] = b |
	  | [[rec(6).set]] = a |
	  | [[rec(7).set]] = 3 |
	  | [[rec(8).set]] = 2 |
	  | [[rec(9).set]] = 1 |
	  | [[rec(10).set]] =0 |
	  And the 'Sort Records2' in WorkFlow 'Recordset - Sort Records' debug inputs as
	   | Sort Field         | Sort Order |
	   | [[rec(1).set]] = z |            |
	   | [[rec(2).set]] = y |            |
	   | [[rec(3).set]] = x |            |
	   | [[rec(4).set]] = c |            |
	   | [[rec(5).set]] = b |            |
	   | [[rec(6).set]] = a |            |
	   | [[rec(7).set]] = 3 |            |
	   | [[rec(8).set]] = 2 |            |
	   | [[rec(9).set]] = 1 |            |
	   | [[rec(10).set]] =0 | Forward    |
	  And the 'Sort Records2' in Workflow 'Recordset - Sort Records' debug outputs as  
	  |                    |
	  | [[rec(1).set]] = 0 |
	  | [[rec(2).set]] = 1 |
	  | [[rec(3).set]] = 2 |
	  | [[rec(4).set]] = 3 |
	  | [[rec(5).set]] = a |
	  | [[rec(6).set]] = b |
	  | [[rec(7).set]] = c |
	  | [[rec(8).set]] = x |
	  | [[rec(9).set]] = y |
	  | [[rec(10).set]] =z |

Scenario: Example Executing Recordset - Unique Records example workflow
	  Given I have a workflow "Recordset - Unique Records Test"
	  And "Recordset - Unique Records Test" contains "Recordset - Unique Records" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Recordset - Unique Records Test" is executed
	  Then the workflow execution has "NO" error
	  And the 'Unique Records1' in WorkFlow 'Recordset - Unique Records' debug inputs as
	   | #           |                                  | Return Fields         |
	   | In Field(s) | [[Example(4).FirstName]] = Jerome | [[Example().Index]] = |
	   And the 'Unique Records1' in Workflow 'Recordset - Unique Records' debug outputs as  
	   | # |                            |
	   | 1 | [[Result(1).example1]] = 1 |
	   |   | [[Result(2).example1]] = 4 |
      And the 'Unique Records2' in WorkFlow 'Recordset - Unique Records' debug inputs as
	   | #           |                                   | Return Fields         |
	   | In Field(s) | [[Example(4).FirstName]] = Jerome |                       |
	   |             | [[Example(4).LastName]] = Smit    | [[Example().Index]] = |
	   And the 'Unique Records2' in Workflow 'Recordset - Unique Records' debug outputs as  
	   | # |                            |
	   | 1 | [[Result(1).example2]] = 1 |
	   |   | [[Result(2).example2]] = 2 |
	   |   | [[Result(3).example2]] = 4 |
     
Scenario: Example Executing Loop Constructs - For Each example workflow
	  Given I have a workflow "Loop Constructs - For Each Test"
	  And "Loop Constructs - For Each Test" contains "Loop Constructs - For Each" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Loop Constructs - For Each Test" is executed
	  Then the workflow execution has "NO" error	  
      And the 'For Each1' in WorkFlow 'Loop Constructs - For Each' debug inputs as 
	    |                 | Number |
	    | No. of Executes | 6      |
	   And the 'For Each1' in WorkFlow 'Loop Constructs - For Each' has  "6" nested children 
	   And the 'Random1' in step 1 for 'For Each1' debug inputs as
	     | Random | Length |
	     | GUID   |        |
	   And the 'Random1' in step 1 for 'For Each1' debug outputs as
        |                         |
        | [[rec(1).set]] = String |
		And the 'Random1' in step 2 for 'For Each1' debug inputs as
	     | Random | Length |
	     | GUID   |        |
	   And the 'Random1' in step 2 for 'For Each1' debug outputs as
        |                         |
        | [[rec(2).set]] = String |
		And the 'Random1' in step 3 for 'For Each1' debug inputs as
	     | Random | Length |
	     | GUID   |        |
	   And the 'Random1' in step 3 for 'For Each1' debug outputs as
        |                         |
        | [[rec(3).set]] = String |
		And the 'Random1' in step 4 for 'For Each1' debug inputs as
	     | Random | Length |
	     | GUID   |        |
	   And the 'Random1' in step 4 for 'For Each1' debug outputs as
        |                         |
        | [[rec(4).set]] = String |
		And the 'Random1' in step 5 for 'For Each1' debug inputs as
	     | Random | Length |
	     | GUID   |        |
	   And the 'Random1' in step 5 for 'For Each1' debug outputs as
        |                         |
        | [[rec(5).set]] = String |

Scenario: Example Executing Control Flow - Sequence example workflow
	  Given I have a workflow "Control Flow - Sequence Test"
	  And "Control Flow - Sequence Test" contains "Control Flow - Sequence" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Control Flow - Sequence Test" is executed
	  Then the workflow execution has "NO" error
      And the 'Split Names (3)' in Workflow 'Organize Customers' debug outputs as
	   | # |                                                       |
	   | 1 | [[TempCustomers(1).FirstName]] = bart                 |
	   |   | [[TempCustomers(2).FirstName]] = homer                |
	   |   | [[TempCustomers(3).FirstName]] = marge                |
	   |   | [[TempCustomers(4).FirstName]] = lisa                 |
	   |   | [[TempCustomers(5).FirstName]] = maggie               |
	   | 2 | [[TempCustomers(1).LastName]] = simpson               |
	   |   | [[TempCustomers(2).LastName]] = simpson               |
	   |   | [[TempCustomers(3).LastName]] = simpson               |
	   |   | [[TempCustomers(4).LastName]] = simpson               |
	   |   | [[TempCustomers(5).LastName]] = simpson               |
	   | 3 | [[TempCustomers(1).Email]] = bart.simpson@couch.com   |
	   |   | [[TempCustomers(2).Email]] = homer.simpson@couch.com  |
	   |   | [[TempCustomers(3).Email]] = marge.simpson@couch.com  |
	   |   | [[TempCustomers(4).Email]] = lisa.simpson@couch.com   |
	   |   | [[TempCustomers(5).Email]] = maggie.simpson@couch.com |        
	   And the 'Sort Names Alphabetically' in WorkFlow 'Organize Customers' debug inputs as
	   | Sort Field                          | Sort Order |
	   | [[Customers(1).FirstName]] = bart   |            |
	   | [[Customers(2).FirstName]] = homer  |            |
	   | [[Customers(3).FirstName]] = marge  |            |
	   | [[Customers(4).FirstName]] = lisa   |            |
	   | [[Customers(5).FirstName]] = maggie | Forward    |
	   And the 'Sort Names Alphabetically' in Workflow 'Organize Customers' debug outputs as
	   |                                     |
	   | [[Customers(1).FirstName]] = bart   |
	   | [[Customers(2).FirstName]] = homer  |
	   | [[Customers(3).FirstName]] = lisa   |
	   | [[Customers(4).FirstName]] = maggie |
	   | [[Customers(5).FirstName]] = marge  |

Scenario: Example Executing Control Flow - Decision example workflow
	  Given I have a workflow "Control Flow - Decision Test"
	  And "Control Flow - Decision Test" contains "Control Flow - Decision" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Control Flow - Decision Test" is executed
	  Then the workflow execution has "NO" error

Scenario: Example Executing Control Flow - Switch example workflow
	  Given I have a workflow "Control Flow - Switch Test"
	  And "Control Flow - Switch Test" contains "Control Flow - Switch" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  When "Control Flow - Switch Test" is executed
	  Then the workflow execution has "NO" error



Scenario: Example Executing Scripting - Script example workflow
  Given I have a workflow "Scripting - Script Test"
  And "Scripting - Script Test" contains "Scripting - Script" from server "localhost" with mapping as
  | Input to Service | From Variable | Output from Service | To Variable     |
  When "Scripting - Script Test" is executed
  Then the workflow execution has "NO" error
  And the 'Script1' in WorkFlow 'Scripting - Script' debug inputs as	
  | Language | Script   |
  | Ruby     | sleep(5) | 
  And the 'Script1' in Workflow 'Scripting - Script' debug outputs as    
  |                |
  | [[Result]] = 5 | 
  And the 'Script2' in WorkFlow 'Scripting - Script' debug inputs as	
  | Language   | Script          |
  | JavaScript | String = String |
  And the 'Script2' in Workflow 'Scripting - Script' debug outputs as    
  |                |
  | [[Result]] = 1 | 
  And the 'Script3' in WorkFlow 'Scripting - Script' debug inputs as	
  | Language | Script          |
  | Python   | String = String |
  And the 'Script3' in Workflow 'Scripting - Script' debug outputs as    
  |                  |
  | [[Result]] = one | 


Scenario: Gather System tool throws error when debug with 2 variables in one row 
	  Given I have a workflow "WorkflowW"
	  And "WorkflowW" contains an Assign "IndexVal" as
	  | variable | value   |
	  | [[a]]    | b       |
	   And "WorkflowW" contains Gather System Info "System info" as
	  | Variable   | Selected    |
	  | [[a]][[b]] | Date & Time |
	  When "WorkflowW" is executed
	  Then the workflow execution has "AN" error
	   And the 'IndexVal' in WorkFlow 'WorkflowW' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  And the 'IndexVal' in Workflow 'WorkflowW' debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  And the 'System info' in WorkFlow 'WorkflowW' debug inputs as
	  | # |              |             |
	 And the 'System info' in Workflow 'WorkflowW' debug outputs as    
	  | # |              |
	  | 1 | [[a]][[b]] = |

	   
	  
	   
Scenario: Gather System tool throws error when debug with invalid variableb
	  Given I have a workflow "WorkflowW1"
	  And "WorkflowW1" contains an Assign "IndexVal" as
	  | variable | value   |
	  | [[a]]    | b       |
	   And "WorkflowW1" contains Gather System Info "System info" as
	  | Variable         | Selected    |
	  | [[a]][[rec().a]] | Date & Time |
	  When "WorkflowW1" is executed
	  Then the workflow execution has "AN" error
	   And the 'IndexVal' in WorkFlow 'WorkflowW1' debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  And the 'IndexVal' in Workflow 'WorkflowW1' debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  And the 'System info' in WorkFlow 'WorkflowW1' debug inputs as
	  | # |                      |             |
	 And the 'System info' in Workflow 'WorkflowW1' debug outputs as    
	  | # |                      |
	  | 1 | [[a]][[rec().a]] = |

Scenario: Workflow Base Convert and Case Convert passing invalid variable through execution
	  Given I have a workflow "WorkflowWithBaseCase1"
	  And "WorkflowWithBaseCase1" contains an Assign "Assign1" as
	  | variable       | value    |
	  | [[a]]          | 1        |
	  | [[rec(1).a]]   | Warewolf |
	  | [[rec(2).a]]   | Test     |
	  | [[index(1).a]] | a$*      |
	  And "WorkflowWithBaseCase1" contains case convert "Case1" as
	  | Variable                  | Type  |
	  | [[rec([[index(1).a]]).a]] | UPPER |
	  And "WorkflowWithBaseCase1" contains Base convert "Base1" as
	  | Variable                  | From | To      |
	  | [[rec([[index(1).a]]).a]] | Text | Base 64 |
	  When "WorkflowWithBaseCase1" is executed
	  Then the workflow execution has "AN" error
	  And the 'Assign1' in WorkFlow 'WorkflowWithBaseCase1' debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | Warewolf  |
	  | 3 | [[rec(2).a]] =   | Test      |
	  | 4 | [[index(1).a]] = | a$*       |
	   And the 'Assign1' in Workflow 'WorkflowWithBaseCase1' debug outputs as   
	  | # |                            |
	  | 1 | [[a]]         =  1         |
	  | 2 | [[rec(1).a]]   =  Warewolf |
	  | 3 | [[rec(2).a]]  =  Test      |
	  | 4 | [[index(1).a]] =  a$*      |
	  And the 'Case1' in WorkFlow 'WorkflowWithBaseCase1' debug inputs as
	  | # | Convert                     | To    |
	  | 1 | [[rec(a$*).a]] = | UPPER |
	  And the 'Case1' in Workflow 'WorkflowWithBaseCase1' debug outputs as  
	  | # |                     |
	  And the 'Base1' in WorkFlow 'WorkflowWithBaseCase1' debug inputs as
	  | # | Convert          | From | To      |
	  | 1 | [[rec(a$*).a]] = | Text | Base 64 |
      And the 'Base1' in Workflow 'WorkflowWithBaseCase1' debug outputs as  
	  | # |                     |

Scenario: Workflow Base Convert coverting same variable multiple times
	 Given I have a workflow "WorkflowWithBaseConvertUsingSameVariable"
	 And "WorkflowWithBaseConvertUsingSameVariable" contains an Assign "Assign1" as
	 | variable | value |
	 | [[test]] | data  |
	 And "WorkflowWithBaseConvertUsingSameVariable" contains Base convert "Base12" as
	 | Variable | From    | To      |
	 | [[test]] | Text    | Base 64 |
	 | [[test]] | Base 64 | Text    |
	 When "WorkflowWithBaseConvertUsingSameVariable" is executed
	 Then the workflow execution has "NO" error
	 And the 'Assign1' in WorkFlow 'WorkflowWithBaseConvertUsingSameVariable' debug inputs as
	 | # | Variable   | New Value |
	 | 1 | [[test]] = | data      |
	  And the 'Assign1' in Workflow 'WorkflowWithBaseConvertUsingSameVariable' debug outputs as   
	 | # |                  |
	 | 1 | [[test]] =  data |
	 And the 'Base12' in WorkFlow 'WorkflowWithBaseConvertUsingSameVariable' debug inputs as
	 | # | Convert             | From    | To      |
	 | 1 | [[test]] = data     | Text    | Base 64 |
	 | 2 | [[test]] = ZGF0YQ== | Base 64 | Text    |
    And the 'Base12' in Workflow 'WorkflowWithBaseConvertUsingSameVariable' debug outputs as  
	 | # |                     |
	 | 1 | [[test]] = ZGF0YQ== |
	 | 2 | [[test]] = data     |

Scenario: Workflow Assign and Find Record Index executing with incorrect format of Inputs 
      Given I have a workflow "WFWithAssignandFindRecordindexs"
	  And "WFWithAssignandFindRecordindex" contains an Assign "Record" as
      | variable     | value |
      | [[rec(1).a]] | 23    |
      | [[rec(2).a]] | 34    |
      | [[xr(1).a]]  | 10    |
	  And "WFWithAssignandFindRecordindexs" contains Find Record Index "FindRecord0" into result as "[[a]][[b]]"
	  | # | In Field              | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
	  | # | [[rec().a]][[xr().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexs" is executed
	  Then the workflow execution has "AN" error


	
Scenario: Executing Workflow Service and Decision tool expected bubling out error in workflow service
	  Given I have a workflow "Utility - Assign WF"
	  And "Utility - Assign WF" contains "Utility - Assign" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable        |
	  |                  |               | rec(*).set      | [[myrec().set]]      |
	  |                  |               | hero(*).pushups | [[thehero().pushups]] |
	  |                  |               | hero(*).name    | [[thehero().name]]    |
	  When "Utility - Assign WF" is executed
	  Then the workflow execution has "NO" error
	  And the 'Utility - Assign' in Workflow 'Utility - Assign WF' debug outputs as    
	  |                                                                   |
	  | [[myrec(1).set]] =    Bart Simpson: I WILL NOT INSTIGATE REVOLUTION |
	  | [[thehero(1).pushups]] = All of them.                                |
	  | [[thehero(1).name]] =   Chuck Norris                                 |
	
Scenario: Error from workflow service is expected to buble out
	  Given I have a workflow "TestAssignWithRemoteOutputsError"
	  And "TestAssignWithRemoteOutputsError" contains an Assign "AssignData" as
	  | variable      | value |
	  | [[inputData]] | hello |
	  And "TestAssignWithRemoteOutputsError" contains "WorkflowUsedBySpecs" from server "Remote Connection Integration" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  | inputData        | [[inputData]] | output              | [[output]]       |
	  |                  |               | values(*).up     | [[values().&up]] |
	  |                  |               | values(*).low     | [[values().low]] |
	  When "TestAssignWithRemoteOutputsError" is executed
	  Then the "TestAssignWithRemoteOutputsError" workflow execution has "AN" error
	  And the 'AssignData' in WorkFlow 'TestAssignWithRemoteOutputsError' debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[inputData]] = | hello     |
	  And the 'AssignData' in Workflow 'TestAssignWithRemoteOutputsError' debug outputs as    
	  | # |                       |
	  | 1 | [[inputData]] = hello |
	   And the 'WorkflowUsedBySpecs' in WorkFlow 'TestAssignWithRemoteOutputsError' debug inputs as
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
	  | 2 | [[values(1).up]] = HELLO |
	  | 3 | [[values(1).low]] = hello |	  	 

Scenario Outline: Workflow to Workflow Mappings 
Given I have a workflow "<Name>"
And "<Name>" contains an Assign "AssignData" as
        | variable         | value         |
        | <AssignVariable> | <AssignValue> |
And "<Name>" contains "WorkflowMappingsInnerWorkflow" from server "localhost" with mapping as
| From Variable  | Input to Service | Output from Service | To Variable  |
| <FromVariable> | <ToService>      | <FromService>       | <ToVariable> |
When "<Name>" is executed
Then the workflow execution has "NO" error
And the 'WorkflowMappingsInnerWorkflow' in WorkFlow '<Name>' debug inputs as
      |                        |
      | <ToServiceAssignValue> |	  
And the 'WorkflowMappingsInnerWorkflow' in Workflow '<Name>' debug outputs as
      |                        |
      |  <ToVariableAndResult> |
Examples: 
| Name                       | AssignVariable | AssignValue | FromVariable   | ToService  | FromService | ToVariable     | ToServiceAssignValue   | ToVariableAndResult    |
| ScalToRecInAndScaltoRecOut | [[OuterIn]]    | hello       | [[OuterIn]]    | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| BlnkToRecIn                |                |             |                | in(*).in   | InnerOutput | [[OuterOut]]   |                        | [[OuterOut]] =         |
| BlnkToScalIn               |                |             |                | InnerInput | InnerOutput | [[OuterOut]]   |                        | [[OuterOut]] =         |
| HdCdScalToRecInSclToSclOut | [[OuterIn]]    | ll          | he[[OuterIn]]o | in(*).in   | InnerOutput | [[OuterOut]]   | [[in(1).in]] = hello   | [[OuterOut]] = hello   |
| RecToRecInAndRecOut        | [[rec().out]]  | hello       | [[rec().out]]  | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| ScalToScalIn&ScalOut       | [[OuterIn]]    | hello       | [[OuterIn]]    | InnerInput | InnerOutput | [[OuterOut]]   | [[InnerInput]] = hello | [[OuterOut]] = hello   |
| Static                     |                |             | hello          | in(*).in   | InnerOutput | [[out(*).out]] | [[in(1).in]] = hello   | [[out(1).out]] = hello |
| RecToScalOut               | [[rec().in]]   | hello       | [[rec().in]]   | InnerInput | InnerOutput | [[Output]]     | [[InnerInput]] = hello | [[Output]] = hello     |
| RecToBlank                 | [[rec().in]]   | hello       | [[rec().in]]   | InnerInput | InnerOutput |                | [[InnerInput]] = hello |                        |
| ScalToBlank                | [[var]]        | hello       | [[var]]        | InnerInput | InnerOutput |                | [[InnerInput]] = hello |                        |


Scenario Outline: Database MySqlDB Database service using * indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithDBServiceMails2 | MySQLEmail  | [[rec(*).name]] | [[rec(*).email]] | NO           |

Scenario Outline: Database ODBC Database service using * indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "odbc database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestODBCWFWithDBServiceMails2 | ODBCEmail  | [[rec(*).name]] | [[rec(*).email]] | NO           |

Scenario Outline: Database Oracle Database service using * indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "oracle database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestOracleWFWithDBServiceMails2 | OracleEmail  | [[rec(*).name]] | [[rec(*).email]] | NO           |

Scenario Outline: Database SqlDB  service using int indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "sqlserver database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName              | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestWFWithDBServiceMails3 | SqlEmail    | [[rec(1).name]] | [[rec(1).email]] | NO           |

Scenario Outline: Database MySqlDB Database service using int indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithDBServiceMails3 | MySQLEmail  | [[rec(1).name]] | [[rec(1).email]] | NO           |

Scenario Outline: Database ODBCDB Database service using int indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "odbc database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestODBCWFWithDBServiceMails16 | ODBCEmail  | [[rec(1).name]] | [[rec(1).email]] | NO           |

Scenario Outline: Database OracleDB Database service using int indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "oracle database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestOracleWFWithDBServiceMails9 | OracleEmail  | [[rec(1).name]] | [[rec(1).email]] | NO           |

Scenario Outline: Database SqlDB  service using last indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "sqlserver database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName              | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestWFWithDBServiceMails4 | SqlEmail    | [[rec().name]] | [[rec().email]] | NO           |

Scenario Outline: Database MySqlDB Database service last  indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestMySqlWFWithDBServiceMails5 | MySQLEmail  | [[rec().name]] | [[rec().email]] | NO           |
 
 Scenario Outline: Database ODBCDB Database service last  indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "odbc database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestODBCWFWithDBServiceMails5 | ODBCEmail  | [[rec().name]] | [[rec().email]] | NO           |      

Scenario Outline: Database OracleDB Database service last  indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "oracle database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                   | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestOracleWFWithDBServiceMails5 | OracleEmail  | [[rec().name]] | [[rec().email]] | NO           |

  Scenario Outline: Database SqlDB  service using scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "sqlserver database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                  |
	  | [[name]] = dora                  |
	  | [[email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName              | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestWFWithDBServiceMails5 | SqlEmail    | [[name]]     | [[email]]     | NO           |

Scenario Outline: Database MySqlDB Database service scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                      |
	  | [[name]] = Monk |
	  | [[email]] = dora@explorers.com |
Examples: 
    | WorkflowName                    | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestMySqlWFWithDBServiceMails63 | MySQLEmail  | [[name]]     | [[email]]     | NO           |
 
Scenario Outline: Database ODBCDB Database service scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "odbc database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                      |
	  | [[name]] = Monk |
	  | [[email]] = dora@explorers.com |
Examples: 
    | WorkflowName                    | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestODBCWFWithDBServiceMails63 | ODBCEmail  | [[name]]     | [[email]]     | NO           |

Scenario Outline: Database OracleDB Database service scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "oracle database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                      |
	  | [[name]] = Monk |
	  | [[email]] = dora@explorers.com |
Examples: 
    | WorkflowName                    | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestOracleWFWithDBServiceMails63 | OracleEmail  | [[name]]     | [[email]]     | NO           |

Scenario Outline: Database MySqlDB Database service Error outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | [[rec(*).name]]     | <nameVariable>  |
	  |                  |               | [[rec(*).email]]    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
    | WorkflowName                   | ServiceName | nameVariable         | emailVariable | errorOccured |
    | TestMySqlWFWithDBServiceMails7 | MySQLEmail  | [[rec(-1).name]]     | [[email]]     | YES          |
    | TestMySqlWFWithDBServiceMails8 | MySQLEmail  | [[123]]              | [[email]]     | YES          |
    | TestMySqlWFWithDBServiceMails9 | MySQLEmail  | [[rec(-1).name.bob]] | [[email]]     | YES          |

Scenario Outline: Database MySqlDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "mysql database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  | name             | afg%          | [[countries(*).countryid]]   | <nameVariable>  |
	  |                  |               | [[countries(*).description]] | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(2).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
	  | [[countries(2).description]] = Afghanistan |
Examples: 
    | WorkflowName                    | ServiceName       | nameVariable        | emailVariable                | errorOccured |
    | TestMySqlWFWithDBServiceMails15 | MySqlGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

Scenario Outline: Database ODBCDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "odbc database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  | name             | afg%          | [[countries(*).countryid]]   | <nameVariable>  |
	  |                  |               | [[countries(*).description]] | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(2).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
	  | [[countries(2).description]] = Afghanistan |
Examples: 
    | WorkflowName                    | ServiceName       | nameVariable        | emailVariable                | errorOccured |
    | TestODBCWFWithDBServiceMails15 | ODBCGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

Scenario Outline: Database OracleDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "oracle database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  | name             | afg%          | [[countries(*).countryid]]   | <nameVariable>  |
	  |                  |               | [[countries(*).description]] | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(2).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
	  | [[countries(2).description]] = Afghanistan |
Examples: 
    | WorkflowName                    | ServiceName       | nameVariable        | emailVariable                | errorOccured |
    | TestOracleWFWithDBServiceMails15 | OracleGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

Scenario Outline: Database SqlDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "sqlserver database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service          | To Variable     |
	  | Prefix           | afg           | [[countries(*).countryid]]   | <nameVariable>  |
	  |                  |               | [[countries(*).description]] | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the '<ServiceName>' in Workflow '<WorkflowName>' debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
Examples: 
    | WorkflowName                         | ServiceName           | nameVariable        | emailVariable                | errorOccured |
    | TestSqlWFWithDBServiceMailsCountries | GetCountriesSqlServer | [[countries(*).id]] | [[countries(*).description]] | NO           |

 Scenario Outline: Database SqlDB  service DBErrors
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a "sqlserver database" service "<ServiceName>" with mappings
	  | Input to Service | From Variable | Output from Service | To Variable |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
     | WorkflowName                  | ServiceName          | nameVariable | emailVariable | errorOccured |
     | TestWFWithDBServiceMailsError | willalwayserror      | [[name]]     | [[email]]     | YES          |
     | TestWFWithDBServiceMailsError | willalwaysErrorMySql | [[name]]     | [[email]]     | YES          |


 Scenario: Executing Asynchrounous testing workflow base
	  Given I have a workflow "Testing - Async Test Master Testc"
	  And "Testing - Async Test Master Testc" contains "Async Test Master" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable      |
	  When "Testing - Async Test Master Testc" is executed
	  Then the workflow execution has "NO" error
	  And the 'Async Test Master' in Workflow 'Async Test Master' debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: MYSQL backward Compatiblity
	Given I have a workflow "MySQLMigration"
	And "MySQLMigration" contains "MySQLDATA" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      |                  |               | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "MySQLMigration" is executed
	Then the workflow execution has "NO" error


# SQL Tool Execution specs

Scenario: ODBC Passing Null Input value
	Given I have a workflow "PassingNullValue"
	And "PassingNullValue" contains "Acceptance Testing Resources/ODBCSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullValue" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/ODBCSource' in Workflow 'PassingNullValue' debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |


Scenario: ODBC Mapped To Recordsets incorrect
	Given I have a workflow "WillAlwaysError"
	And "WillAlwaysError" contains "Acceptance Testing Resources/ODBCSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	And "WillAlwaysError" contains "Acceptance Testing Resources/ODBCSource" from server "localhost" with Mapping To as
	| Mapped From | Mapped To               |
	| 1           | [[willalwayserror().1]] |
	When "WillAlwaysError" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/ODBCSource' in Workflow 'WillAlwaysError' debug outputs as
	  |                                                                    |
	  | [[willalwayserror()]]: Recordset must contain one or more field(s) |



Scenario: ODBC Parameter not found in the collection
	Given I have a workflow "BadODBCParameterName"
	And "BadODBCParameterName" contains "Testing/ODBCParameters" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadODBCParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Testing/ODBCParameters' in Workflow 'BadODBCParameterName' debug outputs as
	  |                                                      |
	  | Parameter 'p_startswith' not found in the collection |



Scenario: ODBC Recordset has invalid character
	Given I have a workflow "RenameRecordsetIncorrectly"
	And "RenameRecordsetIncorrectly" contains "Acceptance Testing Resources/ODBCSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "RenameRecordsetIncorrectly" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/ODBCSource' in Workflow 'RenameRecordsetIncorrectly' debug outputs as
	  |                                                              |
	  | [[getCountrie.s().id]] : Recordset name has invalid format   |
	  | [[getCountrie.s().value]]: Recordset name has invalid format |


#Wolf-1262

Scenario: ODBC backward Compatiblity
	Given I have a workflow "ODBCMigration"
	And "ODBCMigration" contains "ODBCDATA" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      |                  |               | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "ODBCMigration" is executed
	Then the workflow execution has "NO" error


Scenario: Oracle Passing Null Input value
	Given I have a workflow "PassingNullValue"
	And "PassingNullValue" contains "Acceptance Testing Resources/OracleSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullValue" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/OracleSource' in Workflow 'PassingNullValue' debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |


Scenario: Oracle Mapped To Recordsets incorrect
	Given I have a workflow "WillAlwaysError"
	And "WillAlwaysError" contains "Acceptance Testing Resources/OracleSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	And "WillAlwaysError" contains "Acceptance Testing Resources/OracleSource" from server "localhost" with Mapping To as
	| Mapped From | Mapped To               |
	| 1           | [[willalwayserror().1]] |
	When "WillAlwaysError" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/OracleSource' in Workflow 'WillAlwaysError' debug outputs as
	  |                                                                    |
	  | [[willalwayserror()]]: Recordset must contain one or more field(s) |



Scenario: Oracle Parameter not found in the collection
	Given I have a workflow "BadOracleParameterName"
	And "BadOracleParameterName" contains "Testing/OracleParameters" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadOracleParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Testing/OracleParameters' in Workflow 'BadOracleParameterName' debug outputs as
	  |                                                      |
	  | Parameter 'p_startswith' not found in the collection |



Scenario: Oracle Recordset has invalid character
	Given I have a workflow "RenameRecordsetIncorrectly"
	And "RenameRecordsetIncorrectly" contains "Acceptance Testing Resources/OracleSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "RenameRecordsetIncorrectly" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/OracleSource' in Workflow 'RenameRecordsetIncorrectly' debug outputs as
	  |                                                              |
	  | [[getCountrie.s().id]] : Recordset name has invalid format   |
	  | [[getCountrie.s().value]]: Recordset name has invalid format |


#Wolf-1262

Scenario: Oracle backward Compatiblity
	Given I have a workflow "OracleMigration"
	And "OracleMigration" contains "OracleDATA" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      |                  |               | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "OracleMigration" is executed
	Then the workflow execution has "NO" error


Scenario: SQL No Action to be loaded Error
	Given I have a workflow "NoStoredProceedureToLoad"
	And "NoStoredProceedureToLoad" contains "Testing/NoSqlStoredProceedure" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "NoStoredProceedureToLoad" is executed
	Then the workflow execution has "An" error
	And the 'Testing/NoSqlStoredProceedure' in Workflow 'NoStoredProceedureToLoad' debug outputs as
	  |                                                                  |
	  | Error: The selected database does not contain actions to perform |


Scenario: SQL Passing Null Input values
	Given I have a workflow "PassingNullInputValue"
	And "PassingNullInputValue" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullInputValue" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'PassingNullInputValue' debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |


Scenario: SQL Mapped To Recordsets incorrect
	Given I have a workflow "BadSqlParameterName"
	And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     |                            | ProductId | True          |
	And And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with Mapping To as
	| Mapped From | Mapped To                      |
	| Column1     | [[dbo_ImportOrder()..Column1]] |
	When "BadSqlParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'BadSqlParameterName' debug outputs as
	  |                               |
	  | Error: Sql Error: parse error |



#Needs Work
Scenario: Parameter not found in the collection
	Given I have a workflow "BadMySqlParameterName"
	And "BadMySqlParameterName" contains "Testing/MySqlParameters" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadMySqlParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Testing/MySql/MySqlParameters' in Workflow 'BadMySqlParameterName' debug outputs as
	  |                                                      |
	  | Parameter 'p_startswith' not found in the collection |



Scenario: SQL Recordset has invalid character
	Given I have a workflow "MappingHasIncorrectCharacter"
	And "MappingHasIncorrectCharacter" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | 1                          | charValue | True          |
	When "MappingHasIncorrectCharacter" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'MappingHasIncorrectCharacter' debug outputs as
	  |                                                                    |
	  | [[dbo_ConvertTo,Int().result]] : Recordset name has invalid format |
	  


#Wolf-1262
Scenario: Data connector backward Compatiblity
	Given I have a workflow "DataMigration"
	And "DataMigration" contains "DataCon" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      | [[ProductId]]    | productId     | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "DataMigration" is executed
	Then the workflow execution has "NO" error

Scenario: Mappings from nested workflow
	Given I have a workflow "OutterWolf1371"
	And "OutterWolf1371" contains "Wolf-1371" from server "localhost" with mapping as
         | Input to Service | From Variable | Output from Service | To Variable |
         | [[b]]            | b             | a                   | [[a]]       |
	When "OutterWolf1371" is executed
	Then the workflow execution has "NO" error

Scenario: Plugin connector backward Compatiblity
	Given I have a workflow "PluginMigration"
	And "PluginMigration" contains "PluginService" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service      | To Variable          |
      | [[s]]            | s             | [[PrimitiveReturnValue]] | PrimitiveReturnValue |
	When "PluginMigration" is executed
	Then the workflow execution has "NO" error

@ignore
Scenario: workflow without StackOverflow exception check
         Given I have a workflow "Testing - LoopTest"
         And "Testing - LoopTest" contains "LoopTest" from server "localhost" with mapping as
         | Input to Service | From Variable | Output from Service | To Variable      |
         When "Testing - LoopTest" is executed
         Then the workflow execution has "NO" error      

Scenario: Executing WF on a remote server 
         Given I have a workflow "Testing - TestRemoteTools"
         And "Testing - TestRemoteTools" contains "TestRemoteTools" from server "Remote Connection Integration" with mapping as
         | Input to Service | From Variable | Output from Service | To Variable      |
         When "Testing - TestRemoteTools" is executed
         Then the workflow execution has "NO" error     

		 
Scenario: ForEach with NestedStarTest and Inner WF
	  Given I have a workflow "ForEach Output2"
	  And "ForEach Output2" contains "TestInnerWFForEachOutputs" from server "localhost" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "ForEach Output2" is executed
	Then the workflow execution has "NO" error
	And the 'TestInnerWFForEachOutputs' in Workflow 'ForEach Output2' debug outputs as
	  |                      |
	  | [[Result]] = Pass |

#Wolf-1235
Scenario: Workflow with Performance counters
	  Given I have a workflow "PerfCounterTest"
	  And I have reset local perfromance Counters
	  And "PerfCounterTest" contains "PerfCounter" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "PerfCounterTest" is executed
	Then the perfcounter raw values are
	| Name                                              | Value |
	| Average workflow execution time                   | x     |
	| Concurrent requests currently executing           | 0     |
	| Count of Not Authorised errors                    | 0     |
	| Total Errors                                      | 4     |
	| Request Per Second                                | x     |
	| Count of requests for workflows which don’t exist | 9     |

#flickering test
Scenario: Time Zone Changes
	  Given I have a workflow "TimeZoneChangeTest"
	  And "TimeZoneChangeTest" contains "TimeZoneChange" from server "localhost" with mapping as
	| Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "TimeZoneChangeTest" is executed
	Then the workflow execution has "NO" error
	And the 'TimeZoneChange' in Workflow 'TimeZoneChangeTest' debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: Simple workflow executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |	 
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the "WorkflowWithAssign" has a start and end duration
	  And the 'Rec To Convert' in WorkFlow 'WorkflowWithAssign' debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the 'Rec To Convert' in Workflow 'WorkflowWithAssign' debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |

Scenario: Sharepoint Acceptance Tests
	  Given I have a workflow "Sharepoint Acceptance Tests Outer"
	  And "Sharepoint Acceptance Tests Outer" contains "Sharepoint Connectors Testing" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Sharepoint Acceptance Tests Outer" is executed
	Then the workflow execution has "NO" error
	  And the 'Sharepoint Connectors Testing' in Workflow 'Sharepoint Acceptance Tests Outer' debug outputs as
	  |                   |
	  | [[Result]] = Pass |

Scenario: ForEach using * in CSV executed as a sub execution passes out an ordered recordset
	  Given I have a workflow "Spec - Test For Each Shared Memory"
	  And "Spec - Test For Each Shared Memory" contains "Test For Each Shared Memory" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Spec - Test For Each Shared Memory" is executed
	  Then the workflow execution has "NO" error	  
	  And the 'Test For Each Shared Memory' in Workflow 'Spec - Test For Each Shared Memory' debug outputs as
	  |                      |
	  | [[Result]] = Pass |

Scenario: Workflow with AsyncLogging and ForEach
     Given I have a workflow "WFWithAsyncLoggingForEach"
     And "WFWithAsyncLoggingForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "3000"
	 And "ForEachTest" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[Warewolf]] | bob   |
	 When "WFWithAsyncLoggingForEach" is executed
	 Then the workflow execution has "NO" error
	 And I set logging to "Debug"
	 When "WFWithAsyncLoggingForEach" is executed "first time"
	 Then the workflow execution has "NO" error
	 And I set logging to "OFF"
	 	 When "WFWithAsyncLoggingForEach" is executed "second time"
	 Then the workflow execution has "NO" error
	 And the delta between "first time" and "second time" is less than "1200" milliseconds

#show dependacies possibly meant to be coded ui. Leave here until we figure out how to do it in Coded UI
#Wolf-1415
@ignore
Scenario: View Dependancies on a workflow with no dependancies
	Given I have a workflow "Hello World"
	When I select "Show all dependancies" 
	Then the "Dependancies - Hello World" tab is opened
	And "Show what depends on Hello World" is checked by default
	And "Show what Hello World depends on" is visible
	And Nesting Levels equals "0" equals "All levels"
	And "Refresh" is enabled
	And "Hello World" is visible
	And "Hello World" has no dependancies

#show dependacies possibly meant to be coded ui
#Wolf-1415
	@ignore
Scenario: View workflow with multiple dependancies
	Given I have a workflow "11365_WebService"	
	And I select "Show All Dependancies"
	Then the "Dependancies - 11365_WebService" tab is opened
	And "Show what depends on 11365_WebService" is checked by default
	And "Show what Hello World depends on" is visible
	And Nesting Levels equals "0" equals "All levels"
	And Nothing depends on "11365_WebService"
	When I select "Show what 11365_WebService depends on"
	Then "FetchCities" is shown as the first level of dependancy
	And "Dev2GetCountriesWebService" is shown as the second level of dependancy

#show dependacies possibly meant to be coded ui
#Wolf-1415
	@ignore
Scenario: View workflow based on nested levels
	Given I have a workflow "11365_WebService"	
	And I select "Show All Dependancies"
	Then the "Dependancies - 11365_WebService" tab is opened
	And "Show what depends on 11365_WebService" is checked by default
	And "Show what Hello World depends on" is visible
	And Nothing depends on "11365_WebService"
	When I select "Show what 11365_WebService depends on"
	And Nesting Levels "1" equals "First level" only
	Then "FetchCities" is shown as the first level of dependancy
	And "Dev2GetCountriesWebService" is invisible
	And Nesting Levels "2" equals "Second level" 
	Then "FetchCities" is shown as the first level of dependancy
	And "Dev2GetCountriesWebService" is shown as the second level of dependancy

#show dependacies possibly meant to be coded ui
#Wolf-1415
	@ignore
Scenario: Viewing Depenancies
	Given I have a workflow "11365_WebService"	
	And I select "Show All Dependancies"
	Then the "Dependancies - 11365_WebService" tab is opened
	When I select "Show what 11365_WebService depends on"
	And Nesting Levels equals "0" equals "All levels"
	And I double click "Dev2GetCountriesWebService"
	Then the "Edit - Dev2GetCountriesWebService" tab is opened

#studio persist possibly meant to be coded ui
#Wolf-1415
	@ignore
Scenario: Studio persistence 
	Given I  have the Warewolf studio opened
	And an option to "Lock/Unlock" the side menu is visible
	And I "Lock" the side menu
	And I Dock the tool Box window in the right panel in the studio
	When I close and re-open the studio
	Then the studio side menu is "Locked"
	And the Tool Box window is in the right panel in the studio 

#Wolf-1415
	@ignore
Scenario: Rename workflow 
	Given I have a workflow "11365_WebService"	
	When I right-click on "11365_WebService" to view the context menu
	Then I click "Rename"
	And I change "11365_WebService" to "WebService_11365"
	When I double click "WebService_11365"
	Then "WebService_11365" tab is opened 

#Wolf-1415
	@ignore
Scenario: Create a nested folder 
	Given I have a folder "My Category"	
	When I right-click on "My Category" to view the context menu
	Then I click "New Folder"
	And a New Folder is visible in "My Category" in the "Edit" State
	When I name the Folder "My Sub Category"
	Then "My Sub Category" is visible in path "localhost\My Category\My Sub Category" 

#Wolf-1415
	@ignore
Scenario: Move workflow 
	Given I have a workflow "Hello World"
	And "Hello World" is visible on "Localhost"
	And I move "Hello World" to folder "My Category"
	Then "Hello World" is visible as "My Category\Hello World"

#Wolf-1415
	@ignore
Scenario: Delete workflow 
	Given I have a workflow "Test"
	And "Test" is visible on "Localhost"
	When I right-click on "Test" to view the context menu
	And I click "Delete"
	Then "Test" is "Not" visible 



Scenario: Ensure that End this Workflow is working 
	  Given I have a workflow "EndNestedWorkflows"
	  And "EndNestedWorkflows" contains "Testing/Bugs/wolf-402" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "EndNestedWorkflows" is executed
	Then the workflow execution has "NO" error

Scenario: Xml Serialisation bug when returning xml
	Given I have a workflow "XmlSerialisation"
	And "XmlSerialisation" contains "Testing/Bugs/wolf-829" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "XmlSerialisation" is executed
	Then the workflow execution has "NO" error

#Wolf-860

Scenario: Mixing Scalar And Recordset bug 
	Given I have a workflow "MixingScalarAndRecordset"
	And "MixingScalarAndRecordset" contains "Testing/Bugs/Wolf-860" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "MixingScalarAndRecordset" is executed
	Then the workflow execution has "NO" error

Scenario: ForEach using * and web get request with error
	  Given I have a workflow "Spec - Test For Each  Get"
	  And "Spec - Test For Each  Get" contains "GetRequestErrorHandling" from server "localhost" with mapping as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  When "Spec - Test For Each  Get" is executed
	  Then the workflow execution has "AN" error	  
	  And the 'GetRequestErrorHandling' in Workflow 'Spec - Test For Each  Get' debug outputs as
	  |                   |
	  | [[Result]] = Pass |


Scenario: Error not bubbling up
	Given I have a workflow "Wolf-1212_Test"
	And "Wolf-1212_Test" contains "ErrorHandled" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	  |                  |               | Error              | [[Error]]  |
	When "Wolf-1212_Test" is executed
	Then the workflow execution has "NO" error
	And the 'ErrorHandled' in Workflow 'Wolf-1212_Test' debug outputs as
	  |                                                                                                                                     |
	  | [[Result]] = Fail                                                                                                                   |
	  | [[Error]] = Could not parse input datetime with given input format (even after trying default datetime formats from other cultures) |


Scenario: Error not bubbling up error message
	Given I have a workflow "Wolf-1212_2"
	And "Wolf-1212_2" contains "ErrorBubbleUp" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service | To Variable |
	  |                  |               | Result              | [[Result]]  |
	When "Wolf-1212_2" is executed
	Then the workflow execution has "NO" error
	And the 'ErrorBubbleUp' in Workflow 'Wolf-1212_2' debug outputs as
	  |                   |
	  | [[Result]] = Pass |
