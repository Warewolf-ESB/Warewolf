Feature: DataSplit
	In order to split data
	As a Warewolf user
	I want a tool that splits two or more pieces of data

Scenario: Split text to a recordset using Index using Star notation
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "1" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).letters]]" will be
	| rs           | value |
	| vowels().row | a     |
	| vowels().row | b     |
	| vowels().row | c     |
	| vowels().row | d     |
	| vowels().row | e     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| abcde           | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 1     | No      |        |
#    And the debug output as
#	| # |                           |
#	| 1 | [[vowels(1).letters]] = a |
#	|   | [[vowels(2).letters]] = b |
#	|   | [[vowels(3).letters]] = c |
#	|   | [[vowels(4).letters]] = d |
#	|   | [[vowels(5).letters]] = e |

Scenario: Split text to a scalar
	Given A string to split with value "abcde"
	And  assign to variable "[[a]]" split type "Index" at "1" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[a]]" will be
	| rs | value |
	| a  | a     |
	And the execution has "NO" error


Scenario: Split text to a recordset using Index using Append notation
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels().letters]]" split type "Index" at "1" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).letters]]" will be
	| rs           | value |
	| vowels().row | a     |
	| vowels().row | b     |
	| vowels().row | c     |
	| vowels().row | d     |
	| vowels().row | e     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| abcde           | Forward           | No              | 1 | [[vowels().letters]] = | Index | 1     | No      |        |
#    And the debug output as
#	| # |                           |
#	| 1 | [[vowels(1).letters]] = a |
#	|   | [[vowels(2).letters]] = b |
#	|   | [[vowels(3).letters]] = c |
#	|   | [[vowels(4).letters]] = d |
#	|   | [[vowels(5).letters]] = e |

Scenario: Split characters using Index Going Backwards Using Star notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Backward"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).chars]]" will be
	| rs           | value   |
	| vowels().row | _)(*&^~ |
	| vowels().row | ><":}{+ |
	| vowels().row | @!?     |

	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
#	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels(*).chars]] = | Index | 7     | No      |        |
#	And the debug output as
#	| # |                               |
#	| 1 | [[vowels(1).chars]] = _)(*&^~ |
#	|   | [[vowels(2).chars]] = ><":}{+ |
#	|   | [[vowels(3).chars]] = @!?     |	

Scenario: Split characters using Index Going Backwards Using Append notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels().chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Backward"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).chars]]" will be
	| rs           | value   |
	| vowels().chars | _)(*&^~ |
	| vowels().chars | ><":}{+ |
	| vowels().chars | @!?     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split   | Process Direction | Skip blank rows | # |                      | With  | Using | Include | Escape |
#	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels().chars]] = | Index | 7     | No      |        |
#	And the debug output as
#	| # |                               |
#	| 1 | [[vowels(1).chars]] = _)(*&^~ |
#	|   | [[vowels(2).chars]] = ><":}{+ |
#	|   | [[vowels(3).chars]] = @!?     |

Scenario: Split characters using Index Going Forward using Star notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Forward"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).chars]]" will be
	| rs           | value   |
	| vowels().chars | @!?><": |
	| vowels().chars | }{+_)(* |
	| vowels().chars | &^~     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
#	| @!?><":}{+_)(*&^~ | Forward           | No              | 1 | [[vowels(*).chars]] = | Index | 7     | No      |        |
#	And the debug output as
#	| # |                               |
#	| 1 | [[vowels(1).chars]] = @!?><": |
#	|   | [[vowels(2).chars]] = }{+_)(* |
#	|   | [[vowels(3).chars]] = &^~     |

Scenario: Split characters using Index Going Forward using Append Notation
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels().chars]]" split type "Index" at "7" and Include "unselected"
	And the direction is "Forward"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).chars]]" will be
	| rs           | value   |
	| vowels().chars | @!?><": |
	| vowels().chars | }{+_)(* |
	| vowels().chars | &^~     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split   | Process Direction | Skip blank rows | # |                      | With  | Using | Include | Escape |
#	| @!?><":}{+_)(*&^~ | Forward           | No              | 1 | [[vowels().chars]] = | Index | 7     | No      |        |
#	And the debug output as
#	| # |                               |
#	| 1 | [[vowels(1).chars]] = @!?><": |
#	|   | [[vowels(2).chars]] = }{+_)(* |
#	|   | [[vowels(3).chars]] = &^~     |

Scenario: Split text using All split types - Some with Include selected
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels(*).letters]]" split type "Tab" at "" and Include "unselected"
	And  assign to variable "[[vowels(*).letters]]" split type "Chars" at "ars," and Include "Selected" and Escape '' 
	And  assign to variable "[[vowels(*).letters]]" split type "Space" at "1" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels(*).letters]]" split type "End" at "" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).letters]]" will be
	| rs             | value  |
	| vowels().chars | Index  |
	| vowels().chars | Tab    |
	| vowels().chars | Chars, |
	| vowels().chars | space  |
	| vowels().chars | end    |

	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split          | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| IndexTab	Chars,space end | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 5     | Yes     |        |
