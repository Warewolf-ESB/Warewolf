﻿@AssignWorkflowExecution
Feature: AssignWorkflowExecution
	In order to execute a workflow that contains an assign
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
			Given Debug events are reset
			And Debug states are cleared

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
	  And the "Assign1" in WorkFlow "WorkflowWithAssignBaseConvertandCaseconvert" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  | 2 | [[rec().a]] = | test      |
	  | 3 | [[rec().a]] = | 100       |
	   And the "Assign1" in Workflow "WorkflowWithAssignBaseConvertandCaseconvert" debug outputs as  
	  | # |                      |
	  | 1 | [[rec(1).a]] =  50   |
	  | 2 | [[rec(2).a]] =  test |
	  | 3 | [[rec(3).a]] =  100  |
	  And the "Case to Convert" in WorkFlow "WorkflowWithAssignBaseConvertandCaseconvert" debug inputs as
	  | # | Convert             | To    |
	  | 1 | [[rec(2).a]] = test | UPPER |
	  And the "Case to Convert" in Workflow "WorkflowWithAssignBaseConvertandCaseconvert" debug outputs as  
	  | # |                     |
	  | 1 | [[rec(2).a]] = TEST |
	  And the "Base to Convert" in WorkFlow "WorkflowWithAssignBaseConvertandCaseconvert" debug inputs as
	  | # | Convert           | From | To     |
	  | 1 | [[rec(1).a]] = 50 | Text | Base 64 |
      And the "Base to Convert" in Workflow "WorkflowWithAssignBaseConvertandCaseconvert" debug outputs as  
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
	  And the "Assign to delete" in WorkFlow "WorkflowWithAssignand2Deletetools" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the "Assign to delete" in Workflow "WorkflowWithAssignand2Deletetools" debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the "Delet1" in WorkFlow "WorkflowWithAssignand2Deletetools" debug inputs as
	  | Records          |
	  | [[rec(1).a]] = 50 |
	  And the "Delet1" in Workflow "WorkflowWithAssignand2Deletetools" debug outputs as  
	  |                       |
	  | [[result1]] = Success |
	  And the "Delet2" in WorkFlow "WorkflowWithAssignand2Deletetools" debug inputs as
	   | Records      |
	   | [[rec(1)]] = |	  
	  And the "Delet2" in Workflow "WorkflowWithAssignand2Deletetools" debug outputs as  
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
	  And the "Assigntool1" in WorkFlow "WorkflowWith3Assigntools" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | rec(1).a  |
	  And the "Assigntool1" in Workflow "WorkflowWith3Assigntools" debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = rec(1).a |
	  And the "Assigntool2" in WorkFlow "WorkflowWith3Assigntools" debug inputs as
	  | # | Variable                | New Value |
	  | 1 | [[test]] =              | rec(1).a  |
	  | 2 | [[rec(1).a]] = rec(1).a | Warewolf  |
	  And the "Assigntool2" in Workflow "WorkflowWith3Assigntools" debug outputs as  
	  | # |                         |
	  | 1 | [[test]] = rec(1).a     |
	  | 2 | [[rec(1).a]] = Warewolf |
	   And the "Assigntool3" in WorkFlow "WorkflowWith3Assigntools" debug inputs as
	  | # | Variable  | New Value               |
	  | 1 | [[new]] = | [[rec(1).a]] = Warewolf |
	  And the "Assigntool3" in Workflow "WorkflowWith3Assigntools" debug outputs as  
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
	  And the "InputDates" in WorkFlow "WorkflowWithAssignAndDateTimeDifferencetools1" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | 2014      |
	  | 2 | [[b]] =  | 10.       |
	  And the "InputDates" in Workflow "WorkflowWithAssignAndDateTimeDifferencetools1" debug outputs as  
	  | # |              |
	  | 1 | [[a]] = 2014 |
	  | 2 | [[b]] = 10.0 |
	  And the "DateAndTime" in WorkFlow "WorkflowWithAssignAndDateTimeDifferencetools1" debug inputs as
	  | Input 1       | Input 2    | Input Format | Output In |
	  | 2020/[[b]]/01 = 2020/10.0/01 | 2030/01/01 | yyyy/mm/dd   | Years     |
	  And the "DateAndTime" in Workflow "WorkflowWithAssignAndDateTimeDifferencetools1" debug outputs as 
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
	  And the "Assign To merge" in WorkFlow "WorkflowWithAssignDataMergeAndDataSplittools" debug inputs as 
	  | # | Variable        | New Value |
	  | 1 | [[a]] =         | Test      |
	  | 2 | [[b]] =         | Warewolf  |
	  | 3 | [[split().a]] = | Workflow  |
	 And the "Assign To merge" in Workflow "WorkflowWithAssignDataMergeAndDataSplittools" debug outputs as   
	  | # |                           |
	  | 1 | [[a]]         =  Test     |
	  | 2 | [[b]]         =  Warewolf |
	  | 3 | [[split(1).a]] =  Workflow |
	  And the "Data Merge" in WorkFlow "WorkflowWithAssignDataMergeAndDataSplittools" debug inputs as 
	  | # |                   | With  | Using | Pad | Align |
	  | 1 | [[a]] = Test     | Index | "4"   | ""  | Left  |
	  | 2 | [[b]] = Warewolf | Index | "8"   | ""  | Left  |
	  And the "Data Merge" in Workflow "WorkflowWithAssignDataMergeAndDataSplittools" debug outputs as  
	  |                           |
	  | [[result]] = TestWarewolf |
	  And the "Data Split" in WorkFlow "WorkflowWithAssignDataMergeAndDataSplittools" debug inputs as 
	  | String to Split                                | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[result]][[split().a]] = TestWarewolfWorkflow | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  |                                                |                   |                 | 2 | [[rec().b]] = | Index | 8     | No      |        |
	  And the "Data Split" in Workflow "WorkflowWithAssignDataMergeAndDataSplittools" debug outputs as  
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
	  And the "splitvalues1" in WorkFlow "WorkflowWithAssignandDataSplittools" debug inputs as 
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | 2         |
	  | 3 | [[rs(1).a]] = | test      |
	 And the "splitvalues1" in Workflow "WorkflowWithAssignandDataSplittools" debug outputs as   
	  | # |                       |
	  | 1 | [[a]]         =  b    |
	  | 2 | [[b]]         =  2    |
	  | 3 | [[rs(1).a]]   =  test |
	 And the "splitvalues2" in WorkFlow "WorkflowWithAssignandDataSplittools" debug inputs as 
	  | # | Variable   | New Value |
	  | 1 | [[test]] = | warewolf  | 
	 And the "splitvalues2" in Workflow "WorkflowWithAssignandDataSplittools" debug outputs as   
	  | # |                      |
	  | 1 | [[test]] =  warewolf |
	  And the "DataSpliting" in WorkFlow "WorkflowWithAssignandDataSplittools" debug inputs as 
	  | String to Split     | Process Direction | Skip blank rows | # |                | With  | Using     | Include | Escape |
	  | [[test]] = warewolf | Forward           | No              | 1 | [[rec(1).a]] = | Index | [[b]] = 2 | No      |        |
	  And the "DataSpliting" in Workflow "WorkflowWithAssignandDataSplittools" debug outputs as  
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
	  And the "Assign for sequence" in WorkFlow "workflowithAssignandsequence" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().b]] = | nothing   |
	  | 3 | [[rec().a]] = | warewolf  |
	  | 4 | [[rec().b]] = | nothing   |
	   And the "Assign for sequence" in Workflow "workflowithAssignandsequence" debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = test     |
	  | 2 | [[rec(1).b]] = nothing  |
	  | 3 | [[rec(2).a]] = warewolf |
	  | 4 | [[rec(2).b]] = nothing  |
	  And the "Data Merge" in WorkFlow "Test1" debug inputs as
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = test     | Index | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = warewolf | Index | "8"   | ""  | Left  |
	  And the "Data Merge" in Workflow "Test1" debug outputs as
	  |                           |
	  | [[result]] = testwarewolf |
	  And the "Data Split" in WorkFlow "Test1" debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).b]] = nothing | Index | 4     | No      |        |
	  |                 |                   |                 | 2 | [[rec(2).b]] = nothing | Index | 8     | No      |        |
	  And the "Data Split" in Workflow "Test1" debug outputs as
	  | # |                         |
	  | 1 | [[rec(1).b]] = test     |
	  | 2 | [[rec(2).b]] = warewolf |
      And the "Index" in WorkFlow "Test1" debug inputs as
	  | In Field                | Index           | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurence | e          | Left to Right |
	  And the "Index" in Workflow "Test1" debug outputs as
	  |                     |
	  | [[indexResult]] = 4 |
	  And the "Replacing" in WorkFlow "Test1" debug inputs as 
	  | In Field(s)             | Find | Replace With |
	  | [[rec(1).a]] = test     |      |              |
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the "Replacing" in Workflow "Test1" debug outputs as
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
	  And the "Assign to create" in WorkFlow "WorkflowWithAssignCreateandDeleteRecord" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | C:\copied00.txt       |
	  And the "Assign to create" in Workflow "WorkflowWithAssignCreateandDeleteRecord" debug outputs as     
	  | # |                         |
	  | 1 | [[rec(1).a]] = C:\copied00.txt      |
	 And the "Create1" in WorkFlow "WorkflowWithAssignCreateandDeleteRecord" debug inputs as
	  | File or Folder                | Overwrite | Username | Password |
	  | [[rec(1).a]] = C:\copied00.txt | True      | ""       | ""       |  
	   And the "Create1" in Workflow "WorkflowWithAssignCreateandDeleteRecord" debug outputs as    
	   |                    |
	   | [[res1]] = Success |
	  And the "DeleteFolder" in WorkFlow "WorkflowWithAssignCreateandDeleteRecord" debug inputs as
	  | Input Path                    | Username | Password |
	  | [[rec(1).a]] = C:\copied00.txt | ""       | ""       |
	  And the "DeleteFolder" in Workflow "WorkflowWithAssignCreateandDeleteRecord" debug outputs as    
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
	  And the "Assign to create" in WorkFlow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug inputs as
	  | # | Variable      | New Value   |
	  | 1 | [[rec().a]] = | create.txt |
	  And the "Assign to create" in Workflow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug outputs as     
	  | # |                            |
	  | 1 | [[rec(1).a]] = create.txt |
	  And the "Create1" in WorkFlow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug inputs as
	  | File or Folder            | Overwrite | Username | Password |
	  | [[rec(1).a]] = create.txt | True      | " "      | " "      |
	  And the "Create1" in Workflow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug outputs as    
	   |                    |
	   | [[res1]] = Failure  |
	  And the "Delete" in WorkFlow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug inputs as
	  | Input Path                | Username   | Password   |
	  | [[rec(1).a]] = create.txt | " " | " " |
	  And the "Delete" in Workflow "WorkflowWithAssignCreateDeleteRecordNoneExist1" debug outputs as    
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
	  And the "tool1" in WorkFlow "WorkflowWith2Assigntools" debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[a]] =    | b         |
	  | 2 | [[b]] =    | test      |
	  | 3 | [[test]] = | warewolf  |
	  And the "tool1" in Workflow "WorkflowWith2Assigntools" debug outputs as  
	  | # |                     |
	  | 1 | [[a]] = b           |
	  | 2 | [[b]] = test        |
	  | 3 | [[test]] = warewolf |
	  And the "tool2" in WorkFlow "WorkflowWith2Assigntools" debug inputs as
	  | # | Variable         | New Value                |
	  | 1 | [[b]] = test | [[test]] = warewolf |

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
	  And the "rec1" in WorkFlow "WorkflowWith2Assigntoolswithrecordsets" debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec().a]] =  | rec(2).a  |
	  | 2 | [[rec(2).a]] = | test      |
	  And the "rec1" in Workflow "WorkflowWith2Assigntoolswithrecordsets" debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = rec(2).a |
	  | 2 | [[rec(2).a]] = test     |
	  And the "rec2" in WorkFlow "WorkflowWith2Assigntoolswithrecordsets" debug inputs as
	  | # | Variable            | New Value |
	  | 1 | [[rec(2).a]] = test | warewolf  |
	  And the "rec2" in Workflow "WorkflowWith2Assigntoolswithrecordsets" debug outputs as  
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
	  And the "scl1" in WorkFlow "WorkflowWith2Assigntoolswithrscalars" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the "scl1" in Workflow "WorkflowWith2Assigntoolswithrscalars" debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the "scl2" in WorkFlow "WorkflowWith2Assigntoolswithrscalars" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[b]] =  | warewolf  |
	  And the "scl2" in Workflow "WorkflowWith2Assigntoolswithrscalars" debug outputs as  
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
	  And the "countrecordval1" in WorkFlow "WorkflowWithAssignCountDataMerge&2Delete" debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec().a]] = | 21        |
	  | 2 | [[rec().a]] = | 22        |
	  | 3 | [[rec().a]] = | ""        |
	  And the "countrecordval1" in Workflow "WorkflowWithAssignCountDataMerge&2Delete" debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 21 |
	  | 2 | [[rec(2).a]] = 22 |
	  | 3 | [[rec(3).a]] =    |
	  And the "Cnt1" in WorkFlow "WorkflowWithAssignCountDataMerge&2Delete" debug inputs as 
	  | Recordset         |
	  | [[rec(1).a]] = 21 |
	  | [[rec(2).a]] = 22 |
	  | [[rec(3).a]] =    |
	  And the "Cnt1" in Workflow "WorkflowWithAssignCountDataMerge&2Delete" debug outputs as 
	  |                 |
	  | [[result1]] = 3 |
	  And the "Delrec" in WorkFlow "WorkflowWithAssignCountDataMerge&2Delete" debug inputs as
	  | Records        |
	  | [[rec(3).a]] = |
	  And the "Delrec" in Workflow "WorkflowWithAssignCountDataMerge&2Delete" debug outputs as  
	  |                       |
	  | [[result2]] = Success |	
	  And the "DataMerge1" in WorkFlow "WorkflowWithAssignCountDataMerge&2Delete" debug inputs as
	  | # |                   | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 21 | Index | "2"   | ""  | Left  |
	  | 2 | [[rec(2).a]] = 22 | Index | "2"   | ""  | Left  |
	  And the "DataMerge1" in Workflow "WorkflowWithAssignCountDataMerge&2Delete" debug outputs as 
	  |                     |
	  | [[rec(3).a]] = 2122 |
	   And the "Cnt2" in WorkFlow "WorkflowWithAssignCountDataMerge&2Delete" debug inputs as 
	  | Recordset           |
	  | [[rec(1).a]] = 21   |
	  | [[rec(2).a]] = 22   |
	  | [[rec(3).a]] = 2122 |
	  And the "Cnt2" in Workflow "WorkflowWithAssignCountDataMerge&2Delete" debug outputs as 
	  |                 |
	  | [[result3]] = 3 |

