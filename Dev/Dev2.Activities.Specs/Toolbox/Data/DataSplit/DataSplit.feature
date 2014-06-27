Feature: DataSplit
	In order to split data
	As a Warewolf user
	I want a tool that splits two or more pieces of data

Scenario: Split text to a recordset using Index using Star notation
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "1" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| a                |
	| b                |
	| c                |
	| d                |
	| e                |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| abcde           | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 1     | No      |        |
    And the debug output as
	| # |                           |
	| 1 | [[vowels(1).letters]] = a |
	|   | [[vowels(2).letters]] = b |
	|   | [[vowels(3).letters]] = c |
	|   | [[vowels(4).letters]] = d |
	|   | [[vowels(5).letters]] = e |

Scenario: Split text to a recordset using Index using Append notation
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels().letters]]" split type "Index" at "1" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| a                |
	| b                |
	| c                |
	| d                |
	| e                |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| abcde           | Forward           | No              | 1 | [[vowels().letters]] = | Index | 1     | No      |        |
    And the debug output as
	| # |                           |
	| 1 | [[vowels(1).letters]] = a |
	|   | [[vowels(2).letters]] = b |
	|   | [[vowels(3).letters]] = c |
	|   | [[vowels(4).letters]] = d |
	|   | [[vowels(5).letters]] = e |

Scenario: Split characters using Index Going Backwards Using Star notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Backward"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars |
	| _)(*&^~        |
	| ><":}{+        |
	| @!?            |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels(*).chars]] = | Index | 7     | No      |        |
	And the debug output as
	| # |                               |
	| 1 | [[vowels(1).chars]] = _)(*&^~ |
	|   | [[vowels(2).chars]] = ><":}{+ |
	|   | [[vowels(3).chars]] = @!?     |
	

Scenario: Split characters using Index Going Backwards Using Append notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels().chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Backward"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars |
	| _)(*&^~        |
	| ><":}{+        |
	| @!?            |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split   | Process Direction | Skip blank rows | # |                      | With  | Using | Include | Escape |
	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels().chars]] = | Index | 7     | No      |        |
	And the debug output as
	| # |                               |
	| 1 | [[vowels(1).chars]] = _)(*&^~ |
	|   | [[vowels(2).chars]] = ><":}{+ |
	|   | [[vowels(3).chars]] = @!?     |

Scenario: Split characters using Index Going Forward using Star notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Forward"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars |
	| @!?><":        |
	| }{+_)(*        |
	| &^~            |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
	| @!?><":}{+_)(*&^~ | Forward           | No              | 1 | [[vowels(*).chars]] = | Index | 7     | No      |        |
	And the debug output as
	| # |                               |
	| 1 | [[vowels(1).chars]] = @!?><": |
	|   | [[vowels(2).chars]] = }{+_)(* |
	|   | [[vowels(3).chars]] = &^~     |

Scenario: Split characters using Index Going Forward using Append Notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels().chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Forward"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars |
	| @!?><":        |
	| }{+_)(*        |
	| &^~            |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split   | Process Direction | Skip blank rows | # |                      | With  | Using | Include | Escape |
	| @!?><":}{+_)(*&^~ | Forward           | No              | 1 | [[vowels().chars]] = | Index | 7     | No      |        |
	And the debug output as
	| # |                               |
	| 1 | [[vowels(1).chars]] = @!?><": |
	|   | [[vowels(2).chars]] = }{+_)(* |
	|   | [[vowels(3).chars]] = &^~     |

Scenario: Split text using All split types - Some with Include selected
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels(*).letters]]" split type "Tab" at "" and Include "unselected"
	And  assign to variable "[[vowels(*).letters]]" split type "Chars" at "ars," and Include "Selected" and Escape '' 
	And  assign to variable "[[vowels(*).letters]]" split type "Space" at "1" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels(*).letters]]" split type "End" at "" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| Index		  |
	| Tab		  |
	| Chars,	  |
	| space		  |
	| end		  |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split          | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
	| IndexTab	Chars,space end | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 5     | Yes     |        |
	|                          |                   |                  | 2 | [[vowels(*).letters]] = | Tab   |       | No      |        |
	|                          |                   |                  | 3 | [[vowels(*).letters]] = | Chars | ars,  | Yes     | " "    |
	|                          |                   |                  | 4 | [[vowels(*).letters]] = | Space |       | No      |        |
	|                          |                   |                  | 5 | [[vowels(*).letters]] = | End   |       | No      |        |
	And the debug output as
	| # |                          |
	| 1 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 2 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 3 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 4 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 5 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |

Scenario: Split text using All split types - Some with Include selected using a Star Notation
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels(*).letters]]" split type "Tab" at "" and Include "unselected"
	And  assign to variable "[[vowels(*).letters]]" split type "Chars" at "ars," and Include "Selected" and Escape '' 
	And  assign to variable "[[vowels(*).letters]]" split type "Space" at "1" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels(*).letters]]" split type "End" at "" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| Index		  |
	| Tab		  |
	| Chars,	  |
	| space		  |
	| end		  |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split          | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| IndexTab	Chars,space end | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 5     | Yes     |        |
	|                          |                   |                 | 2 | [[vowels(*).letters]] = | Tab   |       | No      |        |
	|                          |                   |                 | 3 | [[vowels(*).letters]] = | Chars | ars,  | Yes     | " "    |
	|                          |                   |                 | 4 | [[vowels(*).letters]] = | Space |       | No      |        |
	|                          |                   |                 | 5 | [[vowels(*).letters]] = | End   |       | No      |        |
	And the debug output as
	| # |                                |
	| 1 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 2 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 3 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 4 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |
	| 5 | [[vowels(1).letters]] = Index  |
	|   | [[vowels(2).letters]] = Tab    |
	|   | [[vowels(3).letters]] = Chars, |
	|   | [[vowels(4).letters]] = space  |
	|   | [[vowels(5).letters]] = end    |

Scenario: Split CSV file format into recordset - some fields blank
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[rec().id]]" split type "Chars" at "," and Include "unselected" and Escape ''
	And  assign to variable "[[rec().name]]" split type "Chars" at "," and Include "unselected" and Escape ''
	And  assign to variable "" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[rec().phone]]" split type "New Line" at "" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	|          |            |             |
	| 3        | Mo         |             |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                   | With     | Using | Include | Escape |
	| String          | Forward           | No              | 1 | [[rec().id]]    = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 2 | [[rec().name]]  = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 3 | " "             = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 4 | [[rec().phone]] = | New Line |       | No      |        |
	And the debug output as
	| # |                    |
	| 1 | [[rec(1).id]] = ID       |
	|   | [[rec(2).id]] = 1        |
	|   | [[rec(3).id]] = 2        |
	|   | [[rec(4).id]] =          |
	|   | [[rec(5).id]] = 3        |
	| 2 | [[rec(1).name]] = NAME   |
	|   | [[rec(2).name]] = Barney |
	|   | [[rec(3).name]] = Tshepo |
	|   | [[rec(4).name]] =        |
	|   | [[rec(5).name]] = Mo     |
	| 3 | " "                      | 
	| 4 | [[rec(1).phone]] = PHONE | 
	|   | [[rec(2).phone]] = 1234  | 
	|   | [[rec(3).phone]] = 5678  | 
	|   | [[rec(4).phone]] =       | 
	|   | [[rec(5).phone]] =       | 

Scenario: Split CSV file format into recordset - Skip blank rows selected
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[rec().id]]" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[rec().name]]" split type "Chars" at "," and Include "unselected" and Escape '' 	
	And  assign to variable "" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[rec().phone]]" split type "New Line" at "" and Include "unselected"
	And  Skip Blanks rows is "enabled"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	| 3        | Mo         | 01          |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                   | With     | Using | Include | Escape |
	| String          | Forward           | Yes             | 1 | [[rec().id]]    = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 2 | [[rec().name]]  = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 3 | (null)             = | Chars    | ,     | No      | " "    |
	|                 |                   |                 | 4 | [[rec().phone]] = | New Line |       | No      |        |
	And the debug output as
	| # |                    |
	| 1 | [[rec(1).id]] = ID       |
	|   | [[rec(2).id]] = 1        |
	|   | [[rec(3).id]] = 2        |
	|   | [[rec(4).id]] = 3        |
	| 2 | [[rec(1).name]] = NAME   |
	|   | [[rec(2).name]] = Barney |
	|   | [[rec(3).name]] = Tshepo |
	|   | [[rec(4).name]] = Mo     |
	| 3 | " "                      | 
	| 4 | [[rec(1).phone]] = PHONE |
	|   | [[rec(2).phone]] = 1234  |
	|   | [[rec(3).phone]] = 5678  |
	|   | [[rec(4).phone]] =       |

