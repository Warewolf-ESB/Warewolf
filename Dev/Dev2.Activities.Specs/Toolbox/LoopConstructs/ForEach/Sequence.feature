Feature: Sequence
	In order to execute sequence 
	As a Warewolf user
	I want to a tool that will allow me to construct and execute tools and services in sequence

Scenario: Execute Sequence with Assign
          Given I have a Sequence 
          And it contains an Assign “TestAssign” as
          | variable | value |
          | [[var1]] | 1     |
          | [[var2]] | 2     |
          When the sequence is executed
          Then the execution has “No” error
          And “TestAssign” debug inputs as
          | # | Variable | New Value |
          | 1 | [[var1]] | 1         |
          | 2 | [[var2]] | 2         |
          And “TestAssign” debug outputs as
          | # |              |
          | 1 | [[var1]] = 1 |
          | 2 | [[var2]] = 2 |           

Scenario: Execute a Sequence with Assign and Calculate
       Given I have a Sequence
       And it contains an Assign “SetVariables” as
       | Variable | New Value |
       | [[var1]] | 1         |
       | [[var2]] | 2         |
       And Calculate “Calculate Sum” with formula "[[a]]+[[b]]"
       When the Sequence tool is executed
       Then the execution has "NO" error
       And the “SetVariables” debug inputs as
       | # | Variable   | New Value |
       | 1 | [[var1]] = | 1         |
       | 2 | [[var2]] = | 2         |    
       And the “SetVariables” debug output as
       | # |              |
       | 1 | [[var1]] = 1 |
       | 2 | [[var2]] = 2 |
       And the “Calculate Sum” debug inputs as
       | fx =                                                                 |
       | ((([[var]]+[[var]])/[[var2]])+[[var2]]*[[var]]) = (((1+1)/20)+20*1) |          
       And the “Calculate Sum” debug output as
       | # |                |
       | 1 | [[result]] = 3 |

 Scenario: Execute a Sequence with Assign and Count
      Given I have a Sequence
	  And it contains an Assign "Records" as
      | Variable    | New Value |
      | [[rec().a]] | 1         |
      | [[rec().a]] | 2         |
      | [[rec().a]] | 3         |
      | [[rec().a]] | 4         |
	  And Count "Count Record" on "[[rec()]]"
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "Records" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "Records" debug output as
	  | # |                   |
	  | 1 | [[rec(1).a]] =  1 |
	  | 2 | [[rec(2).a]] =  2 |
	  | 3 | [[rec(3).a]] =  3 |
	  | 4 | [[rec(4).a]] =  4 |
	  And the "Count Record" debug inputs as 
	  | Recordset         |
	  | [[rs(1).row]] = 1 |
	  | [[rs(2).row]] = 2 |
	  | [[rs(3).row]] = 3 |
	  | [[rs(4).row]] = 4 |
	  And the "Count Record" debug outputs as 
	  |                |
	  | [[result]] = 3 |

Scenario: Execute a Sequence with Assign and Delete
      Given I have a Sequence
	  And it contains an Assign "All Records" as
      | Variable    | New Value |
      | [[rec().a]] | 1         |
      | [[rec().a]] | 2         |
      | [[rec().a]] | 3         |
      | [[rec().a]] | 4         |
	  And delete "Delete Record" a record "[[rs(2)]]"
	  And Assign "Delete check" the value [[rec(2).a]] to a variable "[[check]]
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "All Records" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "All Records" debug output as
	  |   | Records           |
	  | 1 | [[rec(1).a]] =  1 |
	  | 2 | [[rec(2).a]] =  2 |
	  | 3 | [[rec(3).a]] =  3 |
	  | 4 | [[rec(4).a]] =  4 |
	  And the "Delete Record" debug inputs as 
	  | Recordset        |
	  | [[rec(2).a]] = 2 |
	  And the "Delete Record" debug outputs as 
	  |                      |
	  | [[result]] = Success |
	  And the "Delete Check" debug input as
	  | # | Variable     | New Value    |
	  | 1 | [[rec(2).a]] | [[rec(2).a]] |
	  And the "Delete Check" debug output as
	  |   | Records     |
	  | 1 | [[check]] = |
	

