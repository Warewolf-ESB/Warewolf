Feature: CaseConversion
	In order to convert the case of words
	As a Warewolf user
	I want a tool that converts words from their current case to a selected case


Scenario: Convert a sentence to uppercase
	Given I have a case convert variable "[[var]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[var]]" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF ROCKS"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                  | To    |
	| 1 | [[var]] = Warewolf Rocks | UPPER |
	And the debug output as  
	| # |                          |
	| 1 | [[var]] = WAREWOLF ROCKS | 

Scenario: Convert a sentence to lowercase
	Given I have a case convert variable "[[var]]" with a value of "Warewolf Rocks"	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be "warewolf rocks"
	And the execution has "NO" error
	And the debug inputs as  
	|#| Convert                  | To    |
	| 1 | [[var]] = Warewolf Rocks | lower |
	And the debug output as  
	| # |                          |
	| 1 | [[var]] = warewolf rocks |

Scenario: Convert a sentence to Sentence
	Given I have a case convert variable "[[var]]" with a value of "WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Sentence"
	When the case conversion tool is executed
	Then the sentence will be "Warewolf rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                  | To       |
	| 1 | [[var]] = WAREWOLF Rocks | Sentence |
	And the debug output as  
	| # |                          |
	| 1 | [[var]] = Warewolf rocks |

Scenario: Convert a sentence to Title Case
	Given I have a case convert variable "[[var]]" with a value of "WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF Rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                  | To         |
	| 1 | [[var]] = WAREWOLF Rocks | Title Case |
	And the debug output as  
	| # |                          |
	| 1 | [[var]] = WAREWOLF Rocks |

Scenario: Convert a sentence starting with a number to UPPER CASE
	Given I have a case convert variable "[[var]]" with a value of "1 Warewolf Rocks"	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF ROCKS"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | To    |
	| 1 | [[var]] = 1 Warewolf Rocks | UPPER |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 1 WAREWOLF ROCKS |

Scenario: Convert a sentence starting with a number to lower case
	Given I have a case convert variable "[[var]]" with a value of "1 Warewolf Rocks"	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | To    |
	| 1 | [[var]] = 1 Warewolf Rocks | lower |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 1 warewolf rocks |

Scenario: Convert a sentence starting with a number to Sentence case
	Given I have a case convert variable "[[var]]" with a value of "1 WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | To       |
	| 1 | [[var]] = 1 WAREWOLF Rocks | Sentence |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 1 warewolf rocks |

Scenario: Convert a sentence starting with a number to Title Case
	Given I have a case convert variable "[[var]]" with a value of "1 WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF Rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | To         |
	| 1 | [[var]] = 1 WAREWOLF Rocks | Title Case |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 1 WAREWOLF Rocks |

Scenario: Convert a blank to Title Case
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | To         |
	| 1 | [[var]] = | Title Case |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert a blank to Sentencecase
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "Sentence"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | To       |
	| 1 | [[var]] = | Sentence |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert a blank to UPPER CASE
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | To    |
	| 1 | [[var]] = | UPPER |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert a blank to lowercase
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | To    |
	| 1 | [[var]] = | lower |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert a recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "rs().row" will be
	| rs       | val                 |
	| rs().row | <X ID="1">ONE</X>   |
	| rs().row | <X ID="2">TWO</X>   |
	| rs().row | <X ID="3">THREE</X> |
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                             | To    |
	| 1 | [[rs(1).row]] = <x id="1">One</x>   |       |
	|   | [[rs(2).row]] = <x id="2">two</x>   |       |
	|   | [[rs(3).row]] = <x id="3">three</x> | UPPER |	
	And the debug output as  
	| # |                                     |
	| 1 | [[rs(1).row]] = <X ID="1">ONE</X>   |
	|   | [[rs(2).row]] = <X ID="2">TWO</X>   |
	|   | [[rs(3).row]] = <X ID="3">THREE</X> |

Scenario: Convert an empty recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | val |
	| rs().row |     |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "[[rs().row]]" will be
	| rs       | row                 |
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert         | To    |
	| 1 | [[rs(1).row]] = | UPPER |
	And the debug output as  
	| # |                 |
	| 1 | [[rs(1).row]] = |