Scenario: Workflow with multiple tools Assign and SQL Bulk Insert executing against the server
	  Given I have a workflow "WorkflowWithAssignAndSQLBulkInsert"
	  And "WorkflowWithAssignAndSQLBulkInsert" contains an Assign "InsertData" as
	  | variable    | value    |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowWithAssignAndSQLBulkInsert" contains an SQL Bulk Insert "BulkInsert" using database "NewSqlServerSource" and table "dbo.MailingList" and KeepIdentity set "false" and Result set "[[result]]" as
	  | Column | Mapping             | IsNullable | DataTypeName | MaxLength | IsAutoIncrement |
	  | Id     |                     | false      | int          |           | true            |
	  | Name   | [[rec().a]]         | false      | varchar      | 50        | false           |
	  | Email  | Warewolf@dev2.co.za | false      | varchar      | 50        | false           |
	  When "WorkflowWithAssignAndSQLBulkInsert" is executed
	  Then the workflow execution has "NO" error
	  And the "InsertData" in WorkFlow "WorkflowWithAssignAndSQLBulkInsert" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Warewolf  |
	  And the "InsertData" in Workflow "WorkflowWithAssignAndSQLBulkInsert" debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = Warewolf |
	  And the "BulkInsert" in WorkFlow "WorkflowWithAssignAndSQLBulkInsert" debug inputs as
	  | # |                         | To Field | Type         | Batch Size | Timeout | Check Constraints | Keep Table Lock | Fire Triggers | Keep Identity | Use Internal Transaction | Skip Blank Rows |
	  | 1 | [[rec(1).a]] = Warewolf | Name     | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  | 2 | Warewolf@dev2.co.za     | Email    | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  |   |                         |          |              | 0          | 0       | NO                | NO              | NO            | NO            | NO                       | YES             |
	  And the "BulkInsert" in Workflow "WorkflowWithAssignAndSQLBulkInsert" debug outputs as
	  |                      |
	  | [[result]] = Success |

