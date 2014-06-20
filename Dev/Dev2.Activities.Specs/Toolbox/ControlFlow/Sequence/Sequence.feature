Feature: Sequence
	In order to execute sequence 
	As a Warewolf user
	I want to a tool that will allow me to construct and execute tools and services in sequence

Scenario: Execute Sequence with Assign
          Given I have a Sequence "Test"
          And "Test" contains an Assign "TestAssign" as
          | variable | value |
          | [[var1]] | 1     |
          | [[var2]] | 2     |
          When the Sequence tool is executed
          Then the execution has "NO" error
          And the "TestAssign" debug inputs as
          | # | Variable   | New Value |
          | 1 | [[var1]] = | 1         |
          | 2 | [[var2]] = | 2         |
          And the "TestAssign" debug outputs as
          | # |              |
          | 1 | [[var1]] = 1 |
          | 2 | [[var2]] = 2 |           
	
Scenario: Execute a Sequence with Assign and Calculate
       Given I have a Sequence "Test"
	   And "Test" contains an Assign "SetVariables" as
       | variable | value |
       | [[var1]] | 1         |
       | [[var2]] | 2         |
       And "Test" contains Calculate "Calculate Sum" with formula "[[var1]]+[[var2]]" into "[[result]]"
       When the Sequence tool is executed
       Then the execution has "NO" error
       And the "SetVariables" debug inputs as
       | # | Variable   | New Value |
       | 1 | [[var1]] = | 1         |
       | 2 | [[var2]] = | 2         |    
       And the "SetVariables" debug outputs as
       | # |              |
       | 1 | [[var1]] = 1 |
       | 2 | [[var2]] = 2 |
       And the "Calculate Sum" debug inputs as
       | fx =              |
       | [[var1]]+[[var2]] = 1+2 |          
       And the "Calculate Sum" debug outputs as
	   |                |
	   | [[result]] = 3 |

 Scenario: Execute a Sequence with Assign and Count
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Records" as
      | variable    | value |
      | [[rec().a]] | 1     |
      | [[rec().a]] | 2     |
      | [[rec().a]] | 3     |
      | [[rec().a]] | 4     |
	  And "Test" contains Count Record "Count" on "[[rec()]]" into "[[result]]"
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "Records" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "Records" debug outputs as
	  | # |                   |
	  | 1 | [[rec(1).a]] =  1 |
	  | 2 | [[rec(2).a]] =  2 |
	  | 3 | [[rec(3).a]] =  3 |
	  | 4 | [[rec(4).a]] =  4 |
	  And the "Count" debug inputs as 
	  | Recordset         |
	  | [[rec(1).a]] = 1 |
	  | [[rec(2).a]] = 2 |
	  | [[rec(3).a]] = 3 |
	  | [[rec(4).a]] = 4 |
	  And the "Count" debug outputs as 
	  |                |
	  | [[result]] = 4 |

Scenario: Execute a Sequence with Assign and Delete
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "All Records" as
      | variable    | value |
      | [[rec().a]] | 1         |
      | [[rec().a]] | 2         |
      | [[rec().a]] | 3         |
      | [[rec().a]] | 4         |
	  And "Test" contains Delete "Delete Record" as
	  | Variable   | result     |
	  | [[rec(2)]] | [[result]] |
      And "Test" contains an Assign "Delete check" as 
	   | variable    | value |
	   | [[check]] | [[rec(2).a]] |
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "All Records" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "All Records" debug outputs as
	  |  # |            |
	  | 1 | [[rec(1).a]] = 1 |
	  | 2 | [[rec(2).a]] = 2 |
	  | 3 | [[rec(3).a]] = 3 |
	  | 4 | [[rec(4).a]] = 4 |
	  And the "Delete Record" debug inputs as 
	  | Records        |
	  | [[rec(2).a]] = 2 |
	  And the "Delete Record" debug outputs as 
	  |                      |
	  | [[result]] = Success |
	  And the "Delete check" debug inputs as
	  | # | Variable    | New Value    |
	  | 1 | [[check]] = | [[rec(2).a]] = |
	  And the "Delete check" debug outputs as
	  | #  |      |
	  | 1 | [[check]] = |
	

