Feature: Length
	In order to get the length of a records
	As a Warewolf user
	I want a tool that takes a record set gives me its length

Scenario: Length of a recordset with 3 rows
	Given I get  the length from a recordset that looks like with this shape
	| [[rs]]    |   |
	| rs(1).row | 1 |
	| rs(3).row | 2 |
	| rs(5).row | 3 |
	And get length on record "[[rs()]]"	
	When the length tool is executed
	Then the length result should be 5
	And the execution has "NO" error
	And the debug inputs as 
	| Recordset         |
	| [[rs(1).row]] = 1 |
	| [[rs(2).row]] =   |
	| [[rs(3).row]] = 2 |
	| [[rs(4).row]] =   |
	| [[rs(5).row]] = 3 |
	And the debug output as 
	|                |
	| [[result]] = 5 |


Scenario: Length of a recordset with 8 rows
	Given I get  the length from a recordset that looks like with this shape
	| rs        |   |
	| rs(1).row | 1 |
	| rs(2).row | 2 |
	| rs(3).row | 3 |
	| rs(4).row | 4 |
	| rs(5).row | 5 |
	| rs(6).row | 6 |
	| rs(7).row | 7 |
	| rs(8).row | 8 |
	And get length on record "[[rs()]]"	
	When the length tool is executed
	Then the length result should be 8
	And the execution has "NO" error
	And the debug inputs as  
	| Recordset          |
	| [[rs(1).row]] =  1 |
	| [[rs(2).row]] =  2 |
	| [[rs(3).row]] =  3 |
	| [[rs(4).row]] =  4 |
	| [[rs(5).row]] =  5 |
	| [[rs(6).row]] =  6 |
	| [[rs(7).row]] =  7 |
	| [[rs(8).row]] =  8 |
	And the debug output as 
	|                 |
	| [[result]] = 8 |

Scenario: Length of a recordset with 0 rows
	Given I get  the length from a recordset that looks like with this shape
	| rs      |
	And get length on record "[[rs()]]"	
	When the length tool is executed
	Then the length result should be 0
	And the execution has "NO" error
	And the debug inputs as  
	| Recordset  |
	| [[rs()]] = |
	And the debug output as 
	|                |
	| [[result]] = 0 |
