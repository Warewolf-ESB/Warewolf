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
	And the sort execution has "NO" error

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
	And the sort execution has "NO" error

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
	And the sort execution has "NO" error

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
	And I sort a record "[[rs().row]]"
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
	And the sort execution has "NO" error
	
Scenario: Sort a recordset forwards empty recordset
	Given I have the following recordset to sort
	| rs       | value    |	
	And I sort a record "[[rs().row]]"
	And my sort order is "Forward"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | value    |
	And the sort execution has "AN" error
	
Scenario: Sort a recordset backwards empty recordset
	Given I have the following recordset to sort
	| rs       | row    |	
	And I sort a record "[[rs().row]]"
	And my sort order is "Backwards"
	When the sort records tool is executed
	Then the sorted recordset "[[rs().row]]"  will be 
	| rs       | row    |
	And the sort execution has "AN" error
			
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
	And the sort execution has "NO" error
	
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
	And the sort execution has "NO" error
	
Scenario: Sort a recordset backwards using negative recordset index
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs(-1).row]]"
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
	And the sort execution has "NO" error

Scenario: Sort a recordset forwards using negative recordset index
	Given I have the following recordset to sort
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	| rs().row | so far   |
	And I sort a record "[[rs(-1).row]]"
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
	And the sort execution has "NO" error