Scenario: Execute a Sequence with Assign and Find Record Index
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Assign Records" as
      | variable    | value |
      | [[rec().a]] | 1         |
      | [[rec().a]] | 2         |
      | [[rec().a]] | 3         |
      | [[rec().a]] | 4         |
	  And "Test" contains Find Record Index "Find Record" search "[[rec().a]]" and result "[[result]]" as
	  | Match Type | Match | 
	  | =          | 1     | 
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "Assign Records" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "Assign Records" debug outputs as    
	  | # |                   |
	  | 1 | [[rec(1).a]] = 1 |
	  | 2 | [[rec(2).a]] = 2 |
	  | 3 | [[rec(3).a]] = 3 |
	  | 4 | [[rec(4).a]] = 4 |      
	  And the "Find Record" debug inputs as 
	  | #           |                  | # |     |  | And | Require All Fields To Match | Require All Matches To Be True |
	  | In Field(s) | [[rec(1).a]] = 1 |   |     |  |     |                             |                                |
	  |             | [[rec(2).a]] = 2 |   |     |  |     |                             |                                |
	  |             | [[rec(3).a]] = 3 |   |     |  |     |                             |                                |
	  |             | [[rec(4).a]] = 4 | 1 |  =  | 1 |     | NO                          | NO                             |
	  And the "Find Record" debug outputs as      
	  |                |					      
	  | [[result]] = 1 |


Scenario: Execute a Sequence with Assign and Unique Records
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Assign data" as
      | variable    | value |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 12        |
      | [[rec().a]] | 12        |
      | [[rec().a]] | 13        |
      | [[rec().a]] | 13        |
      | [[rec().a]] | 13        |
      And "Test" contains find unique "Unique" as
	  | In Fields   | Return Fields | Result           |
	  | [[rec(*).a]] | [[rec().a]]   | [[rec().unique]] |
      When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Assign data" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 11        |
	  | 2 | [[rec().a]] = | 11        |
	  | 3 | [[rec().a]] = | 11        |
	  | 4 | [[rec().a]] = | 12        |
	  | 5 | [[rec().a]] = | 12        |
	  | 6 | [[rec().a]] = | 13        |
	  | 7 | [[rec().a]] = | 13        |
	  | 8 | [[rec().a]] = | 13        |    
	  And the "Assign data" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] =  11 |
	  | 2 | [[rec(2).a]] =  11 |
	  | 3 | [[rec(3).a]] =  11 |
	  | 4 | [[rec(4).a]] =  12 |
	  | 5 | [[rec(5).a]] =  12 |
	  | 6 | [[rec(6).a]] =  13 |
	  | 7 | [[rec(7).a]] =  13 |
	  | 8 | [[rec(8).a]] =  13 |        
	  And the "Unique" debug inputs as 
	  | #           |                   | Return Fields  |
	  | In Field(s) | [[rec(1).a]] = 11 |                |
	  |             | [[rec(2).a]] = 11 |                |
	  |             | [[rec(3).a]] = 11 |                |
	  |             | [[rec(4).a]] = 12 |                |
	  |             | [[rec(5).a]] = 12 |                |
	  |             | [[rec(6).a]] = 13 |                |
	  |             | [[rec(7).a]] = 13 |                |
	  |             | [[rec(8).a]] = 13 | [[rec().a]]  = |
	  And the "Unique" debug outputs as 
	  | # |                         |
	  | 1 | [[rec(9).unique]] = 11  |
	  |   | [[rec(10).unique]] = 12 |
	  |   | [[rec(11).unique]] = 13 |


Scenario: Execute a Sequence with Assign, Base Convert and Case Convert
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Rec To Convert" as
      | variable    | value |
      | [[rec().a]] | 0x4141    |
      | [[rec().a]] | warewolf  |
      And "Test" contains case convert "Case Convert" as
	  | Variable     | Type  |
	  | [[rec(2).a]] | UPPER |
	  And "Test" contains Base convert "Base Convert" as
	  | Variable     | From | To     |
	  | [[rec(1).a]] | Hex  | Binary |
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Rec To Convert" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 0x4141    |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Rec To Convert" debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = 0x4141   |
	  | 2 | [[rec(2).a]] = warewolf |
	  And the "Case Convert" debug inputs as
	  | # | Convert                 | To    |
	  | 1 | [[rec(2).a]] = warewolf | UPPER |
	  And the "Case Convert" debug outputs as  
	  | # |                         |
	  | 1 | [[rec(2).a]] = WAREWOLF |
	  And the "Base Convert" debug inputs as  
	  | # | Convert               | From | To     |
	  | 1 | [[rec(1).a]] = 0x4141 | Hex  | Binary |
	  And the "Base Convert" debug outputs as  
	  | # |                                 |
	  | 1 | [[rec(1).a]] = 0100000101000001 |

