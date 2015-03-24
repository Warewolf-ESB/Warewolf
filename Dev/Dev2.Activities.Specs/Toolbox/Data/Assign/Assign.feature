Feature: Assign
	In order to use variables 
	As a Warewolf user
	I want a tool that assigns data to variables

Scenario: Assign a value to a variable
	Given I assign the value 10 to a variable "[[var]]"	
	When the assign tool is executed
	Then the value of "[[var]]" equals 10
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value |
	| 1 | [[var]] = | 10        |
	And the debug output as 
	| # |              |
	| 1 | [[var]] = 10 |

Scenario: Assign a variable to a variable
	Given I assign the value 20 to a variable "[[var]]"	
	And I assign the value 60 to a variable "[[test]]"
	And I assign the value [[test]] to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals 60
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable      | New Value     |
	| 1 | [[var]]  =    | 20            |
	| 2 | [[test]] =    | 60            |
	| 3 | [[var]]  = 20 | [[test]] = 60 |
	And the debug output as
    | # |               |
    | 1 | [[var]] = 20  |
    | 2 | [[test]] = 60 |
    | 3 | [[var]] = 60  |


Scenario: Assign multiple variables with a calculate expression to a variable
	Given I assign the value SUM(1,2,3)-5 to a variable "[[var]]"	
	And I assign the value =[[var]] to a variable "[[test]]"	
	When the assign tool is executed
	Then the value of "[[test]]" equals 1
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable   | New Value                |
	| 1 | [[var]]  = | SUM(1,2,3)-5             |
	| 2 | [[test]] = | =[[var]] ==SUM(1,2,3)-5 |
	And the debug output as
    | # |                        |
    | 1 | [[var]] = SUM(1,2,3)-5 |
    | 2 | [[test]] = 1           |

Scenario: Assign multiple variables to a variable
	Given I assign the value Hello to a variable "[[var]]"	
	And I assign the value World to a variable "[[test]]"
	And I assign the value [[var]][[test]] to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals HelloWorld
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable    | New Value                        |
	| 1 | [[var]]   = | Hello                            |
	| 2 | [[test]]  = | World                            |
	| 3 | [[value]] = | [[var]][[test]] = HelloWorld |
	And the debug output as
    | # |                         |
    | 1 | [[var]] = Hello         |
    | 2 | [[test]] = World        |
    | 3 | [[value]]  = HelloWorld |

Scenario: Assign a variable to mixed scalar, char and recordset values
	Given I assign the value Hello to a variable "[[var]]"	
	And I assign the value World to a variable "[[rec(1).set]]"
	And I assign the value [[var]] [[rec(1).set]] ! to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals "Hello World !"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value                                  |
	| 1 | [[var]]        = | Hello                                      |
	| 2 | [[rec(1).set]] = | World                                      |
	| 3 | [[value]]      = | [[var]] [[rec(1).set]] ! = Hello World ! |
	And the debug output as
    | # |                           |
    | 1 | [[var]] = Hello           |
    | 2 | [[rec(1).set]] = World    |
    | 3 | [[value]] = Hello World ! |

Scenario: Assign multiple variables to the end of a recordset
	Given I assign the value 10 to a variable "[[rec().set]]"	
	And I assign the value 20 to a variable "[[rec().set]]"
	And I assign the value 30 to a variable "[[rec().set]]"
	And I assign the value [[rec(3).set]] to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals 30
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable        | New Value           |
	| 1 | [[rec().set]] = | 10                  |
	| 2 | [[rec().set]] = | 20                  |
	| 3 | [[rec().set]] = | 30                  |
	| 4 | [[value]]     = | [[rec(3).set]] = 30 |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[value]] = 30      |

Scenario: Assign all recordset values to a single variable
	Given I assign the value 10 to a variable "[[rec(1).set]]"	
	And I assign the value 20 to a variable "[[rec(2).set]]"
	And I assign the value 30 to a variable "[[rec(3).set]]"
	And I assign the value "" to a variable "[[rec(*).set]]"
	When the assign tool is executed
	Then the value of "[[rec(3).set]]" equals ""
	And the value of "[[rec(2).set]]" equals ""
	And the value of "[[rec(1).set]]" equals ""
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable            | New Value |
	| 1 | [[rec(1).set]] =    | 10        |
	| 2 | [[rec(2).set]] =    | 20        |
	| 3 | [[rec(3).set]] =    | 30        |
	| 4 | [[rec(1).set]] = 10 |           |
	|   | [[rec(2).set]] = 20 |           |
	|   | [[rec(3).set]] = 30 | " "       |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[rec(1).set]] = "" |
    |   | [[rec(2).set]] = "" |
    |   | [[rec(3).set]] = "" |

