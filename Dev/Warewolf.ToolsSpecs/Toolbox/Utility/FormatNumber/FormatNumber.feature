﻿Feature: FormatNumber
	In order to round off numbers
	As a Warewolf user
	I want a tool that will aid me to do so 

Scenario: Format number rounding normal with zero rounding and zero decimals to show 
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 0 
	And I want to show 0 decimals with value "0"
	When the format number is executed
	Then the result 789 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 0              | 0                |
	And the debug output as 
	|                   |
	| [[result]] = 789 |

Scenario: Format number rounding down 
	Given I have a number 788.894564545645
	And I selected rounding "Down" to 3 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.894 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Down     | 3              | 3                |
	And the debug output as 
	|                       |
	| [[result]] = 788.894 |

Scenario: Format number rounding up
	Given I have a number 788.894564545645
	And I selected rounding "Up" to 3 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.895 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Up       | 3              | 3                |
	And the debug output as 
	|                       |
	| [[result]] = 788.895 |

Scenario: Format number rounding normal
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 2 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.890 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 2              | 3                |
	And the debug output as 
	|                       |
	| [[result]] = 788.890 |

Scenario: Format number rounding none
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0 
	And I want to show 4 decimals decimals with value "4"
	When the format number is executed
	Then the result 788.8945 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | None     | 0              | 4                |
	And the debug output as 
	|                        |
	| [[result]] = 788.8945 |

Scenario: Format number rounding down to negative number
	Given I have a number 788.894564545645
	And I selected rounding "Down" to -2
	And I want to show 0 decimals decimals with value "0"
	When the format number is executed
	Then the result 700 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Down     | -2             | 0                |
	And the debug output as 
	|                   |
	| [[result]] = 700 |

Scenario: Format number large number to negative decimals
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result 7 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | None     | 0              | -2               |
	And the debug output as 
	|                 |
	| [[result]] = 7 |

Scenario: Format number single digit to negative decimals
	Given I have a number 7
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result 0 will be returned
	And the debug inputs as  
	| Number | Rounding | Rounding Value | Decimals to show |
	| 7      | None     | 0              | -2               |
	And the debug output as 
	|                 |
	| [[result]] = 0 |
	And the execution has "NO" error

Scenario: Format number rounding up to a character
	Given I have a number 34.2
	And I selected rounding "Up" to "c"
	And I want to show 2 decimals decimals with value "2"
	When the format number is executed
	Then the result "" will be returned
	And the execution has "AN" error
	And the debug inputs as  
	| Number | Rounding | Rounding Value | Decimals to show |
	| 34.2   | Up       | c              | 2                |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Format number that is blank
	Given I have a number ""
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error


Scenario: Format non numeric
	Given I have a number "asdf"
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error


Scenario: Format number to charater decimals
	Given I have a number 34.2
	And I selected rounding "Up" to "1"
	And I want to show "asdf" decimals decimals with value "asdf"
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error


Scenario: Format number with multipart variables and numbers for number rounding and decimals to show
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I have a formatnumber variable "[[rounding]]" equal to 2
	And I selected rounding "Up" to "-[[rounding]]"
	And I have a formatnumber variable "[[decimals]]" equal to "-"
	And I want to show "[[decimals]]1" decimals with value "[[decimals]]1" 
	When the format number is executed
	Then the result 80 will be returned
    And the execution has "NO" error
	And the debug inputs as  
	| Number                                  | Rounding | Rounding Value     | Decimals to show   |
	| [[int]].894564545645 = 788.894564545645 | Up       | -[[rounding]] = -2 | [[decimals]]1 = -1 |
	And the debug output as 
	|                  |
	| [[result]] = 80 |

Scenario: Format number with negative recordset index for number
	Given I have a number "[[my(-1).int]]"
	When the format number is executed
	Then the execution has "AN" error


Scenario: Format number with negative recordset index for rounding
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I selected rounding "Up" to "[[my(-1).rounding]]"
	When the format number is executed
	Then the execution has "AN" error


Scenario: Format number with negative recordset index for decimals to show
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I want to show "[[my(-1).decimals]]" decimals with value "[[my(-1).decimals]]"
	When the format number is executed
	Then the execution has "AN" error


Scenario: Format number with unknown scalar for rounding
	Given I have a formatnumber variable "[[int]]" equal to ""
	And I have a number ""
	And I selected rounding "up" to "[[var]]" equal to ""
	And I want to show "[[decimal]]" decimals with value "2" 
	When the format number is executed
	Then the execution has "AN" error

Scenario: Format number rounding with unknown scalar decimals value to show 
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 0 
	And I want to show "[[var]]" decimals with values ""
	When the format number is executed
	Then the result 789 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 0              | ""               |
	And the debug output as 
	|                   |
	| [[result]] = 789 |

@ignore
#Complex Types WOLF-1042
Scenario Outline: Format number using complex types 
	Given I have a number '<Number>'
	And I selected rounding '<Rounding>' to '<RoundingValue>' 
	And I want to show '<Decimals>' decimals with value '<DecimalVal>'
	When the format number is executed
	Then the result '<result>' will be returned
	And the execution has "<Error>" error
	And the debug inputs as  
	| Number   | Rounding   | RoundingValue   | Decimals to show |
	| <Number> | <Rounding> | <RoundingValue> | <DecimalVal>     |
	And the debug output as 
	| Result   |
	| <result> |
Examples: 
| Number           | Rounding | RoundingValue                       | Decimals               | DecimalVal | Error | Result                          |
| 788.894564545645 | Normal   | [[rs().set().value]] = 0            | [[rj().count().value]] | 0          | No    | [[rec(2).result().value]] = 789 |
| 788.894564545645 | Normal   | [[rs(1).set([[int]]).value(*).set]] | [[rj().count().value]] | 0          | No    | [[rec(1).result(2).value]] = 10 |
| 788.894564545645 | Normal   | [[rs(*).set().value()]]             | [[rj().count().value]] | 0          | No    | [[rec(2).result(1).value]] = 50 |
Scenario: Format a variable with a null value
	Given I have a formatnumber variable "[[int]]" equal to NULL
	And I have a number "[[int]]"
	And I want to show "2" decimals 
	When the format number is executed
	Then the execution has "AN" error

Scenario: Format a variable with a non existent value
	Given I have a number "[[int]]"
	And I want to show "2" decimals 
	When the format number is executed
	Then the execution has "AN" error