Scenario: Convert a empty sentence starting with a number to upper
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | To    |
	| 1 | [[var]] = | UPPER |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert a negative recordset index to uppercase
	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "UPPER"		
	When the case conversion tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | To    |
	| 1 | [[my(-1).sentenct]] = | UPPER |
	And the debug output as  
	| # |                       |
	| 1 | [[my(-1).sentenct]] = |

Scenario: Convert a negative recordset index to lowercase
	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "lower"		
	When the case conversion tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | To    |
	| 1 | [[my(-1).sentenct]] = | lower |
	And the debug output as  
	| # |                       |
	| 1 | [[my(-1).sentenct]] = |
#
#Scenario Outline: Convert two variables in one row
#	Given I have a case convert variable "[[a]]" with a value of "Warewolf Rocks"
#	And I convert a variable "[[a]][[b]]" to '<Case>'		
#	When the case conversion tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| # | Convert      | To     |
#	| 1 | [[a]][[b]] = | <Case> |
#	And the debug output as  
#	| # |  |
#Examples: 
#    | Case       |
#    | UPPER      |
#    | Lower      |
#    | SENTENCE   |
#    | TITLE CASE |
#
#Scenario Outline: Convert variables with data in one row
#	Given I have a case convert variable "[[a]]" with a value of "Warewolf Rocks"
#	And I convert a variable "[[a]]test" to '<Case>'		
#	When the case conversion tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| # | Convert     | To     |
#	| 1 | [[a]]test = | <Case> |
#	And the debug output as  
#	| # |                       |
#Examples: 
#    | Case       |
#    | UPPER      |
#    | Lower      |
#    | SENTENCE   |
#    | TITLE CASE |
#
#
#

#Scenario Outline: Error messages when convert a Invalid variable
#	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
#	And I convert a variable '<Variable>' to '<To>"		
#	When the case conversion tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| # | Convert      | To     |
#	| 1 | <Variable> = | <Case> |
#	And the debug output as  
#	| # |           |
#	| 1 | <error> = |
#Examples: 
#	| No | Variable                                  | To    |
#	| 1  | [[my(-1).var]]                            | UPPER | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
#	| 2  | [[var  ]]                                 | UPPER | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
#	| 3  | [[my(%).var]]                             | UPPER | Recordset index (q) contains invalid character(s)                                                                                                                                                                                                       |
#	| 4  | [rec().a]]=]]                             | UPPER | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	| 5  | [[rec'()'.a]]                             | UPPER | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
#	| 6  | [[rec"()".a]]                             | UPPER | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
#	| 7  | [[rec".a]]                                | UPPER | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
#	| 8  | [[rec.a]]                                 | UPPER | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
#	| 9  | [[rec()*.a]]                              | UPPER | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
#	| 10 | [[rec().a]]*                              | UPPER | One variable only allowed in the output field                                                                                                                                                                                                           |
#	| 11 | [[1]]                                     | UPPER | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
#	| 12 | [[@]]                                     | UPPER | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
#	| 13 | [[var#]]                                  | UPPER | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
#	| 14 | [[var]]00]]                               | UPPER | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	| 15 | [[var]]@]]                                | UPPER | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	| 16 | [[var.()]]                                | UPPER | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
#	| 17 | [[]]                                      | UPPER | Variable [[]] is missing a name                                                                                                                                                                                                                         |
#	| 18 | [[()]]                                    | UPPER | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
#	| 19 | [[var[[a]*]]]                             | UPPER | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
#	| 20 | [[var[[]]                                 | UPPER | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
#	| 21 | [[var1.a]]                                | UPPER | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
#	| 22 | [[rec()!a]]                               | UPPER | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
#	| 23 | [[rec()         a]]                       | UPPER | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
#	| 24 | [[{{rec(_).a}}]]]                         | UPPER | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
#	| 25 | [[rec(23).[[var*]]]]                      | UPPER | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
#	| 26 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | UPPER | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
#	| 27 | [[rec().a]]&[[a]]                         | UPPER | One variable only allowed in the output field                                                                                                                                                                                                           |
#	| 28 | a[[rec([[[[b]]]]).a]]@                    | UPPER | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
#	| 29 | [[rec()                                   | UPPER | Recordset variable that needs a field name(s)                                                                                                                                                                                                           |
#
#
#
#