Scenario: Split blank text using All split types
	Given A string to split with value ""
	And  assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels().letters]]" split type "Tab" at "" and Include "unselected"	
	And  assign to variable "[[vowels().letters]]" split type "Chars" at "ars," and Include "selected" and Escape '' 
	And  assign to variable "[[vowels().letters]]" split type "Space" at "" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels().letters]]" split type "End" at "" and Include "unselected"
	And  assign to variable "[[vowels().letters]]" split type "NewLine" at "" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                        | With    | Using | Include | Escape |
	| " "             | Forward           | No              | 1 | [[vowels().letters]] = | Index   | 5     | Yes     |        |
	|                 |                   |                 | 2 | [[vowels().letters]] = | Tab     |       | No      |        |
	|                 |                   |                 | 3 | [[vowels().letters]] = | Chars   | ars,  | Yes     | " "    |
	|                 |                   |                 | 4 | [[vowels().letters]] = | Space   |       | No      |        |
	|                 |                   |                 | 5 | [[vowels().letters]] = | End     |       | No      |        |
	|                 |                   |                 | 6 | [[vowels().letters]] = | NewLine |       |         |        |	
	And the debug output as
	| # |                  |	

Scenario: Split text using Index where and Space > 
	Given A string to split with value "123"	
	And assign to variable "[[var]]" split type "Index" at "," and Include "Selected"
	And  assign to variable "[[vowels().letters]]" split type "Space" at "" and Include "unselected" and Escape '\' 
	When the data split tool is executed	
    Then the split result for "[[var]]" will be ""
    And the execution has "AN" error
    And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	| 123             | Forward           | No              | 1 | [[var]]    =           | Index | ,     | Yes     |        |
	|                 |                   |                 | 2 | [[vowels().letters]] = | Space |       | No      |        |
	And the debug output as
	| # |                         |
	| 1 | [[var]] =               |
	| 2 | [[vowels(1).letters]] = |

Scenario: Split text using Char and Escape character
	Given A string to split with value "123\,45,1"
	And assign to variable "[[var]]" split type "Chars" at "," and Include "Unselected" and Escape '\'
	When the data split tool is executed
	Then the split result for "[[var]]" will be "123\,45"
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |            | With  | Using | Include | Escape |
	| 123\,45,1       | Forward           | No              | 1 | [[var]]  = | Chars | ,     | No      | \      |
	And the debug output as
	| # |                   |
	| 1 | [[var]] = 123\,45 |

Scenario: Split blank text	
	Given A string to split with value ""
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "1" and Include "Unselected" and Escape ''
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the execution has "NO" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| " "             | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 1     | No      |        |
	And the debug output as
	| # |                   | 

Scenario: Split text to a recordset using a negative Index 
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "-1" and Include "unselected"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the execution has "AN" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| abcde           | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | -1    | No      |        |
	And the debug output as
	| # |                   |
	| 1 | [[vowels(1).letters]] = |

Scenario: Split text into negative recordset index as the index to split at
	Given A string to split with value "abcd"
	And assign to variable "[[vowels().letters]]" split type "Index" at "[[my(-1).index]]" and Include "Selected" and Escape '\'	
	When the data split tool is executed
	Then the execution has "AN" error

Scenario: Split text using a negative recordset index as escape character
	Given A string to split with value "abcd"
	And assign to variable "[[vowels().letters]]" split type "Index" at "2" and Include "Selected" and Escape '[[my(-1).escape]]'	
	When the data split tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
	| abcd            | Forward           | No              | 1 | [[vowels().letters]] = | Index | 2     | Yes     |        |
	And the debug output as
	| # |                   |
	| 1 | [[vowels(1).letters]] = |