#	|                          |                   |                 | 2 | [[vowels(*).letters]] = | Tab   |       | No      |        |
#	|                          |                   |                 | 3 | [[vowels(*).letters]] = | Chars | ars,  | Yes     | " "    |
#	|                          |                   |                 | 4 | [[vowels(*).letters]] = | Space |       | No      |        |
#	|                          |                   |                 | 5 | [[vowels(*).letters]] = | End   |       | No      |        |
#	And the debug output as
#	| # |                          |
#	| 1 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 2 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 3 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 4 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 5 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |

Scenario: Split text using All split types - Some with Include selected using a Star Notation
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels(*).letters]]" split type "Tab" at "" and Include "unselected"
	And  assign to variable "[[vowels(*).letters]]" split type "Chars" at "ars," and Include "Selected" and Escape '' 
	And  assign to variable "[[vowels(*).letters]]" split type "Space" at "1" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels(*).letters]]" split type "End" at "" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[vowels(*).letters]]" will be
	| rs             | value  |
	| vowels().chars | Index  |
	| vowels().chars | Tab    |
	| vowels().chars | Chars, |
	| vowels().chars | space  |
	| vowels().chars | end    |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split          | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| IndexTab	Chars,space end | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 5     | Yes     |        |
#	|                          |                   |                 | 2 | [[vowels(*).letters]] = | Tab   |       | No      |        |
#	|                          |                   |                 | 3 | [[vowels(*).letters]] = | Chars | ars,  | Yes     | " "    |
#	|                          |                   |                 | 4 | [[vowels(*).letters]] = | Space |       | No      |        |
#	|                          |                   |                 | 5 | [[vowels(*).letters]] = | End   |       | No      |        |
#	And the debug output as
#	| # |                                |
#	| 1 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 2 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 3 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 4 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |
#	| 5 | [[vowels(1).letters]] = Index  |
#	|   | [[vowels(2).letters]] = Tab    |
#	|   | [[vowels(3).letters]] = Chars, |
#	|   | [[vowels(4).letters]] = space  |
#	|   | [[vowels(5).letters]] = end    |

Scenario: Split CSV file format into recordset - some fields blank
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[rec().id]]" split type "Chars" at "," and Include "unselected" and Escape ''
	And  assign to variable "[[rec().name]]" split type "Chars" at "," and Include "unselected" and Escape ''
	And  assign to variable "" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[rec().phone]]" split type "New Line" at "" and Include "unselected"
	When the data split tool is executed
	Then the split recordset "[[rec(*).name]]" will be
	| rs            | value  |
	| vowels().name | NAME   |
	| vowels().name | Barney |
	| vowels().name | Tshepo |
	| vowels().name |        |
	| vowels().name | Mo     |
	Then the split recordset "[[rec(*).phone]]" will be
	| rs             | value |
	| vowels().phone | PHONE |
	| vowels().phone | 1234  |
	| vowels().phone | 5678  |
	| vowels().phone |       |
	| vowels().phone |       |
	Then the split recordset "[[rec(*).id]]" will be
	| rs          | value |
	| vowels().id | ID    |
	| vowels().id | 1     |
	| vowels().id | 2     |
	| vowels().id |       |
	| vowels().id | 3     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                   | With     | Using | Include | Escape |
#	| String          | Forward           | No              | 1 | [[rec().id]]    = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 2 | [[rec().name]]  = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 3 | " "             = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 4 | [[rec().phone]] = | New Line |       | No      |        |
#	And the debug output as
#	| # |                    |
#	| 1 | [[rec(1).id]] = ID       |
#	|   | [[rec(2).id]] = 1        |
#	|   | [[rec(3).id]] = 2        |
#	|   | [[rec(4).id]] =          |
#	|   | [[rec(5).id]] = 3        |
#	| 2 | [[rec(1).name]] = NAME   |
#	|   | [[rec(2).name]] = Barney |
#	|   | [[rec(3).name]] = Tshepo |
#	|   | [[rec(4).name]] =        |
#	|   | [[rec(5).name]] = Mo     |
#	| 3 | " "                      | 
#	| 4 | [[rec(1).phone]] = PHONE | 
#	|   | [[rec(2).phone]] = 1234  | 
#	|   | [[rec(3).phone]] = 5678  | 
#	|   | [[rec(4).phone]] =       | 
#	|   | [[rec(5).phone]] =       | 