Scenario: Workflow with multiple tools Assign and SQL Bulk Insert with negative Recordset Index executing against the server
	  Given I have a workflow "WorkflowWithAssignAndSQLBulk"
	  And "WorkflowWithAssignAndSQLBulk" contains an Assign "InsertData" as
	  | variable    | value |
	  | [[rec().a]] | Warewolf     |
	  And "WorkflowWithAssignAndSQLBulk" contains an SQL Bulk Insert "BulkInsert" using database "NewSqlServerSource" and table "dbo.MailingList" and KeepIdentity set "false" and Result set "[[result]]" as
	  | Column | Mapping             | IsNullable | DataTypeName | MaxLength | IsAutoIncrement |
	  | Id     |                     | false      | int          |           | true            |
	  | Name   | [[rec(-1).a]]       | false      | varchar      | 50        | false           |
	  | Email  | Warewolf@dev2.co.za | false      | varchar      | 50        | false           |
	  When "WorkflowWithAssignAndSQLBulk" is executed
	  Then the workflow execution has "AN" error
	  And the "InsertData" in WorkFlow "WorkflowWithAssignAndSQLBulk" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Warewolf  |
	  And the "InsertData" in Workflow "WorkflowWithAssignAndSQLBulk" debug outputs as  
	  | # |                         |
	  | 1 | [[rec(1).a]] = Warewolf |
	  And the "BulkInsert" in WorkFlow "WorkflowWithAssignAndSQLBulk" debug inputs as
	  | # |                     | To Field | Type         | Batch Size | Timeout | Check Constraints | Keep Table Lock | Fire Triggers | Keep Identity | Use Internal Transaction | Skip Blank Rows |
	  | 1 | [[rec(-1).a]] =     | Name     | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  | 2 | Warewolf@dev2.co.za | Email    | varchar (50) |            |         |                   |                 |               |               |                          |                 |
	  And the "BulkInsert" in Workflow "WorkflowWithAssignAndSQLBulk" debug outputs as
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
	  And the "Base Var" in WorkFlow "WorkflowWithAssignandBase" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12        |
	  And the "Base Var" in Workflow "WorkflowWithAssignandBase" debug outputs as  
	  | # |            |
	  | 1 | [[a]] = b  |
	  | 2 | [[b]] = 12 |
	   And the "Base" in WorkFlow "WorkflowWithAssignandBase" debug inputs as
	  | # | Convert        | From | To      |
	  | 1 | [[b]] = 12 | Text | Base 64 |
      And the "Base" in Workflow "WorkflowWithAssignandBase" debug outputs as  
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
	  And the "BaseVar" in WorkFlow "WorkflowWithAssignandBasec" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | rec(1).a  |
	  | 2 | [[rec().a]] = | 12        |
	  And the "BaseVar" in Workflow "WorkflowWithAssignandBasec" debug outputs as  
	  | # |                        |
	  | 1 | [[rs(1).a]] = rec(1).a |
	  | 2 | [[rec(1).a]] = 12      |
	   And the "Base" in WorkFlow "WorkflowWithAssignandBasec" debug inputs as
	  | # | Convert              | From | To      |
	  | 1 | [[rec(1).a]] = 12 | Text | Base 64 |
      And the "Base" in Workflow "WorkflowWithAssignandBasec" debug outputs as  
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
	  And the "Case Var" in WorkFlow "WorkflowWithAssignandcCse" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | warewolf  |
	  And the "Case Var" in Workflow "WorkflowWithAssignandcCse" debug outputs as  
	  | # |                  |
	  | 1 | [[a]] = b        |
	  | 2 | [[b]] = warewolf |
	 And the "CaseConvert" in WorkFlow "WorkflowWithAssignandcCse" debug inputs as
	  | # | Convert              | To    |
	  | 1 | [[b]] = warewolf | UPPER |
	  And the "CaseConvert" in Workflow "WorkflowWithAssignandcCse" debug outputs as  
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
	  And the "Case Var" in WorkFlow "WorkflowWithAssignandcCase" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | rec(1).a  |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Case Var" in Workflow "WorkflowWithAssignandcCase" debug outputs as  
	  | # |                         |
	  | 1 | [[rs(1).a]] = rec(1).a  |
	  | 2 | [[rec(1).a]] = warewolf |
	 And the "CaseConvert" in WorkFlow "WorkflowWithAssignandcCase" debug inputs as
	  | # | Convert                    | To    |
	  | 1 | [[rec(1).a]] = warewolf | UPPER |
	  And the "CaseConvert" in Workflow "WorkflowWithAssignandcCase" debug outputs as  
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
	 And the "Datam" in WorkFlow "WorkflowWithAssignandData" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | warewolf  |
	  | 3 | [[rs().a]] =  | rec(1).a  |
	  | 4 | [[rec().a]] = | test      |
	 And the "Datam" in Workflow "WorkflowWithAssignandData" debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = b              |
	  | 2 | [[b]] = warewolf       |
	  | 3 | [[rs(1).a]] = rec(1).a |
	  | 4 | [[rec(1).a]] = test    |
	 And the "Datamerge" in WorkFlow "WorkflowWithAssignandData" debug inputs as
	  | # |                     | With  | Using | Pad | Align |
	  | 1 | [[b]] = warewolf    | Index | "8"   | ""  | Left  |
	  | 2 | [[rec(1).a]] = test | Index | "4"   | ""  | Left  |
	  And the "Datamerge" in Workflow "WorkflowWithAssignandData" debug outputs as  
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
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignandFindIndex" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the "IndexVal" in Workflow "WorkflowWithAssignandFindIndex" debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	   And the "Indexchar" in WorkFlow "WorkflowWithAssignandFindIndex" debug inputs as 	
	  | In Field         | Index           | Characters | Direction     |
	  | [[b]] = test | First Occurence | s          | Left to Right |
	  And the "Indexchar" in Workflow "WorkflowWithAssignandFindIndex" debug outputs as 
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
	  And the "Index Val" in WorkFlow "WorkflowWithAssignandFindIndex1" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | test      |
	  And the "Index Val" in Workflow "WorkflowWithAssignandFindIndex1" debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = test    |
	   And the "Index char" in WorkFlow "WorkflowWithAssignandFindIndex1" debug inputs as 	
	  | In Field               | Index           | Characters | Direction     |
	  | [[new(1).a]] = test | First Occurence | s          | Left to Right |
	  And the "Index char" in Workflow "WorkflowWithAssignandFindIndex1" debug outputs as 
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
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignandReplace" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the "IndexVal" in Workflow "WorkflowWithAssignandReplace" debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the "Replac" in WorkFlow "WorkflowWithAssignandReplace" debug inputs as 	
	 | In Field(s)         | Find | Replace With |
	 | [[b]] = test | s    | REPLACE      |
	    And the "Replac" in Workflow "WorkflowWithAssignandReplace" debug outputs as 
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
	  And the "Vals" in WorkFlow "WorkflowWithAssignandReplacebyrec" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | test      |
	  And the "Vals" in Workflow "WorkflowWithAssignandReplacebyrec" debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = test    |
	  And the "Rep" in WorkFlow "WorkflowWithAssignandReplacebyrec" debug inputs as 	
	  | In Field(s)             | Find | Replace With |
	  | [[new(1).a]] = test | s    | REPLACE      |
	    And the "Rep" in Workflow "WorkflowWithAssignandReplacebyrec" debug outputs as 
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
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignandFormat" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12.3412   |
	  And the "IndexVal" in Workflow "WorkflowWithAssignandFormat" debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  | 2 | [[b]] = 12.3412 |
	  And the "Fnumber" in WorkFlow "WorkflowWithAssignandFormat" debug inputs as 	
	  | Number              | Rounding | Rounding Value | Decimals to show |
	  | [[b]] = 12.3412 | Up       | 3              | 3                |
	  And the "Fnumber" in Workflow "WorkflowWithAssignandFormat" debug outputs as 
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
	  And the "IndVal" in WorkFlow "WorkflowWithAssignandFormatn" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | new().a   |
	  | 2 | [[new().a]] = | 12.3412   |
	  And the "IndVal" in Workflow "WorkflowWithAssignandFormatn" debug outputs as  
	  | # |                        |
	  | 1 | [[rec(1).a]] = new().a |
	  | 2 | [[new(1).a]] = 12.3412 |
	  And the "Fnumb" in WorkFlow "WorkflowWithAssignandFormatn" debug inputs as 	
	  | Number                    | Rounding | Rounding Value | Decimals to show |
	  | [[new(1).a]] = 12.3412 | Up       | 3              | 3                |
	  And the "Fnumb" in Workflow "WorkflowWithAssignandFormatn" debug outputs as 
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
	  And the "Valforrandno" in WorkFlow "WorkflowWithAssignandRandom" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | b         |
	  | 2 | [[b]] =       | 10        |
	  | 3 | [[rec().a]] = | new().a   |
	  | 4 | [[new().a]] = | 20        |
	  And the "Valforrandno" in Workflow "WorkflowWithAssignandRandom" debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = b              |
	  | 2 | [[b]] = 10             |
	  | 3 | [[rec(1).a]] = new().a |
	  | 4 | [[new(1).a]] = 20      |
	  And the "Rand" in WorkFlow "WorkflowWithAssignandRandom" debug inputs as 
	  | Random  | From           | To                   |
	  | Numbers | [[b]] = 10 | [[new(1).a]] = 20 |
	  And the "Rand" in Workflow "WorkflowWithAssignandRandom" debug outputs as
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
	  And the "Dateandtime" in WorkFlow "WorkflowWithAssignandDateTimetool" debug inputs as
	  | # | Variable      | New Value  |
	  | 1 | [[a]] =       | b          |
	  | 2 | [[b]] =       | 01/02/2014 |
	  | 3 | [[rec().a]] = | new().a    |
	  | 4 | [[new().a]] = |  dd/mm/yyyy|
	   And the "Dateandtime" in Workflow "WorkflowWithAssignandDateTimetool" debug outputs as  
	   | # |                            |
	   | 1 | [[a]] = b                  |
	   | 2 | [[b]] = 01/02/2014         |
	   | 3 | [[rec(1).a]] = new().a     |
	   | 4 | [[new(1).a]] =  dd/mm/yyyy |
	   And the "AddDate" in WorkFlow "WorkflowWithAssignandDateTimetool" debug inputs as
	   | Input                  | Input Format                  | Add Time |   | Output Format |
	   | [[b]] = 01/02/2014 | [[new(1).a]] = dd/mm/yyyy | Years    | 1 | dd/mm/yyyy    |	
	   And the "AddDate" in Workflow "WorkflowWithAssignandDateTimetool" debug outputs as   
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
	   And the "Dateandtime" in WorkFlow "WorkflowWithAssignandDateTimeDiff" debug inputs as
	   | # | Variable      | New Value  |
	   | 1 | [[a]] =       | b          |
	   | 2 | [[b]] =       | 01/02/2016 |
	   | 3 | [[rec().a]] = | new().a    |
	   | 4 | [[new().a]] = | 01/02/2014 |
	   And the "Dateandtime" in Workflow "WorkflowWithAssignandDateTimeDiff" debug outputs as  
	   | # |                           |
	   | 1 | [[a]] = b                 |
	   | 2 | [[b]] = 01/02/2016        |
	   | 3 | [[rec(1).a]] = new().a    |
	   | 4 | [[new(1).a]] = 01/02/2014 |
	   And the "DateTimedif" in WorkFlow "WorkflowWithAssignandDateTimeDiff" debug inputs as
	   | Input 1                   | Input 2            | Input Format | Output In |
	   | [[new(1).a]] = 01/02/2014 | [[b]] = 01/02/2016 | dd/mm/yyyy   | Years     |
	   And the "DateTimedif" in Workflow "WorkflowWithAssignandDateTimeDiff" debug outputs as   
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
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignReplace" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | test      |
	  And the "IndexVal" in Workflow "WorkflowWithAssignReplace" debug outputs as  
	  | # |              |
	  | 1 | [[a]] = b    |
	  | 2 | [[b]] = test |
	  And the "Replac" in WorkFlow "WorkflowWithAssignReplace" debug inputs as 	
	 | In Field(s)      | Find | Replace With |
	 | [[b]] = test | s    | REPLACE      |
	    And the "Replac" in Workflow "WorkflowWithAssignReplace" debug outputs as 
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
	  And the "IndexVal1" in WorkFlow "WorkflowAssignandFormat" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  | 2 | [[b]] =  | 12.3412   |
	  And the "IndexVal1" in Workflow "WorkflowAssignandFormat" debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  | 2 | [[b]] = 12.3412 |
	  And the "Fnumber1" in WorkFlow "WorkflowAssignandFormat" debug inputs as 	
	  | Number              | Rounding | Rounding Value | Decimals to show |
	  | [[b]] = 12.3412 | Up       | 3              | 3                |
	  And the "Fnumber1" in Workflow "WorkflowAssignandFormat" debug outputs as 
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
	  And the "Data" in WorkFlow "WorkflowWithAssignDatamergeandSplit" debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[a]] =        | 1         |
	  | 2 | [[b]] =        | 2         |
	  | 3 | [[rec(1).a]] = | warewolf  |
	  | 4 | [[rec(2).a]] = | test      |
	  And the "Data" in Workflow "WorkflowWithAssignDatamergeandSplit" debug outputs as 
	  | # |                         |
	  | 1 | [[a]] = 1               |
	  | 2 | [[b]] = 2               |
	  | 3 | [[rec(1).a]] = warewolf |
	  | 4 | [[rec(2).a]] = test     |  	
      And the "Merge" in WorkFlow "WorkflowWithAssignDatamergeandSplit" debug inputs as
	  | # |                         | With  | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = warewolf | Index | "8"   | ""  | Left  |
	  | 2 | [[a]] = 1               | Index | "4"   | ""  | Left  |
	  And the "Merge" in Workflow "WorkflowWithAssignDatamergeandSplit" debug outputs as
	  |                        |
	  | [[result]] = warewolf1 |
	  And the "DataSplit" in WorkFlow "WorkflowWithAssignDatamergeandSplit" debug inputs as  
	  | String to Split         | Process Direction | Skip blank rows | # |         | With  | Using | Include | Escape |
	  | [[rec(1).a]] = warewolf | Forward           | No              | 1 | [[d]] = | Index | 4     | No      |        |
	  |                         |                   |                 | 2 | [[c]] = | Index | 4     | No      |        |
	  And the "DataSplit" in Workflow "WorkflowWithAssignDatamergeandSplit" debug outputs as
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
	 And the "Data" in WorkFlow "WorkflowWithAssignMergeandSplit" debug inputs as
	 | # | Variable         | New Value |
	 | 1 | [[a]] =          | 1         |
	 | 2 | [[b]] =          | 2         |
	 | 3 | [[rec(1).a]] =   | warewolf  |
	 | 4 | [[rec(2).a]] =   | test      |
	 | 5 | [[index(1).a]] = | 1         |
	 | 6 | [[index(2).a]] = | 3         |
	 And the "Data" in Workflow "WorkflowWithAssignMergeandSplit" debug outputs as 
	 | # |                         |
	 | 1 | [[a]] = 1               |
	 | 2 | [[b]] = 2               |
	 | 3 | [[rec(1).a]] = warewolf |
	 | 4 | [[rec(2).a]] = test     |
	 | 5 | [[index(1).a]] = 1      |
	 | 6 | [[index(2).a]] = 3      |  	
    And the "Merge" in WorkFlow "WorkflowWithAssignMergeandSplit" debug inputs as
	 | # |                                      | With  | Using | Pad | Align |
	 | 1 | [[rec(1).a]] = warewolf | Index | "8"   | ""  | Left  |
	 | 2 | [[a]] = 1                            | Index | "4"   | ""  | Left  |
	 And the "Merge" in Workflow "WorkflowWithAssignMergeandSplit" debug outputs as
	 |                        |
	 | [[result]] = warewolf1 |
	 And the "DataSplit" in WorkFlow "WorkflowWithAssignMergeandSplit" debug inputs as  
	 | String to Split         | Process Direction | Skip blank rows | # |                | With  | Using | Include | Escape |
	 | [[rec(1).a]] = warewolf | Forward           | No              | 1 | [[d]] =        | Index | 4     | No      |        |
	 |                         |                   |                 | 2 | [[rec(3).a]] = | Index | 4     | No      |        |
	  And the "DataSplit" in Workflow "WorkflowWithAssignMergeandSplit" debug outputs as
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
	  And the "Data1" in WorkFlow "WorkflowWithAssignandAssign" debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | 2         |
	  | 3 | [[index(1).a]] = | 2         |
	  And the "Data1" in Workflow "WorkflowWithAssignandAssign" debug outputs as 
	  | # |                        |
	  | 1 | [[a]] = 1              |
	  | 2 | [[rec(1).a]] = 2   |
	  | 3 | [[index(1).a]] = 2 |
	   And the "Data2" in WorkFlow "WorkflowWithAssignandAssign" debug inputs as
	  | # | Variable                  | New Value |
	  | 1 | [[new(1).a]] =        | test      |
	  | 2 | [[rec(2).a]] = | warewolf  |
	  And the "Data2" in Workflow "WorkflowWithAssignandAssign" debug outputs as 
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
	  And the "values1" in WorkFlow "WFWithAssignHasCalculate" debug inputs as 
	  | # | Variable       | New Value                       |
	  | 1 | [[a]] =        | 1                               |
	  | 2 | [[b]] =        | 2                               |
	  | 3 | [[rec(1).a]] = | [[a]] = 1                       |
	  | 4 | [[rec(1).b]] = | [[b]] = 2                       |
	  | 5 | [[rec(1).c]] = | [[rec(1).a]]+[[rec(1).b]] = 1+2 |
	  And the "values1" in Workflow "WFWithAssignHasCalculate" debug outputs as   
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
	  And the "values1" in WorkFlow "WFWithAssignCalculateindexrecordset" debug inputs as 
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | 2         |
	  | 3 | [[index(1).a]] = | 1         |
	  | 4 | [[rec(2).a]] =   | 6         |
	 And the "values1" in Workflow "WFWithAssignCalculateindexrecordset" debug outputs as   
	  | # |                    |
	  | 1 | [[a]]         =  1 |
	  | 2 | [[rec(1).a]]  =  2 |
	  | 3 | [[index(1).a]] = 1 |
	  | 4 | [[rec(2).a]]   = 6 |
	  And the "Calculate1" in WorkFlow "WFWithAssignCalculateindexrecordset" debug inputs as 
      | fx =                                  |
      | [[rec([[index(1).a]]).a]]+[[a]] = 2+1 |       
      And the "Calculate1" in Workflow "WFWithAssignCalculateindexrecordset" debug outputs as  
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
	  And the "values1" in WorkFlow "WFAssignCalculateRecursion" debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[b]] =        | rec(1).b  |
	  | 2 | [[rec(1).a]] = | b         |
	  | 3 | [[rec(1).b]] = | 1         |
	 And the "values1" in Workflow "WFAssignCalculateRecursion" debug outputs as   
	  | # |                           |
	  | 1 | [[b]]         =  rec(1).b |
	  | 2 | [[rec(1).a]]  =  b        |
	  | 3 | [[rec(1).b]]   = 1        |
	  And the "Calculate1" in WorkFlow "WFAssignCalculateRecursion" debug inputs as 
      | fx =                         |
      | [[rec(1).b]]+1 = 1+1 |       
      And the "Calculate1" in Workflow "WFAssignCalculateRecursion" debug outputs as  
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
	  And the "values1" in WorkFlow "WFAssign&Calculate" debug inputs as 
	  | # | Variable         | New Value |
	  | 1 | [[Honda().a1]] = | 1         |
	  | 2 | [[Honda().a2]] = | 2         |
	  | 3 | [[Honda().a3]] = | 3         |
	  | 4 | [[Benz().a1]]  = | 10        |
	  | 5 | [[Benz().a2]]  = | 20        |
	  | 6 | [[Benz().a3]]  = | 30        |
	 And the "values1" in Workflow "WFAssign&Calculate" debug outputs as   
	  | # |                      |
	  | 1 | [[Honda(1).a1]] =  1  |
	  | 2 | [[Honda(1).a2]] =  2  |
	  | 3 | [[Honda(1).a3]] =  3  |
	  | 4 | [[Benz(1).a1]]  =  10 |
	  | 5 | [[Benz(1).a2]]  =  20 |
	  | 6 | [[Benz(1).a3]]  =  30 |
	  And the "Calculate1" in WorkFlow "WFAssign&Calculate" debug inputs as 
      | fx =                                                |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(10)+sum(1) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(10)+sum(2) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(10)+sum(3) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(20)+sum(1) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(20)+sum(2) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(20)+sum(3) |
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(30)+sum(1) |       
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(30)+sum(2) |       
      | sum([[Benz(*)]])+sum([[Honda(*)]]) = sum(30)+sum(3) |       
      And the "Calculate1" in Workflow "WFAssign&Calculate" debug outputs as  
	  |                 |
	  | [[result]] = 33 |

