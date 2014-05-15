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
	|              |
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



Scenario: Merge a variable inside a variable
	Given a merge variable "[[a]]" equal to "b"
	And a merge variable "[[b]]" equal to "c"
	And a merge variable "[[c]]" equal to "test"
	And a merge variable "[[test]]" equal to "Warewolf"
	And an Input "[[[[[[[[a]]]]]]]]" and merge type "Index" and string at as "8" and Padding "" and Alignment "Left"
	And an Input "[[c]]" and merge type "Index" and string at as "4" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "Warewolftest"
	And the execution has "NO" error
	And the debug inputs as  
	| # |                              | With  | Using | Pad | Align |
	| 1 | [[[[[[[[a]]]]]]]] = Warewolf | Index | "8"   | ""  | Left  |
	| 2 | [[c]]             = test     | Index | "4"   | ""  | Left  |
	And the debug output as 
	|                           |
	| [[result]] = Warewolftest |

#Scenario: Merge a variable inside the invalid varaible
#	Given a merge variable "[[a]]" equal to "test%$ "
#	And a merge variable "[[b]]" equal to "warewolf "
#	And an Input "[[[[a]]]]" and merge type "Index" and string at as "" and Padding "" and Alignment "Left"
#	When the data merge tool is executed
#	Then the merged result is ""
#	And the execution has "AN" error
#	And the debug inputs as  
#	| # |                        | With  | Using | Pad | Align |
#	| 1 | [[[[a]]]] = [[test%$]] | Index | "4"   | ""  | Left  |
#	And the debug output as 
#	|              |
#	| [[result]] = |

#Scenario Outline: Validation errors for all Invalid variables in datamerge 
#	Given a merge variable '<Variable1>' equal to "aA "
#	And a merge variable "[[b]]" equal to "bB "
#	And an Input '<Variable1>' and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
#	And an Input "[[b]]" and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
#	When the data merge tool is executed
#	Then the merged result is ""
#	And the execution has "AN" error
#	And the debug inputs as  
#	| # |                  | With  | Using | Pad | Align |
#	| 1 | <Variable1> = aA | Index | ""    | ""  | Left  |
#	| 2 | [[b]] = bB       | Index | 1     | ""  | Left  |
#	And the debug output as 
#	| # |           |
#	|   | <error> = |
#Examples: 	
#	 | no | Variable                                  | error                                                                                                                                                                                                                                                   |
#	 | 1  | [[rec().a]]=]]                            | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 2  | [[rec'()'.a]]                             | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
#	 | 3  | [[rec"()".a]]                             | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 4  | [[rec".a]]                                | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 5  | [[rec.a]]                                 | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
#	 | 6  | [[rec()*.a]]                              | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 9  | [[rec().a]]*                              | Variable name [[rec().a]]* contains invalid character(s)                                                                                                                                                                                                |
#	 | 10 | [[1]]                                     | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
#	 | 11 | [[@]]                                     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
#	 | 12 | [[var#]]                                  | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 13 | [[var]]00]]                               | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 14 | [[var]]@]]                                | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#	 | 15 | [[var.()]]                                | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 16 | [[]]                                      | Variable [[]] is missing a name                                                                                                                                                                                                                         |
#	 | 17 | [[()]]                                    | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
#	 | 28 | [[var[[]]                                 | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
#	 | 29 | [[var1.a]]                                | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 20 | [[rec()!a]]                               | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
#	 | 21 | [[rec()         a]]                       | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
#	 | 22 | [[{{rec(_).a}}]]]                         | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
#	 | 23 | [[rec(23).[[var*]]]]                      | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
#	 | 24 | [[rec().a]]&[[a]]                         | One variable only allowed in the output field                                                                                                                                                                                                           |
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
#	 | 87 | [[rec().a[[a]]                            | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |  |
#	 | 89 | [[rec(-1).a                               | Recordset index -1]is not greater than zero                                                                                                                                                                                                             |
#	 | 90 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
#	 | 91 | [[rec()                                   | Recordset variable that needs a field name(s)                                                                                                                                                                                                           |                                                                                                                                                                                                                                                

	 