Scenario: Split CSV file format into recordset - Skip blank rows selected
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[bob().id]]" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[bob().name]]" split type "Chars" at "," and Include "unselected" and Escape '' 	
	And  assign to variable "" split type "Chars" at "," and Include "unselected" and Escape '' 
	And  assign to variable "[[bob().phone]]" split type "New Line" at "" and Include "unselected"
	And  Skip Blanks rows is "enabled"
	When the data split tool is executed
	Then the split recordset "[[bob(*).name]]" will be
	| rs            | value  |
	| rec().name | NAME   |
	| rec().name | Barney |
	| rec().name | Tshepo |
	| rec().name | Mo     |
	Then the split recordset "[[bob(*).phone]]" will be
	| rs             | value |
	| rec().phone | PHONE |
	| rec().phone | 1234  |
	| rec().phone | 5678  |
	| rec().phone |       |
	Then the split recordset "[[bob(*).id]]" will be
	| rs          | value |
	| rec().id | ID    |
	| rec().id | 1     |
	| rec().id | 2     |
	| rec().id | 3     |
	And the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                   | With     | Using | Include | Escape |
#	| String          | Forward           | Yes             | 1 | [[rec().id]]    = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 2 | [[rec().name]]  = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 3 | (null)             = | Chars    | ,     | No      | " "    |
#	|                 |                   |                 | 4 | [[rec().phone]] = | New Line |       | No      |        |
#	And the debug output as
#	| # |                    |
#	| 1 | [[rec(1).id]] = ID       |
#	|   | [[rec(2).id]] = 1        |
#	|   | [[rec(3).id]] = 2        |
#	|   | [[rec(4).id]] = 3        |
#	| 2 | [[rec(1).name]] = NAME   |
#	|   | [[rec(2).name]] = Barney |
#	|   | [[rec(3).name]] = Tshepo |
#	|   | [[rec(4).name]] = Mo     |
#	| 3 | " "                      | 
#	| 4 | [[rec(1).phone]] = PHONE |
#	|   | [[rec(2).phone]] = 1234  |
#	|   | [[rec(3).phone]] = 5678  |
#	|   | [[rec(4).phone]] =       |

Scenario: Split blank text using All split types
	Given A string to split with value ""
	And  assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	And  assign to variable "[[vowels().letters]]" split type "Tab" at "" and Include "unselected"	
	And  assign to variable "[[vowels().letters]]" split type "Chars" at "ars," and Include "selected" and Escape '' 
	And  assign to variable "[[vowels().letters]]" split type "Space" at "" and Include "unselected" and Escape '\'
	And  assign to variable "[[vowels().letters]]" split type "End" at "" and Include "unselected"
	And  assign to variable "[[vowels().letters]]" split type "NewLine" at "" and Include "unselected"
	When the data split tool is executed
	Then the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                        | With    | Using | Include | Escape |
#	| " "             | Forward           | No              | 1 | [[vowels().letters]] = | Index   | 5     | Yes     |        |
#	|                 |                   |                 | 2 | [[vowels().letters]] = | Tab     |       | No      |        |
#	|                 |                   |                 | 3 | [[vowels().letters]] = | Chars   | ars,  | Yes     | " "    |
#	|                 |                   |                 | 4 | [[vowels().letters]] = | Space   |       | No      |        |
#	|                 |                   |                 | 5 | [[vowels().letters]] = | End     |       | No      |        |
#	|                 |                   |                 | 6 | [[vowels().letters]] = | NewLine |       |         |        |	
#	And the debug output as
#	| # |                         |
#	| 1 | [[vowels(1).letters]] = |
#	| 2 | [[vowels(1).letters]] = |
#	| 3 | [[vowels(1).letters]] = |
#	| 4 | [[vowels(1).letters]] = |
#	| 5 | [[vowels(1).letters]] = |
#	| 6 | [[vowels(1).letters]] = |

Scenario: Split text using Index where and Space > 
	Given A string to split with value "123"	
	And assign to variable "[[var]]" split type "Index" at "," and Include "Selected"
	And  assign to variable "[[vowels().letters]]" split type "Space" at "" and Include "unselected" and Escape '\' 
	When the data split tool is executed	
    Then the split result for "[[var]]" will be ""
    And the execution has "AN" error