Scenario: Execute a Sequence with Assign, Data Merge and Data Split
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Assign To Merge" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
	  And "Test" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4      |         | Left       |
	  | [[rec(2).a]]  | Index | 8      |         | Left       |
	  And "Test" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Assign To Merge" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().b]] = | nothing   |
	  | 3 | [[rec().a]] = | warewolf  |
	  | 4 | [[rec().b]] = | nothing   |
	  And the "Assign To Merge" debug outputs as    
	  | # |                        |
	  | 1 | [[rec(1).a]] =  test   |
	  | 2 | [[rec(1).b]] = nothing |
	  | 3 | [[rec(2).a]] =  warewolf |
	  | 4 | [[rec(2).b]] =  nothing |
	  And the "Data Merge" debug inputs as  
	  | # |                          | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] =  test     | Index | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] =  warewolf | Index | "8"   | ""  | Left  |
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


Scenario: Execute a Sequence with Assign, Data Merge, Data Split, Find Index and Replace
      Given I have a Sequence "Test"
	  And "Test" contains an Assign "Assign To Merge" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
	  And "Test" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4      |         | Left       |
	  | [[rec(2).a]]  | Index | 8      |         | Left       |
	  And "Test" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	  And "Test" contains Find Index "Index" into "[[indexResult]]" as
	  | In Fields    | Index           | Character | Direction     |
	  | [[rec().a]] | First Occurence | e         | Left to Right |
	  And "Test" contains Replace "Replacing" into "[[replaceResult]]" as	
	  | In Fields  | Find | Replace With |
	  | [[rec(*)]] | e    | REPLACED     |
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Assign To Merge" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().b]] = | nothing   |
	  | 3 | [[rec().a]] = | warewolf  |
	  | 4 | [[rec().b]] = | nothing   |
	  And the "Assign To Merge" debug outputs as    
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
	  | In Field                | Index            | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurence | e          | Left to Right |
	  And the "Index" debug outputs as
	  |                |
	  | [[indexResult]] = 4 |
	  And the "Replacing" debug inputs as 
	  | In Field(s)              | Find | Replace With |
	  | [[rec(1).a]] = test     |      |              |
	  | [[rec(1).b]] = test     |      |              |
	  | [[rec(2).a]] = warewolf |      |              |
	  | [[rec(2).b]] = warewolf | e    | REPLACED     |
	  And the "Replacing" debug outputs as 
	  |                                 |  
	  | [[rec(1).a]] = tREPLACEDst     |  
	  | [[rec(1).b]] = tREPLACEDst     |
	  | [[rec(2).a]] = warREPLACEDwolf |
	  | [[rec(2).b]] = warREPLACEDwolf |
	  | [[replaceResult]] = 4 |