Scenario: Workflow with Assign and AssignObject
     Given I have a workflow "WFWithAssignForAssignObject"	 
	 And "WFWithAssignForAssignObject" contains an Assign for Json "JSonToVar" as
	  | variable  |
	  | [[Human]] | 
     And "WFWithAssignForAssignObject" contains an Assign Object "AssignPerson" as
	 | variable    | value     |
	 | [[@Person]] | [[Human]] |
	 When "WFWithAssignForAssignObject" is executed
	 Then the workflow execution has "NO" error
	  And the "JSonToVar" in Workflow "WFWithAssignForAssignObject" debug outputs as 
	  | # |                            |
	  | 1 | [[Human]] = {"Name":"Bob"} |
	  And the "AssignPerson" in Workflow "WFWithAssignForAssignObject" debug output contains as
	  | # |                              |
	   
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
	 And the "ForEachTest" in WorkFlow "WFWithAssignForEach" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 3      |
	 And the "ForEachTest" in WorkFlow "WFWithAssignForEach" has  "3" nested children 
	 And each "11714Nested" contains debug outputs for "Assign (1)" as
      | variable | value    |
      | [[a]]    | warewolf | 

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
	  And the "Records" in WorkFlow "workflowithAssignandUnique" debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[rs().row]] =  | 10        |
	  | 2 | [[rs().data]] = | 10        |
	  | 3 | [[rs().row]] =  | 40        |
	  | 4 | [[rs().data]] = | 20        |
	  | 5 | [[rs().row]] =  | 20        |
	  | 6 | [[rs().data]] = | 20        |
	  | 7 | [[rs().row]] =  | 30        |
	  | 8 | [[rs().data]] = | 40        |
	  And the "Records" in Workflow "workflowithAssignandUnique" debug outputs as  
	  | # |                     |
	  | 1 | [[rs(1).row]] =  10 |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the "Unique rec" in WorkFlow "workflowithAssignandUnique" debug inputs as
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
      And the "Unique rec" in Workflow "workflowithAssignandUnique" debug outputs as  
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
	  And the "Records" in WorkFlow "workflowithAssignandUniqueToolc" debug inputs as
	  | # | Variable        | New Value |
	  | 1 | [[rs(1).row]] = | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  | 3 | [[rs(2).row]] =  | 40        |
	  | 4 | [[rs(2).data]] = | 20        |
	  | 5 | [[rs(3).row]] =  | 20        |
	  | 6 | [[rs(3).data]] = | 20        |
	  | 7 | [[rs(4).row]] =  | 30        |
	  | 8 | [[rs(4).data]] = | 40        |
	  And the "Records" in Workflow "workflowithAssignandUniqueToolc" debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the "Unique rec" in WorkFlow "workflowithAssignandUniqueToolc" debug inputs as
       | #           |                     | Return Fields |
       | In Field(s) | [[rs(4).row]] = 30  |               |
       |             | [[rs(4).data]] = 40 | [[rs().row]] =  |
      And the "Unique rec" in Workflow "workflowithAssignandUniqueToolc" debug outputs as  
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
	  And the "Records" in WorkFlow "workflowithAssignandUniqueTools" debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[rs(1).row]] =  | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  | 3 | [[rs(2).row]] =  | 40        |
	  | 4 | [[rs(2).data]] = | 20        |
	  | 5 | [[rs(3).row]] =  | 20        |
	  | 6 | [[rs(3).data]] = | 20        |
	  | 7 | [[rs(4).row]] =  | 30        |
	  | 8 | [[rs(4).data]] = | 40        |
	  And the "Records" in Workflow "workflowithAssignandUniqueTools" debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  | 3 | [[rs(2).row]] =  40  |
	  | 4 | [[rs(2).data]] =  20 |
	  | 5 | [[rs(3).row]] =  20  |
	  | 6 | [[rs(3).data]] =  20 |
	  | 7 | [[rs(4).row]] =  30  |
	  | 8 | [[rs(4).data]] =  40 |
	  And the "Unique rec" in WorkFlow "workflowithAssignandUniqueTools" debug inputs as
       | #           |                     | Return Fields |
       | In Field(s) | [[rs(4).row]] = 30  |               |
       |             | [[rs(4).data]] = 40 | [[rs().row]] =  |
      And the "Unique rec" in Workflow "workflowithAssignandUniqueTools" debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 10 |
       |   | [[rec(2).unique]] = 40 |
       |   | [[rec(3).unique]] = 20 |
       |   | [[rec(4).unique]] = 30 |

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
	  And the "Assignval" in WorkFlow "WorkflowDataSplit" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | rec().a   |
	  And the "Assignval" in Workflow "WorkflowDataSplit" debug outputs as  
	  | # |                  |
	  | 1 | [[a]] =  rec().a |
	  And the "DataSplit" in WorkFlow "WorkflowDataSplit" debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	  | abcd            | Forward           | No              | 1 |  [[rec().a]] = | Index | 4     | No      |        |
	  And the "DataSplit" in Workflow "WorkflowDataSplit" debug outputs as  
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
      And the "Assignval1" in WorkFlow "Workflowwithforeachcontainscalculates" debug inputs as
      | # | Variable     | New Value |
      | 1 | [[rs().a]] = | 1         |
      | 2 | [[rs().a]] = | 2         |
      | 3 | [[rs().a]] = | 3         |
      And the "Assignval1" in Workflow "Workflowwithforeachcontainscalculates" debug outputs as 
      | # |                 |
      | 1 | [[rs(1).a]] = 1 |
      | 2 | [[rs(2).a]] = 2 |
      | 3 | [[rs(3).a]] = 3 |   
      And the "ForEachTesting" in WorkFlow "Workflowwithforeachcontainscalculates" has  "3" nested children
	  And the "Cal" in step 1 for "ForEachTesting" debug inputs as
      | fx =                |
      | [[rs(1).a]]+1 = 1+1 |           
      And the "Cal" in step 1 for "ForEachTesting" debug outputs as
	  |                |
	  | [[result]] = 2 |
	 And the "Cal" in step 2 for "ForEachTesting" debug inputs as 
      | fx =                |
      | [[rs(2).a]]+1 = 2+1 |           
       And the "Cal" in step 2 for "ForEachTesting" debug outputs as  
	  |                |
	  | [[result]] = 3 |
	   And the "Cal" in step 3 for "ForEachTesting" debug inputs as
      | fx =                |
      | [[rs(3).a]]+1 = 3+1 |           
       And the "Cal" in step 3 for "ForEachTesting" debug outputs as
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
      And the "Assigl" in WorkFlow "WorkflowDwithforeachcontainscalinvalid" debug inputs as
      | # | Variable     | New Value |
      | 1 | [[rs().a]] = | 1         |
      | 2 | [[rs().a]] = | 2         |
      | 3 | [[rs().a]] = | 3         |
      And the "Assigl" in Workflow "WorkflowDwithforeachcontainscalinvalid" debug outputs as 
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
	  And the "Assignval" in WorkFlow "WorkflowDataSplit" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | rec().a   |
	  And the "Assignval" in Workflow "WorkflowDataSplit" debug outputs as  
	  | # |                  |
	  | 1 | [[a]] =  rec().a |
	  And the "DataSplit" in WorkFlow "WorkflowDataSplit" debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | abcd            | Forward           | No              | 1 | [[rec().a]] = | Index | 4     | No      |        |
	  And the "DataSplit" in Workflow "WorkflowDataSplit" debug outputs as  
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
	  And the "Records1" in WorkFlow "WorkflowUniqueWithNames" debug inputs as
	  | # | Variable             | New Value |
	  | 1 | [[emp().firstname]] = | Smith     |
	  | 2 | [[emp().lastname]] = | Gordan    |
	  | 3 | [[emp().firstname]] = | Nicholas  |
	  | 4 | [[emp().lastname]] = | Cage      |
	  | 5 | [[emp().firstname]] = | Cage      |
	  | 6 | [[emp().lastname]] = | Nicholas  |
	  And the "Records1" in Workflow "WorkflowUniqueWithNames" debug outputs as  
	  | # |                                |
	  | 1 | [[emp(1).firstname]] =  Smith    |
	  | 2 | [[emp(1).lastname]] =  Gordan   |
	  | 3 | [[emp(2).firstname]] =  Nicholas |
	  | 4 | [[emp(2).lastname]] =  Cage     |
	  | 5 | [[emp(3).firstname]] =  Cage     |
	  | 6 | [[emp(3).lastname]] =  Nicholas |
	  And the "Unique" in WorkFlow "WorkflowUniqueWithNames" debug inputs as
       | #           |                                 | Return Fields         |
       | In Field(s) | [[emp(1).firstname]] = Smith    |                       |
       |             | [[emp(2).firstname]] = Nicholas |                       |
       |             | [[emp(3).firstname]] = Cage     |                       |
       |             | [[emp(1).lastname]] = Gordan    |                       |
       |             | [[emp(2).lastname]] = Cage      |                       |
       |             | [[emp(3).lastname]] = Nicholas  |                       |
       |             |                                 | [[emp().firstname]] = |     
      And the "Unique" in Workflow "WorkflowUniqueWithNames" debug outputs as  
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
	  And the "Records1" in WorkFlow "UniqueNamesTest" debug inputs as
	  | # | Variable              | New Value |
	  | 1 | [[emp().firstname]] = | Smith     |
	  | 2 | [[emp().lastname]] =  | Gordan    |
	  | 3 | [[emp().firstname]] = | Nicholas  |
	  | 4 | [[emp().lastname]] =  | Cage      |
	  | 5 | [[emp().firstname]] = | Cage      |
	  | 6 | [[emp().lastname]] =  | Nicholas  |
	  | 7 | [[emp().firstname]] = | Cage      |
	  | 8 | [[emp().lastname]] =  | Nicholas  |
	  And the "Records1" in Workflow "UniqueNamesTest" debug outputs as  
	  | # |                                  |
	  | 1 | [[emp(1).firstname]] =  Smith    |
	  | 2 | [[emp(1).lastname]] =  Gordan    |
	  | 3 | [[emp(2).firstname]] =  Nicholas |
	  | 4 | [[emp(2).lastname]] =  Cage      |
	  | 5 | [[emp(3).firstname]] =  Cage     |
	  | 6 | [[emp(3).lastname]] =  Nicholas  |
	  | 7 | [[emp(4).firstname]] =  Cage     |
	  | 8 | [[emp(4).lastname]] =  Nicholas  |
	  And the "Unique" in WorkFlow "UniqueNamesTest" debug inputs as
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
      And the "Unique" in Workflow "UniqueNamesTest" debug outputs as  
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
	  And the "Records" in WorkFlow "WorkflowAssingUnique" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 19        |
	  | 2 | [[rs(2).a]] = | 20        |
	  | 3 | [[rs(3).a]] = | 40        |
	  | 4 | [[rs(4).a]] = | 50        |
	  | 5 | [[rs(1).b]] = | 19        |
	  | 6 | [[rs(2).b]] = | 20        |
	  | 7 | [[rs(3).b]] = | 30        |
	  | 8 | [[rs(4).b]] = | 80        |
	  And the "Records" in Workflow "WorkflowAssingUnique" debug outputs as  
	  | # |                   |
	  | 1 | [[rs(1).a]] =  19 |
	  | 2 | [[rs(2).a]] =  20 |
	  | 3 | [[rs(3).a]] =  40 |
	  | 4 | [[rs(4).a]] =  50 |
	  | 5 | [[rs(1).b]] =  19 |
	  | 6 | [[rs(2).b]] =  20 |
	  | 7 | [[rs(3).b]] =  30 |
	  | 8 | [[rs(4).b]] =  80 |
	  And the "Unique rec" in WorkFlow "WorkflowAssingUnique" debug inputs as
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
      And the "Unique rec" in Workflow "WorkflowAssingUnique" debug outputs as  
       | # |                        |
       | 1 | [[rec(1).unique]] = 19 |
       |   | [[rec(2).unique]] = 20 |
       |   | [[rec(3).unique]] = 40 |
       |   | [[rec(4).unique]] = 50 |

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
	  And the "Recordset" in WorkFlow "workflowithAssignUniquedebugoutputs" debug inputs as
	  | # | Variable             | New Value |
	  | 1 | [[team(1).Names]]  = | test      |
	  | 2 | [[team(1).Id]]     = | 23        |
	  | 3 | [[team(2).Names]]  = | test      |
	  | 4 | [[team(2).Id]]     = | 23        |
	  And the "Recordset" in Workflow "workflowithAssignUniquedebugoutputs" debug outputs as  
	  | # |                            |
	  | 1 | [[team(1).Names]] =   test |
	  | 2 | [[team(1).Id]]    =  23    |
	  | 3 | [[team(2).Names]] =  test  |
	  | 4 | [[team(2).Id]]    =  23    |
	  And the "Uni" in WorkFlow "workflowithAssignUniquedebugoutputs" debug inputs as
       | #           |                          | Return Fields      |
       | In Field(s) | [[team(1).Names]] = test |                    |
       |             | [[team(2).Names]] = test | [[team().Names]] = |
      And the "Uni" in Workflow "workflowithAssignUniquedebugoutputs" debug outputs as  
       | # |                         |
       | 1 | [[List(1).Name]] = test |
       
