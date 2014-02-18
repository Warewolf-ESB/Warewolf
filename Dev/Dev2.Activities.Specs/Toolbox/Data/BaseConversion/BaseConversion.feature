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
	| # | Convert             | From | To     |
	| 1 | [[my(-1).var]] = AA | Text | Binary |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from text to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert             | From | To  |
	| 1 | [[my(-1).var]] = AA | Text | Hex |
	And the debug output as  
	| # |           |
	
Scenario: Convert negative recordset index from text to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert             | From | To      |
	| 1 | [[my(-1).var]] = AA | Text | Base 64 |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from binary to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                           | From   | To   |
	| 1 | [[my(-1).var]] = 0100000101000001 | Binary | Text |
	And the debug output as  
	| # |                  |
		
Scenario: Convert negative recordset index from binary to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001""
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                           | From   | To  |
	| 1 | [[my(-1).var]] = 0100000101000001 | Binary | Hex |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from binary to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                           | From   | To      |
	| 1 | [[my(-1).var]] = 0100000101000001 | Binary | Base 64 |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from hexadecimal to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                 | From | To   |
	| 1 | [[my(-1).var]] = 0x4141 | Hex  | Text |
	And the debug output as  
	| # |           |	

Scenario: Convert negative recordset index from hexadecimal to binary 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                 | From | To     |
	| 1 | [[my(-1).var]] = 0x4141 | Hex  | Binary |
	And the debug output as  
	| # |           |

Scenario: Convert negative recordset index from hexadecimal to base64 
	Given I have a convert variable "[[my(-1).var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert                 | From | To      |
	| 1 | [[my(-1).var]] = 0x4141 | Hex  | Base 64 |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from base64 to hexadecimal 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | From    | To  |
	| 1 | [[my(-1).var]] = QUE= | Base 64 | Hex |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from base64 to text 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | From    | To   |
	| 1 | [[my(-1).var]] = QUE= | Base 64 | Text |
	And the debug output as  
	| # |                  |	

Scenario: Convert negative recordset index from base64 to binary 
	Given I have a convert variable "[[my(-1).var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # | Convert               | From    | To     |
	| 1 | [[my(-1).var]] = QUE= | Base 64 | Binary |
	And the debug output as  
	| # |                  |	