#    And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
#	| 123             | Forward           | No              | 1 | [[var]]    =           | Index | ,     | Yes     |        |
#	|                 |                   |                 | 2 | [[vowels().letters]] = | Space |       | No      |        |
#	And the debug output as
#	| # |                         |
#	| 1 | [[var]] =               |
#	| 2 | [[vowels(1).letters]] = |

Scenario: Split text using Char and Escape character
	Given A string to split with value "123\,45,1"
	And assign to variable "[[var]]" split type "Chars" at "," and Include "Unselected" and Escape '\'
	When the data split tool is executed
	Then the split recordset "[[var]]" will be
	| rs         | value   |
	| [[var]] | 123\,45 |

	And the execution has "NO" error

#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |            | With  | Using | Include | Escape |
#	| 123\,45,1       | Forward           | No              | 1 | [[var]]  = | Chars | ,     | No      | \      |
#	And the debug output as
#	| # |                   |
#	| 1 | [[var]] = 123\,45 |

Scenario: Split blank text	
	Given A string to split with value ""
	And assign to variable "[[vowels(*).letters]]" split type "Index" at "1" and Include "Unselected" and Escape ''
	When the data split tool is executed
	Then the execution has "NO" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| " "             | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | 1     | No      |        |
#	And the debug output as
#	| # |                         |
#	| 1 | [[vowels(1).letters]] = |

Scenario: Split text to a recordset using a negative Index 
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "-1" and Include "unselected"
	When the data split tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| abcde           | Forward           | No              | 1 | [[vowels(*).letters]] = | Index | -1    | No      |        |
#	And the debug output as
#	| # |                   |
#	| 1 | [[vowels(1).letters]] = |

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
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                        | With  | Using | Include | Escape |
#	| abcd            | Forward           | No              | 1 | [[vowels().letters]] = | Index | 2     | Yes     |        |
#	And the debug output as
#	| # |                   |
#	| 1 | [[vowels(1).letters]] = |

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
     Then the split recordset "[[var]]" will be
	| rs      | value   |
	| [[var]] | 12 |
     And the execution has "NO" error

Scenario: Split text using Index where index > provided
     Given A string to split with value "123" 
     And assign to variable "[[var]]" split type "Index" at "7" and Include "Selected" and Escape '\'
     When the data split tool is executed     
     Then the split recordset "[[var]]" will be
	 | rs      | value |
	 | [[var]] | 123   |
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
#    And the debug inputs as
#	| String to Split   | Process Direction | Skip blank rows | # |                       | With  | Using | Include | Escape |
#	| @!?><":}{+_)(*&^~ | Backward          | No              | 1 | [[vowels(*).chars]] = | Index | *     | No      |        |
#    And the debug output as
#	| # |                       |
#	| 1 | [[vowels(1).chars]] = |

Scenario: Split negative record index as Input
	Given A string to split with value "[[my(-1).var]]"
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	When the data split tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| String to Split  | Process Direction | Skip blank rows | # |                         | With  | Using | Include | Escape |
#	| [[my(-1).var]] = | Forward           | No              | 1 | [[vowels().letters]]  = | Index | 5     | Yes     |        |
#	And the debug output as
#	| # |                         |
#	| 1 | [[vowels(1).letters]] = |

Scenario: Split text into negative recordset index
	Given A string to split with value "abcd"
	And assign to variable "[[vowels(-1).letters]]" split type "Index" at "5" and Include "Selected" and Escape ''	
	When the data split tool is executed
	Then the execution has "AN" error	 
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |                          | With  | Using | Include | Escape |
#	| abcd            | Forward           | No              | 1 | [[vowels(-1).letters]] = | Index | 5     | Yes     |        |
#	And the debug output as
#	| # |                          |
#	| 1 | [[vowels(-1).letters]] = |
	
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

Scenario Outline: Split Text using type Index but Index is invalid
	Given A string to split with value "abcd"
	And assign to variable '[[rec().a]]' split type "Index" at '<Type>' and Include 'Selected' and Escape ''
	When the data split tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| String to Split | Process Direction | Skip blank rows | # |               | With  | Using       | Include | Escape |