Scenario: Workflow with Assign Base Convert and Case Convert testing variable that hasn"t been assigned
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
	  And the "Assign1" in WorkFlow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | 1         |
	   And the "Assign1" in Workflow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug outputs as  
	  | # |              |
	  | 1 | [[res]] =  1 |
	  And the "Case to Convert" in WorkFlow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug inputs as
	  | # | Convert    | To    |
	  | 1 | [[res12]] = | UPPER |
	  And the "Case to Convert" in Workflow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug outputs as  
	  | # |             |
	  And the "Base to Convert" in WorkFlow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug inputs as
	  | # | Convert     | From | To      |
	  | 1 | [[res12]] = | Text | Base 64 |
      And the "Base to Convert" in Workflow "WorkflowBaseConvertandCaseconvertTestingUnassignedVariablevalues" debug outputs as  
	  | # |             |

Scenario: Workflow with Assigns DataMerge and DataSplit and testing variables that hasn"t been assigned
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
	  And the "Assign To merge" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug inputs as 
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | Test      |
	 And the "Assign To merge" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug outputs as   
	  | # |                          |
	  | 1 | [[res]]          =  Test |
	  And the "Data Merge" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug inputs as 
	  | # |             | With  | Using | Pad | Align |
	  | 1 | [[Value]] =   | Index | "4"   | ""  | Left  |
	  And the "Data Merge" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug outputs as  
	  |             |
	  | [[result]] = |
	  And the "Data Split" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug inputs as 
	  | String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[Value12]]  =  | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  And the "Data Split" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues" debug outputs as  
	  | # |               |