Scenario: Execute a Sequence with Assign and Find Record Index
      Given I have a Sequence
	  And it contains an Assign "Assign Records" as
      | Variable    | New Value |
      | [[rec().a]] | 1         |
      | [[rec().a]] | 2         |
      | [[rec().a]] | 3         |
      | [[rec().a]] | 4         |
	  And Find Record Index "Find Record" search type and criteria as
	  | Match Type | Match |
	  | =          | 1     |
	  | =          | 2     |
	  | =          | 3     |
	  | =          | 4     |
	  And Find Record Index "Find Record" "Require All Fields To Match" as "Yes"
	  And Find Record Index "Find Record" "Require All Fields To Match" as "No"
	  When the Sequence tool is executed
      Then the execution has "NO" error
	  And the "Assign Records" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1         |
	  | 2 | [[rec().a]] = | 2         |
	  | 3 | [[rec().a]] = | 3         |
	  | 4 | [[rec().a]] = | 4         |  
	  And the "Assign Records" debug output as    
	  |   | Records           |
	  | 1 | [[rec(1).a]] =  1 |
	  | 2 | [[rec(2).a]] =  2 |
	  | 3 | [[rec(3).a]] =  3 |
	  | 4 | [[rec(4).a]] =  4 |      
	  And the "Find Record" debug inputs as 
	  | Records | Require All Fields To Match | Require All Matches To Be True |
	  | 1 = 1   |                             |                                |
	  | 2 = 2   |                             |                                |
	  | 3 = 3   |                             |                                |
	  | 4 = 4   | Yes                         | No                             |
	  And the "Find Record" debug outputs as 
	  |                      |
	  | [[result]] = 1,2,3,4 |


Scenario: Execute a Sequence with Assign, Sort Records and Unique Records
      Given I have a Sequence
	  And it contains an Assign "Assing data" as
      | Variable    | New Value |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 11        |
      | [[rec().a]] | 12        |
      | [[rec().a]] | 12        |
      | [[rec().a]] | 13        |
      | [[rec().a]] | 13        |
      | [[rec().a]] | 13        |
	  And  sort a record "Sort" field "[[rec(*).a]]"
	  And "Sort" sort order is "Forward"
      And find unique "Unique" in field "[[rec().a]]" with the return field "[[rec().a]]"
	  And The "Unique" result variable is "[[rec().unique]]"
      When the Sequence tool is executed
	 Then the execution has "NO" error
	  And the "Assign data" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 11        |
	  | 2 | [[rec().a]] = | 11        |
	  | 3 | [[rec().a]] = | 11        |
	  | 4 | [[rec().a]] = | 12        |
	  | 1 | [[rec().a]] = | 12        |
	  | 2 | [[rec().a]] = | 13        |
	  | 3 | [[rec().a]] = | 13        |
	  | 4 | [[rec().a]] = | 13        |    
	  And the "Assign data" debug output as    
	  |   | Records            |
	  | 1 | [[rec(1).a]] =  11 |
	  | 2 | [[rec(2).a]] =  11 |
	  | 3 | [[rec(3).a]] =  11 |
	  | 4 | [[rec(4).a]] =  12 |
	  | 1 | [[rec(5).a]] =  12 |
	  | 2 | [[rec(6).a]] =  13 |
	  | 3 | [[rec(7).a]] =  13 |
	  | 4 | [[rec(8).a]] =  13 |        
	  And the "Sort" debug inputs as  
	  | Sort Field        | Sort Order |
	  | [[rec(1).a]] = 11 |            |
	  | [[rec(2).a]] = 11 |            |
	  | [[rec(3).a]] = 11 |            |
	  | [[rec(4).a]] = 12 |            |
	  | [[rec(5).a]] = 13 |            |
	  | [[rec(6).a]] = 13 |            |
	  | [[rec(7).a]] = 13 | Forward    |
	  And the "Sort" debug output as
	  |                   |
	  | [[rec(1).a]] = 11 |
	  | [[rec(2).a]] = 11 |
	  | [[rec(3).a]] = 11 |
	  | [[rec(4).a]] = 12 |
	  | [[rec(5).a]] = 13 |
	  | [[rec(6).a]] = 13 |
	  | [[rec(7).a]] = 13 |
	  And the "Unique" debug input as 
	  | #           |                    | Return Fields |
	  | In Field(s) | [[rec(1).a]] = 11 |               |
	  |             | [[rec(2).a]] = 11 |               |
	  |             | [[rec(3).a]] = 11 |               |
	  |             | [[rec(4).a]] = 12 |               |
	  |             | [[rec(2).a]] = 13 |               |
	  |             | [[rec(3).a]] = 13 |               |
	  |             | [[rec(4).a]] = 13 | [[rec().a]]  |  
	  And the "Unique" debug output as 
	  | # |                        |
	  | 1 | [[rec(1).unique]] = 11 |
	  |   | [[rec(2).unique]] = 12 |
	  |   | [[rec(3).unique]] = 13 |