Scenario: Split text using a index with "," and space  
     Given A string to split with value "a bc, def"
	 And  assign to variable "[[vowels(*).letters]]" split type "Index" at "," and Include "UnSelected"
	 And  assign to variable "[[vowels(*).letters]]" split type "Space" at "" and Include "unselected" and Escape '\' 
	 When the data split tool is executed
	 Then the execution has "AN" error

Scenario: Split text using Index where index is not numeric - variable
     Given A string to split with value "123" 
	 And I have a variable "[[idx]]" with a value "2"	 
     And assign to variable "[[var]]" split type "Index" at "[[idx]]" and Include "unselected"
     When the data split tool is executed     
     Then the split result for "[[var]]" will be "12"
     And the execution has "NO" error

Scenario: Split text using Index where index > provided
     Given A string to split with value "123" 
     And assign to variable "[[var]]" split type "Index" at "7" and Include "Selected" and Escape '\'
     When the data split tool is executed     
     Then the split result for "[[var]]" will be "123"
     And the execution has "NO" error

Scenario: Sending Error in error variable and calling webservice
    Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "*" and Include "unselected"
	And the direction is "Backward"
    And assign error to variable "[[error]]"
    And call the web service "http://TST-CI-REMOTE:3142/services/ONERROR/OnError_WriteToFile.xml?errorLog=[[error]]"
    When the data split tool is executed
    Then the execution has "AN" error
    And the result from the web service "http://TST-CI-REMOTE:3142/services/ONERROR/OnError_ReadFromFile.xml" will have the same data as variable "[[error]]"
    And the debug inputs as
	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels(*).chars]] = | Index | *     | No      |        |
    And the debug output as
	| # |                       |
	| 1 | [[vowels(1).chars]] = |


Scenario: Split negative record index as Input
	Given A string to split with value "[[my(-1).var]]"
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	When the data split tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| String to Split  | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
	| [[my(-1).var]] = | Forward           | No              | 1 | [[vowels().letters]]  = | Index | 5     | Yes     |        |
	And the debug output as
	| # |                         |
	| 1 | [[vowels(1).letters]] = |


Scenario: Split text into negative recordset index
	Given A string to split with value "abcd"
	And assign to variable "[[vowels(-1).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	When the data split tool is executed
	Then the execution has "AN" error	 
	And the debug inputs as  
	| String to Split | Process Direction | Skip blank rows | # |                          | With  | Using | Include | Escape |
	| abcd            | Forward           | No              | 1 | [[vowels(-1).letters]] = | Index | 5     | Yes     |        |
	And the debug output as
	| # |                            |
	| 1 | [[vowels(-1).letters]] =   |
	
#Scenario Outline: Split Text by using two variables in one row
#	Given A string to split with value "abcd"
#	And assign to variable '<variables>' split type "Index" at "4" and Include "Selected" and Escape ''	
#	When the data split tool is executed
#	Then the execution has "AN" error	 
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
#	| abcd            | Forward           | No              | 1 | <variables> = | Index | 4     | Yes     |        |
#	And the debug output as
#	| #               |                   |
#	| 1               | <variables> =     |
#Examples: 
#	| No | varaibles                  |
#	| 1  | [[vowels(1).letters]][[a]] |
#	| 2  | [[a]][[b]]                 |
#
#Scenario: Split Text by using variable inside varaibles    
#	Given  A string to split with value "abcd"
#	And I have a variable "[[a]]" with a value "rec().a"
#	And assign to variable "[[[[a]]]]" split type "Index" at "4" and Include "Selected" and Escape ''	
#	When the data split tool is executed
#	Then the execution has "NO" error	 
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| abcd            | Forward           | No              | 1 | [[[[a]]]] = [[rec().a]] | Index | 5     | Yes     |        |
#	And the debug output as
#	| # |                    |
#	| 1 | [[rec().a]] = abcd |