Scenario: Workflow with Assigns DataMerge and DataSplit and testing variables that hasn"t been assigned2
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
	  And the "Assign To merge" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug inputs as 
	  | # | Variable  | New Value |
	  | 1 | [[res]] = | Test      |
	 And the "Assign To merge" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug outputs as   
	  | # |                 |
	  | 1 | [[res]] =  Test |
	  And the "Data Merge" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug inputs as 
	  | # |                 | With  | Using | Pad | Align |
	  | 1 | [[Value]]Test = | Index | "4"   | ""  | Left  |
	  And the "Data Merge" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug outputs as  
	  |              |
	  | [[result]] = |
	  And the "Data Split" in WorkFlow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug inputs as 
	  | String to Split   | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
	  | [[Value12]]Test = | Forward           | No              | 1 | [[rec().b]] = | Index | 4     | No      |        |
	  And the "Data Split" in Workflow "WorkflowWithMergeAndSlitToTestunAssignrdvaraiblevalues2" debug outputs as  
	  | # |                |

Scenario: Workflow with Assigns Replace and testing variables that hasn"t been assigned
      Given I have a workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues"
       And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains an Assign "Assign34" as
      | variable | value |
      | [[Val]]  | test  |
	  And "workflowithAssignandReplaceTestingUnassignedvariablevalues" contains Replace "Replacing" into "[[replac]]" as	
	  | In Fields  | Find     | Replace With |
	  | [[rec()]] | [[Val1]] | [[Val2]]     |
	  When "workflowithAssignandReplaceTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "No" error
	  And the "Assign34" in WorkFlow "workflowithAssignandReplaceTestingUnassignedvariablevalues" debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | test      |
	   And the "Assign34" in Workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues" debug outputs as    
	  | # |                |
	  | 1 | [[Val]] = test |
	  And the "Replacing" in WorkFlow "workflowithAssignandReplaceTestingUnassignedvariablevalues" debug inputs as 
	  | In Field(s) | Find       | Replace With |
	  | [[rec()]] = | [[Val1]] = | [[Val2]] =   |