Scenario: Execute a Sequence with Assign, Base Convert and Case Convert
      Given I have a Sequence
	  And it contains an Assign "Rec To Convert " as
      | Variable    | New Value |
      | [[rec().a]] | 0x4141    |
      | [[rec().a]] | warewolf  |
      And case convert "Case Convert" a variable "[[rec(2).a]]" to "UPPER"
	  And ase convert "Base Convert" a variable "[[rec(1).a]]" from type "Hex" to type "Binary"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Rec To Convert" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 0x4141    |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Rec To Convert" debug output as    
	  |   | Records                   |
	  | 1 | [[rec(1).a]] =  0x4141    |
	  | 2 | [[rec(2).a]] =   warewolf |
	  And the "Case Tonvert" debug input as
	  | # | Convert                 | To    |
	  | 1 | [[rec(2).a]] = warewolf | UPPER |
	  And the "Case Convert" debug output as  
	  | # |                         |
	  | 1 | [[rec(2).a]] = WAREWOLF |
	  And the "Base Convert" debug inputs as  
	  | # | Convert               | From | To     |
	  | 1 | [[rec(1).a]] = 0x4141 | Hex  | Binary |
	  And the "Base Convert" debug output as  
	  | # |                                 |
	  | 1 | [[rec(1).a]] = 0100000101000001 |

Scenario: Execute a Sequence with Assign, Data Merge and Data Split
      Given I have a Sequence
	  And it contains an Assign "Assign To Merge" as
      | Variable    | New Value |
      | [[rec().a]] | test      |
      | [[rec().a]] | warewolf  |
	  And Data Merge "Data Merge" Input "[[rec(1).a]]" and merge type "Index" and string at as "4" and Padding "" and Alignment "Left"	
	  And Data Merge "Data Merge" Input "[[rec(2).a]]" and merge type "Index" and string at as "8" and Padding "" and Alignment "Left"
	  And Data Split "Data Split" string with value "testwarewolf" 
	  And  Data Split "Data Split" output assign to variable "[[rec(1).b]]" split type "Index" at "4" and Include "unselected"
	  And  Data Split "Data Split" output assign to variable "[[rec(2).b]]" split type "Index" at "8" and Include "unselected"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Assign To Merge" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Assign To Merge" debug output as    
	  |   | Records                 |
	  | 1 | [[rec(1).a]] =  test     |
	  | 2 | [[rec(2).a]] =  warewolf |
	  And the "Data Merge" debug inputs as  
	  | # |                          | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] =  test     | None | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] =  warewolf | None | "8"   | ""  | Left  |
	  And the debug output as 
	  |                           |
	  | [[result]] = testwarewolf |
	  And the "Data Split" debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).a]] = | Index | 4     | No      |        |
	  |                 | Forward           | No              | 1 | [[rec(2).a]] = | Index | 8     | No      |        |
	  And the "Data Split" debug output as
	  | # |                           |
	  | 1 |  [[rec(1).a]] =  test     |
	  |   |  [[rec(2).a]] =  warewolf |


