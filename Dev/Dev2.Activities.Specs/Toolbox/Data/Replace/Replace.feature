Feature: Replace
	In order to search and replace
	As a Warewolf user
	I want a tool I can use to search and replace for words


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
	| In Field(s)                                               | Find | Replace With |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | XXXX | Warewolf user |
	And the debug output as 
	|                                                                    |                |
	| [[sentence]] = Dear Mr Warewolf user, We welcome you as a customer | [[result]] = 1 |

Scenario: Replace when the in field(s) is blank
	Given I have a replace variable "[[sentence]]" equal to ""
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
	| In Field(s)                                               | Find | Replace With |
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
	And the debug inputs as 
	| In Field(s)                                               | Find              | Replace With  |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | [[my(-1).text]] = | Warewolf user |
	And the debug output as 
	|                |
	| [[result]] =  |

Scenario: Replace when the replace with is negative recordset index
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "[[my(-1).text]]"
	When the replace tool is executed
	Then the execution has "AN" error
	And the debug inputs as 
	| In Field(s)                                               | Find | Replace With      |
	| [[sentence]] = Dear Mr XXXX, We welcome you as a customer | XXXX | [[my(-1).text]] = |
	And the debug output as 
	|                 |
	| [[result]] =  |

Scenario: Replace when negative recordset index is input
	Given I have a sentence "[[my(-1).sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "YYYY"
	When the replace tool is executed
	Then the execution has "AN" error
	And the debug inputs as 
	| In Field(s)           | Find | Replace With |
	| [[my(-1).sentence]] = | XXXX | YYYY         |
	And the debug output as 
	|                |
	| [[result]] =  |