#	  And the "Replacing" in Workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues" debug outputs as
#	  |              |
#	  | [[replac]] = |

Scenario: Workflow with Assigns Replace and testing variables that hasn"t been assigned2
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
	  And the "Assign34" in WorkFlow "workflowithAssignandReplaceTestingUnassignedvariablevalues2" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[Val]] =     | test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	   And the "Assign34" in Workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues2" debug outputs as    
	  | # |                         |
	  | 1 | [[Val]] = test          |
	  | 2 | [[rec(1).a]] = Warewolf |
	  And the "Replacing" in WorkFlow "workflowithAssignandReplaceTestingUnassignedvariablevalues2" debug inputs as 
	  | In Field(s)             | Find                | Replace With   |
	  | [[rec(1).a]] = Warewolf | [[Val1]]Test =  | [[Val]] = test |
#	  And the "Replacing" in Workflow "workflowithAssignandReplaceTestingUnassignedvariablevalues2" debug outputs as
#	  |                |
#	  | [[replac]] =  |	 

Scenario: Workflow with Assign Format Numbers and testing variables that hasn"t been assigned
	  Given I have a workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues"
	  And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[val]]  | 1     | 	  
      And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" contains Format Number "Fnumber" as 
	  | Number   | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[val1]] | Up                | [[val1]]    | [[val1]]        | [[fresult]] |
	  When "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" is executed
	  Then the workflow execution has "AN" error
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 1         |
	  And the "IndexVal" in Workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" debug outputs as  
	  | # |              |
	  | 1 | [[val]]  = 1 |   
	  And the "Fnumber" in WorkFlow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" debug inputs as 	
	  | Number     | Rounding | Rounding Value | Decimals to show |
	  | [[val1]] = | Up       | [[val1]] =     | [[val1]]  =      |
	  And the "Fnumber" in Workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues" debug outputs as 
	  |             |
	  | [[fresult]]  = |

Scenario: Workflow with Assign Format Numbers and testing variables that hasn"t been assigned2
	  Given I have a workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2"
	  And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" contains an Assign "IndexVal" as
	  | variable | value |
	  | [[val]]  | 1     | 	  
     And "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" contains Format Number "Fnumber" as 
	  | Number      | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | [[val1]]234 | Up                | [[val]]     | [[val]]         | [[fresult]] |
	  When "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" is executed
	  Then the workflow execution has "AN" error
	  And the "IndexVal" in WorkFlow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 1         |
	  And the "IndexVal" in Workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" debug outputs as  
	  | # |              |
	  | 1 | [[val]]  = 1 |   
	  And the "Fnumber" in WorkFlow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" debug inputs as 	
	  | Number            | Rounding | Rounding Value | Decimals to show |
	  | [[val1]]234 =  | Up       | [[val]] = 1    | [[val]]  = 1     |
	  And the "Fnumber" in Workflow "WorkflowWithAssignandFormatTestingUnassignedvariablevalues2" debug outputs as 
	  |                      |
	  | [[fresult]]  =  |

Scenario: Workflow with Assign Create Delete folder and testing variable values that hasn"t been assigned
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
	  And the "AssignT" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug inputs as
	  | # | Variable      | New Value       |
	  | 1 | [[rec().a]] = | C:\copied00.txt |
	  And the "AssignT" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug outputs as     
	  | # |                              |
	  | 1 | [[rec(1).a]] = C:\copied00.txt |
	 And the "Create12" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug inputs as
	  | File or Folder | Overwrite | Username | Password |
	  | [[NoValue]] =  | True      |   ""       |    ""      |
	   And the "Create12" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug outputs as    
	   |                    |
	   | [[res1]] = Failure |
	  And the "DeleteFolder1" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug inputs as
	  | Input Path  | Username | Password |
	  | [[NoValue]] = |   ""       |   ""       |
	  And the "DeleteFolder1" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues" debug outputs as    
	  |                    |
	  | [[res2]] = Failure |

Scenario: Workflow with Assign Create Delete folder and testing variable values that hasn"t been assigned2
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
	  And the "AssignT" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug inputs as
	  | # | Variable      | New Value       |
	  | 1 | [[rec().a]] = | C:\copied00.txt |
	  And the "AssignT" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug outputs as     
	  | # |                              |
	  | 1 | [[rec(1).a]] = C:\copied00.txt |
	 And the "Create12" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug inputs as
	  | File or Folder           | Overwrite | Username | Password |
	  | [[NoValue]]\copied00.txt =  | True      |    ""      |   ""       |
	   And the "Create12" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug outputs as    
	   |                    |
	   | [[res1]] = Failure |
	  And the "DeleteFolder1" in WorkFlow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug inputs as
	  | Input Path               | Username | Password |
	  | [[NoValue]]\copied00.txt =  |    ""      |   ""       |
	  And the "DeleteFolder1" in Workflow "WorkflowWithAssignCreateandDeleteRecordTestingUnassignedvariablevalues2" debug outputs as    
	  |                    |
	  | [[res2]] = Failure |

