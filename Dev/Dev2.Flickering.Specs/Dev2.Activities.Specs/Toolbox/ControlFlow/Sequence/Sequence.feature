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
		  And the Sequence Has a Duration

Scenario: Execute a Sequence with For each with 3 executions
      Given I have a ForEach "ForEachTest" as "NumOfExecution" executions "3"
	  And I have a Sequence "Test"
	  And "Test" contains Gather System Info "Sys info" as
	  | Variable        | Selected    |
	  | [[test().date]] | Date & Time |
	  And "Test" contains Date and Time Difference "Date&Time" as	
	  | Input1     | Input2     | Input Format | Output In | Result             |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     | [[test().result1]] |  
	  And "Test" contains Date and Time "Date" as
	  | Input      | Input Format | Add Time | Output Format | Result             |
	  | 2013-11-29 | yyyy-mm-dd   | 1        | yyyy-mm-dd    | [[test().result2]] |
	  And "Test" contains Random "Random" as
	  | Type    | From | To | Result             |
	  | Numbers | 1    | 10 | [[test().result3]] |
	  And "Test" contains Format Number "Fnumber" as 
	  | Number           | Rounding Selected | Rounding To | Decimal to show | Result             |
	  | 788.894564545645 | Up                | 3           | 3               | [[test().result4]] |
	  When the ForEach "ForEachTest" tool is executed
	  Then the execution has "NO" error
	  And the "ForEachTest" debug inputs as
	  |                 | Number |
	  | No. of Executes | 3      |
	   And the "Sys info" debug inputs as
	  | # |                    |             |  
	  | 1 | [[test().date]] =  | Date & Time |
	    And the "Sys info" debug outputs as 
	  | # |                           |
	  | 1 | [[test(6).date]] = String |
	   And the "Date&Time" debug inputs as  
	  | Input 1    | Input 2    | Input Format | Output In |
	  | 2013-11-29 | 2050-11-29 | yyyy-mm-dd   | Years     |
	  And the "Date&Time" debug outputs as 
	  |                          |
	  | [[test(6).result1]] = 37 |
	  And the "Date" debug inputs as  
	  | Input      | Input Format | Add Time |   | Output Format |
	  | 2013-11-29 | yyyy-mm-dd   | Years    | 1 | yyyy-mm-dd    |	
	  And the "Date" debug outputs as 
	  |                                  |
	  | [[test(6).result2]] = 2014-11-29 |
	  And the "Random" debug inputs as  
	  | Random  | From | To |
	  | Numbers | 1    | 10 |
	  And the "Random" debug outputs as 
	  |                             |
	  | [[test(6).result3]] = Int32 |
	  And the "Fnumber" debug inputs as  
	  | Number           | Rounding | Rounding Value | Decimals to show |
	  | 788.894564545645 | Up       | 3              | 3                |
	  And the "Fnumber" debug outputs as 
	  |                              |
	  | [[test().result4]] = 788.895 |