Feature: DataMerge
	In order to merge data
	As Warewolf user
	I want a tool that joins two or more pieces of data together

Scenario: Merge a scalar to a scalar using merge type none
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"		
	And an Input "[[a]]" and merge type "None" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "[[b]]" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "Warewolf Rocks"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                  | With | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | None |  ""     | ""    | Left  |
	| 2 | [[b]] = Rocks    | None |    ""   |  ""   | Left  |
	And the debug output as 
	|                              |
	| [[result]] = Warewolf Rocks |

Scenario: Merge a recordset table and free text using None
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "100200300"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                     | With | Using | Pad | Align |
	| 1 | [[rs(1).row]]0 = 10 |      |       |     |       |
	|   | [[rs(2).row]]0 = 20 |      |       |     |       |
	|   | [[rs(3).row]]0 = 30 |      |       |     |       |
	|   |                     | None | ""      | ""    | Left  |
	| 2 | 0                   | None |   ""    |  ""   | Left  |	
	And the debug output as 
	|                         |  
	| [[result]] = 100200300 |


Scenario: Merge a recordset table and free text using Chars
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]" and merge type "Chars" and string at as "0" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "Chars" and string at as "0" and Padding "" and Alignment "Left"
	And an Input "0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "100002000030000"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                   | With  | Using | Pad | Align |
	| 1 | [[rs(1).row]] = 1 |       |       |     |       |
	|   | [[rs(2).row]] = 2 |       |       |     |       |
	|   | [[rs(3).row]] = 3 |       |       |     |       |
	|   |                   | Chars | 0     | ""  | Left  |
	| 2 | 0                 | Chars | 0     | ""  | Left  |
	| 3 | 0                 | None  | ""    | ""  | Left  |
	And the debug output as 
	|                               |
	| [[result]] = 100002000030000 |

Scenario: Merge a recordset table and free text using New Line
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]" and merge type "New Line" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "New Line" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is the same as file "NewLineExample.txt"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                   | With     | Using | Pad | Align |
	| 1 | [[rs(1).row]] = 1 |          |       |     |       |
	|   | [[rs(2).row]] = 2 |          |       |     |       |
	|   | [[rs(3).row]] = 3 |          |       |     |       |
	|   |                   | New Line | ""    | ""  | Left  |
	| 2 | 0                 | New Line | ""    | ""  | Left  |
	
Scenario: Merge a recordset table and free text using Tab
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]tab->" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "<-" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "1tab->	<-2tab->	<-3tab->	<-"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                             | With | Using | Pad | Align |
	| 1 | [[rs(1).row]]tab-> = 1tab-> |      |       |     |       |
	|   | [[rs(2).row]]tab-> = 2tab-> |      |       |     |       |
	|   | [[rs(3).row]]tab-> = 3tab-> |      |       |     |       |
	|   |                             | Tab  | ""    | ""  | Left  |
	| 2 | <-                          | None | ""    | ""  | Left  |
	And the debug output as 
	|                                           |
	| [[result]] = 1tab->	<-2tab->	<-3tab->	<- |

Scenario: Merge a variable using index that is a char
	Given a merge variable "[[a]]" equal to "aA " 	
	And an Input "[[a]]" and merge type "Index" and string at as "b" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |            | With  | Using | Pad | Align |
	| 1 | [[a]] = aA | Index | b     | ""  | Left  |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Merge a variable using index that is a variable and is blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to ""	
	And an Input "[[a]]" and merge type "Index" and string at as "[[b]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |            | With  | Using   | Pad | Align |
	| 1 | [[a]] = aA | Index | [[b]] = | ""  | Left  |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Merge multiple variables on Chars with blank lines
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"	
	And an Input "[[a]]" and merge type "Chars" and string at as "/" and Padding " " and Alignment "Left"		
	And an Input "[[b]]" and merge type "Chars" and string at as "/" and Padding " " and Alignment "Left"	
	When the data merge tool is executed
	Then the merged result is "Warewolf /Rocks/"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Chars | /     | " " | Left  |
	| 2 | [[b]] = Rocks    | Chars | /     | " " | Left  |	
	And the debug output as 
	|                                |
	| [[result]] = Warewolf /Rocks/ |