Scenario: Workflow with Assign and Random and testing variable values that hasn"t been assigned
	 Given I have a workflow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue"
	 And "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" contains an Assign "Valforrandno" as
	  | variable    | value   |
	  | [[a]]       | 1       |	  
	   And "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" contains Random "Rand" as
	  | Type    | From     | To       | Result        |
	  | Numbers | [[val1]] | [[val2]] | [[ranresult]] |
	  When "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" is executed
	  Then the workflow execution has "AN" error
	  And the "Valforrandno" in WorkFlow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[a]] =       | 1         |
	  And the "Valforrandno" in Workflow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" debug outputs as  
	  | # |                        |
	  | 1 | [[a]] = 1              |
	  And the "Rand" in WorkFlow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" debug inputs as 
	  | Random  | From        | To         |
	  | Numbers | [[val1]]  = | [[val2]] = |
	  And the "Rand" in Workflow "WorkflowWithAssignandRandomTestingUnassignedvariablevalue" debug outputs as
	  |                 |
	  | [[ranresult]] = |


Scenario: Workflow with Assign, Date Time Difference tools and testing variable values that hasn"t been assigned
	  Given I have a workflow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue"
	  And "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" contains an Assign "InputDates" as
	  | variable | value |
	  | [[val]]  | 2014  |
	  And "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" contains Date and Time Difference "DateAndTime" as	
	  | Input1   | Input2   | Input Format | Output In | Result     |
	  | [[val1]] | [[val2]] | [[val3]]     | Years     | [[result]] |  
	  When "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" is executed
	  Then the workflow execution has "AN" error
	  And the "InputDates" in WorkFlow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 2014      |
	  And the "InputDates" in Workflow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" debug outputs as  
	  | # |                 |
	  | 1 | [[val]]  = 2014 |
	  And the "DateAndTime" in WorkFlow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" debug inputs as
	  | Input 1    | Input 2 | Input Format | Output In |
	  | [[val1]] = | [[val2]] =      | [[val3]]  =    | Years     |
	  And the "DateAndTime" in Workflow "WorkflowWithAssignAndDateTimeDifferencetoolsTestingUnassignedvariablevalue" debug outputs as 
	  |              |

Scenario: Workflow with Assign, Date Time Difference tools and testing variable values that hasn"t been assigned2
	  Given I have a workflow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2"
	  And "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" contains an Assign "InputDates2" as
	  | variable | value |
	  | [[val]]  | 2014    |
	  And "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" contains Date and Time Difference "DateTime4" as	
	  | Input1                                 | Input2     | Input Format | Output In | Result     |
	  | 10/01/1991  [[val1]]/[[val1]]/[[val1]] | 10/01/1991 | dd/mm/yyyy   | Years     | [[result]] |  
	  When "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" is executed
	  Then the workflow execution has "AN" error
	  And the "InputDates2" in WorkFlow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" debug inputs as
	  | # | Variable   | New Value |
	  | 1 | [[val]]  = | 2014      |
	  And the "InputDates2" in Workflow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" debug outputs as  
	  | # |                 |
	  | 1 | [[val]]  = 2014 |
	  And the "DateTime4" in WorkFlow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" debug inputs as
	  | Input 1                                                | Input 2    | Input Format | Output In |
	  | 10/01/1991  [[val1]]/[[val1]]/[[val1]] =  | 10/01/1991 | dd/mm/yyyy   | Years     |
	  And the "DateTime4" in Workflow "WorkflowContainsDateTimeDifferencetoolsTestingUnassignedvariablevalue2" debug outputs as 
	  |                |
	  |  |

Scenario: Workflow with Assign  Delete and testing variables that hasn"t been assigned
	  Given I have a workflow "WorkflowWithAssignDelete12"
	  And "WorkflowWithAssignDelete12" contains an Assign "DelRec" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "WorkflowWithAssignDelete12" contains Delete "Delet12" as
	  | Variable   | result      |
	  | [[Del(1)]] | [[result1]] |
	  When "WorkflowWithAssignDelete12" is executed
      Then the workflow execution has "AN" error
	  And the "DelRec" in WorkFlow "WorkflowWithAssignDelete12" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the "DelRec" in Workflow "WorkflowWithAssignDelete12" debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the "Delet12" in WorkFlow "WorkflowWithAssignDelete12" debug inputs as
	  | Records      |
	  | [[Del(1)]] = |
	  And the "Delet12" in Workflow "WorkflowWithAssignDelete12" debug outputs as  
	  |                       |
	  | [[result1]] = Failure |

Scenario: Workflow with Assign  DeleteNullHandler and testing variables that hasn"t been assigned
	  Given I have a workflow "WorkflowWithAssignDelete12"
	  And "WorkflowWithAssignDelete12" contains an Assign "DelRec" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "WorkflowWithAssignDelete12" contains NullHandlerDelete "Delet12" as
	  | Variable   | result      |
	  | [[Del(1)]] | [[result1]] |
	  When "WorkflowWithAssignDelete12" is executed
      Then the workflow execution has "NO" error
	  And the "DelRec" in WorkFlow "WorkflowWithAssignDelete12" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 50        |
	  And the "DelRec" in Workflow "WorkflowWithAssignDelete12" debug outputs as  
	  | # |                   |
	  | 1 | [[rec(1).a]] = 50 |
	  And the "Delet12" in Workflow "WorkflowWithAssignDelete12" debug outputs as  
	  |                       |
	  | [[result1]] = Success |


Scenario: Workflow with Assign Sort and testing variables that hasn"t been assigned
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
	  And the "sortval5" in WorkFlow "workflowithAssignandsortingrec12" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 10        |
	  | 2 | [[rs(5).a]] = | 20        |
	  | 3 | [[rs(7).a]] = | 30        |
	  | 4 | [[rs(2).b]] = | 6         |
	  | 5 | [[rs(4).b]] = | 4         |
	  | 6 | [[rs(6).b]] = | 2         |
	  And the "sortval5" in Workflow "workflowithAssignandsortingrec12" debug outputs as    
	  | # |                  |
	  | 1 | [[rs(1).a]] = 10 |
	  | 2 | [[rs(5).a]] = 20 |
	  | 3 | [[rs(7).a]] = 30 |
	  | 4 | [[rs(2).b]] = 6  |
	  | 5 | [[rs(4).b]] = 4  |
	  | 6 | [[rs(6).b]] = 2  |
	  And the "sortRec1" in WorkFlow "workflowithAssignandsortingrec12" debug inputs as
	  | Sort Field    | Sort Order |
	  | [[xs(*).a]] = | Backwards  |
	  And the "sortRec1" in Workflow "workflowithAssignandsortingrec12" debug outputs as
	  |               |
	  | [[xs(*).a]] = |
	 
Scenario: Workflow with Assign Unique Tool and testing variables in Returnfield hasn"t been assigned
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
	  And the "Records1" in WorkFlow "workflowithAssignUni" debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[rs(1).row]] =  | 10        |
	  | 2 | [[rs(1).data]] = | 10        |
	  And the "Records1" in Workflow "workflowithAssignUni" debug outputs as  
	  | # |                      |
	  | 1 | [[rs(1).row]] =  10  |
	  | 2 | [[rs(1).data]] =  10 |
	  And the "Unrec" in WorkFlow "workflowithAssignUni" debug inputs as
      | #           |                    | Return Fields   |
      | In Field(s) | [[rs(1).row]] = 10 |                 |
      |             |                    | [[new().row]] = |
      And the "Unrec" in Workflow "workflowithAssignUni" debug outputs as  
       |                     |
       | [[rec(*).unique]] = |

Scenario: Workflow with Assign and AssignObject using append notation
     Given I have a workflow "WFWithAssignForAssignObjectAppendNot"	 
	  And "WFWithAssignForAssignObjectAppendNot" contains an Assign "Data" as
	  | variable       | value                       |
	  | [[msgs().val]] | TestingDotnetDllCascading.Food.ToJson |
	  | [[msgs().val]] | TestingDotnetDllCascading.Food.ToJson |
     And "WFWithAssignForAssignObjectAppendNot" contains an Assign Object "AssignPerson" as
	 | variable    | value          |
	 | [[@Food]] | [[msgs().val]] |
	 When "WFWithAssignForAssignObjectAppendNot" is executed
	 Then the workflow execution has "NO" error
	  And the "AssignPerson" in Workflow "WFWithAssignForAssignObjectAppendNot" debug outputs as 
	  | # |                            |
	  | 1 | [[@Food]] = "FoodName:null" |
