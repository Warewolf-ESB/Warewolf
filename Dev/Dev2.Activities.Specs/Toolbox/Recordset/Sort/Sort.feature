Feature: Sort
	In order to sort a recordset
	As a Warewolf user
	I want a tool I can use to arrange records in either ascending or descending order 

Scenario: Sort a recordset forwards using star notation
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs(*).row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | are      |
	| rs().row | best     |
	| rs().row | so far   |
	| rs().row | the      |
	| rs().row | user     |
	| rs().row | Warewolf |
	| rs().row | You      |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = You      |            |
	| [[rs(2).row]] = are      |            |
	| [[rs(3).row]] = the      |            |
	| [[rs(4).row]] = best     |            |
	| [[rs(5).row]] = Warewolf |            |
	| [[rs(6).row]] = user     |            |
	| [[rs(7).row]] = so far   | Forward    |
	And the debug output as
	|                          |
	| [[rs(1).row]] = are      |
	| [[rs(2).row]] = best     |
	| [[rs(3).row]] = so far   |
	| [[rs(4).row]] = the      |
	| [[rs(5).row]] = user     |
	| [[rs(6).row]] = Warewolf |
	| [[rs(7).row]] = You      |

Scenario: Sort a recordset backwards using star notation
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs(*).row]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | You      |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | the      |
	| rs().row | so far   |
	| rs().row | best     |
	| rs().row | are      |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = You      |            |
	| [[rs(2).row]] = are      |            |
	| [[rs(3).row]] = the      |            |
	| [[rs(4).row]] = best     |            |
	| [[rs(5).row]] = Warewolf |            |
	| [[rs(6).row]] = user     |            |
	| [[rs(7).row]] = so far   | Backwards  |
	And the debug output as
	|                           |
	| [[rs(1).row]] = You      |
	| [[rs(2).row]] = Warewolf |
	| [[rs(3).row]] = user     |
	| [[rs(4).row]] = the      |
	| [[rs(5).row]] = so far   |
	| [[rs(6).row]] = best     |
	| [[rs(7).row]] = are      |	

Scenario: Sort a recordset forwards 
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs().row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | are      |
	| rs().row | best     |
	| rs().row | so far   |
	| rs().row | the      |
	| rs().row | user     |
	| rs().row | Warewolf |
	| rs().row | You      |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = You      |            |
	| [[rs(2).row]] = are      |            |
	| [[rs(3).row]] = the      |            |
	| [[rs(4).row]] = best     |            |
	| [[rs(5).row]] = Warewolf |            |
	| [[rs(6).row]] = user     |            |
	| [[rs(7).row]] = so far   | Forward    |
	And the debug output as
	|                           |
	| [[rs(1).row]] = are      |
	| [[rs(2).row]] = best     |
	| [[rs(3).row]] = so far   |
	| [[rs(4).row]] = the      |
	| [[rs(5).row]] = user     |
	| [[rs(6).row]] = Warewolf |
	| [[rs(7).row]] = You      |	

Scenario: Sort a recordset backwards 
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs(*).row]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | You      |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | the      |
	| rs().row | so far   |
	| rs().row | best     |
	| rs().row | are      |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = You      |            |
	| [[rs(2).row]] = are      |            |
	| [[rs(3).row]] = the      |            |
	| [[rs(4).row]] = best     |            |
	| [[rs(5).row]] = Warewolf |            |
	| [[rs(6).row]] = user     |            |
	| [[rs(7).row]] = so far   | Backwards  |
	And the debug output as
	|                          |
	| [[rs(1).row]] = You      |
	| [[rs(2).row]] = Warewolf |
	| [[rs(3).row]] = user     |
	| [[rs(4).row]] = the      |
	| [[rs(5).row]] = so far   |
	| [[rs(6).row]] = best     |
	| [[rs(7).row]] = are      |	
	
Scenario: Sort a recordset forwards empty recordset
	Given I have the following recordset to sort
	| rs       | value    |	
	And I sort a record "[[rs().row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field      | Sort Order |
	| [[rs(*).row]] = | Forward    |
	And the debug output as
    |  |
    |  |
	
Scenario: Sort a recordset backwards empty recordset
	Given I have the following recordset to sort
	| rs       | row    |	
	And I sort a record "[[rs().row]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | row    |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field      | Sort Order |
	| [[rs(*).row]] = | Backwards  |
	And the debug output as
    |                 |
    | [[rs(*).row]] = |
			
Scenario: Sort a recordset forwards with one row
	Given I have the following recordset to sort
	| rs       | value    |	
	| rs().row | Warewolf |
	And I sort a record "[[rs().row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | Warewolf |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = Warewolf | Forward    |
	And the debug output as
    |                          |
    | [[rs(1).row]] = Warewolf |
	