@ignore
Scenario: Assign all recordset values to all recordset
	Given I assign the value 10 to a variable "[[rec(1).set]]"	
	And I assign the value 20 to a variable "[[rec(2).set]]"
	And I assign the value 30 to a variable "[[rec(3).set]]"
	And I assign the value Hello to a variable "[[rs().val]]"
	And I assign the value "[[rec(*).set]]" to a variable "[[rs(*).val]]"
	When the assign tool is executed
	Then the value of "[[rs(1).val]]" equals 10
	And the value of "[[rs(2).val]]" equals 20
	And the value of "[[rs(3).val]]" equals 30
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable              | New Value           |
	| 1 | [[rec(1).set]] =      | 10                  |
	| 2 | [[rec(2).set]] =      | 20                  |
	| 3 | [[rec(3).set]] =      | 30                  |
	| 4 | [[rs().val]] =        | Hello               |
	| 5 | [[rs(1).set]] = Hello | [[rec(1).set]] = 10 |
	|   |                       | [[rec(2).set]] = 20 |
	|   |                       | [[rec(3).set]] = 30 |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[rs(1).val]] = 10  |
    |   | [[rs(2).val]] = 20  |
    |   | [[rs(3).val]] = 30  |

Scenario: Assign a record set to a scalar
	Given I assign the value 10 to a variable "[[rec(1).set]]"	
	And I assign the value 20 to a variable "[[rec(2).set]]"
	And I assign the value 30 to a variable "[[rec(3).set]]"
	And I assign the value "[[rec(*).set]]" to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals "30"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value           |
	| 1 | [[rec(1).set]] = | 10                  |
	| 2 | [[rec(2).set]] = | 20                  |
	| 3 | [[rec(3).set]] = | 30                  |
	| 4 | [[var]]        = | [[rec(1).set]] = 10 |
	|   |                  | [[rec(2).set]] = 20 |
	|   |                  | [[rec(3).set]] = 30 |
	And the debug output as
	| # |                     |
	| 1 | [[rec(1).set]] = 10 |
	| 2 | [[rec(2).set]] = 20 |
	| 3 | [[rec(3).set]] = 30 |
	| 4 | [[var]] = 30        |

Scenario: Assign a scalar equal to a record set
	Given I assign the value 30 to a variable "[[var]]"
	And I assign the value "[[var]]" to a variable "[[rec().set]]"
	When the assign tool is executed
	Then the value of "[[rec(1).set]]" equals "30"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable        | New Value     |
	| 1 | [[var]]       = | 30            |
	| 2 | [[rec().set]] = | [[var]]  = 30 |
	And the debug output as
	| # |                     |
	| 1 | [[var]] = 30        |
	| 2 | [[rec(1).set]] = 30 |	 

Scenario: Assign a scalar equal to a calculation
	Given I assign the value 30 to a variable "[[var]]"
	And I assign the value "=30-[[var]]" to a variable "[[Result]]"	
	When the assign tool is executed
	Then the value of "[[Result]]" equals "0"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable     | New Value            |
	| 1 | [[var]]    = | 30                   |
	| 2 | [[Result]] = | =30-[[var]] ==30-30 |
	And the debug output as
	| # |                |
	| 1 | [[var]] = 30   |
	| 2 | [[Result]] = 0 |

Scenario: Assign a variable equal to a group calculation (sum)
	Given I assign the value 30 to a variable "[[var1]]"
	And I assign the value 30 to a variable "[[var2]]"
	And I assign the value "=SUM([[var1]],[[var2]])" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "60"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable     | New Value                             |
	| 1 | [[var1]]   = | 30                                    |
	| 2 | [[var2]]   = | 30                                    |
	| 3 | [[Result]] = | =SUM([[var1]],[[var2]]) ==SUM(30,30) |
	And the debug output as
	| # |                 |
	| 1 | [[var1]] = 30   |
	| 2 | [[var2]] = 30   |
	| 3 | [[Result]] = 60 |

Scenario: Assign multiple recordset to the end of a recordset
	Given I assign the value 10 to a variable "[[rec().set]]"	
	And I assign the value 20 to a variable "[[rec().set]]"
	And I assign the value 30 to a variable "[[rec().set]]"
	And I assign the value [[rec(3).set]] to a variable "[[des().val]]"
	When the assign tool is executed
	Then the value of "[[des().val]]" equals 30
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable        | New Value          |
	| 1 | [[rec().set]] = | 10                 |
	| 2 | [[rec().set]] = | 20                 |
	| 3 | [[rec().set]] = | 30                 |
	| 4 | [[des().val]] = | [[rec(3).set]] =30 |
	And the debug output as
	| # |                     |
	| 1 | [[rec(1).set]] = 10 |
	| 2 | [[rec(2).set]] = 20 |
	| 3 | [[rec(3).set]] = 30 |
	| 4 | [[des(1).val]] = 30 |

