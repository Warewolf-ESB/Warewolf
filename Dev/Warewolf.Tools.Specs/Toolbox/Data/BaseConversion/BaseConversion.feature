@Data
Feature: BaseConversion
	In order to convert base encoding types
	As a Warewolf user
	I want a tool that converts data from one base econding to another

Scenario: Convert from text to text 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert      | From | To   |
	| 1 | [[var]] = AA | Text | Text |
	And the debug output as  
	| # |              |
	| 1 | [[var]] = AA |

Scenario: Convert from text to binary 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert      | From | To     |
	| 1 | [[var]] = AA | Text | Binary |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 0100000101000001 |

Scenario: Convert from text to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert      | From | To  |
	| 1 | [[var]] = AA | Text | Hex |	
	And the debug output as  
	| # |                  |
	| 1 | [[var]] = 0x4141 |

Scenario: Convert from text to base64 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert      | From | To      |
	| 1 | [[var]] = AA | Text | Base 64 |
	And the debug output as  
	| # |                |
	| 1 | [[var]] = QUE= |

Scenario: Convert from binary to binary 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | From   | To     |
	| 1 | [[var]] = 0100000101000001 | Binary | Binary |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 0100000101000001 |

Scenario: Convert from binary to text 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | From   | To   |
	| 1 | [[var]] = 0100000101000001 | Binary | Text |
	And the debug output as  
	| # |              |
	| 1 | [[var]] = AA |

Scenario: Convert from binary to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | From   | To  |
	| 1 | [[var]] = 0100000101000001 | Binary | Hex |
	And the debug output as  
	| # |                  |
	| 1 | [[var]] = 0x4141 |

Scenario: Convert from binary to base64 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert                    | From   | To      |
	| 1 | [[var]] = 0100000101000001 | Binary | Base 64 |
	And the debug output as  
	| # |                |
	| 1 | [[var]] = QUE= |

Scenario: Convert from hexadecimal to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert          | From | To  |
	| 1 | [[var]] = 0x4141 | Hex  | Hex |
	And the debug output as  
	| # |                  |
	| 1 | [[var]] = 0x4141 |

Scenario: Convert from hexadecimal to text 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert          | From | To   |
	| 1 | [[var]] = 0x4141 | Hex  | Text |
	And the debug output as  
	| # |              |
	| 1 | [[var]] = AA |

Scenario: Convert from hexadecimal to binary 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert          | From | To     |
	| 1 | [[var]] = 0x4141 | Hex  | Binary |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 0100000101000001 |

Scenario: Convert from hexadecimal to base64 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert          | From | To      |
	| 1 | [[var]] = 0x4141 | Hex  | Base 64 |
	And the debug output as  
	| # |                |
	| 1 | [[var]] = QUE= |

Scenario: Convert from base64 to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert        | From    | To  |
	| 1 | [[var]] = QUE= | Base 64 | Hex |
	And the debug output as  
	| # |                  |
	| 1 | [[var]] = 0x4141 |

Scenario: Convert from base64 to text 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert        | From    | To   |
	| 1 | [[var]] = QUE= | Base 64 | Text |
	And the debug output as  
	| # |              |
	| 1 | [[var]] = AA |

Scenario: Convert from base64 to binary 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert        | From    | To     |
	| 1 | [[var]] = QUE= | Base 64 | Binary |
	And the debug output as  
	| # |                            |
	| 1 | [[var]] = 0100000101000001 |

Scenario: Convert from base64 to base64 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert        | From    | To      |
	| 1 | [[var]] = QUE= | Base 64 | Base 64 |
	And the debug output as  
	| # |                |
	| 1 | [[var]] = QUE= |

Scenario: Convert blank from text to binary 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To     |
	| 1 | [[var]] = | Text | Binary |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from text to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To  |
	| 1 | [[var]] = | Text | Hex |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from text to base64 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To      |
	| 1 | [[var]] = | Text | Base 64 |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from binary to text 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From   | To   |
	| 1 | [[var]] = | Binary | Text |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from binary to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From   | To  |
	| 1 | [[var]] = | Binary | Hex |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from binary to base64 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From   | To      |
	| 1 | [[var]] = | Binary | Base 64 |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from hexadecimal to text 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To   |
	| 1 | [[var]] = | Hex  | Text |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from hexadecimal to binary 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To     |
	| 1 | [[var]] = | Hex  | Binary |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from hexadecimal to base64 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From | To      |
	| 1 | [[var]] = | Hex  | Base 64 |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from base64 to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From    | To  |
	| 1 | [[var]] = | Base 64 | Hex |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from base64 to text 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From    | To   |
	| 1 | [[var]] = | Base 64 | Text |
	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert blank from base64 to binary 
	Given I have a convert variable "[[var]]" with a value of "blank"
	And I convert a variable "[[var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error
	And the debug inputs as  
	| # | Convert   | From    | To     |
	| 1 | [[var]] = | Base 64 | Binary |
 	And the debug output as  
	| # |           |
	| 1 | [[var]] = |

Scenario: Convert negative recordset index from text to binary 
	Given I have a convert variable "[[my(-1).var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To     |
	| 1 | [[my(-1).var]] = | Text | Binary |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from text to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To  |
	| 1 | [[my(-1).var]] = | Text | Hex |
	And the debug output as  
	| # |           |
	
Scenario: Convert negative recordset index from text to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To      |
	| 1 | [[my(-1).var]] = | Text | Base 64 |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from binary to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From   | To   |
	| 1 | [[my(-1).var]] = | Binary | Text |
	And the debug output as  
	| # |                  |
		
Scenario: Convert negative recordset index from binary to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001""
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From   | To  |
	| 1 | [[my(-1).var]] = | Binary | Hex |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from binary to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From   | To      |
	| 1 | [[my(-1).var]] = | Binary | Base 64 |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from hexadecimal to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To   |
	| 1 | [[my(-1).var]] = | Hex  | Text |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from hexadecimal to binary 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To     |
	| 1 | [[my(-1).var]] = | Hex  | Binary |
	And the debug output as  
	| # |           |

Scenario: Convert negative recordset index from hexadecimal to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From | To      |
	| 1 | [[my(-1).var]] = | Hex  | Base 64 |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from base64 to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From    | To  |
	| 1 | [[my(-1).var]] = | Base 64 | Hex |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from base64 to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From    | To   |
	| 1 | [[my(-1).var]] = | Base 64 | Text |
	And the debug output as  
	|  |                  |	

Scenario: Convert negative recordset index from base64 to binary 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert          | From    | To     |
	| 1 | [[my(-1).var]] = | Base 64 | Binary |
	And the debug output as  
	|   |              |


Scenario Outline: Converting two varibles on one row 
	Given I have a convert variable "[[a]]" with a value of "QUE="
	And I have a convert variable "[[b]]" with a value of "QUE="
	And I convert a variable "[[a]][[b]]" from type "<From>" to type "<To>" 
	When the base conversion tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | From   | To   |
	| 1 | [[a]][[b]] = QUE=QUE= | <From> | <To> |
Examples: 
	| No | From         | To      |
	| 1  | Base 64      | Binary  |
	| 2  | Base 64      | Text    |
	| 3  | Base 64      | Hex     |
	| 4  | Binary       | Text    |
	| 5  | Binary       | Hex     |
	| 6  | Binary       | Base 64 |
	| 7  | Text         | Binary  |
	| 8  | Text         | Hex     |
	| 9  | Text         | Base 64 |
	| 10 | Hex          | Binary  |
	| 11 | Hex          | Text    |
	| 12 | Hex          | Base 64 |

#Bug 12177
Scenario Outline: Converting varibles with data  
	Given I have a convert variable "[[a]]" with a value of "QUE="
	And I convert a variable "[[a]]test" from type "<From>" to type "<To>" 
	When the base conversion tool is executed
	Then the execution has "AN" error
Examples: 
	| no | From         | To      |
	| 1  | Base 64      | Binary  |
	| 2  | Base 64      | Text    |
	| 3  | Base 64      | Hex     |
	| 4  | Base 64      | Base 64 |
	| 5  | Binary       | Binary  |
	| 6  | Binary       | Text    |
	| 7  | Binary       | Hex     |
	| 8  | Binary       | Base 64 |
	| 9  | Text         | Binary  |
	| 10 | Text         | Text    |
	| 11 | Text         | Hex     |
	| 12 | Text         | Base 64 |
	| 13 | Hex          | Binary  |
	| 14 | Hex          | Text    |
	| 15 | Hex          | Hex     |
	| 16 | Hex          | Base 64 |



Scenario Outline: Validation messages when Convert Invalid Variables  
	Given I have a convert variable "<Variable>" with a value of "<Value>"
	And I convert a variable "<Variable>" from type "<From>" to type "<To>" 	
	When the base conversion tool is executed
	Then the execution has "AN" error
Examples: 
	| No  | Variable                                  | Value            | From    | To      | 
	| 1   | [[my(-1).var]]                            | QUE=             | Base 64 | Binary  | 
	| 2   | [[my(-1).var]]                            | QUE=             | Base 64 | Text    | 
	| 3   | [[my([-1]).var]]                          | QUE=             | Base 64 | Hex     | 
	| 4   | [rec().a]]=]]                             | QUE=             | Base 64 | Binary  | 
	| 5   | [[rec"()".a]]                             | QUE=             | Base 64 | Text    | 
	| 6   | [[rec"()".a]]                             | QUE=             | Base 64 | Hex     | 
	| 7   | [[rec".a]]                                | QUE=             | Base 64 | Binary  | 
	| 8   | [[rec.a]]                                 | QUE=             | Base 64 | Text    | 
	| 9   | [[rec()*.a]]                              | QUE=             | Base 64 | Hex     | 
	| 10  | [[rec().a]]*                              | QUE=             | Base 64 | Binary  | 
	| 11  | [[1]]                                     | QUE=             | Base 64 | Text    | 
	| 12  | [[@]]                                     | QUE=             | Base 64 | Hex     | 
	| 13  | [[var#]]                                  | QUE=             | Base 64 | Binary  | 
	| 14  | [[var]]00]]                               | QUE=             | Base 64 | Text    | 
	| 15  | [[var]]@]]                                | QUE=             | Base 64 | Hex     | 
	| 16  | [[var.()]]                                | QUE=             | Base 64 | Binary  | 
	| 17  | [[]]                                      | QUE=             | Base 64 | Text    | 
	| 18  | [[()]]                                    | QUE=             | Base 64 | Hex     | 
	| 19  | [[var[[a]*]]]                             | QUE=             | Base 64 | Binary  | 
	| 20  | [[var[[]]                                 | QUE=             | Base 64 | Text    | 
	| 21  | [[var1.a]]                                | QUE=             | Base 64 | Hex     | 
	| 22  | [[rec()!a]]                               | QUE=             | Base 64 | Binary  | 
	| 23  | [[rec()         a]]                       | QUE=             | Base 64 | Text    | 
	| 24  | [[{{rec(_).a}}]]]                         | QUE=             | Base 64 | Hex     | 
	| 25  | [[rec(23).[[var*]]]]                      | QUE=             | Base 64 | Binary  | 
	| 26  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | QUE=             | Base 64 | Text    | 
	| 27  | [[rec().a]]&[[a]]                         | QUE=             | Base 64 | Hex     | 
	| 28  | a[[rec([[[[b]]]]).a]]@                    | QUE=             | Base 64 | Binary  | 
	| 29  | [[var  ]]                                 | QUE=             | Base 64 | Text    | 
	| 30  | [[my(-1).var]]                            | AA               | Text    | Binary  | 
	| 31  | [[my(-1).var]]                            | AA               | Text    | Hex     | 
	| 32  | [[my(-1).var]]                            | AA               | Text    | Base 64 | 
	| 33  | [[rec"()".a]]                             | AA               | Text    | Text    | 
	| 34  | [[rec"()".a]]                             | AA               | Text    | Hex     | 
	| 35  | [[rec".a]]                                | AA               | Text    | Base 64 | 
	| 36  | [[rec.a]]                                 | AA               | Text    | Text    | 
	| 37  | [[rec()*.a]]                              | AA               | Text    | Hex     | 
	| 38  | [[rec().a]]*                              | AA               | Text    | Base 64 | 
	| 39  | [[1]]                                     | AA               | Text    | Base 64 | 
	| 40  | [[@]]                                     | AA               | Text    | Hex     | 
	| 41  | [[var#]]                                  | AA               | Text    | Base 64 | 
	| 42  | [[var]]00]]                               | AA               | Text    | Text    | 
	| 43  | [[var]]@]]                                | AA               | Text    | Hex     | 
	| 44  | [[var.()]]                                | AA               | Text    | Binary  | 
	| 45  | [[]]                                      | AA               | Text    | Base 64 | 
	| 46  | [[()]]                                    | AA               | Text    | Hex     | 
	| 47  | 19                                        | AA               | Text    | Base 64 | 
	| 48  | [[var[[]]                                 | AA               | Text    | Text    | 
	| 49  | [[var1.a]]                                | AA               | Text    | Hex     | 
	| 50  | [[rec()!a]]                               | AA               | Text    | Binary  | 
	| 51  | [[rec()         a]]                       | AA               | Text    | Base 64 | 
	| 52  | [[{{rec(_).a}}]]]                         | AA               | Text    | Hex     | 
	| 53  | [[rec(23).[[var*]]]]                      | AA               | Text    | Binary  | 
	| 54  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | AA               | Text    | Binary  | 
	| 55  | [[rec().a]]&[[a]]                         | AA               | Text    | Hex     | 
	| 56  | a[[rec([[[[b]]]]).a]]@                    | AA               | Text    | Binary  | 
	| 57  | [[var  ]]                                 | AA               | Text    | Base 64 | 
	| 58  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Text    | 
	| 59  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Hex     | 
	| 60  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Base 64 | 
	| 61  | [[rec"()".a]]                             | 0100000101000001 | Binary  | Text    | 
	| 62  | [[rec"()".a]]                             | 0100000101000001 | Binary  | Hex     | 
	| 63  | [[rec".a]]                                | 0100000101000001 | Binary  | Base 64 | 
	| 64  | [[rec.a]]                                 | 0100000101000001 | Binary  | Text    | 
	| 65  | [[rec()*.a]]                              | 0100000101000001 | Binary  | Hex     | 
	| 66  | [[rec().a]]*                              | 0100000101000001 | Binary  | Base 64 | 
	| 67  | [[1]]                                     | 0100000101000001 | Binary  | Base 64 | 
	| 68  | [[@]]                                     | 0100000101000001 | Binary  | Hex     | 
	| 69  | [[var#]]                                  | 0100000101000001 | Binary  | Base 64 | 
	| 70  | [[var]]00]]                               | 0100000101000001 | Binary  | Text    | 
	| 71  | [[var]]@]]                                | 0100000101000001 | Binary  | Hex     | 
	| 72  | [[var.()]]                                | 0100000101000001 | Binary  | Hex     | 
	| 73  | [[]]                                      | 0100000101000001 | Binary  | Base 64 | 
	| 74  | [[()]]                                    | 0100000101000001 | Binary  | Hex     | 
	| 75  | 19                                        | 0100000101000001 | Binary  | Base 64 | 
	| 75  | [[var[[]]                                 | 0100000101000001 | Binary  | Text    | 
	| 77  | [[var1.a]]                                | 0100000101000001 | Binary  | Hex     | 
	| 78  | [[rec()!a]]                               | 0100000101000001 | Binary  | Hex     | 
	| 79  | [[rec()         a]]                       | 0100000101000001 | Binary  | Base 64 | 
	| 80  | [[{{rec(_).a}}]]]                         | 0100000101000001 | Binary  | Hex     | 
	| 81  | [[rec(23).[[var*]]]]                      | 0100000101000001 | Binary  | Hex     | 
	| 82  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | 0100000101000001 | Binary  | Hex     | 
	| 83  | [[rec().a]]&[[a]]                         | 0100000101000001 | Binary  | Hex     | 
	| 84  | a[[rec([[[[b]]]]).a]]@                    | 0100000101000001 | Binary  | Hex     | 
	| 85  | [[var  ]]                                 | 0100000101000001 | Binary  | Base 64 | 
	| 86  | [[my(-1).var]]                            | 0x4141           | Hex     | Binary  | 
	| 87  | [[my(-1).var]]                            | 0x4141           | Hex     | Base 64 | 
	| 88  | [[my(-1).var]]                            | 0x4141           | Hex     | Text    | 
	| 89  | [[rec"()".a]]                             | 0x4141           | Hex     | Text    | 
	| 90  | [[rec"()".a]]                             | 0x4141           | Hex     | Base 64 | 
	| 91  | [[rec".a]]                                | 0x4141           | Hex     | Base 64 | 
	| 92  | [[rec.a]]                                 | 0x4141           | Hex     | Text    | 
	| 93  | [[rec()*.a]]                              | 0x4141           | Hex     | Base 64 | 
	| 94  | [[rec().a]]*                              | 0x4141           | Hex     | Base 64 | 
	| 95  | [[1]]                                     | 0x4141           | Hex     | Base 64 | 
	| 96  | [[@]]                                     | 0x4141           | Hex     | Hex     | 
	| 97  | [[var#]]                                  | 0x4141           | Hex     | Base 64 | 
	| 98  | [[var]]00]]                               | 0x4141           | Hex     | Text    | 
	| 99  | [[var]]@]]                                | 0x4141           | Hex     | Binary  | 
	| 100 | [[var.()]]                                | 0x4141           | Hex     | Binary  | 
	| 101 | [[]]                                      | 0x4141           | Hex     | Base 64 | 
	| 102 | [[()]]                                    | 0x4141           | Hex     | Binary  | 
	| 103 | 19                                        | 0x4141           | Hex     | Base 64 | 
	| 104 | [[var[[]]                                 | 0x4141           | Hex     | Text    | 
	| 105 | [[var1.a]]                                | 0x4141           | Hex     | Binary  | 
	| 106 | [[rec()!a]]                               | 0x4141           | Hex     | Binary  | 
	| 107 | [[rec()         a]]                       | 0x4141           | Hex     | Base 64 | 
	| 108 | [[{{rec(_).a}}]]]                         | 0x4141           | Hex     | Binary  | 
	| 109 | [[rec(23).[[var*]]]]                      | 0x4141           | Hex     | Binary  | 
	| 110 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1   | 0x4141           | Hex     | Binary  | 

Scenario: Convert a Variable That Does Not Exist
	Given I have a convert variable "[[var]]" with a value of "[[a]]"
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the execution has "AN" error
 
Scenario Outline: Convert an empty recordset * 
	Given I have a convert variable "<Variable>" with a value of "<Value>"
	And I convert a variable "<Variable>" from type "<From>" to type "<To>" 
	When the base conversion tool is executed
	Then the execution has "AN" error
Examples: 
	| No | Variable            | Value | From    | To      | Error             |
	| 1  | [[rs(*).row]]       |       | Binary  | Binary  | Invalid Recordset |
	| 2  | [[rs(*).row]]       |       | Binary  | Text    | Invalid Recordset |
	| 3  | [[rs(*).row]]       |       | Binary  | Hex     | Invalid Recordset |
	| 4  | [[rs(*).row]]       |       | Binary  | Base 64 | Invalid Recordset |
	| 5  | [[rs(*).row]]       |       | Text    | Binary  | Invalid Recordset |
	| 6  | [[rs(*).row]]       |       | Text    | Text    | Invalid Recordset |
	| 7  | [[rs(*).row]]       |       | Text    | Hex     | Invalid Recordset |
	| 8  | [[rs(*).row]]       |       | Text    | Base 64 | Invalid Recordset |
	| 9  | [[rs(*).row]]       |       | Hex     | Binary  | Invalid Recordset |
	| 10 | [[rs(*).row]]       |       | Hex     | Text    | Invalid Recordset |
	| 11 | [[rs(*).row]]       |       | Hex     | Hex     | Invalid Recordset |
	| 12 | [[rs(*).row]]       |       | Hex     | Base 64 | Invalid Recordset |
	| 13 | [[rs(*).row]]       |       | Base 64 | Binary  | Invalid Recordset |
	| 14 | [[rs(*).row]]       |       | Base 64 | Text    | Invalid Recordset |
	| 15 | [[rs(*).row]]       |       | Base 64 | Hex     | Invalid Recordset |
	| 16 | [[rs(*).row]]       |       | Base 64 | Base 64 | Invalid Recordset |
	| 17 | [[rs([[var]]).row]] |       | Binary  | Binary  | Invalid Index     |
	| 18 | [[rs([[var]]).row]] |       | Binary  | Text    | Invalid Index     |
	| 19 | [[rs([[var]]).row]] |       | Binary  | Hex     | Invalid Index     |
	| 20 | [[rs([[var]]).row]] |       | Binary  | Base 64 | Invalid Index     |
	| 21 | [[rs([[var]]).row]] |       | Text    | Binary  | Invalid Index     |
	| 22 | [[rs([[var]]).row]] |       | Text    | Text    | Invalid Index     |
	| 23 | [[rs([[var]]).row]] |       | Text    | Hex     | Invalid Index     |
	| 24 | [[rs([[var]]).row]] |       | Text    | Base 64 | Invalid Index     |
	| 25 | [[rs([[var]]).row]] |       | Hex     | Binary  | Invalid Index     |
	| 26 | [[rs([[var]]).row]] |       | Hex     | Text    | Invalid Index     |
	| 27 | [[rs([[var]]).row]] |       | Hex     | Hex     | Invalid Index     |
	| 28 | [[rs([[var]]).row]] |       | Hex     | Base 64 | Invalid Index     |
	| 29 | [[rs([[var]]).row]] |       | Base 64 | Binary  | Invalid Index     |
	| 30 | [[rs([[var]]).row]] |       | Base 64 | Text    | Invalid Index     |
	| 31 | [[rs([[var]]).row]] |       | Base 64 | Hex     | Invalid Index     |
	| 32 | [[rs([[var]]).row]] |       | Base 64 | Base 64 | Invalid Index     |
	| 33 | [[rs().row]]        |       | Binary  | Binary  | Invalid Recordset |
	| 34 | [[rs().row]]        |       | Binary  | Text    | Invalid Recordset |
	| 35 | [[rs().row]]        |       | Binary  | Hex     | Invalid Recordset |
	| 36 | [[rs().row]]        |       | Binary  | Base 64 | Invalid Recordset |
	| 37 | [[rs().row]]        |       | Text    | Binary  | Invalid Recordset |
	| 38 | [[rs().row]]        |       | Text    | Text    | Invalid Recordset |
	| 39 | [[rs().row]]        |       | Text    | Hex     | Invalid Recordset |
	| 40 | [[rs().row]]        |       | Text    | Base 64 | Invalid Recordset |
	| 41 | [[rs().row]]        |       | Hex     | Binary  | Invalid Recordset |
	| 42 | [[rs().row]]        |       | Hex     | Text    | Invalid Recordset |
	| 43 | [[rs().row]]        |       | Hex     | Hex     | Invalid Recordset |
	| 44 | [[rs().row]]        |       | Hex     | Base 64 | Invalid Recordset |
	| 45 | [[rs().row]]        |       | Base 64 | Binary  | Invalid Recordset |
	| 46 | [[rs().row]]        |       | Base 64 | Text    | Invalid Recordset |
	| 47 | [[rs().row]]        |       | Base 64 | Hex     | Invalid Recordset |
	| 48 | [[rs().row]]        |       | Base 64 | Base 64 | Invalid Recordset |
 

Scenario: Convert a Variable that is null 
	Given I have a convert variable "[[var]]" with a value of "NULL"
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the execution has "AN" error
	And the execution has "Scalar value {[[var]]} is NULL" error