#	| abcd            | Forward           | No              | 1 | [[rec().a]] = | Index | <Type> = "" | Yes     |        |
##	And the debug output as
#	| # |               |
#	| 1 | [[rec(1).a]] = |
	Examples: 
	| No | Type    |
	| 1  |         |
	| 2  | [[%#$]] |

Scenario Outline: Debug output Validation errors
	Given A string to split with value "Warewolf"	
	And assign to variable '<Variable>' split type "Index" at '5' and Include 'Selected' and Escape ''
	When the data split tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| String to Split  | Process Direction | Skip blank rows | # |               | With  | Using | Include | Escape |
#	| "" =  "Warewolf" | Forward           | No              | 1 | <Variable>  = | Index | 5     | Yes     |        |
#	And the debug output as
#	| # |          |
#	| 1 | String = |
Examples: 
	 | No | Variable                                  |
	 | 1  | [[rec().a]]=]]                            |
	 | 2  | [[rec'()'.a]]                             |
	 | 3  | [[rec"()".a]]                             |
	 | 4  | [[rec".a]]                                |
	 | 5  | [[rec.a]]                                 |
	 | 6  | [[rec()*.a]]                              |
	 | 9  | [[rec().a]]*                              |
	 | 10 | [[1]]                                     |
	 | 11 | [[@]]                                     |
	 | 12 | [[var#]]                                  |
	 | 13 | [[var]]00]]                               |
	 | 14 | [[var]]@]]                                |
	 | 15 | [[var.()]]                                |
	 | 16 | [[]]                                      |
	 | 17 | [[()]]                                    |
	 | 28 | [[var[[]]                                 |
	 | 29 | [[var1.a]]                                |
	 | 20 | [[rec()!a]]                               |
	 | 21 | [[rec()         a]]                       |
	 | 22 | [[{{rec(_).a}}]]]                         |
	 | 23 | [[rec(23).[[var*]]]]                      |
	 | 24 | [[rec()                                   |
	 | 25 | a[[rec([[[[b]]]]).a]]@                    |
	 | 26 | [[var  ]]                                 |
	 | 27 | [[var@]]                                  |
	 | 28 | [[var#]]                                  |
	 | 29 | [[var]]]]                                 |
	 | 30 | [[(1var)]]                                |
	 | 31 | [[1var)]]                                 |
	 | 32 | [[var.()]]                                |
	 | 33 | [[var  ]]                                 |
	 | 34 | [[var~]]                                  |
	 | 35 | [[var+]]                                  |
	 | 36 | [[var]a]]                                 |
	 | 37 | [[var[a]]                                 |
	 | 38 | [[var 1]]                                 |
	 | 39 | [[var[[]]                                 |
	 | 40 | [[var[[1]]]]                              |
	 | 41 | [[var.a]]                                 |
	 | 42 | [[var1.a]]                                |
	 | 43 | [[[[a]].[[b]]]]cd]]                       |
	 | 44 | [[var*]]                                  |
	 | 45 | [[1var]]                                  |
	 | 46 | [[@var]]                                  |
	 | 47 | [[var]](var)]]                            |
	 | 48 | [[var,]]                                  |
	 | 49 | [[:var 1]]                                |
	 | 50 | [[,var]]                                  |
	 | 51 | [[test,var]]                              |
	 | 52 | [[test. var]]                             |
	 | 53 | [[test.var]]                              |
	 | 54 | [[test. 1]]                               |
	 | 55 | [[rec(*).&]]                              |
	 | 56 | [[rec(),a]]                               |
	 | 57 | [[rec()         a]]                       |
	 | 58 | [[rec(1).[[rec().1]]]]                    |
	 | 59 | [[rec(a).[[rec().a]]]]                    |
	 | 60 | [[{{rec(_).a}}]]]                         |
	 | 61 | [[*[{{rec(_).a}}]]]                       |
	 | 62 | [[rec(23).[[var}]]]]                      |
	 | 63 | [[rec(23).[[var*]]]]                      |
	 | 64 | [[rec(23).[[var%^&%]]]]                   |
	 | 65 | [[rec().a]]234234]]                       |
	 | 66 | [[rec().a]]=]]                            |
	 | 67 | [[rec()..]]                               |
	 | 68 | [[rec().a.b]]                             |
	 | 69 | [[rec().a]].a]]                           |
	 | 70 | [[rec(@).a]]                              |
	 | 71 | [[rec(().a]]                              |
	 | 72 | [[rec()).a]]                              |
	 | 73 | [[rec(+).a]]                              |
	 | 74 | [[rec(-).a]]                              |
	 | 75 | [[rec(!).a]]                              |
	 | 76 | [[rec(q).a]]                              |
	 | 77 | [[rec(w).a]]                              |
	 | 78 | [[rec(.).a]]                              |
	 | 79 | [[rec(:).a]]                              |
	 | 80 | [[rec(,).a]]                              |
	 | 81 | [[rec"()".a]]                             |
	 | 82 | [[rec'()'.a]]                             |
	 | 83 | [[rec").a]]                               |
	 | 84 | [[rec{a]]                                 |
	 | 85 | [[rec{a}]]                                |
	 | 86 | [[rec()*.a]]                              |	 
	 | 89 | [[rec(-1).a                               |
	 | 90 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] |