Scenario: Assign the value of a negative recordset index
	Given I assign the value 10 to a variable "[[rec().set]]"	
	And I assign the value [[rec(-1).set]] to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals ""
	And the execution has "AN" error
	And the debug inputs as
	| # | Variable        | New Value         |
	| 1 | [[rec().set]] = | 10                |
	| 2 | [[var]]       = | [[rec(-1).set]] = |
	And the debug output as
	| # |                     |
	| 1 | [[rec(1).set]] = 10 |
	| 2 | [[var]] =           |

Scenario: Assign the value of a negative recordset index and another assign after
	Given I assign the value 10 to a variable "[[rec().set]]"	
	And I assign the value [[rec(-1).set]] to a variable "[[var]]"
	And I assign the value 30 to a variable "[[scalar]]"	
	When the assign tool is executed
	Then the value of "[[rec().set]]" equals "10"
	Then the value of "[[var]]" equals ""
	Then the value of "[[scalar]]" equals "30"
	And the execution has "AN" error
	And the debug inputs as
    | # | Variable        | New Value         |
    | 1 | [[rec().set]] = | 10                |
    | 2 | [[var]]       = | [[rec(-1).set]] = |
    | 3 | [[scalar]]    = | 30                |    
	And the debug output as
	| # |                     |
	| 1 | [[rec(1).set]] = 10 |
	| 2 | [[var]] =           |
	| 3 | [[scalar]] = 30     |

Scenario: Assign to a negative recordset index
	Given I assign the value 10 to a variable "[[des(-1).val]]"
	When the assign tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| # | Variable          | New Value |
	And the debug output as
	| # |                      |

Scenario: Assign a scalar equal to a calculation with a blank variable
	Given I assign the value "=[[cnt]]+1" to a variable "[[cnt]]"
	When the assign tool is executed
	Then the value of "[[cnt]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable  | New Value       |
	| 1 | [[cnt]] = | String = String |
	And the debug output as
	| # |             |
	| 1 | [[cnt]] = 1 |