Scenario: Execute a Sequence with Assign, Data Merge, Data Split, Find Index and Replace
      Given I have a Sequence
	  And it contains an Assign "Assign To Merge" as
      | Variable    | New Value |
      | [[rec().a]] | test      |
      | [[rec().a]] | warewolf  |
	  And Data Merge "Data Merge" Input "[[rec(1).a]]" and merge type "Index" and string at as "4" and Padding "" and Alignment "Left"	
	  And Data Merge "Data Merge" Input "[[rec(2).a]]" and merge type "Index" and string at as "8" and Padding "" and Alignment "Left"
	  And Data Split "Data Split" string with value "testwarewolf" 
	  And Data Split "Data Split" output assign to variable "[[rec(1).b]]" split type "Index" at "4" and Include "unselected"
	  And Data Split "Data Split" output assign to variable "[[rec(2).b]]" split type "Index" at "8" and Include "unselected"
	  And Find Index "Index" In Fields "[[rec().a]]"
	  And Find Index "Index" Selected Index "First Occurence"
	  And Find Index "Index" search for character "e"
	  And Find Index "Index" selected direction as "Left to right"
	  And Replace "Replacing" a sentence "[[rec(*)]]
	  And Replace "Replacing" Find the character "e"
	  And Replace "Replacing" them with "REPLACED"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Assign To Merge" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Assign To Merge" debug output as    
	  |   | Records                 |
	  | 1 | [[rec(1).a]] =  test     |
	  | 2 | [[rec(2).a]] =  warewolf |
	  And the "Data Merge" debug inputs as  
	  | # |                          | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] =  test     | None | "4"   | ""  | Left  |
	  | 2 | [[rec(2).a]] =  warewolf | None | "8"   | ""  | Left  |
	  And the debug output as 
	  |                           |
	  | [[result]] = testwarewolf |
	  And the "Data Split" debug inputs as  
	  | String to Split | Process Direction | Skip blank rows | # |                | With  | Using | Include | Escape |
	  | testwarewolf    | Forward           | No              | 1 | [[rec(1).a]] = | Index | 4     | No      |        |
	  |                 | Forward           | No              | 1 | [[rec(2).a]] = | Index | 8     | No      |        |
	  And the "Data Split" debug output as
	  | # |                           |
	  | 1 |  [[rec(1).a]] =  test     |
	  |   |  [[rec(2).a]] =  warewolf |
      And the "Index" debug inputs as
	  | In Field                | Index            | Characters | Direction     |
	  | [[rec(2).a]] = warewolf | First Occurrence | e          | Left to Right |
	  And the "Index" debug output as
	  |                |
	  | [[result]] = 4 |
	  And the "Replacing" debug inputs as 
	  | In Field(s)              | Find | Replace With |
	  | [[rec(1).a]] =  test     |      |              |
	  | [[rec(1).b]] =  test     |      |              |
	  | [[rec(2).a]] =  warewolf |      |              |
	  | [[rec(2).b]] =  warewolf | e    | REPLACED     |
	  And the "Replacinig" debug output as 
	  |                                 |  
	  | [[rec(1).a]] =  tREPLACEDst     |  
	  | [[rec(1).b]] =  tREPLACEDst     |
	  | [[rec(2).a]] =  warREPLACEDwolf |
	  | [[rec(2).b]] =  warREPLACEDwolf |

