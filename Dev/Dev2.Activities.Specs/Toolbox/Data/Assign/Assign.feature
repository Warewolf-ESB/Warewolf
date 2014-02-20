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
	| # | Variable   | New Value  |
	| 1 | [[var]]  = | 20         |
	| 2 | [[test]] = | 60         |
	| 3 | [[var]]  = | [[test]] = |
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
	| # | Variable   | New Value          |
	| 1 | [[var]]  = | SUM(1,2,3)-5       |
	| 2 | [[test]] = | =[[var]]  = String |  
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
	| # | Variable    | New Value         |
	| 1 | [[var]]   = | Hello             |
	| 2 | [[test]]  = | World             |
	| 3 | [[value]] = | [[var]][[test]] = |
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
	| # | Variable         | New Value                    |
	| 1 | [[var]]        = | Hello                        |
	| 2 | [[rec(1).set]] = | World                        |
	| 3 | [[value]]      = | [[var]] [[rec(1).set]] ! = ! |
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
	| # | Variable        | New Value        |
	| 1 | [[rec().set]] = | 10               |
	| 2 | [[rec().set]] = | 20               |
	| 3 | [[rec().set]] = | 30               |
	| 4 | [[value]]     = | [[rec(3).set]] = |
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
	| # | Variable       | New Value |
	| 1 | [[rec(1).set]] = | 10        |
	| 2 | [[rec(2).set]] = | 20        |
	| 3 | [[rec(3).set]] = | 30        |
	| 4 | [[rec(*).set]] = | " "       |
	And the debug output as
    | # |                     |
    | 1 | [[rec(1).set]] = 10 |
    | 2 | [[rec(2).set]] = 20 |
    | 3 | [[rec(3).set]] = 30 |
    | 4 | [[rec(1).set]] = "" |
    |   | [[rec(2).set]] = "" |
    |   | [[rec(3).set]] = "" |

Scenario: Assign a record set to a scalar
	Given I assign the value 10 to a variable "[[rec(1).set]]"	
	And I assign the value 20 to a variable "[[rec(2).set]]"
	And I assign the value 30 to a variable "[[rec(3).set]]"
	And I assign the value "[[rec(*).set]]" to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals "30"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable         | New Value        |
	| 1 | [[rec(1).set]] = | 10               |
	| 2 | [[rec(2).set]] = | 20               |
	| 3 | [[rec(3).set]] = | 30               |
	| 4 | [[var]]        = | [[rec(1).set]] = |
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
	| # | Variable        | New Value  |
	| 1 | [[var]]       = | 30         |
	| 2 | [[rec().set]] = | [[var]]  = |
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
	| 2 | [[Result]] = | =30-[[var]] = String |
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
	| # | Variable     | New Value                        |
	| 1 | [[var1]]   = | 30                               |
	| 2 | [[var2]]   = | 30                               |
	| 3 | [[Result]] = | =SUM([[var1]],[[var2]]) = String |
	And the debug output as
	| # |                |
	| 1 | [[var1]] = 30    |
	| 2 | [[var2]] = 30    |
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
	| # | Variable        | New Value        |
	| 1 | [[rec().set]] = | 10               |
	| 2 | [[rec().set]] = | 20               |
	| 3 | [[rec().set]] = | 30               |
	| 4 | [[des().val]] = | [[rec(3).set]] = |
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
	|#||

Scenario: Assign to a negative recordset index
	Given I assign the value 10 to a variable "[[des(-1).val]]"
	When the assign tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| # | Variable          | New Value |
	| 1 | [[des(-1).val]] = | 10        |	
	And the debug output as
	| # |                      |