#Bug 11499
Scenario Outline: Assign to a invalid variable
   Given I assign the value 10 to a variable '<var>'
   When the assign tool is executed
   Then the execution has '<error>' error
   And the debug inputs as
	| # | Variable | New Value |	
   And the debug output as
	| # |          |
   Examples:
	| no | var                                       | error |
	| 1  | [rec").a]]                                | AN    |
	| 2  | [[rec'()'.a]]                             | AN    |
	| 3  | [[rec"()".a]]                             | AN    |
	| 4  | [[rec".a]]                                | AN    |
	| 5  | [[rec.a]]                                 | AN    |
	| 6  | [[rec()*.a]]                              | AN    |
	| 7  | [[rec().a]].[[a]]                         | AN    |
	| 8  | [[rec().a]][[a]]                          | AN    |
	| 9  | [[rec().a]]*                              | AN    |
	| 10 | [[rec().a]] a                             | AN    |
	| 11 | [[1]]                                     | AN    |
	| 12 | [[rs(),.val]                              | AN    |
	| 13 | [[var#]]                                  | AN    |
	| 14 | [[var]]00]]                               | AN    |
	| 15 | [[var]]@]]                                | AN    |
	| 16 | [[var.()]]                                | AN    |
	| 17 | [[]]                                      | AN    |
	| 18 | [[()]]                                    | AN    |
	| 19 | [[var[[a]]]]                              | AN    |
	| 20 | [[var[[]]                                 | AN    |
	| 21 | [[var1.a]]                                | AN    |
	| 22 | [[rec()!a]]                               | AN    |
	| 23 | [[rec()         a]]                       | AN    |
	| 24 | [[{{rec(_).a}}]]]                         | AN    |
	| 25 | [[rec(23).[[var*]]]]                      | AN    |
	| 26 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | AN    |
	| 27 | [[rec().a]]&[[a]]                         | AN    |
	| 28 | a[[rec([[[[b]]]]).a]]@                    | AN    |
	| 29 | [[var  ]]                                 | AN    |
	| 30 | [[rec()                                   | AN    |
	
Scenario: Assign values to different columns in a reccord set
       Given I assign the value 10 to a variable "[[rec().a]]"       
       And I assign the value 20 to a variable "[[rec().b]]"
       And I assign the value 30 to a variable "[[rec().c]]"
       And I assign the value [[rec().a]] to a variable "[[d]]"
       And I assign the value [[rec().b]] to a variable "[[e]]"
       And I assign the value [[rec().c]] to a variable "[[f]]"
       When the assign tool is executed
       Then the value of "[[d]]" equals 10
       And the value of "[[e]]" equals 20
       And the value of "[[f]]" equals 30
       And the execution has "NO" error
       And the debug inputs as
       | # | Variable      | New Value        |
       | 1 | [[rec().a]] = | 10               |
       | 2 | [[rec().b]] = | 20               |
       | 3 | [[rec().c]] = | 30               |
       | 4 | [[d]]     =   | [[rec().a]] = 10 |
       | 5 | [[e]]     =   | [[rec().b]] = 20 |
       | 6 | [[f]]     =   | [[rec().c]] = 30 |
       And the debug output as
    | # |                   |
    | 1 | [[rec(1).a]] = 10 |
    | 2 | [[rec(1).b]] = 20 |
    | 3 | [[rec(1).c]] = 30 |
    | 4 | [[d]] = 10        |
    | 5 | [[e]] = 20        |
    | 6 | [[f]] = 30        |

#Below 3 Scenarios should be passed after the bug 12131 is fixed
Scenario: Assign a record set variable equal to a group calculation (sum)
	Given I assign the value 30 to a variable "[[rec(1).a]]"
	And I assign the value 30 to a variable "[[rec(1).b]]"
	And I assign the value "=SUM([[rec(1).a]],[[rec(1).b]])" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "60"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value                                   |
	| 1 | [[rec(1).a]]   = | 30                                          |
	| 2 | [[rec(1).b]]   = | 30                                          |
	| 3 | [[Result]] =     | =SUM([[rec(1).a]],[[rec(1).b]]) ==SUM(30,30) |
	And the debug output as
	| # |                   |
	| 1 | [[rec(1).a]] = 30 |
	| 2 | [[rec(1).b]] = 30 |
	| 3 | [[Result]] = 60   |

#
Scenario: Assign a variable equal to a group calculation with scalar and recordset
	Given I assign the value 1 to a variable "[[a]]"
	And I assign the value 2 to a variable "[[b]]"
	And I assign the value [[a]] to a variable "[[rec(1).a]]"
	And I assign the value [[b]] to a variable "[[rec(1).b]]"
	And I assign the value "=SUM([[rec(1).a]],[[rec(1).b]])" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "3"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value                                 |
	| 1 | [[a]]          =  | 1                                         |
	| 2 | [[b]]          =  | 2                                         |
	| 3 | [[rec(1).a]]   = | [[a]] = 1                                 |
	| 4 | [[rec(1).b]]   = | [[b]] = 2                                 |
	| 5 | [[Result]] =     | =SUM([[rec(1).a]],[[rec(1).b]]) ==SUM(1,2) |
	And the debug output as
	| # |                  |
	| 1 | [[a]] = 1        |
	| 2 | [[b]] = 2        |
	| 3 | [[rec(1).a]] = 1 |
	| 4 | [[rec(1).b]] = 2 |
	| 5 | [[Result]] = 3   |
#
#The following 3 Scenarios should be passed after the bug 12132 is fixed
Scenario: Evaluating recursive variable in a group calculation
	Given I assign the value 1 to a variable "[[a]]"
	And I assign the value "a" to a variable "[[b]]"
	And I assign the value "=SUM([[[[b]]]],1)" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable     | New Value                   |
	| 1 | [[a]]    =   | 1                           |
	| 2 | [[b]]    =   | a                           |
	| 3 | [[Result]] = | =SUM([[[[b]]]],1) ==SUM(1,1) |
	And the debug output as
	| # |                      |
	| 1 | [[a]]     =        1 |
	| 2 | [[b]]     =        a |
	| 3 | [[Result]]     =  2  |
#
Scenario: Evaluating recursive recordset variable in a group calculation
	Given I assign the value 1 to a variable "[[rec(1).a]]"
	And I assign the value "rec(1).a" to a variable "[[rec(1).b]]"
	And I assign the value "=[[[[rec(1).b]]]]+1" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value                               |
	| 1 | [[rec(1).a]]   = | 1                                       |
	| 2 | [[rec(1).b]]   = | rec(1).a                                |
	| 3 | [[Result]] =     | =[[[[rec(1).b]]]]+1 ==1+1 |
	And the debug output as
	| # |                         |
	| 1 | [[rec(1).a]] = 1        |
	| 2 | [[rec(1).b]] = rec(1).a |
	| 3 | [[Result]] =  2         |

Scenario: Evaluating recursive invalid recordset variable in a group calculation
	Given I assign the value 1 to a variable "[[rec(1).a]]"
	And I assign the value "rec(1).a*" to a variable "[[rec(1).b]]"
	And I assign the value "=[[[[rec(1).b]]]]+1" to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals ""
	And the execution has "AN" error
	And the debug inputs as
	| # | Variable         | New Value |
	| 1 | [[rec(1).a]]   = | 1         |
	| 2 | [[rec(1).b]]   = | rec(1).a* |
	And the debug output as
	| # |                          |
	| 1 | [[rec(1).a]] = 1         |
	| 2 | [[rec(1).b]] = rec(1).a* |

Scenario: Assign two recordset values to scalar
	Given I assign the value A to a variable "[[rec(1).a]]"	
	And I assign the value B to a variable "[[rec(2).a]]"
	And I assign the value [[rec(1).a]][[rec(2).a]] to a variable "[[Scalar]]"
	When the assign tool is executed
	Then the value of "[[Scalar]]" equals "AB"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable       | New Value                     |
	| 1 | [[rec(1).a]] = | A                             |
	| 2 | [[rec(2).a]] = | B                             |
	| 3 | [[Scalar]] =   | [[rec(1).a]][[rec(2).a]] = AB |
	And the debug output as
	| # |                  |
	| 1 | [[rec(1).a]] = A |
	| 2 | [[rec(2).a]] = B |
	| 3 | [[Scalar]] = AB  |

Scenario: Assign two recordsets and data 
	Given I assign the value 1 to a variable "[[rec(1).a]]"	
	And I assign the value 2 to a variable "[[rec(2).a]]"
	And I assign the value Test[[rec(1).a]].Warewolf[[rec(2).a]] to a variable "[[Lr(1).a]]"
	When the assign tool is executed
	Then the value of "[[Lr(1).a]]" equals "Test1.Warewolf2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable       | New Value                                               |
	| 1 | [[rec(1).a]] = | 1                                                       |
	| 2 | [[rec(2).a]] = | 2                                                       |
	| 3 | [[Lr(1).a]] =  | Test[[rec(1).a]].Warewolf[[rec(2).a]] = Test1.Warewolf2 |
	And the debug output as
	| # |                                |
	| 1 | [[rec(1).a]] = 1               |
	| 2 | [[rec(2).a]] = 2               |
	| 3 | [[Lr(1).a]]  = Test1.Warewolf2 |

Scenario: Assign two recordset with index as variable to scalr
	Given I assign the value Test to a variable "[[rec(1).test]]"	
	And I assign the value Warewolf to a variable "[[rec(2).test]]"
	And I assign the value 1 to a variable "[[a]]"
	And I assign the value 2 to a variable "[[b]]"
	And I assign the value [[rec([[a]]).test]][[rec([[b]]).test]] to a variable "[[c]]"
	When the assign tool is executed
	Then the value of "[[c]]" equals "TestWarewolf"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable          | New Value                                             |
	| 1 | [[rec(1).test]] = | Test                                                  |
	| 2 | [[rec(2).test]] = | Warewolf                                              |
	| 3 | [[a]]           = | 1                                                     |
	| 4 | [[b]]           = | 2                                                     |
	| 5 | [[c]]           = | [[rec([[a]]).test]][[rec([[b]]).test]] = TestWarewolf |
	And the debug output as
	| # |                            |
	| 1 | [[rec(1).test]] = Test     |
	| 2 | [[rec(2).test]] = Warewolf |
	| 3 | [[a]]  = 1                 |
	| 4 | [[b]]  = 2                 |
	| 5 | [[c]]  = TestWarewolf      |

Scenario: Assign two recordset with index as recordset variable to scalr
	Given I assign the value Test to a variable "[[rec(1).test]]"	
	And I assign the value Warewolf to a variable "[[rec(2).test]]"
	And I assign the value 1 to a variable "[[Index(1).a]]"
	And I assign the value 2 to a variable "[[Index(2).a]]"
	And I assign the value [[rec([[Index(1).a]]).test]][[rec([[Index(2).a]]).test]] to a variable "[[Result]]"
	When the assign tool is executed
	Then the value of "[[Result]]" equals "TestWarewolf"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable          | New Value                                                               |
	| 1 | [[rec(1).test]] = | Test                                                                    |
	| 2 | [[rec(2).test]] = | Warewolf                                                                |
	| 3 | [[Index(1).a]]  = | 1                                                                       |
	| 4 | [[Index(2).a]]  = | 2                                                                       |
	| 5 | [[Result]]      = | [[rec([[Index(1).a]]).test]][[rec([[Index(2).a]]).test]] = TestWarewolf |
	And the debug output as
	| # |                            |
	| 1 | [[rec(1).test]] = Test     |
	| 2 | [[rec(2).test]] = Warewolf |
	| 3 | [[Index(1).a]]  = 1        |
	| 4 | [[Index(2).a]]  = 2        |
	| 5 | [[Result]]  = TestWarewolf |

Scenario: Assign addition of all variables to scalar2
	Given I assign the value 1 to a variable "[[rec(1).test]]"	
	And I assign the value 2 to a variable "[[rec(2).test]]"
	And I assign the value 3 to a variable "[[rec(3).test]]"
	And I assign the value 4 to a variable "[[rec(4).test]]"
	And I assign the value 5 to a variable "[[rec(5).test]]"
	And I assign the value 6 to a variable "[[rec(6).test]]"
	And I assign the value 7 to a variable "[[rec(7).test]]"
	And I assign the value 8 to a variable "[[rec(8).test]]"
	And I assign the value 9 to a variable "[[rec(9).test]]"
	And I assign the value 10 to a variable "[[rec(10).test]]"
	And I assign the value Warewolf to a variable "[[Lr(1).a]]"
	And I assign the value [[rec(1).test]][[rec(2).test]][[rec(3).test]][[rec(4).test]][[rec(5).test]][[rec(6).test]][[rec(7).test]][[rec(8).test]][[rec(9).test]][[rec(10).test]][[Lr(1).a]] to a variable "[[new(1).a]]"
	When the assign tool is executed
	Then the execution has "NO" error
	And the debug inputs as
	| #  | Variable            | New Value                                                                                                                                                                                |
	| 1  | [[rec(1).test]]   = | 1                                                                                                                                                                                        |
	| 2  | [[rec(2).test]]   = | 2                                                                                                                                                                                        |
	| 3  | [[rec(3).test]]   = | 3                                                                                                                                                                                        |
	| 4  | [[rec(4).test]]   = | 4                                                                                                                                                                                        |
	| 5  | [[rec(5).test]]   = | 5                                                                                                                                                                                        |
	| 6  | [[rec(6).test]]   = | 6                                                                                                                                                                                        |
	| 7  | [[rec(7).test]]   = | 7                                                                                                                                                                                        |
	| 8  | [[rec(8).test]]   = | 8                                                                                                                                                                                        |
	| 9  | [[rec(9).test]]   = | 9                                                                                                                                                                                        |
	| 10 | [[rec(10).test]]  = | 10                                                                                                                                                                                       |
	| 11 | [[Lr(1).a]]    =    | Warewolf                                                                                                                                                                                 |
	| 12 | [[new(1).a]]      = | [[rec(1).test]][[rec(2).test]][[rec(3).test]][[rec(4).test]][[rec(5).test]][[rec(6).test]][[rec(7).test]][[rec(8).test]][[rec(9).test]][[rec(10).test]][[Lr(1).a]] = 12345678910Warewolf |
	And the debug output as
	| #  |                                         |
	| 1  | [[rec(1).test]]   =  1                  |
	| 2  | [[rec(2).test]]   =  2                  |
	| 3  | [[rec(3).test]]   =  3                  |
	| 4  | [[rec(4).test]]   =  4                  |
	| 5  | [[rec(5).test]]   =  5                  |
	| 6  | [[rec(6).test]]   =  6                  |
	| 7  | [[rec(7).test]]   =  7                  |
	| 8  | [[rec(8).test]]   =  8                  |
	| 9  | [[rec(9).test]]   =  9                  |
	| 10 | [[rec(10).test]]  =  10                 |
	| 11 | [[Lr(1).a]]       =  Warewolf           |
	| 12 | [[new(1).a]]      = 12345678910Warewolf |


#Scenario Outline: Assign multiple variables to the end of a recordset1
#    Given I have a variable "[[a]]" with a value '<Val1>'
#   Given I have a variable "[[b]]" with a value '<Val2>'
#   Given I have a variable "[[z]]" with a value "1"
#   Given I have a variable "[[rec(1).a]]" with a value '<Val1>'
#   Given I have a variable "[[rec(2).a]]" with a value '<Val2>'
#   Given I have a variable "[[index]]" with a value "1"
#	And I assign the value '<Value>' to a variable '<Variable>'	
#	When validating the tool
#	Then validation is '<Validation>'
#	And validation message is '<DesignValidation>'
#	When the assign tool is executed
#	And the execution has "<errorOccured>" error
#	And execution error message will be '<ExecutionError>'	
#Examples:
#      | No | Variable             | Value                               | Val1      | Val2     | Validation | DesignValidation                                                                                  | errorOccured | ExecutionError                                                                                       |
#      | 1  | [[a]]                | ""                                  | ""        | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 2  | [[rec().a]]          | ""                                  | ""        | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 3  | [[rec([[index]]).a]] | Test                                | Test      | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 4  | [[rec().[[z]]]]      | Test                                | Test      | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 5  | [[[[Mr().a]]]]       | Test                                | ""        | ""       | False      | ""                                                                                                | AN           | 1.Invalid Region [[[[Mr().a]]]]                                                                      |
#      | 6  | [[a]][[b]]           | Test                                | Te        | st       | False      | ""                                                                                                | AN           | 1.Invalid Region [[[[Mr().a]]]]                                                                      |
#      | 7  | [[mr().[[z]]]]       | Test                                | Test      | a        | True       | 'Variable'-[[mr()]] does not exist in your variable list                                          | AN           | 1.'Variable'-[[mr()]] does not exist in your variable list                                           |
#      | 8  | ""                   | Test                                | ""        | ""       | True       | 'Variable' cannot be empty                                                                        | AN           | 1.'Variable' cannot be empty                                                                         |
#      | 9  | [[rec().a b]]        | Test                                | ""        | ""       | True       | 'Variable'-Recordset field name a b contains invalid character(s)                                 | AN           | 1.'Variable'-Recordset field name a b contains invalid character(s)                                  |
#      | 10 | [[rec(**).a]]        | Test                                | ""        | ""       | True       | 'Variable'-Recordset field name (**) contains invalid character(s)                                | AN           | 1.'Variable'-Recordset field name (**) contains invalid character(s)                                 |
#      | 11 | [[=[[a]]+[[b]]]]     | Test                                | Tes       | t        | True       | 'Variable'-Recordset field name [[=+]] contains invalid character(s)                              | AN           | 1.'Variable'-Recordset field name [[=+]] contains invalid character(s)                               |
#      | 12 | [[[[a]]]]            | Test                                | rec(10).a | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 13 | [[[[rec(1).a]]]]     | Test                                | rec(10).a | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 14 | [[[[rec(1).a]]]]]    | Test                                | rec(10).a | ""       | True       | 'Variable'-Invalid expression: opening and closing brackets don't match                           | AN           | 1.'Variable' - Invalid expression: opening and closing brackets don't match                          |
#      | 15 | [[[[a]]]]]           | Test                                | rec(10).a | ""       | True       | 'Variable'-Invalid expression: opening and closing brackets don't match                           | AN           | 1.'Variable' - Invalid expression: opening and closing brackets don't match                          |
#      | 16 | [[[[[a]]]]           | Test                                | rec(10).a | ""       | True       | 'Variable'-Invalid expression: opening and closing brackets don't match                           | AN           | 1.'Variable' - Invalid expression: opening and closing brackets don't match                          |
#      | 17 | [[[[[rec(1).a]]]]    | Test                                | rec(10).a | ""       | True       | 'Variable'-Invalid expression: opening and closing brackets don't match                           | AN           | 1.'Variable' - Invalid expression: opening and closing brackets don't match                          |
#      | 18 | [[a[[b]]]]           | Test                                | rec(      | ).a      | False      | ""                                                                                                | AN           | 1.Invalid Region [[a[[b]]]]                                                                          |
#      | 19 | [[[[a]][[b]]]]       | Test                                | rec()     | .a       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 20 | [[[[c]][[d]]]]       | Test                                | ""        | ""       | False      | ""                                                                                                | AN           | 1.No Value assigned for:[[d]]                                                                        |
#      | 21 | [[]]                 | Test                                | ""        | ""       | True       | 'Variable'-Variable [[]] is missing name                                                          | AN           | 1.[[]]-Variable [[]] is missing a name                                                               |
#      | 22 | [[a1]]               | Test                                | ""        | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 23 | [[a@]]               | Test                                | ""        | ""       | True       | 'Variable'-Variable name [[a@]] contains invalid character(s)                                     | AN           | 1.'Variable'-Variable name [[a@]] contains invalid character(s)                                      |
#      | 24 | [[a b]]              | Test                                | ""        | ""       | True       | 'Variable'-Variable name [[a b]] contains invalid character(s)                                    | AN           | 1.'Variable'-Variable name [[a b]] contains invalid character(s)                                     |
#      | 25 | [[rec().a1]]         | Test                                | ""        | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 26 | [[rec().a!]]         | Test                                | ""        | ""       | False      | 'Variable'-Recordset field name a! contains invalid character(s)                                  | AN           | 1.'Variable'-Recordset field name a b contains invalid character(s)                                  |
#      | 27 | [[rec().a]]]]        | Test                                | ""        | ""       | True       | 'Variable'-Invalid expression: opening and closing brackets don't match                           | AN           | 1.'Variable'-Invalid expression: opening and closing brackets don't match                            |
#      | 28 | [[a]][[b]]]          | Test                                | ""        | ""       | True       | Invalid Region [[a]][[b]]]                                                                        | AN           | 1.Invalid Region [[a]][[b]]]                                                                         |
#      | 29 | [[a]]                | [[rec(1).a]]                        | Test      | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 30 | [[a]]                | [[rec(1).a]]Warewolf                | Test      | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 31 | [[a]]                | [[rec(1).a]]Warewolf.#$%#%6         | Test      | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 32 | [[a]]                | [[a]][[b]]                          | Test      | Warewolf | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 33 | [[a]]                | [[a]]&[[b]]                         | Test      | Warewolf | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 34 | [[a]]                | [[rec(1).a]]#$                      | Test      |          | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 35 | [[a]]                | [[mr().a]]                          | Test      |          | False      | ""                                                                                                | AN           | 1.No Value assigned for: [[mr(1).a]]                                                                 |
#      | 36 | [[a]]                | =[[a]]+[[b]]*1                      | 1         | 2        | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 37 | [[a]]                | =[[a]]+[[b]]*                       | 1         | 2        | True       | Syntax Error An error occured while parsing { [[a]]+[[b]]*} it appears to be malformed            | AN           | 1.Syntax Error An error occured while parsing { [[a]]+[[b]]*} it appears to be malformed             |
#      | 38 | [[a]]                | =cos(30)+sin(60)                    | ""        | ""       | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 39 | [[d]]                | =cos([[a]])+sin([[b]])              | 0         | 0        | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 40 | [[d]]                | =cos([[a]]])+sin([[b]])             | 0         | 0        | True       | Syntax Error An error occured while parsing { cos([[a]]])+sin([[b]]) } it appears to be malformed | AN           | 1. Syntax Error An error occured while parsing { cos([[a]]])+sin([[b]]) } it appears to be malformed |
#      | 41 | [[d]]                | =cos([[a]])+sin([[b%]])             | 0         | 0        | True       | The-Variable name [[b%]] contains invalid character(s)                                            | AN           | 1. The-Variable name [[b%]] contains invalid character(s)                                            |
#      | 42 | [[New().a]]]]        | =cos([[a]])+sin([[rec().a])         | 45        | 0        | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 43 | [[New().a]]]]        | =cos([[a]])+sin([[rec().a.]])       | 45        | 0        | True       | The-Recordset name [[rec().a]] contains invalid character(s)                                      | AN           | 1.The-Recordset name [[rec().a]] contains invalid character(s)                                       |
#      | 44 | [[a]]                | =cos([[a]])+sin([[rec().[[a]]]]])   | 45        | 0        | True       | Synat Error An error occured while parsing { cos([[a]])+sin([[rec().[[a]]]]])                     | AN           | 1.Synat Error An error occured while parsing { cos([[a]])+sin([[rec().[[a]]]]])                      |
#      | 45 | [[a]]                | =cos([[a]])+sin([[rec().[[a]]])     | 45        | 0        | True       | The-Recordset field name ])!~~calculation~! contains invalid character(s)                         | AN           | 1.The-Recordset field name ])!~~calculation~! contains invalid character(s)                          |
#      | 46 | [[a]]                | =[[rec().a]]++[[a]]                 | 45        | 0        | True       | Syntax Error An error occured while parsing { [[rec().a]]++[[a]] it appears to be malformed       | AN           | 1.Syntax Error An error occured while parsing { [[rec().a]]++[[a]] it appears to be malformed        |
#      | 47 | [[a]]                | =[[rec().a]]+[[a]]                  | 45        | 0        | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 48 | [[rec().a]]+[[a]]    | 10                                  | 0         | 0        | True       | 'Variable'-Invalid Region [[rec().a]]+[[a]]                                                       | AN           | 1.'Variable'-Invalid Region [[rec().a]]+[[a]]                                                        |
#      | 49 | [[rec().a]]          | =(2+3)&                             | 0         | 0        | True       | Syntax Error An error occured while parsing(2+3)& it appears to be malformed                      | AN           | 1. Syntax Error An error occured while parsing (2+3)& it appears to be malformed                     |
#      | 50 | [[z]]                | =[[a]]+[[b]]/([[rec().[[a]]]]+123)  | 0         | 0        | False      | ""                                                                                                | NO           | ""                                                                                                   |
#      | 51 | [[z]]                | =[[a]]+[[b]]/([[[rec().[[a]]]]+123) | 0         | 0        | True       | The-Recordset name [[[rec]] contains invalid character(s)                                         | AN           | 1.The-Recordset name [[[rec]] contains invalid character(s)                                          |


Scenario: Assign a variable to another variable
	Given I assign the value a to a variable "[[x]]"	
	And I assign the value x to a variable "[[b]]"
	And I assign the value [[[[b]]]] to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals a
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable   | New Value     |
	| 1 | [[x]]  =   | a             |
	| 2 | [[b]] =    | x             |
	| 3 | [[var]]  = | [[[[b]]]] = a |
	And the debug output as
    | # |             |
    | 1 | [[x]] = a   |
    | 2 | [[b]] = x   |
    | 3 | [[var]] = a |


#Wolf - 485
#Scenario: Assign values to recordsets
#	Given I assign the value 1 to a variable "[[AB().a]]"	
#	And I assign the value a to a variable "[[CD().a]]"
#	And I assign the value b to a variable "[[CD().a]]"
#	And I assign the value 2 to a variable "[[AB().a]]"	
#	When the assign tool is executed
#	Then the value of "[[AB(2).a]]" equals 2
#	And the execution has "NO" error
#	And the debug inputs as
#	| # | Variable      | New Value |
#	| 1 | [[AB().a]]  = | 1         |
#	| 2 | [[CD().a]]  = | a         |
#	| 3 | [[CD().a]]  = | b         |
#	| 4 | [[AB().a]]  = | 2         |
#	And the debug output as
#    | # |                 |
#    | 1 | [[AB(1).a]] = 1 |
#    | 2 | [[CD(1).a]] = a |
#    | 3 | [[CD(2).a]] = b |
#    | 4 | [[AB(2).a]] = 2 |

