Scenario: Execute a Sequence with Gather System Information, Date and Time Difference, Date and Time, Random, and Format Number tools.
      Given I have a Sequence
	  And it contains Gather System Info "Sys info" with variable "[[test]]" and selected "Date&Time"
	  And it contains Date and Time Difference "Date&Time" with first Input date as "2013-11-29"  
	  And it contains Date and Time Difference "Date&Time" with second Input date as "2050-11-29"  
	  And it contains Date and Time Difference "Date&Time" with date format as "yyyy-mm-dd"  
	  And it contains Date and Time Difference "Date&Time" selected output in "Years"  
	  And it contains Date and Time "Date" with input date as "2013-11-29"
	  And it contains Date and Time "Date" with input format as "2013-11-29"
	  And it contains Date and Time "Date" Add time as "Years" with a value 1
	  And it contains Date and Time "Date" with output format  as ""yyyy-mm-dd"
	  And it contains Random "Random" type as "Numbers"
	  And it contains Random "Random" Range as "1" to "10"
	  And it contains Format Number "Fnumber" number as 788.894564545645
	  And it contains Format Number "Fnumber" selected rounding "up" to 3
	  And it contains Format Number "Fnumber" Decimal to show as 3
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Sys info" debug input as
	  | # |          |
	  | 1 | [[test]] |
	   
	  And the "Sys info" debug output as 
	  | # |                   |
	  | 1 | [[test]] = String |
      And the"Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-28 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug output as 
	  |                 |
	  | [[result]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the debug output as 
	  |                         |
	  | [[result]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 0    | 9  |
	  And the "Random" debug output as 
	  |                    |
	  | [[result]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug output as 
	  |                      |
	  | [[result]] = 788.895 |
     
Scenario: Execute a Sequence with For each
     Given ForEach type as  "NumOfExecution" as "1"
	 And there is a sequence with this shape
	   Given I have a Sequence
	    And it contains Gather System Info "Sys info" with variable "[[test]]" and selected "Date&Time"
	    And it contains Date and Time Difference "Date&Time" with first Input date as "2013-11-29"  
	    And it contains Date and Time Difference "Date&Time" with second Input date as "2050-11-29"  
	    And it contains Date and Time Difference "Date&Time" with date format as "yyyy-mm-dd"  
	    And it contains Date and Time Difference "Date&Time" selected output in "Years"  
	    And it contains Date and Time "Date" with input date as "2013-11-29"
	    And it contains Date and Time "Date" with input format as "2013-11-29"
	    And it contains Date and Time "Date" Add time as "Years" with a value 1
	    And it contains Date and Time "Date" with output format  as ""yyyy-mm-dd"
	    And it contains Random "Random" type as "Numbers"
	    And it contains Random "Random" Range as "1" to "10"
	    And it contains Format Number "Fnumber" number as 788.894564545645
	    And it contains Format Number "Fnumber" selected rounding "up" to 3
	    And it contains Format Number "Fnumber" Decimal to show as 3
	 When the Sequence tool is executed
	 And the debug inputs as
	|                 | Number |
	| No. of Executes | 1      |


Scenario: Execute a Sequence with Saved workflow, Gather System Information, Date and Time Difference, Date and Time, Random, and Format Number tools.
      Given I have a Sequence
	  And it contains Gather System Info "Sys info" with variable "[[test]]" and selected "Date&Time"
	  And it contains Date and Time Difference "Date&Time" with first Input date as "2013-11-29"  
	  And it contains Date and Time Difference "Date&Time" with second Input date as "2050-11-29"  
	  And it contains Date and Time Difference "Date&Time" with date format as "yyyy-mm-dd"  
	  And it contains Date and Time Difference "Date&Time" selected output in "Years"  
	  And it contains Date and Time "Date" with input date as "2013-11-29"
	  And it contains Date and Time "Date" with input format as "2013-11-29"
	  And it contains Date and Time "Date" Add time as "Years" with a value 1
	  And it contains Date and Time "Date" with output format  as ""yyyy-mm-dd"
	  And it contains Random "Random" type as "Numbers"
	  And it contains Random "Random" Range as "1" to "10"
	  And it contains Format Number "Fnumber" number as 788.894564545645
	  And it contains Format Number "Fnumber" selected rounding "up" to 3
	  And it contains Format Number "Fnumber" Decimal to show as 3
	  And it contains the underlying dropped activity is a(n) "Assignwf"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Sys info" debug input as
	  | # |          |
	  | 1 | [[test]] |
	   
	  And the "Sys info" debug output as 
	  | # |                   |
	  | 1 | [[test]] = String |
      And the"Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-28 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug output as 
	  |                 |
	  | [[result]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the debug output as 
	  |                         |
	  | [[result]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 0    | 9  |
	  And the "Random" debug output as 
	  |                    |
	  | [[result]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug output as 
	  |                      |
	  | [[result]] = 788.895 |
	  And "Assignwf" debug inputs as
      | # | Variable | New Value |
      | 1 | [[a]] | 1         |
      | 2 | [[b]] | 2         |
      And "Assignwf" debug outputs as
      | # |              |
      | 1 | [[a]] = 1 |
      | 2 | [[b]] = 2 |           

Scenario: Execute a Sequence with Web Service, Assign, Base Convert, Case Convert and xpath
      Given I have a Sequence
	  And it contains an Assign "Rec To Convert " as
      | Variable    | New Value |
      | [[rec().a]] | 0x4141    |
      | [[rec().a]] | warewolf  |
      And case convert "Case Convert" a variable "[[rec(2).a]]" to "UPPER"
	  And and convert "Base Convert" a variable "[[rec(1).a]]" from type "Hex" to type "Binary"
	  And it contains the underlying dropped activity is a(n) "Web service" 
	  And it contains xpath "xpathtool" with XML '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	  And it contains xpath "xpathtool" variable "[[firstNum]]" output with xpath "//root/number[@id='2']/text()"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Rec To Convert" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 0x4141    |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Rec To Convert" debug output as    
	  |   | Records                  |
	  | 1 | [[rec().a]] =  0x4141    |
	  | 2 | [[rec().a]] =   warewolf |
	  And the "Case Tonvert" debug input as
	  | # | Convert                 | To    |
	  | 1 | [[rec(2).a]] = warewolf | UPPER |
	  And the "Case Convert" debug output as  
	  | # |                         |
	  | 1 | [[rec(2).a]] = WAREWOLF |
	  And the "Base Convert" debug inputs as  
	  | # | Convert               | From | To     |
	  | 1 | [[rec(1).a]] = 0x4141 | Hex  | Binary |
	  And the "Base Convert" debug output as  
	  | # |                                 |
	  | 1 | [[rec(1).a]] = 0100000101000001 |
	  And the "Web service" debug input as
	  | Variable      |  
	  | [[extension]] |  
	  | [[prefix]]    |  
	  And the "Web service" debug input as
	  | #                                                                   |  
	  | [[DocumentElement().Pr_CitiesGetCountriesCountryID]] =10            |  
	  | [[DocumentElement().Pr_CitiesGetCountriesDescription]] = Azerbaijan |  
	  And the "xpathtool" debug inputs as  
	  | XML                                                                                              | # |                                              |
	  | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[firstNum]] = //root/number[@id='2']/text() |
	  And the "xpathtool" debug output as 
	  | # |                    |
	  | 1 | [[firstNum]] = Two |

Scenario: Execute a Sequence with Database Service, Assign.
      Given I have a Sequence
	  And it contains an Assign "Sequence assign" as
      | Variable    | New Value |
      | [[rec().a]] | test    |
      | [[rec().a]] | warewolf  |
	  And it contains the underlying dropped activity is a(n) "dbservicemaillist"
	  When the Sequence tool is executed
	  Then the execution has "NO" error
	  And the "Sequence assign" debug input as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | test      |
	  | 2 | [[rec().a]] = | warewolf  |
	  And the "Sequence assign" debug output as    
	  |   | Records                  |
	  | 1 | [[rec(1).a]] =  test     |
	  | 2 | [[rec(2).a]] =  warewolf |
	  And the "dbservicemaillist" debug outut as
	  |                                                               |
	  | [[dbo_GetMailingList(1).Name]]   = Tshepo                     |
	  | [[dbo_GetMailingList(2).Name]]   = Hags                       |
	  | [[dbo_GetMailingList(3).Name]]   = Ashley                     |
	  | [[dbo_GetMailingList(1).Email]]  = tshepo.ntlhokoa@dev2.co.za |
	  | [[dbo_GetMailingList(2).Email]]  = hagashen.naidu@dev2.co.za  |
	  | [[dbo_GetMailingList(3).Email]]  = ashley.lewis@dev2.co.za    |