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
	| 1 | [[a]] = Warewolf | None | ""    | ""  | Left  |
	| 2 | [[b]] = Rocks    | None | ""    | ""  | Left  |
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
	|   |                     | None | ""    | ""  | Left  |
	| 2 | 0                   | None | ""    | ""  | Left  |	
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
	| # |      | With  | Using | Pad | Align |
	| 1 | "" = | Index | 10    | " " | Left  |  
	And the debug output as 
	|              |
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
	|              |
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

Scenario: Merge a variable inside the invalid varaible
	Given a merge variable "[[a]]" equal to "test%$ "
	And a merge variable "[[b]]" equal to "warewolf "
	And an Input "[[[[a]]]]" and merge type "Index" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |             | With  | Using | Pad | Align |
	| 1 | [[[[a]]]] = | Index | ""    | ""  | Left  |
	And the debug output as 
	|              |
	| [[result]] = |
#
Scenario Outline: Validation errors for all Invalid variables in datamerge 
	Given a merge variable '<Variable>' equal to "aA "
	And a merge variable "[[b]]" equal to "bB "	
	And an Input "<Variable>" and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |            | With  | Using | Pad | Align |
	| 1 | "" = ""    | Index | 2    | ""  | Left  |
	| 2 | [[b]] = bB | Index | 2     | ""  | Left  |
	And the debug output as 
	| # |              |
	|   | [[result]] = | 