Scenario: Merge a recordset that has xml data using Tabs
	Given a merge recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |	
	And an Input "<record>" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"		
	And an Input "[[rs(*).row]]" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"		
	And an Input "</record>" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "<record>	<x id="1">One</x>	</record><record>	<x id="2">two</x>	</record><record>	<x id="3">three</x>	</record>"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                                     | With | Using | Pad | Align |
	| 1 | <record>                            | Tab  | ""    | ""  | Left  |
	| 2 | [[rs(1).row]] = <x id="1">One</x>   |      |       |     |       |
	|   | [[rs(2).row]] = <x id="2">two</x>   |      |       |     |       |
	|   | [[rs(3).row]] = <x id="3">three</x> |      |       |     |       |
	|   |                                     | Tab  | ""    | ""  | Left  |
	| 3 | </record>                           | None | ""    | ""  | Left  |
	And the debug output as 
	|                                                                                                                              |
	| [[result]] = <record>	<x id="1">One</x>	</record><record>	<x id="2">two</x>	</record><record>	<x id="3">three</x>	</record> |


Scenario: Merge a short string using big index and padding and alignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "123"
	And an Input "[[a]]" and merge type "Index" and string at as "10" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "5" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "Warewolf  00123"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Index | 10    | " " | Left  |
	| 2 | [[b]] = 123      | Index | 5     | 0   | Right |
	And the debug output as 
	|                               |
	| [[result]] = Warewolf  00123 |
	

Scenario: Merge a long string using small index and padding and alignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "3" and Padding "" and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "3" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "War123"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Index | 3     | ""  | Left  |
	| 2 | [[b]] = 12345    | Index | 3     | 0   | Right |
	And the debug output as 
	|                     |
	| [[result]] = War123 |

	 
Scenario: Merge a long string using small index and padding and alignment at invalid index
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "-1" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "-1" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Index | -1    | " " | Left  |
	| 2 | [[b]] = 12345    | Index | -1    | 0   | Right |	
	And the debug output as 
	|               |
	| [[result]] = |
	
Scenario: Merge a long string using small index and padding and alignment at invalid quoted index
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as ""-1"" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as ""-1"" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Index | "-1"    | " " | Left  |
	| 2 | [[b]] = 12345    | Index | "-1"    | 0   | Right |	
	And the debug output as 
	|               |
	| [[result]] = |
		
Scenario: Merge a long string using small index and padding multiple character and alignment at index
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "1" and Padding "eee" and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "1" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |                  | With  | Using | Pad | Align |
	| 1 | [[a]] = Warewolf | Index | 1     | eee | Left  |
	| 2 | [[b]] = 12345    | Index | 1     | 0   | Right |	
	And the debug output as 
	|               |
	| [[result]] = |
	
Scenario: Merge a negative recordset index Input
	Given an Input "[[my(-1).a]]" and merge type "Index" and string at as "10" and Padding " " and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # |                | With  | Using | Pad | Align |
	| 1 | [[my(-1).a]] = | Index | 10    | " " | Left  |  
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Merge a negative recordset index for String At
	Given an Input "12" and merge type "Index" and string at as "[[my(-1).a]]" and Padding " " and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # |       | With  | Using          | Pad | Align |
	| 1 | 12    | Index | [[my(-1).a]] = | " " | Left  |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Merge a negative recordset index for Padding
	Given an Input "12" and merge type "Index" and string at as "10" and Padding "[[my(-1).a]]" and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| # |       | With  | Using | Pad            | Align |
	| 1 | 12    | Index | 10    | [[my(-1).a]] = | Left  |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Merge a variable using index that is a variable and is not blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to "bB "
	And a merge variable "[[c]]" equal to "1"	
	And an Input "[[a]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "ab"
	And the execution has "NO" error
	And the debug inputs as  
	| # |            | With  | Using     | Pad | Align |
	| 1 | [[a]] = aA | Index | [[c]] = 1 | ""  | Left  |
	| 2 | [[b]] = bB | Index | [[c]] = 1 | ""  | Left  |
	And the debug output as 
	|                  |
	| [[result]] = ab |
	
Scenario: Merge a variable using index that is blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to "bB "
	And a merge variable "[[c]]" equal to "1"	
	And an Input "[[a]]" and merge type "Index" and string at as "" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |            | With  | Using     | Pad | Align |
	| 1 | [[a]] = aA | Index | ""        | ""  | Left  |
	| 2 | [[b]] = bB | Index | [[c]] = 1 | ""  | Left  |
	And the debug output as 
	|               |
	| [[result]] = |
	