Scenario: Sort a recordset backwards recordset  with one row
	Given I have the following recordset to sort
	| rs       | value    |	
	| rs().row | Warewolf |
	And I sort a record "[[rs().row]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	| rs().row | Warewolf |
	And the execution has "NO" error
	And the debug inputs as  
	| Sort Field               | Sort Order |
	| [[rs(1).row]] = Warewolf | Backwards  |
	And the debug output as
    |                          |
    | [[rs(1).row]] = Warewolf |	
#
#Scenario: Sort a random recordset indexes using star notation to Forward
#	Given I have the following recordset to sort
#	| rs        | value |
#	| rs(1).row | a     |
#	| rs(3).row | b     |
#	| rs(6).row | c     |
#	| rs(8).row | b     |
#	And I sort a record "[[rs(*).row]]"
#	And my sort order is "Forward"
#	When the sort records tool is executed
#	Then the sorted recordset "[[rs().row]]"  will be 
#	| rs        | value |
#	| rs(1).row | a     |
#	| rs(3).row | b     |
#	| rs(6).row | b     |
#	| rs(8).row | c     |
#	And the execution has "NO" error
#	And the debug inputs as  
#	| Sort Field        | Sort Order |
#	| [[rs(1).row]] = a |            |
#	| [[rs(3).row]] = b |            |
#	| [[rs(6).row]] = c |            |
#	| [[rs(8).row]] = b | Forward    |
#	And the debug output as
#	|                   |
#	| [[rs(1).row]] = a |
#	| [[rs(3).row]] = b |
#	| [[rs(6).row]] = b |
#	| [[rs(8).row]] = c |
#
#Scenario: Sort a random recordset indexes using star notation to Backwards
#	Given I have the following recordset to sort
#	| rs        | value |
#	| rs(1).row | a     |
#	| rs(3).row | b     |
#	| rs(6).row | c     |
#	| rs(8).row | b     |
#	And I sort a record "[[rs(*).row]]"
#	And my sort order is "Backwards"
#	When the sort records tool is executed
#	Then the sorted recordset "[[rs().row]]"  will be 
#	| rs        | value |
#	| rs(1).row | c     |
#	| rs(3).row | b     |
#	| rs(6).row | b     |
#	| rs(8).row | a     |
#	And the execution has "NO" error
#	And the debug inputs as  
#	| Sort Field        | Sort Order |
#	| [[rs(1).row]] = a |            |
#	| [[rs(3).row]] = b |            |
#	| [[rs(6).row]] = c |            |
#	| [[rs(8).row]] = b | Forward    |
#	And the debug output as
#	|                   |
#	| [[rs(1).row]] = c |
#	| [[rs(3).row]] = b |
#	| [[rs(6).row]] = b |
#	| [[rs(8).row]] = a |

#This following 4 Scenarios should be passed after the bug 11838 is fixed	
Scenario: Sort 2 columns backwards
	Given I have the following recordset to sort
	| rs       | value     |
	| rs(1).a  | Zambia    |
	| rec(1).a | Mangolia  |
	| rs(2).a  | America   |
	| rec(2).a | Australia |
	And I sort a record "[[rs(*).a]],[[rec(*).a]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Sort Field                         | Sort Order |
	| [[rs(*).a]],[[rec(*).a]] = America | Backwards  |
	|                                    |            |
	And the debug output as
	    |                                    |
	    | [[rs(*).a]],[[rec(*).a]] = America |


Scenario: Sort 2 columns Forwards
	Given I have the following recordset to sort
	| rs       | value     |
	| rs(1).a  | Zambia    |
	| rec(1).a | Mangolia  |
	| rs(2).a  | America   |
	| rec(2).a | Australia |
	And I sort a record "[[rs(*).a]],[[rec(*).a]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Sort Field                        | Sort Order |
	| [[rs(*).a]],[[rec(*).a]] = America | Forward    |
	And the debug output as
	    |                                    |
	    | [[rs(*).a]],[[rec(*).a]] = Zambia |
#
Scenario: Sort Recordset without field Forwards
	Given I have the following recordset to sort
	| rs       | value     |
	| rs(1).a  | Zambia    |
	| rec(1).a | Mangolia  |
	| rs(2).a  | America   |
	| rec(2).a | Australia |
	And I sort a record "[[rs(*)]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Sort Field            | Sort Order |
	| [[rs(1).a]] = Zambia  |            |
	| [[rs(2).a]] = America | Forward    |
	And the debug output as
    |                          |
#				
Scenario: Sort Recordset without field Backwards
	Given I have the following recordset to sort
	| rs       | value     |
	| rs(1).a  | Zambia    |
	| rec(1).a | Mangolia  |
	| rs(2).a  | America   |
	| rec(2).a | Australia |
	And I sort a record "[[rs(*)]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Sort Field            | Sort Order |
	| [[rs(1).a]] = Zambia  |            |
	| [[rs(2).a]] = America | Backwards  |
	And the debug output as
    |                          |






















