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

Scenario: Assign scalars to 
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
    | # |                 |
    | 1 | [[rec(1).a]] = 10 |
    | 2 | [[rec(1).b]] = 20 |
    | 3 | [[rec(1).c]] = 30 |
    | 4 | [[d]] = 10       |
    | 5 | [[e]] = 20       |
    | 6 | [[f]] = 30       |

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
	| 5 | [[rs(1).set]] = Hello | [[rec(1).set = 10   |
	|   |                       | [[rec(2).set]] = 20 |
	|   |                       | [[rec(3).set]] = 30 |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[rs(1).val]] = 10 |
    |   | [[rs(2).val]] = 20 |
    |   | [[rs(3).val]] = 30 |

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
	| 1 | [[des(-1).val]] = | 10        |	
	And the debug output as
	| # |                      |
	
#
#Scenario Outline: Assign to a invalid variable
#   Given I assign the value 10 to a variable '<var>'
#   When the assign tool is executed
#   Then the execution has "AN" error
#   And the debug inputs as
#	| # | Variable | New Value |
#	| 1 | <var> =  | 10        |
#   And the debug output as
#   | # |         |
#   |   | <error> |
#   Examples:
#   | no | var                                       | error                                                                                                                                                                                                                                                   |
#   | 1  | [[rec().a]]=]]                            | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#   | 2  | [[rec'()'.a]]                             | Recordset name [[rec'()']] contains invalid character(s)                                                                                                                                                                                                |
#   | 3  | [[rec"()".a]]                             | Recordset name [[rec"()"]] contains invalid character(s)                                                                                                                                                                                                |
#   | 4  | [[rec".a]]                                | Variable name [[rec".a]] contains invalid character(s)                                                                                                                                                                                                  |
#   | 5  | [[rec.a]]                                 | Variable name [[rec.a]]  contains invalid character(s)                                                                                                                                                                                                  |
#   | 6  | [[rec()*.a]]                              | Variable name [[rec()*.a]] contains invalid character(s)                                                                                                                                                                                                |
#   | 7  | [[rec().a]].[[a]]                         | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 8  | [[rec().a]][[a]]                          | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 9  | [[rec().a]]*                              | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 10 | [[rec().a]] a                             | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 11 | [[1]]                                     | Variable name [[1]] begins with a number                                                                                                                                                                                                                |
#   | 12 | [[@]]                                     | Variable name [[@]] contains invalid character(s)                                                                                                                                                                                                       |
#   | 13 | [[var#]]                                  | Variable name [[var#]] contains invalid character(s)                                                                                                                                                                                                    |
#   | 14 | [[var]]00]]                               | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#   | 15 | [[var]]@]]                                | Invalid region detected: A close ]] without a related open [[                                                                                                                                                                                           |
#   | 16 | [[var.()]]                                | Variable name [[var.()]] contains invalid character(s)                                                                                                                                                                                                  |
#   | 17 | [[]]                                      | Variable [[]] is missing a name                                                                                                                                                                                                                         |
#   | 18 | [[()]]                                    | Variable name [[()]] contains invalid character(s)                                                                                                                                                                                                      |
#   | 19 | [[var[[a]]]]                              | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 20 | [[var[[]]                                 | Invalid region detected: An open [[ without a related close ]]                                                                                                                                                                                          |
#   | 21 | [[var1.a]]                                | Variable name [[var1.a]] contains invalid character(s)                                                                                                                                                                                                  |
#   | 22 | [[rec()!a]]                               | Recordset name [[rec()!a]] contains invalid character(s)                                                                                                                                                                                                |
#   | 23 | [[rec()         a]]                       | Recordset name [[rec()         a]] contains invalid character(s)                                                                                                                                                                                        |
#   | 24 | [[{{rec(_).a}}]]]                         | Recordset name [[{{rec]] contains invalid character(s)                                                                                                                                                                                                  |
#   | 25 | [[rec(23).[[var*]]]]                      | Variable name [[var*]] contains invalid character(s)                                                                                                                                                                                                    |
#   | 26 | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | Recordset index (q) contains invalid character(s)  /n  Recordset name [[r()..]] contains invalid character(s)  /n  Variable name [[r"]] contains invalid character(s)  /n Variable [[]] is missing a name  /n  Variable name [[1]] begins with a number |
#   | 27 | [[rec().a]]&[[a]]                         | One variable only allowed in the output field                                                                                                                                                                                                           |
#   | 28 | a[[rec([[[[b]]]]).a]]@                    | Variable name a[[rec([[[[b]]]]).a]]@  contains invalid character(s)                                                                                                                                                                                     |
#   | 29 | [[var  ]]                                 | Variable name [[var  ]] contains invalid character(s)                                                                                                                                                                                                   |
#   | 29 | [[rec()                                   | UPPER                                                                                                                                                                                                                                                   | Recordset variable that needs a field name(s) ||
#











