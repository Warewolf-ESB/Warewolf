﻿Feature: Length
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
	| [[rs(3).row]] = 2 |
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




Scenario: Recordset length for coloumn
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
	And get length on record "[[rs().row]]"	
	When the length tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Recordset          |
	And the debug output as 
	|                |

Scenario: Recordset length for coloumns invalid
	Given I get  the length from a recordset that looks like with this shape
	| rs        |   |
	| rs().row  | 1 |
	| rs().row  | 2 |
	| rs().row  | 3 |
	| rs().row  | 4 |
	| rs().row2 | 5 |
	| rs().row2 | 6 |
	| rs().row2 | 7 |
	And get length on record "[[rs().row]]"	
	When the length tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Recordset           |
	And the debug output as 
	|                |

Scenario: Recordset length 
	Given I get  the length from a recordset that looks like with this shape
	| rs        |   |
	| rs().row  | 1 |
	| rs().row  | 2 |
	| rs().row  | 3 |
	| rs().row  | 4 |
	| rs().row2 | 5 |
	| rs().row2 | 6 |
	| rs().row2 | 7 |
	And get length on record "[[rs()]]"	
	When the length tool is executed
	Then the length result should be 6
	And the execution has "NO" error
	And the debug inputs as  
	| Recordset           |
	| [[rs(1).row]] =  1  |
	| [[rs(2).row]] =  2  |
	| [[rs(3).row]] =  3  |
	| [[rs(4).row]] =  4  |
	| [[rs(4).row2]] =  5 |
	| [[rs(5).row2]] =  6 |
	| [[rs(6).row2]] =  7 |
	And the debug output as 
	|                |
	| [[result]] = 6 |

Scenario: Recordset length for invalid recordset
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
	And get length on record "[[rs().&^]]"	
	When the length tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Recordset          |
	And the debug output as 
	|                |

Scenario Outline: Ensure Recordset length inputs work as expected 
	Given I get  the length from a recordset that looks like with this shape
	| rs        |   |
	| rs().row  | 1 |
	| rs().row  | 2 |
	| rs().row  | 3 |
	| rs().row  | 4 |
	| rs().row2 | 5 |
	| rs().row2 | 6 |
	| rs().row2 | 7 |
	And get length on record "<variable>"	
	When the length tool is executed
	Then the execution has "<Error>" error
Examples: 
| variable         | val  | Error | message                                           | result             | value           |
| [[a]]            | The  | AN    | Scalar not allowed                                | [[b]]              | [[b]] = failure |
| ""               | ""   | AN    |                                                   | [[rec(1).a]]       | 0               |
| dfsd             | dfsd | AN    | Invalid characters have been entered as Recordset | [[rec(*).a]]       | Failure         |
| 12               | 12   | AN    | Invalid characters have been entered as Recordset | [[rec([[int]]).a]] | Failure         |
| [[rec(1)]]       | ""   | AN    |                                                   | [[rs().a]]         | [[rs(1).a]] = 3 |
| [[rec(*)]]       | ""   | AN    | Blank result variable                             | ""                 | ""              |
| [[rec([[int]])]] | ""   | AN    |                                                   | [[sdasd]]          | 3               |
| [[c]]            | ""   | AN    | Scalar not allowed                                | [[d]]              | Failure         |



@ignore
#Complex Types WOLF-1042
Scenario Outline: Ensure the Complex type length inputs work as expected 
	Given I get  the length from a object that looks like with this shape
	| rs             |   |
	| rs().row().set | 1 |
	| rs().row().set | 2 |
	| rs().row().set | 3 |
	| rs().row().set | 4 |
	| rs().row().set | 5 |
	| rs().row().set | 6 |
	| rs().row().set | 7 |
	And get length on record "<variable>"	
	When the length tool is executed
	Then the length result should be "<val>"
	And the result variable '<result>' will be "<value>"
	And the execution has "<Error>" error
	And the debug inputs as "<message>"
Examples: 
| variable                   | val | error | message | result                | value |
| [[rs(1).row().set]]        | 1   | No    |         | [[rec(1).row().val]]  | 1     |
| [[rs(*).row([[int]]).set]] | 4   | No    |         | [[rec(1).row(3).val]] | 4     |









Scenario: Length of an null recordset
	Given I get  the length from a recordset that looks like with this shape
	| rs ||
	| [[rs().row]] | NULL |
	And get length on record "[[rs()]]"	
	When the length tool is executed
	Then the length result should be 0
	And the execution has "No" error

Scenario: Length of an unassigned recordset
	Given get length on record "[[rs()]]"	
	When the length tool is executed
	Then the execution has "An" error

