Examples: 	
	 | no | Variable                                  |	
	 | 2  | [[rec'()'.a]]                             |
	 | 3  | [[rec"()".a]]                             |
	 | 4  | [[rec".a]]                                |
	 | 5  | [[rec.a]]                                 |
	 | 6  | [[rec()*.a]]                              |	
	 | 8  | [[1]]                                     |
	 | 9  | [[@]]                                     |
	 | 10 | [[var#]]                                  |	
	 | 13 | [[var.()]]                                |
	 | 14 | [[]]                                      |
	 | 15 | [[()]]                                    |
	 | 16 | [[var[[]]                                 |
	 | 17 | [[var1.a]]                                |
	 | 18 | [[rec()!a]]                               |
	 | 19 | [[rec()         a]]                       |
	 | 20 | [[{{rec(_).a}}]]]                         |
	 | 21 | [[rec(23).[[var*]]]]                      |	
	 | 24 | [[var  ]]                                 |
	 | 25 | [[var@]]                                  |
	 | 26 | [[var#]]                                  |	
	 | 28 | [[(1var)]]                                |
	 | 29 | [[1var)]]                                 |
	 | 30 | [[var.()]]                                |
	 | 31 | [[var  ]]                                 |
	 | 32 | [[var~]]                                  |
	 | 33 | [[var+]]                                  |
	 | 34 | [[var]a]]                                 |
	 | 35 | [[var[a]]                                 |
	 | 36 | [[var 1]]                                 |
	 | 37 | [[var[[]]                                 |
	 | 38 | [[var[[1]]]]                              |
	 | 39 | [[var.a]]                                 |
	 | 40 | [[var1.a]]                                |	
	 | 42 | [[var*]]                                  |
	 | 43 | [[1var]]                                  |
	 | 44 | [[@var]]                                  |	 
	 | 46 | [[var,]]                                  |
	 | 47 | [[:var 1]]                                |
	 | 48 | [[,var]]                                  |
	 | 49 | [[test,var]]                              |
	 | 50 | [[test. var]]                             |
	 | 51 | [[test.var]]                              |
	 | 52 | [[test. 1]]                               |
	 | 53 | [[rec(*).&]]                              |
	 | 54 | [[rec(),a]]                               |
	 | 55 | [[rec()         a]]                       |
	 | 56 | [[rec(1).[[rec().1]]]]                    |
	 | 57 | [[rec(a).[[rec().a]]]]                    |
	 | 58 | [[{{rec(_).a}}]]]                         |
	 | 60 | [[*[{{rec(_).a}}]]]                       |
	 | 61 | [[rec(23).[[var}]]]]                      |
	 | 62 | [[rec(23).[[var*]]]]                      |
	 | 63 | [[rec(23).[[var%^&%]]]]                   |	 	 
	 | 66 | [[rec()..]]                               |
	 | 67 | [[rec().a.b]]                             |	
	 | 69 | [[rec"()".a]]                             |
	 | 70 | [[rec'()'.a]]                             |
	 | 71 | [[rec").a]]                               |
	 | 72 | [[rec{a]]                                 |
	 | 73 | [[rec{a}]]                                |
	 | 74 | [[rec()*.a]]                              |
	 | 75 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] |	
	  #| 1  | [[rec().a]]=]]                            |
	   #| 68 | [[rec().a]].a]]                           |
	   #| 64 | [[rec().a]]234234]]                       |
	   #| 45 | [[var]](var)]]                            |
	    #| 41 | [[[[a]].[[b]]]]cd]]                       |
		 #| 22 | [[rec().a]]&[[a]]                         |
	 #| 23 | a[[rec([[[[b]]]]).a]]@                    |
	  #| 11 | [[var]]00]]                               |
	 #| 12 | [[var]]@]]                                |
	  #| 7  | [[rec().a]]*                              |
	   #| 27 | [[var]]]]                                 |

Scenario Outline: Validation errors for all Invalid recordset indexes in datamerge 
	Given a merge variable '<Variable>' equal to "aA "
	And a merge variable "[[b]]" equal to "bB "	
	And an Input "<Variable>" and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "2" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug inputs as  
	| # |                 | With  | Using | Pad | Align |
	| 1 | <Variable> = "" | Index | 2     | ""  | Left  |
	| 2 | [[b]] = bB      | Index | 2     | ""  | Left  |
	And the debug output as 
	| # |              |
	|   | [[result]] = | 
Examples: 	
	 | no | Variable     |
	 | 1  | [[rec(@).a]] |
	 | 2  | [[rec(+).a]] |
	 | 3  | [[rec(-).a]] |
	 | 4  | [[rec(!).a]] |
	 | 5  | [[rec(q).a]] |
	 | 6  | [[rec(w).a]] |
	 | 7  | [[rec(:).a]] |
	 | 8  | [[rec(,).a]] |
	 | 9  | [[rec(().a]] |
	 | 10 | [[rec()).a]] |
	 | 11 | [[rec(.).a]] |



#Scenario Outline: Validation errors for all Invalid variables in datamerge 
#	 Given I have a variable "[[a]]" with a value '<Val1>'
#	 Given I have a variable "[[b]]" with a value '<Val2>'
#	 Given I have a variable "[[z]]" with a value "1"
#	 Given I have a variable "[[rec(1).a]]" with a value '<Val1>'
#	 Given I have a variable "[[rec(2).a]]" with a value '<Val2>'	
#	 And "WorkflowWithAssignDataMergeAndDataSplittools" contains Data Merge "Data Merge" into '<Result>' as	
#	  | Variable        | Type  | Using     | Padding | Alignment |
#	  | <InputVariable> | Index | <Padding> |         | Left      |
#	When validating the tool
#	Then validation is '<Validation>'
#	And validation message is '<DesignValidation>'
#	When the assign tool is executed
#	And the execution has "<errorOccured>" error
#	And execution error message will be '<ExecutionError>'	
#Examples: 	
#	 | no | InputVariable             | Result                 | Val1 | Val2 | Using               | Padding             | Validation | DesignValidation                                                                     | errorOccured | ExecutionError                                                                         |
#	 | 1  | [[rec'()'.a]]             | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Recordset name [[rec'()']] contains invalid character(s)                     | AN           | 1 'Input'-Recordset name [[rec'()']] contains invalid character(s)                     |
#	 | 2  | [[rec".a]]                | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[rec".a]] contains invalid character(s)                       | AN           | 1 'Input'-Variable name [[rec".a]] contains invalid character(s)                       |
#	 | 3  | [[rec.a]]                 | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[rec.a]]  contains invalid character(s)                       | AN           | 1 'Input'-Variable name [[rec.a]]  contains invalid character(s)                       |
#	 | 4  | [[rec()*.a]]              | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[rec()*]]  contains invalid character(s)                      | AN           | 1 'Input'-Variable name [[rec()*]]  contains invalid character(s)                      |
#	 | 5  | [[1]]                     | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[1]] begins with a number                                     | AN           | 1 'Input'-Variable name [[1]] begins with a number                                     |
#	 | 6  | [[@]]                     | [[Merge]]              | 0    | 0    | 0                   |                     | True       | 'Input'-Variable name [[@]]  contains invalid character(s)                           | AN           | 1 'Input'-Variable name [[@]]  contains invalid character(s)                           |
#	 | 8  | [[var#]]                  | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[var#]]  contains invalid character(s)                        | AN           | 1 'Input'-Variable name [[var#]]  contains invalid character(s)                        |
#	 | 9  | [[rec.()]]                | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Recordset name [[rec().]] contains invalid character(s)                      | AN           | 1 'Input'-Recordset name [[rec().]] contains invalid character(s)                      |
#	 | 10 | [[]]                      | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable [[]] is missing a name                                              | AN           | 1 'Input'-Variable [[]] is missing a name                                              |
#	 | 1  | [[var[[]]                 | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-[[Var]] contains a space, this is an invalid character for a variable name   | AN           | 1 'Input'-[[Var]] contains a space, this is an invalid character for a variable name   |
#	 | 12 | [[var1.a]]                | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[var1.a]] contains invalid character(s)                       | AN           | 1  'Input'-Variable name [[var1.a]] contains invalid character(s)                      |
#	 | 13 | [[rec()!a]]               | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Recordset name [[rec()!a]]  contains invalid character(s)                    | AN           | 1 'Input'-Recordset name [[rec()!a]]  contains invalid character(s)                    |
#	 | 14 | [[rec()         a]]       | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Recordset name [[rec()         a]]  contains invalid character(s)            | AN           | 1 'Input'-Recordset name [[rec()         a]]  contains invalid character(s)            |
#	 | 15 | [[{{rec(_).a}}]]]         | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Recordset name [[{{rec]]  contains invalid character(s)                      | AN           | 1 'Input'-Recordset name [[{{rec]]  contains invalid character(s)                      |
#	 | 16 | [[rec(1).[[a]]]]          | [[Merge]]              | a    | 0    | 1                   |                     | False      | ""                                                                                   | AN           | ""                                                                                     |
#	 | 18 | [[var.()]]                | [[Merge]]              | 0    | 0    | 1                   |                     | True       | 'Input'-Variable name [[var.()]]  contains invalid character(s)                      | AN           | 1.'Input'-Variable name [[var.()]] contains invalid character(s)                       |
#	 | 19 | [[b[[a]]]]                | [[Merge]]              | a    | b    | 1                   |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 20 | [[b[a]]                   | [[Merge]]              | a    | b    | 1                   |                     | True       | 'Input'-Variable name [[b[a]]  contains invalid character(s)                         | AN           | 1'Input'-Variable name [[b[a]]  contains invalid character(s)                          |
#	 | 21 | [[var]]                   | [[Merge]]              | a    | b    | 1                   |                     | False      | ""                                                                                   | NO           | 1 No Value assigned for: [[var]]                                                       |
#	 | 22 | [[var[[1]]]]              | [[Merge]]              | a    | b    | 1                   |                     | True       | 'Input'-[[var]] does not exist in your variable list                                 | AN           | 1'Input'-[[var]] does not exist in your variable list                                  |
#	 | 23 | [[rec().a]]TEST           | [[Merge]]              | a    | b    | 1                   |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 24 | Test                      | [[Merge]]              | a    | b    | 1                   |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 25 | Test[[a]][[b]][[rec().a]] | [[Merge]]              | a    | b    | 1                   |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 26 | [[a]]                     |                        | Ware | Wolf | 1                   |                     | True       | Result cannot be empty                                                               | AN           | 1 Result Field cannot be empty                                                         |
#	 | 27 | [[a]]                     | [[result]][[a]]        | Ware | Wolf | 1                   |                     | True       | Result - Invalid Region [[var[[a]]]]                                                 | AN           | 1 Result - Invalid Region [[var[[a]]]]                                                 |
#	 | 28 | [[a]]                     | [[a]]*]]               | Ware | Wolf | 1                   |                     | True       | Result - Invalid expression: opening and closing brackets don't match                | AN           | 1 Result - Invalid expression: opening and closing brackets don't match                |
#	 | 29 | [[a]]                     | [[var@]]               | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[var@]]  contains invalid character(s)                       | AN           | 1 Result - Variable name [[var@]]  contains invalid character(s)                       |
#	 | 30 | [[a]]                     | [[var]]00]]            | Ware | Wolf | 1                   |                     | True       | Result - Invalid expression: opening and closing brackets don't match                | AN           | 1 Result - Invalid expression: opening and closing brackets don't match                |
#	 | 31 | [[a]]                     | [[(1var)]]             | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[(1var)]] contains invalid character(s)                      | AN           | 1 Result - Variable name [[(1var)]] contains invalid character(s)                      |
#	 | 32 | [[a]]                     | [[var[[a]]]]           | Ware | Wolf | 1                   |                     | True       | Result - Invalid Region [[var[[a]]]]                                                 | AN           | 1 Result - Invalid Region [[var[[a]]]]                                                 |
#	 | 33 | [[a]]                     | [[var.a]]              | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[var.a]] contains invalid character(s)                       | AN           | 1 Result - Variable name [[var.a]] contains invalid character(s)                       |
#	 | 34 | [[a]]                     | [[@var]]               | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[@var]] contains invalid character(s)                        | AN           | 1 Result - Variable name [[@var]] contains invalid character(s)                        |
#	 | 35 | [[a]]                     | [[var 1]]              | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[var 1]] contains invalid character(s)                       | AN           | 1 Result - Variable name [[var 1]] contains invalid character(s)                       |
#	 | 36 | [[a]]                     | [[rec(1).[[rec().1]]]] | Ware | Wolf | 1                   |                     | True       | Result - Invalid Region [[rec(1).[[rec().1]]]]                                       | AN           | 1 Result - Invalid Region [[rec(1).[[rec().1]]]]                                       |
#	 | 37 | [[a]]                     | [[rec(@).a]]           | Ware | Wolf | 1                   |                     | True       | Result - Recordset index [[rec(@).a]] contains invalid character(s)                  | AN           | 1 Result - Recordset index [[rec(@).a]] contains invalid character(s)                  |
#	 | 38 | [[a]]                     | [[rec"()".a]]          | Ware | Wolf | 1                   |                     | True       | Result - Recordset name [[rec"()"]] contains invalid character(s)                    | AN           | 1 Result - Recordset name [[rec"()"]] contains invalid character(s)                    |
#	 | 39 | [[a]]                     | [[rec([[[[b]]]]).a]]   | Ware | Wolf | 1                   |                     | True       | Result - Invalid Region [[var[[a]]]]                                                 | AN           | 1 Result - Invalid Region [[var[[a]]]]                                                 |
#	 | 40 | [[a]]                     | [[var@]]               | Ware | Wolf | 1                   |                     | True       | Result - Variable name [[rec([[[[b]]]]).a]]    contains invalid character(s)         | AN           | 1 Result - Variable name [[rec([[[[b]]]]).a]]    contains invalid character(s)         |
#	 | 41 | [[a]]                     | C#$%#$]]               | Ware | Wolf | 1                   |                     | True       | Result - Invalid expression: opening and closing brackets don't match                | AN           | 1 Result - Invalid expression: opening and closing brackets don't match                |
#	 | 42 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec'()'.a]]       |                     | True       | 'Using'-Recordset name [[rec'()']] contains invalid character(s)                     | AN           | 1 'Using'-Recordset name [[rec'()']] contains invalid character(s)                     |
#	 | 43 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec".a]]          |                     | True       | 'Using'-Variable name [[rec".a]] contains invalid character(s)                       | AN           | 1 'Using'-Variable name [[rec".a]] contains invalid character(s)                       |
#	 | 44 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec.a]]           |                     | True       | 'Using'-Variable name [[rec.a]]  contains invalid character(s)                       | AN           | 1 'Using'-Variable name [[rec.a]]  contains invalid character(s)                       |
#	 | 45 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec()*.a]]        |                     | True       | 'Using'-Variable name [[rec()*]]  contains invalid character(s)                      | AN           | 1 'Using'-Variable name [[rec()*]]  contains invalid character(s)                      |
#	 | 46 | [[a]]                     | [[Merge]]              | 0    | 0    | [[1]]               |                     | True       | 'Using'-Variable name [[1]] begins with a number                                     | AN           | 1 'Using'-Variable name [[1]] begins with a number                                     |
#	 | 47 | [[a]]                     | [[Merge]]              | 0    |      | [[@]]               |                     | True       | 'Using'-Variable name [[@]]  contains invalid character(s)                           | AN           | 1 'Using'-Variable name [[@]]  contains invalid character(s)                           |
#	 | 48 | [[a]]                     | [[Merge]]              | 0    | 0    | [[var#]]            |                     | True       | 'Using'-Variable name [[var#]]  contains invalid character(s)                        | AN           | 1 'Using'-Variable name [[var#]]  contains invalid character(s)                        |
#	 | 49 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec.()]]          |                     | True       | 'Using'-Recordset name [[rec().]] contains invalid character(s)                      | AN           | 1 'Using'-Recordset name [[rec().]] contains invalid character(s)                      |
#	 | 50 | [[a]]                     | [[Merge]]              | 0    | 0    | [[]]                |                     | True       | 'Using'-Variable [[]] is missing a name                                              | AN           | 1 'Using'-Variable [[]] is missing a name                                              |
#	 | 51 | [[a]]                     | [[Merge]]              | 0    | 0    | [[var[[]]           |                     | True       | 'Using'-[[Var]] contains a space, this is an invalid character for a variable name   | AN           | 1 'Using'-[[Var]] contains a space, this is an invalid character for a variable name   |
#	 | 52 | [[a]]                     | [[Merge]]              | 0    | 0    | [[var1.a]]          |                     | True       | 'Using'-Variable name [[var1.a]] contains invalid character(s)                       | AN           | 1 'Using'-Variable name [[var1.a]] contains invalid character(s)                       |
#	 | 53 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec()!a]]         |                     | True       | 'Using'-Recordset name [[rec()!a]]  contains invalid character(s)                    | AN           | 1 'Using'-Recordset name [[rec()!a]]  contains invalid character(s)                    |
#	 | 54 | [[a]]                     | [[Merge]]              | 0    | 0    | [[rec()         a]] |                     | True       | 'Using'-Recordset name [[rec()         a]]  contains invalid character(s)            | AN           | 1 'Using'-Recordset name [[rec()         a]]  contains invalid character(s)            |
#	 | 55 | [[a]]                     | [[Merge]]              | 0    | 0    | [[{{rec(_).a}}]]]   |                     | True       | 'Using'-Recordset name [[{{rec]]  contains invalid character(s)                      | AN           | 1 'Using'-Recordset name [[{{rec]]  contains invalid character(s)                      |
#	 | 56 | [[a]]                     | [[Merge]]              | 1    | a    | [[rec(1).[[b]]]]    |                     | False      | ""                                                                                   | AN           | ""                                                                                     |
#	 | 57 | [[a]]                     | [[Merge]]              | 0    | 0    | [[var.()]]          |                     | True       | 'Using'-Variable name [[var.()]]  contains invalid character(s)                      | AN           | 1.'Using'-Variable name [[var.()]] contains invalid character(s)                       |
#	 | 58 | Test                      | [[Merge]]              | 1    | a    | [[[[b]]]]           |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 59 | [[a]]                     | [[Merge]]              | a    | b    | [[b[a]]             |                     | True       | 'Using'-Variable name [[b[a]]  contains invalid character(s)                         | AN           | 1'Using'-Variable name [[b[a]]  contains invalid character(s)                          |
#	 | 60 | [[a]]                     | [[Merge]]              | a    | 1    | [[b]]               |                     | False      | ""                                                                                   | NO           | 1 No Value assigned for: [[var]]                                                       |
#	 | 61 | [[a]]                     | [[Merge]]              | a    | b    | [[var[[1]]]]        |                     | True       | 'Using'-[[var]] does not exist in your variable list                                 | AN           | 1'Using'-[[var]] does not exist in your variable list                                  |
#	 | 62 | [[a]]                     | [[Merge]]              | 0    | b    | [[rec().a]]1        |                     | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 63 | Test                      | [[Merge]]              | a    | b    |                     |                     | True       | 'Using' cannot be empty                                                              | AN           | 1 'Using' cannot be empty                                                              |
#	 | 64 | [[a]]                     | [[Merge]]              | a    | b    | A                   |                     | True       | 'Using' must be a real number                                                        | AN           | 1 'Using' must be a real number                                                        |
#	 | 65 | [[a]]                     | [[Merge]]              | a    | b    | -1                  |                     | True       | 'Using' must be a real number                                                        | AN           | 1 'Using' must be a real number                                                        |
#	 | 66 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec'()'.a]]       | True       | 'Padding'-Recordset name [[rec'()']] contains invalid character(s)                   | AN           | 1 'Padding'-Recordset name [[rec'()']] contains invalid character(s)                   |
#	 | 67 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec".a]]          | True       | 'Padding'-Variable name [[rec".a]] contains invalid character(s)                     | AN           | 1 'Padding'-Variable name [[rec".a]] contains invalid character(s)                     |
#	 | 68 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec.a]]           | True       | 'Padding'-Variable name [[rec.a]]  contains invalid character(s)                     | AN           | 1 'Padding'-Variable name [[rec.a]]  contains invalid character(s)                     |
#	 | 69 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec()*.a]]        | True       | 'Padding'-Variable name [[rec()*]]  contains invalid character(s)                    | AN           | 1 'Padding'-Variable name [[rec()*]]  contains invalid character(s)                    |
#	 | 70 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[1]]               | True       | 'Padding'-Variable name [[1]] begins with a number                                   | AN           | 1 'Padding'-Variable name [[1]] begins with a number                                   |
#	 | 71 | [[a]]                     | [[Merge]]              | 0    |      | 4                   | [[@]]               | True       | 'Padding'-Variable name [[@]]  contains invalid character(s)                         | AN           | 1 'Padding'-Variable name [[@]]  contains invalid character(s)                         |
#	 | 72 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[var#]]            | True       | 'Padding'-Variable name [[var#]]  contains invalid character(s)                      | AN           | 1 'Padding'-Variable name [[var#]]  contains invalid character(s)                      |
#	 | 73 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec.()]]          | True       | 'Padding'-Recordset name [[rec().]] contains invalid character(s)                    | AN           | 1 'Padding'-Recordset name [[rec().]] contains invalid character(s)                    |
#	 | 74 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[]]                | True       | 'Padding'-Variable [[]] is missing a name                                            | AN           | 1 'Padding'-Variable [[]] is missing a name                                            |
#	 | 75 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[var[[]]           | True       | 'Padding'-[[Var]] contains a space, this is an invalid character for a variable name | AN           | 1 'Padding'-[[Var]] contains a space, this is an invalid character for a variable name |
#	 | 76 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[var1.a]]          | True       | 'Padding'-Variable name [[var1.a]] contains invalid character(s)                     | AN           | 1 'Padding'-Variable name [[var1.a]] contains invalid character(s)                     |
#	 | 77 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec()!a]]         | True       | 'Padding'-Recordset name [[rec()!a]]  contains invalid character(s)                  | AN           | 1 'Padding'-Recordset name [[rec()!a]]  contains invalid character(s)                  |
#	 | 78 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[rec()         a]] | True       | 'Padding'-Recordset name [[rec()         a]]  contains invalid character(s)          | AN           | 1 'Padding'-Recordset name [[rec()         a]]  contains invalid character(s)          |
#	 | 79 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[{{rec(_).a}}]]]   | True       | 'Padding'-Recordset name [[{{rec]]  contains invalid character(s)                    | AN           | 1 'Padding'-Recordset name [[{{rec]]  contains invalid character(s)                    |
#	 | 80 | [[a]]                     | [[Merge]]              | 1    | a    | 4                   | [[rec(1).[[b]]]]    | True       | 'Padding' must be a single character                                                 | AN           | ""                                                                                     |
#	 | 81 | [[a]]                     | [[Merge]]              | 0    | 0    | 4                   | [[var.()]]          | True       | 'Padding'-Variable name [[var.()]]  contains invalid character(s)                    | AN           | 1.'Padding'-Variable name [[var.()]] contains invalid character(s)                     |
#	 | 82 | Test                      | [[Merge]]              | 1    | a    | 4                   | [[[[b]]]]           | False      | ""                                                                                   | NO           | ""                                                                                     |
#	 | 83 | [[a]]                     | [[Merge]]              | a    | b    | 4                   | [[b[a]]             | True       | 'Padding'-Variable name [[b[a]]  contains invalid character(s)                       | AN           | 1'Padding'-Variable name [[b[a]]  contains invalid character(s)                        |
#	 | 84 | [[a]]                     | [[Merge]]              | a    | 1    | 4                   | [[b]]               | False      | ""                                                                                   | NO           | 1 No Value assigned for: [[var]]                                                       |
#	 | 85 | [[a]]                     | [[Merge]]              | a    | b    | 4                   | [[var[[1]]]]        | True       | 'Padding'-[[var]] does not exist in your variable list                               | AN           | 1'Padding'-[[var]] does not exist in your variable list                                |
#	 | 86 | [[a]]                     | [[Merge]]              | 0    | b    | 4                   | ,                   | False      | ""                                                                                   | NO           | ""                                                                                     |
	   