#SPECFLOW DOES NOT ALLOW MIXING SCENARIO AND SCENARIO OUTLINE LAYOUTS
#Scenario Outline: Split Text by using two variables in one row second
#	Given A string to split with value "abcd"
#	And assign to variable "[[rec().a]]" split type "Index" at '<Type>' and Include "Selected" and Escape ''
#	When the data split tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| # | # | String to Split | Process Direction | Skip blank rows | # |              | With  | Using | Include | Escape |
#	| # | # | abcd            | Forward           | No              | 1 | [[rec().a]]= | Index | 4     | Yes     |        |
#	And the debug output as
#	| # | # |               |
#	| # | 1 | [[rec().a]] = |
#Examples: 
#	| # | No | Type    |
#	| # | 1  |         |
#	| # | 2  | [[%#$]] |

#Scenario Outline: Debug output Validation errors
#	Given A string to split with value "Warewolf"
#	And assign to variable '<Varaible>' split type "Index" at "5" and Include "Selected" and Escape ''	
#	When the data split tool is executed
#	Then the execution has "AN" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| <Varaible> =    | Forward           | No              | 1 | [[vowels().letters]]  = | Index | 5     | Yes     |        |
#	And the debug output as
#	| # |           |
#	| 1 | <error> = |
#Examples: 
#	 | No | Variable             | error                                                            |
#	 | 1  | [[rec().a]]=]]       | Invalid region detected: A close ]] without a related open [[    |
#	 | 2  | [[rec'()'.a]]        | Recordset name [[rec'()']] contains invalid character(s)         |
#	 | 3  | [[rec"()".a]]        | Recordset name [[rec"()"]] contains invalid character(s)         |
#	 | 4  | [[rec".a]]           | Variable name [[rec".a]] contains invalid character(s)           |
#	 | 5  | [[rec.a]]            | Variable name [[rec.a]]  contains invalid character(s)           |
#	 | 6  | [[rec()*.a]]         | Variable name [[rec()*.a]] contains invalid character(s)         |
#	 | 9  | [[rec().a]]*         | Variable name [[rec().a]]* contains invalid character(s)         |
#	 | 10 | [[1]]                | Variable name [[1]] begins with a number                         |
#	 | 11 | [[@]]                | Variable name [[@]] contains invalid character(s)                |
#	 | 12 | [[var#]]             | Variable name [[var#]] contains invalid character(s)             |
#	 | 13 | [[var]]00]]          | Invalid region detected: A close ]] without a related open [[    |
#	 | 14 | [[var]]@]]           | Invalid region detected: A close ]] without a related open [[    |
#	 | 15 | [[var.()]]           | Variable name [[var.()]] contains invalid character(s)           |
#	 | 16 | [[]]                 | Variable [[]] is missing a name                                  |
#	 | 17 | [[()]]               | Variable name [[()]] contains invalid character(s)               |
#	 | 28 | [[var[[]]            | Invalid region detected: An open [[ without a related close ]]   |
#	 | 29 | [[var1.a]]           | Variable name [[var1.a]] contains invalid character(s)           |
#	 | 20 | [[rec()!a]]          | Recordset name [[rec()!a]] contains invalid character(s)         |
#	 | 21 | [[rec()         a]]  | Recordset name [[rec()         a]] contains invalid character(s) |
#	 | 22 | [[{{rec(_).a}}]]]    | Recordset name [[{{rec]] contains invalid character(s)           |
#	 | 23 | [[rec(23).[[var*]]]] | Variable name [[var*]] contains invalid character(s)             |
#	 | 24 | [[rec()              | Recordset variable that needs a field name(s)                       |
#	 | 25 | a[[rec([[[[b]]]]).a]]@                    | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
#	 | 26 | [[var  ]]                                 | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 27 | [[var@]]                                  | Variable name [[var@]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 28 | [[var#]]                                  | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 29 | [[var]]]]                                 | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 30 | [[(1var)]]                                | Variable name [[(1var)]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 31 | [[1var)]]                                 | Variable name [[1var)]] begins with a number                                                                                                                                                                                                            |
#	 | 32 | [[var.()]]                                | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 33 | [[var  ]]                                 | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 34 | [[var~]]                                  | Variable name [[var~]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 35 | [[var+]]                                  | Variable name [[var+]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 36 | [[var]a]]                                 | Variable name [[var]a]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 37 | [[var[a]]                                 | Variable name [[var[a]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 38 | [[var 1]]                                 | Variable name [[var 1]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 39 | [[var[[]]                                 | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
#	 | 40 | [[var[[1]]]]                              | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
#	 | 41 | [[var.a]]                                 | Variable name [[var.a]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 42 | [[var1.a]]                                | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 43 | [[[[a]].[[b]]]]cd]]                       | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 44 | [[var*]]                                  | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 45 | [[1var]]                                  | Variable name [[1var]] begins with a number                                                                                                                                                                                                             |
#	 | 46 | [[@var]]                                  | Variable name [[@var]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 47 | [[var]](var)]]                            | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 48 | [[var,]]                                  | Variable name [[var,]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 49 | [[:var 1]]                                | Variable name [[:var 1]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 50 | [[,var]]                                  | Variable name [[,var]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 51 | [[test,var]]                              | Variable name [[test,var]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 52 | [[test. var]]                             | Variable name [[test. var]] contains invalid character(s)                                                                                                                                                                                               |
#	 | 53 | [[test.var]]                              | Variable name [[test.var]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 54 | [[test. 1]]                               | Variable name [[test. 1]] contains invalid character(s)                                                                                                                                                                                                 |
#	 | 55 | [[rec(*).&]]                              | Recordset field name & contains invalid character(s)                                                                                                                                                                                                    |
#	 | 56 | [[rec(),a]]                               | Recordset name [[rec(),a]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 57 | [[rec()         a]]                       | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
#	 | 58 | [[rec(1).[[rec().1]]]]                    | Recordset field name 1 begins with a number                                                                                                                                                                                                             |
#	 | 59 | [[rec(a).[[rec().a]]]]                    | Recordset index (a) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 60 | [[{{rec(_).a}}]]]                         | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 61 | [[*[{{rec(_).a}}]]]                       | Recordset name [[*[{{rec]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 62 | [[rec(23).[[var}]]]]                      | Variable name [[var}]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 63 | [[rec(23).[[var*]]]]                      | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 64 | [[rec(23).[[var%^&%]]]]                   | Variable name [[var%^&%]] contains invalid character(s)                                                                                                                                                                                                 |
#	 | 65 | [[rec().a]]234234]]                       | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 66 | [[rec().a]]=]]                            | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 67 | [[rec()..]]                               | Recordset name [[rec()..]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 68 | [[rec().a.b]]                             | Invalid Notation - Extra dots detected                                                                                                                                                                                                                  |
#	 | 69 | [[rec().a]].a]]                           | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 70 | [[rec(@).a]]                              | Recordset index (@) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 71 | [[rec(().a]]                              | Recordset index (() contains invalid character(s)                                                                                                                                                                                                       |
#	 | 72 | [[rec()).a]]                              | Recordset index ()) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 73 | [[rec(+).a]]                              | Recordset index (+) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 74 | [[rec(-).a]]                              | Recordset index (-) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 75 | [[rec(!).a]]                              | Recordset index (!) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 76 | [[rec(q).a]]                              | Recordset index (q) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 77 | [[rec(w).a]]                              | Recordset index (w) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 78 | [[rec(.).a]]                              | Invalid Notation - Extra dots detected                                                                                                                                                                                                                  |
#	 | 79 | [[rec(:).a]]                              | Recordset index (:) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 80 | [[rec(,).a]]                              | Recordset index (,) contains invalid character(s)                                                                                                                                                                                                       |
#	 | 81 | [[rec"()".a]]                             | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 82 | [[rec'()'.a]]                             | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
#	 | 83 | [[rec").a]]                               | Variable name [[rec").a]] contains invalid character(s)                                                                                                                                                                                                 |
#	 | 84 | [[rec{a]]                                 | Variable name [[rec{a]] contains invalid character(s)                                                                                                                                                                                                   |
#	 | 85 | [[rec{a}]]                                | Variable name [[rec{a}]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 86 | [[rec()*.a]]                              | Recordset name [[rec()*]] contains invalid character(s)                                                                                                                                                                                                 |
#	 | 87 | [[rec().a[[a]]                            | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |  
#	 | 89 | [[rec(-1).a                               | Recordset index -1]is not greater than zero                                                                                                                                                                                                             |
#	 | 90 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |  
#	                                                                                                                                                                                                                                                  
#
#	 
#



















	