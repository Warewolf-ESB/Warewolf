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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	Given I have a convert variable "[[var]]" with a value of ""
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
	| # |                  |	

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
	| #  |              |


#This Test scenarios should be passed after the bug 11994 is fixed
Scenario Outline: Converting two varibles on one row 
	Given I have a convert variable "[[a]]" with a value of "QUE="
	And I have a convert variable "[[b]]" with a value of "QUE="
	And I convert a variable "[[a]][[b]]" from type '<From>' to type '<To>' 
	When the base conversion tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | From   | To   |
	| 1 | [[a]][[b]] = QUE=QUE= | <From> | <To> |
	And the debug output as  
	| #  |              |
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
#Scenario Outline: Converting varibles with data  
#	Given I have a convert variable "[[a]]" with a value of "QUE="
#	And I convert a variable "[[a]]test" from type '<From>' to type '<To>' 
#	When the base conversion tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| #  | Convert      | From    | To   |
#	| 1  | [[a]]test =  | <From>  | <To> |
#	And the debug output as  
#	| #  |              |
#Examples: 
#	| no | From         | To      |
#	| 1  | Base 64      | Binary  |
#	| 2  | Base 64      | Text    |
#	| 3  | Base 64      | Hex     |
#	| 4  | Base 64      | Base 64 |
#	| 5  | Binary       | Binary  |
#	| 6  | Binary       | Text    |
#	| 7  | Binary       | Hex     |
#	| 8  | Binary       | Base 64 |
#	| 9  | Text         | Binary  |
#	| 10 | Text         | Text    |
#	| 11 | Text         | Hex     |
#	| 12 | Text         | Base 64 |
#	| 13 | Hex          | Binary  |
#	| 14 | Hex          | Text    |
#	| 15 | Hex          | Hex     |
#	| 16 | Hex          | Base 64 |
#


Scenario Outline: Validation messages when Convert Invalid Variables  
	Given I have a convert variable '<Variable>' with a value of '<Value>'
	And I convert a variable '<Variable>' from type '<From>' to type '<To>' 	
	When the base conversion tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| #  | Convert      | From    | To   |
	| 1  | <Variable> = | <From>  | <To> |
	And the debug output as  
	| #   |   |
