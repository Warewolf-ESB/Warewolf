﻿Feature: Replace
	In order to search and replace
	As a Warewolf user
	I want a tool I can use to search and replace for worsd

#Replace placeholders in a sentence with names
#Replace when the in field(s) is blank
#Replace when text to find is blank 
#Replace when the replace with is blank
#Replace using lower case to find uppercase value
#Replace when text to find is negative recordset index
#Replace when the replace with is negative recordset index
#Replace when negative recordset index is input
#Ensuring recordsets work as a Result



Scenario: Replace placeholders in a sentence with names
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the replace result should be "1"
	And "[[sentence]]" should be "Dear Mr Warewolf user, We welcome you as a customer"
	And the execution has "NO" error
	And the debug inputs as 
	| In Field(s)                                               | Find | Replace With  |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | XXXX | Warewolf user |
	And the debug output as 
	|                                                                    |                |
	| [[sentence]] = Dear Mr Warewolf user, We welcome you as a customer | [[result]] = 1 |

Scenario: Replace when the in field(s) is blank
	Given I have a replace variable "[[sentence]]" equal to "blank"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the replace result should be "0"
	And "[[sentence]]" should be ""
	And the execution has "NO" error
	And the debug inputs as 
	| In Field(s)    | Find | Replace With |
	| [[sentence]] = | XXXX | Warewolf user |
	And the debug output as 
	|                |  |
	| [[result]] = 0 |  |

Scenario: Replace when text to find is blank 
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters ""
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the replace result should be "0"
	And "[[sentence]]" should be "Dear Mr XXXX, We welcome you as a customer"
	And the execution has "NO" error
	And the debug inputs as 
	| In Field(s)                                               | Find | Replace With  |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | ""   | Warewolf user |
	And the debug output as 
	|                |
	| [[result]] = 0 |

Scenario: Replace when the replace with is blank
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with ""
	When the replace tool is executed
	Then the replace result should be "1"
	And "[[sentence]]" should be "Dear Mr , We welcome you as a customer"
	And the execution has "NO" error
	And the debug inputs as 
	| In Field(s)                                               | Find | Replace With |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | XXXX | ""           |
	And the debug output as 
	|                                                       |               | 
	| [[sentence]] = Dear Mr , We welcome you as a customer |[[result]] = 1 | 

Scenario: Replace using lower case to find uppercase value
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr AAAA, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I have a replace variable "[[find]]" equal to "AAAA"
	And I want to find the characters "[[find]]"
	And I want to replace them with "Case"
	When the replace tool is executed
	Then the replace result should be "1"
	And "[[sentence]]" should be "Dear Mr Case, We welcome you as a customer"
	And the execution has "NO" error
	And the debug inputs as 
	| In Field(s)                                               | Find            | Replace With |
	| [[sentence]] = Dear Mr AAAA, We welcome you as a customer | [[find]] = AAAA | Case         |
	And the debug output as 
	|                                                           |                |
	| [[sentence]] = Dear Mr Case, We welcome you as a customer | [[result]] = 1 |

Scenario: Replace when text to find is negative recordset index
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "[[my(-1).text]]"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the execution has "AN" error
	

Scenario: Replace when the replace with is negative recordset index
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "[[my(-1).text]]"
	When the replace tool is executed
	Then the execution has "AN" error

Scenario: Replace when negative recordset index is input
	Given I have a sentence "[[my(-1).sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "YYYY"
	When the replace tool is executed
	Then the execution has "AN" error

#Scenario: Replace when undifined recordset index is input
#	Given I have a sentence "[[L]]"
#	And I want to find the characters "XXXX"
#	And I want to replace them with "Parker"
#	When the replace tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as 
#	| In Field(s) | Find | Replace With | Error                                                 |
#	| [[L]]       | XXXX | Parker       | The given variable is not represent in the dictionary |
#

Scenario Outline:  Ensuring recordsets work as a Result
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I have a replace variable "<value>" equal to "<replace>"
	And I have a replace variable "[[var]]" equal to "1"
	And I want to find the characters "XXXX"
	And I want to replace them with "<value>"
	And replace result is "<resultVar>"
	When the replace tool is executed
	Then the execution has "NO" error
	And the result variable '<resultVar>' will be '<result>'
Examples: 
| value         | replace | resultVar               | result | output                                                    |
| [[text]]      | West    | [[rec().string]]        | 1      | [[sentence]] = Dear Mr West, We welcome you as a customer |
| [[rs(*).val]] | Wii     | [[rec().string]]        | 1      | [[sentence]] = Dear Mr Wii, We welcome you as a customer  |
| [[text]]      | West    | [[rec(1).string]]       | 1      | [[sentence]] = Dear Mr West, We welcome you as a customer |
| [[text]]      | West    | [[rec([[var]]).string]] | 1      | [[sentence]] = Dear Mr West, We welcome you as a customer |
| [[text]]      | West    | [[rec(*).string]]       | 1      | [[sentence]] = Dear Mr West, We welcome you as a customer |
| [[text]]      | 12      | [[var]]                 | 1      | [[sentence]] = Dear Mr 12, We welcome you as a customer   |

#Complex types WOLF-1042
@ignore
Scenario Outline: Replace values using complex types
	Given I have a replace variable "<var>" equal to "<value>"
	And I have a sentence "<var>"
	And I want to find the characters "<characters>" 
	And I want to replace them with "<replacement>" 
	When the replace tool is executed
	Then the replace result should be "<count>"
	And "<var>" should be "<result>"
	And the execution has "NO" error
Examples: 
| No | var                                | value    | characters | replacement | count | result    |
| 1  | [[Member().Details().Title         | Ms.Wolfe | Ms.Wolfe   | Mrs.Blake   | 1     | Mrs.Blake |
| 2  | [[Member(1).Details(1).Title       | Mr West  | e          | ei          | 1     | Mr Weist  |
| 3  | [[Member(*).Details([[int]]).Title | Dave     | D          | Ste         | 1     | Steave    |