Scenario: Execute a Sequence with Gather System Information, Date and Time Difference, Date and Time, Random, and Format Number tools.
      Given I have a Sequence "Test"
	  And "Test" contains Gather System Info "Sys info" as
	  | Variable | Selected     |
	  | [[test]] | Date & Time |
	  And "Test" contains Date and Time Difference "Date&Time" as	
	  | Input1     | Input2     | Input Format | Output In | Result      |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     | [[result1]] |  
	  And "Test" contains Date and Time "Date" as
	  | Input      | Input Format | Add Time | Output Format | Result      |
	  | 2013-11-29 | yyyy-mm-dd   | 1        | yyyy-mm-dd    | [[result2]] |
	  And "Test" contains Random "Random" as
	  | Type    | From | To | Result      |
	  | Numbers | 1    | 10 | [[result3]] |
	  And "Test" contains Format Number "Fnumber" as 
	  | Number           | Rounding Selected | Rounding To | Decimal to show | Result  |
	  | 788.894564545645 | Up                | 3           | 3               | [[result4]] |
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Sys info" debug inputs as
	  | # |            |               |  
	  | 1 | [[test]] =  | Date & Time |
	   And the "Sys info" debug outputs as 
	  | # |                   |
	  | 1 | [[test]] = String |
      And the "Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug outputs as 
	  |                 |
	  | [[result1]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the "Date" debug outputs as 
	  |                          |
	  | [[result2]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 1    | 10 |
	  And the "Random" debug outputs as 
	  |                    |
	  | [[result3]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug outputs as 
	  |                      |
	  | [[result4]] = 788.895 |
     
Scenario: Execute a Sequence with For each
      Given I have a ForEach "ForEachTest" as "NumOfExecution" executions "1"
	  And I have a Sequence "Test"
	  And "Test" contains Gather System Info "Sys info" as
	  | Variable | Selected  |
	  | [[test]] | Date & Time |
	  And "Test" contains Date and Time Difference "Date&Time" as	
	  | Input1     | Input2     | Input Format | Output In | Result      |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     | [[result1]] |  
	  And "Test" contains Date and Time "Date" as
	  | Input      | Input Format | Add Time | Output Format | Result      |
	  | 2013-11-29 | yyyy-mm-dd   | 1        | yyyy-mm-dd    | [[result2]] |
	  And "Test" contains Random "Random" as
	  | Type   | From | To | Result      |
	  | Numbers | 1    | 10 | [[result3]] |
	  And "Test" contains Format Number "Fnumber" as 
	  | Number           | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | 788.894564545645 | Up                | 3           | 3               | [[result4]] |
	  When the ForEach "ForEachTest" tool is executed
	  Then the execution has "NO" error
	  And the "ForEachTest" debug inputs as
	  |                 | Number |
	  | No. of Executes | 1      |
	   And the "Sys info" debug inputs as
	  | # |            |               |  
	  | 1 | [[test]] =  | Date & Time |
	    And the "Sys info" debug outputs as 
	  | # |                   |
	  | 1 | [[test]] = String |
	   And the "Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug outputs as 
	  |                 |
	  | [[result1]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the "Date" debug outputs as 
	  |                          |
	  | [[result2]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 1    | 10 |
	  And the "Random" debug outputs as 
	  |                    |
	  | [[result3]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug outputs as 
	  |                      |
	  | [[result4]] = 788.895 |

Scenario: Execute a Sequence with For each with 3 executions
      Given I have a ForEach "ForEachTest" as "NumOfExecution" executions "3"
	  And I have a Sequence "Test"
	  And "Test" contains Gather System Info "Sys info" as
	  | Variable | Selected  |
	  | [[test().date]] | Date & Time |
	  And "Test" contains Date and Time Difference "Date&Time" as	
	  | Input1     | Input2     | Input Format | Output In | Result      |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     | [[test().result1]] |  
	  And "Test" contains Date and Time "Date" as
	  | Input      | Input Format | Add Time | Output Format | Result      |
	  | 2013-11-29 | yyyy-mm-dd   | 1        | yyyy-mm-dd    | [[test().result2]] |
	  And "Test" contains Random "Random" as
	  | Type   | From | To | Result      |
	  | Numbers | 1    | 10 | [[test().result3]] |
	  And "Test" contains Format Number "Fnumber" as 
	  | Number           | Rounding Selected | Rounding To | Decimal to show | Result      |
	  | 788.894564545645 | Up                | 3           | 3               | [[test().result4]] |
	  When the ForEach "ForEachTest" tool is executed
	  Then the execution has "NO" error
	  And the "ForEachTest" debug inputs as
	  |                 | Number |
	  | No. of Executes | 3      |
	   And the "Sys info" debug inputs as
	  | # |            |               |  
	  | 1 | [[test().date]] =  | Date & Time |
	    And the "Sys info" debug outputs as 
	  | # |                   |
	  | 1 | [[test(11).date]] = String |
	   And the "Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug outputs as 
	  |                 |
	  | [[test(12).result1]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the "Date" debug outputs as 
	  |                          |
	  | [[test(13).result2]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 1    | 10 |
	  And the "Random" debug outputs as 
	  |                    |
	  | [[test(14).result3]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug outputs as 
	  |                                |
	  | [[test(15).result4]] = 788.895 |

Scenario: Sending Error in error variable and calling webservice when inner activity errors
    Given I have a Sequence "Test"
	And "Test" contains Date and Time Difference "Date&Time" as	
	  | Input1     | Input2     | Input Format | Output In | Result      |
	  | 2013-11-29 | 2050-11-29 | yyyytt-mm-dd | Years     | [[result1]] |  
    And assign error to variable "[[error]]"
    And call the web service "http://tst-ci-remote:3142/services/OnError_WriteErrorSeq.xml?errorLog=[[error]]"
    When the Sequence tool is executed
    Then the execution has "AN" error
    And the result from the web service "http://tst-ci-remote:3142/services/OnError_ReadErrorSeq.xml" will have the same data as variable "[[error]]"
   And the "Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-29 | yyyytt-mm-dd   | Years     |
	  And the "Date&Time" debug outputs as 
	  |                 |
	  | [[result1]] =  |