Examples: 
	| No  | Variable                                  | Value            | From    | To      | Error                                                                                                                                                                                                                                                   |
	| 1   | [[my(-1).var]]                            | QUE=             | Base 64 | Binary  | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
	| 2   | [[my(-1).var]]                            | QUE=             | Base 64 | Text    | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
	| 3   | [[my([-1]).var]]                          | QUE=             | Base 64 | Hex     | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
	| 4   | [rec().a]]=]]                             | QUE=             | Base 64 | Binary  | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 5   | [[rec'()'.a]]                             | QUE=             | Base 64 | Text    | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
	| 6   | [[rec"()".a]]                             | QUE=             | Base 64 | Hex     | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
	| 7   | [[rec".a]]                                | QUE=             | Base 64 | Binary  | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 8   | [[rec.a]]                                 | QUE=             | Base 64 | Text    | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
	| 9   | [[rec()*.a]]                              | QUE=             | Base 64 | Hex     | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
	| 10  | [[rec().a]]*                              | QUE=             | Base 64 | Binary  | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 11  | [[1]]                                     | QUE=             | Base 64 | Text    | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
	| 12  | [[@]]                                     | QUE=             | Base 64 | Hex     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
	| 13  | [[var#]]                                  | QUE=             | Base 64 | Binary  | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
	| 14  | [[var]]00]]                               | QUE=             | Base 64 | Text    | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 15  | [[var]]@]]                                | QUE=             | Base 64 | Hex     | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 16  | [[var.()]]                                | QUE=             | Base 64 | Binary  | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
	| 17  | [[]]                                      | QUE=             | Base 64 | Text    | Variable [[]] is missing a name                                                                                                                                                                                                                         |
	| 18  | [[()]]                                    | QUE=             | Base 64 | Hex     | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
	| 19  | [[var[[a]*]]]                             | QUE=             | Base 64 | Binary  | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
	| 20  | [[var[[]]                                 | QUE=             | Base 64 | Text    | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
	| 21  | [[var1.a]]                                | QUE=             | Base 64 | Hex     | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 22  | [[rec()!a]]                               | QUE=             | Base 64 | Binary  | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
	| 23  | [[rec()         a]]                       | QUE=             | Base 64 | Text    | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
	| 24  | [[{{rec(_).a}}]]]                         | QUE=             | Base 64 | Hex     | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
	| 25  | [[rec(23).[[var*]]]]                      | QUE=             | Base 64 | Binary  | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
	| 26  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | QUE=             | Base 64 | Text    | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
	| 27  | [[rec().a]]&[[a]]                         | QUE=             | Base 64 | Hex     | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 28  | a[[rec([[[[b]]]]).a]]@                    | QUE=             | Base 64 | Binary  | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
	| 29  | [[var  ]]                                 | QUE=             | Base 64 | Text    | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
	| 30  | [[my(-1).var]]                            | AA               | Text    | Binary  | Recordset index [-1] is not greater than zero                                                                                                                                                                                                           |
	| 31  | [[my(-1).var]]                            | AA               | Text    | Hex     | Recordset index [-1] is not greater than zero                                                                                                                                                                                                           |
	| 32  | [[my(-1).var]]                            | AA               | Text    | Base 64 | Recordset index [-1] is not greater than zero                                                                                                                                                                                                           |
	| 33  | [[rec'()'.a]]                             | AA               | Text    | Text    | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
	| 34  | [[rec"()".a]]                             | AA               | Text    | Hex     | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
	| 35  | [[rec".a]]                                | AA               | Text    | Base 64 | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 36  | [[rec.a]]                                 | AA               | Text    | Text    | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
	| 37  | [[rec()*.a]]                              | AA               | Text    | Hex     | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
	| 38  | [[rec().a]]*                              | AA               | Text    | Base 64 | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 39  | [[1]]                                     | AA               | Text    | Base 64 | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
	| 40  | [[@]]                                     | AA               | Text    | Hex     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
	| 41  | [[var#]]                                  | AA               | Text    | Base 64 | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
	| 42  | [[var]]00]]                               | AA               | Text    | Text    | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 43  | [[var]]@]]                                | AA               | Text    | Hex     | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 44  | [[var.()]]                                | AA               | Text    | Binary  | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
	| 45  | [[]]                                      | AA               | Text    | Base 64 | Variable [[]] is missing a name                                                                                                                                                                                                                         |
	| 46  | [[()]]                                    | AA               | Text    | Hex     | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
	#| 47  | 19                                        | AA               | Text    | Base 64 | [[var[[a]]]]                                                                                                                                                                                                                                            |
	| 48  | [[var[[]]                                 | AA               | Text    | Text    | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
	| 49  | [[var1.a]]                                | AA               | Text    | Hex     | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 50  | [[rec()!a]]                               | AA               | Text    | Binary  | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
	| 51  | [[rec()         a]]                       | AA               | Text    | Base 64 | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
	| 52  | [[{{rec(_).a}}]]]                         | AA               | Text    | Hex     | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
	| 53  | [[rec(23).[[var*]]]]                      | AA               | Text    | Binary  | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
	| 54  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | AA               | Text    | Binary  | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
	| 55  | [[rec().a]]&[[a]]                         | AA               | Text    | Hex     | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 56  | a[[rec([[[[b]]]]).a]]@                    | AA               | Text    | Binary  | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
	| 57  | [[var  ]]                                 | AA               | Text    | Base 64 | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
	| 58  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Text    | Recordset index [-1] is not greater than zer                                                                                                                                                                                                            |
	| 59  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Hex     | Recordset index [-1] is not greater than zer                                                                                                                                                                                                            |
	| 60  | [[my(-1).var]]                            | 0100000101000001 | Binary  | Base 64 | Recordset index [-1] is not greater than zer                                                                                                                                                                                                            |
	| 61  | [[rec'()'.a]]                             | 0100000101000001 | Binary  | Text    | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
	| 62  | [[rec"()".a]]                             | 0100000101000001 | Binary  | Hex     | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
	| 63  | [[rec".a]]                                | 0100000101000001 | Binary  | Base 64 | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 64  | [[rec.a]]                                 | 0100000101000001 | Binary  | Text    | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
	| 65  | [[rec()*.a]]                              | 0100000101000001 | Binary  | Hex     | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
	| 66  | [[rec().a]]*                              | 0100000101000001 | Binary  | Base 64 | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 67  | [[1]]                                     | 0100000101000001 | Binary  | Base 64 | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
	| 68  | [[@]]                                     | 0100000101000001 | Binary  | Hex     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
	| 69  | [[var#]]                                  | 0100000101000001 | Binary  | Base 64 | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
	| 70  | [[var]]00]]                               | 0100000101000001 | Binary  | Text    | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 71  | [[var]]@]]                                | 0100000101000001 | Binary  | Hex     | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 72  | [[var.()]]                                | 0100000101000001 | Binary  | Hex     | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
	| 73  | [[]]                                      | 0100000101000001 | Binary  | Base 64 | Variable [[]] is missing a name                                                                                                                                                                                                                         |
	| 74  | [[()]]                                    | 0100000101000001 | Binary  | Hex     | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
	#| 75  | 19                                        | 0100000101000001 | Binary  | Base 64 | [[var[[a]]]]                                                                                                                                                                                                                                            |
	| 75  | [[var[[]]                                 | 0100000101000001 | Binary  | Text    | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
	| 77  | [[var1.a]]                                | 0100000101000001 | Binary  | Hex     | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 78  | [[rec()!a]]                               | 0100000101000001 | Binary  | Hex     | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
	| 79  | [[rec()         a]]                       | 0100000101000001 | Binary  | Base 64 | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
	| 80  | [[{{rec(_).a}}]]]                         | 0100000101000001 | Binary  | Hex     | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
	| 81  | [[rec(23).[[var*]]]]                      | 0100000101000001 | Binary  | Hex     | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
	| 82  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | 0100000101000001 | Binary  | Hex     | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
	| 83  | [[rec().a]]&[[a]]                         | 0100000101000001 | Binary  | Hex     | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 84  | a[[rec([[[[b]]]]).a]]@                    | 0100000101000001 | Binary  | Hex     | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
	| 85  | [[var  ]]                                 | 0100000101000001 | Binary  | Base 64 | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
	| 86  | [[my(-1).var]]                            | 0x4141           | Hex     | Binary  | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
	| 87  | [[my(-1).var]]                            | 0x4141           | Hex     | Base 64 | Recordset index -1 is not greater than zero                                                                                                                                                                                                             |
	| 88  | [[my(-1).var]]                            | 0x4141           | Hex     | Text    | Recordset index [-1] is not greater than zero                                                                                                                                                                                                           |
	| 89  | [[rec'()'.a]]                             | 0x4141           | Hex     | Text    | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
	| 90  | [[rec"()".a]]                             | 0x4141           | Hex     | Base 64 | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
	| 91  | [[rec".a]]                                | 0x4141           | Hex     | Base 64 | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 92  | [[rec.a]]                                 | 0x4141           | Hex     | Text    | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
	| 93  | [[rec()*.a]]                              | 0x4141           | Hex     | Base 64 | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
	| 94  | [[rec().a]]*                              | 0x4141           | Hex     | Base 64 | One variable only allowed in the output field                                                                                                                                                                                                           |
	| 95  | [[1]]                                     | 0x4141           | Hex     | Base 64 | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
	| 96  | [[@]]                                     | 0x4141           | Hex     | Hex     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
	| 97  | [[var#]]                                  | 0x4141           | Hex     | Base 64 | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
	| 98  | [[var]]00]]                               | 0x4141           | Hex     | Text    | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 99  | [[var]]@]]                                | 0x4141           | Hex     | Binary  | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
	| 100 | [[var.()]]                                | 0x4141           | Hex     | Binary  | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
	| 101 | [[]]                                      | 0x4141           | Hex     | Base 64 | Variable [[]] is missing a name                                                                                                                                                                                                                         |
	| 102 | [[()]]                                    | 0x4141           | Hex     | Binary  | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
	#| 103 | 19                                        | 0x4141           | Hex     | Base 64 | [[var[[a]]]]                                                                                                                                                                                                                                            |
	| 104 | [[var[[]]                                 | 0x4141           | Hex     | Text    | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
	| 105 | [[var1.a]]                                | 0x4141           | Hex     | Binary  | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
	| 106 | [[rec()!a]]                               | 0x4141           | Hex     | Binary  | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
	| 107 | [[rec()         a]]                       | 0x4141           | Hex     | Base 64 | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
	| 108 | [[{{rec(_).a}}]]]                         | 0x4141           | Hex     | Binary  | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
	| 109 | [[rec(23).[[var*]]]]                      | 0x4141           | Hex     | Binary  | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
	| 110 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1   | 0x4141           | Hex     | Binary  | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |

