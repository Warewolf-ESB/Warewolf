@Utility
Feature: Calculate
	In order to perform basic calculations
	As a Warewolf user
	I want a tool that I can input a formula and will calculate and retun a result

##Calculate using a given formula
##Calculate using multiple scalars and recordset inputs
##Calculate with new lines should concatenate values
##Calculate using Recordset (*) input in an agregate function like SUM
##Calculate using incorrect formula
##Calculate using incorrect argument expression for formula 
##Calculate using variable as full calculation
##Calculate using a negative index recordset value
##Calculate using isnumber and blank
##Calculate Assign by evaluating a variable inside a variable
##Calculate Assign by evaluating a variable inside a variable with function
##Calculate Assign by evaluating variables with functions
##Variable that does not exist
##Calculate using Recordset ([[val]]) input in an agregate function like SUM


Scenario: Calculate using a given formula
	Given I have the formula "mod(sqrt(49), 7)"	
	When the calculate tool is executed
	Then the calculate result should be "0"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =             |
	| mod(sqrt(49), 7) |	
	And the debug output as 
	|                 |
	| [[result]] = 0 |

Scenario: Calculate using multiple scalars and recordset inputs
	Given I have a calculate variable "[[var]]" equal to "1"
	And I have a calculate variable "[[var2]]" equal to "20"
	And I have the formula "((([[var]]+[[var]])/[[var2]])+[[var2]]*[[var]])"
	When the calculate tool is executed
	Then the calculate result should be "20.1"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =                                                                 |
	| ((([[var]]+[[var]])/[[var2]])+[[var2]]*[[var]]) = (((1+1)/20)+20*1) |	
	And the debug output as 
	|                   |
	| [[result]] = 20.1 |

Scenario: Calculate with new lines should concatenate values
	Given I have a calculate variable "[[var]]" equal to "1"
	And I have a calculate variable "[[var2]]" equal to "20"
	And I have the formula "[[var]]\r\n[[var2]]"
	When the calculate tool is executed
	Then the calculate result should be "120"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =            |
	| String = String |	
	And the debug output as 
	|                  |
	| [[result]] = 120 |

Scenario: Calculate using Recordset (*) input in an agregate function like SUM
	Given I have a calculate variable "[[var().int]]" equal to 
	| var().int	|
	| 1			|
	| 2			|
	| 3			|
	And I have the formula "SUM([[var(*).int]])"
	When the calculate tool is executed
	Then the calculate result should be "3"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =                         |
	| SUM([[var(*).int]]) = SUM(1) |
	| SUM([[var(*).int]]) = SUM(2) |
	| SUM([[var(*).int]]) = SUM(3) |
	And the debug output as 
	|                |
	| [[result]] = 3 |

Scenario: Calculate using Recordset (*) input in an agregate function like SUM and output recordset star
	Given I have a calculate variable "[[var().int]]" equal to 
	| var().int	|
	| 1			|
	| 2			|
	| 3			|
	And I have a calculate variable "[[rs().val]]" equal to 
	| rs().val |
	| 10       |
	| 23       |
	And I have the formula "SUM([[var(*).int]])"
	And calculate result as "[[rs(*).val]]"
	When the calculate tool is executed
	Then the execution has "NO" error
	And the debug inputs as  
	| fx =                         |
	| SUM([[var(*).int]]) = SUM(1) |
	| SUM([[var(*).int]]) = SUM(2) |
	| SUM([[var(*).int]]) = SUM(3) |
	And the debug output as 
	|                   |
	| [[rs(1).val]] = 1 |
	| [[rs(2).val]] = 2 |
	| [[rs(3).val]] = 3 |

Scenario: Calculate using Recordset (*) input in an agregate function like SUM and output recordset star complex
	Given I have a calculate variable "[[var().int]]" equal to 
	| var().int	|
	| 1			|
	| 2			|
	| 3			|
	And I have a calculate variable "[[rs().val]]" equal to 
	| rs().val |
	| 10       |
	| 23       |
	And I have the formula "SUM([[var(*).int]]) + 15"
	And calculate result as "[[rs(*).val]]"
	When the calculate tool is executed
	Then the execution has "NO" error
	And the debug inputs as  
	| fx =                                   |
	| SUM([[var(*).int]]) + 15 = SUM(1) + 15 |
	| SUM([[var(*).int]]) + 15 = SUM(2) + 15 |
	| SUM([[var(*).int]]) + 15 = SUM(3) + 15 |
	And the debug output as 
	|                    |
	| [[rs(1).val]] = 16 |
	| [[rs(2).val]] = 17 |
	| [[rs(3).val]] = 18 |

Scenario: Calculate using incorrect formula
	Given I have the formula "asdf"
	When the calculate tool is executed
	Then the calculate result should be ""
	And the execution has "Formula syntax error. Unable to compile the formula: Unexpected end of file, on line: 1 column: 5" error
	And the debug inputs as  
	| fx = |
	| asdf |	
	And the debug output as 
	|               |
	| [[result]] = |


Scenario: Calculate using variable as full calculation
	Given I have a calculate variable "[[var]]" equal to "SUM(1,2,3)-5"
	And I have the formula "[[var]]"
	When the calculate tool is executed
	Then the calculate result should be "1"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =                    |
	| [[var]] =  SUM(1,2,3)-5 |	
	And the debug output as 
	|                 |
	| [[result]] = 1 |

Scenario: Calculate using a negative index recordset value
	Given I have the formula "[[my(-1).formula]]"
	When the calculate tool is executed
	Then the execution has "Recordset index [ -1 ] is not greater than zero" error
	And the debug inputs as  
	| fx =                 |
	| [[my(-1).formula]] = |	
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Calculate using isnumber and blank
    Given I have the formula "if(isnumber(""),"Is number","Not number")"
	When the calculate tool is executed
	Then the calculate result should be "Not number"
	And the debug inputs as  
	| fx =                                    |
	| "if(isnumber(""),"Is number","Not number")" |	
	And the execution has "NO" error

#This scenario should pass after the bug 11871 is fixed
Scenario: Calculate Assign by evaluating a variable inside a variable
	Given I have a calculate variable "[[a]]" equal to "b"
	And I have a calculate variable "[[b]]" equal to "20"
	And I have the formula "[[[[a]]]]+1"
	When the calculate tool is executed
	Then the calculate result should be "21"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =               |
	| [[b]]+1 = 20+1 |	
	And the debug output as 
	|                 |
	| [[result]] = 21 |

Scenario: Calculate Assign by evaluating a variable inside a variable with function
	Given I have a calculate variable "[[a]]" equal to "b"
	And I have a calculate variable "[[b]]" equal to "20"
	And I have the formula "SUM([[[[a]]]],[[b]])"
	When the calculate tool is executed
	Then the calculate result should be "40"
	And the execution has "NO" error
	And the debug inputs as  
	| fx =                       |
	| SUM([[b]],20) = SUM(20,20) |
	And the debug output as 
	|                 |
	| [[result]] = 40 |

Scenario Outline: Calculate Assign by evaluating variables with functions
	Given I have a calculate variable "[[x]]" equal to "1"
	And I have a calculate variable "[[y]]" equal to "2"
	And I have a calculate variable "[[z]]" equal to "10"
	And I have a calculate variable "[[rc(1).set]]" equal to "5"
	And I have a calculate variable "[[s]]" equal to "-1"
	And I have a calculate variable "[[t]]" equal to "0"
	And I have a calculate variable "[[a]]" equal to "b"
	And I have a calculate variable "[[b]]" equal to "3"
	And I have a calculate variable "[[e]]" equal to "1000"
	And I have the Example formula "<fx>"
	When the calculate tool is executed
	Then the calculate result should be "<result>"
	And the execution has "NO" error
	Examples: 
	| No                  | fx                                                         | result                   |
	| 1                   | abs([[e]])                                                 | 1000                     |
	| 2                   | acos([[x]])                                                | 0                        |
	| 3                   | acosh([[rc(1).set]])                                       | 2.29243166956118         |
	| 4                   | AND([[s]]<[[z]]<5)                                         | False                    |
	| 5                   | AND([[y]]<5)                                               | True                     |
	| 7                   | ASIN([[s]])                                                | -1.57079632679490        |
	| 8                   | ASINH([[rc(1).set]])                                       | 2.312438341272750        |
	| 9                   | ATAN([[s]])                                                | -0.785398163397448       |
	| 10                  | ATAN2([[x]],[[s]])                                         | -0.785398163397448       |
	| 11                  | ATANH([[t]])                                               | 0                        |
	| 12                  | AVEDEV([[x]],[[z]])                                        | 4.5                      |
	| 13                  | AVERAGE([[x]],[[y]],[[z]],[[rc(1).set]],[[s]],[[t]],[[e]]) | 145.285714285714         |
	| 14                  | BIN2DEC([[z]])                                             | 2                        |
	| 15                  | BIN2HEX([[x]])                                             | 1                        |
	| 16                  | BIN2OCT([[z]])                                             | 2                        |
	| 17                  | CEILING([[z]],[[y]])                                       | 10                       |
	| 18                  | char([[x]][[t]][[x]])                                      | e                        |
	| 19                  | CHOOSE(4,[[z]],[[t]],[[rc(1).set]],100)                    | 100                      |
	| 20                  | CODE(111)                                                  | 49                       |
	| 21                  | COMBIN([[z]],[[y]])                                        | 45                       |
	| 22                  | COMPLEX([[y]],[[e]])                                       | 2+1000i                  |
	| 23                  | CONCATENATE([[z]],[[x]])                                   | 101                      |
	| 24                  | CONVERT([[z]],"m","in")                                    | 393.700787401575         |
	| 25                  | COS([[[[a]]]])                                             | -0.989992496600445       |
	| 26                  | COSH([[z]])                                                | 11013.232920103300       |
	| 27                  | COUNT([[x]],[[y]],[[[[a]]]])                               | 3                        |
	| 29                  | DATE(2000,[[y]],12)                                        | 2/12/2000 12:00:00.000 AM|
	| 30                  | DAY([[x]])                                                 | 1                        |
	| 31                  | DAYS360([[x]],[[y]])                                       | 1                        |
	| 32                  | DB([[e]],[[z]],12,12,12)                                   | 4.66024676978963         |
	| 33                  | DBNull()                                                   |                          |
	| 34                  | DDB([[e]],[[z]],12,12,[[x]])                               | 31.9996025467397         |
	| 35                  | DEC2BIN([[x]],[[z]])                                       | 0000000001               |
	| 36                  | DEC2HEX(1,10)                                              | 0000000001               |
	| 37                  | DEC2OCT([[y]],[[z]])                                       | 0000000002               |
	| 38                  | DEGREES([[z]])                                             | 572.957795130823         |
	| 39                  | DELTA([[t]],[[x]])                                         | 0                        |
	| 40                  | DOLLARDE([[z]],[[y]])                                      | 10                       |
	| 41                  | DOLLARFR([[x]],[[z]])                                      | 1                        |
	| 42                  | EDATE([[z]],[[[[a]]]])                                     | 4/10/1900 12:00:00.000 AM|
	| 43                  | EOMONTH([[z]],[[[[a]]]])                                   | 4/30/1900 12:00:00.000 AM|
#	| 44                  | ERROR.TYPE(#NUM!)                                          | 6.00                     |
	| 45                  | EVEN([[y]])                                                | 2                        |
	| 46                  | EXP([[x]])                                                 | 2.71828182845905         |
	| 47                  | FACT([[z]])                                                | 3628800                  |
	| 48                  | FIND([[t]],[[e]])                                          | 2                        |
	| 49                  | FACTDOUBLE([[z]])                                          | 3840                     |
	| 50                  | FLOOR([[z]],[[z]])                                         | 10                       |
	| 51                  | FV(10,12,100,10,1)                                         | -376611405206410         |
	| 52                  | GCD([[x]],[[z]])                                           | 1                        |
	| 53                  | GESTEP([[[[a]]]],[[s]])                                    | 1                        |
	| 54                  | HEX2BIN([[y]],[[y]])                                       | 10                       |
	| 55                  | HEX2DEC([[rc(1).set]])                                     | 5                        |
	| 56                  | HEX2OCT([[rc(1).set]])                                     | 5                        |
	| 57                  | IF([[z]],[[rc(1).set]])                                    | 5                        |
	| 58                  | IFERROR([[t]],[[x]])                                       | 0                        |
	| 59                  | IMABS([[x]])                                               | 1                        |
	| 60                  | IMAGINARY([[rc(1).set]])                                   | 0                        |
	| 61                  | IMARGUMENT([[e]])                                          | 0                        |
	| 62                  | IMCONJUGATE([[[[a]]]])                                     | 3                        |
	| 63                  | IMCOS([[y]])                                               | -0.416146836547142       |
	| 64                  | IMDIV(1,10)                                                | 0.1                      |
	| 65                  | IMEXP([[rc(1).set]])                                       | 148.413159102577         |
	| 66                  | IMLN([[[[a]]]])                                            | 1.09861228866811         |
	| 67                  | IMLOG10([[y]])                                             | 0.301029995663981        |
	| 68                  | IMLOG2([[y]])                                              | 1                        |
	| 69                  | IMPOWER(2,[[z]])                                           | 1024                     |
	| 70                  | IMPRODUCT([[x]],[[y]])                                     | 2                        |
	| 71                  | IMREAL([[z]])                                              | 10                       |
	| 72                  | IMSIN([[x]])                                               | 0.841470984807897        |
	| 73                  | IMSQRT([[e]])                                              | 31.6227766016838         |
	| 74                  | IMSUB([[x]],[[rc(1).set]])                                 | -4                       |
	| 75                  | IMSUM([[x]],[[rc(1).set]])                                 | 6                        |
	| 77                  | INT([[s]])                                                 | -1                       |
	| 78                  | INTRATE(2015,2030,1000,1,4)                                | -23.976                  |
	| 79                  | IPMT(5,12,100,1000,2000,1)                                 | -833.333358764648        |
	#| 80                  | IRR([[z]],[[rc(1).set]],2)                                 | -1.5                    |
	| 81                  | isdbnull([[x]])                                            | False                    |
	| 82                  | ISBLANK(1)                                                 | False                    |
	| 83                  | ISERR([[e]])                                               | False                    |
	| 84                  | ISERROR([[rc(1).set]])                                     | False                    |
	| 85                  | ISEVEN([[z]])                                              | True                     |
	| 86                  | ISLOGICAL([[x]])                                           | False                    |
	| 87                  | ISNA([[t]])                                                | False                    |
	| 88                  | ISNONTEXT([[[[a]]]])                                       | True                     |
	| 89                  | ISNUMBER([[x]])                                            | True                     |
	| 90                  | ISODD([[[[a]]]])                                           | True                     |
	| 91                  | ISREF([[t]])                                               | False                    |
	| 92                  | ISTEXT([[rc(1).set]])                                      | False                    |
	| 93                  | LCM(1,2,10)                                                | 10                       |
	| 94                  | LEFT([[x]],[[e]])                                          | 1                        |
	| 95                  | LEN([[z]])                                                 | 2                        |
	| 96                  | LN([[rc(1).set]])                                          | 1.6094379124341          |
	| 97                  | LOG([[z]],[[y]])                                           | 3.32192809488736         |
	| 98                  | LOG10([[z]])                                               | 1                        |
	| 99                  | LOWER([[rc(1).set]])                                       | 5                        |
	| 100                 | MAX(10,1,1000)                                             | 1000                     |
	| 101                 | MEDIAN(10,1,1000)                                          | 10                       |
	| 102                 | MID(10,1,1000)                                             | 10                       |
	| 103                 | MIN(10,1,1000)                                             | 1                        |
	| 104                 | MINUTE([[[[a]]]])                                          | 0                        |
	| 105                 | MOD([[b]],[[rc(1).set]])                                   | 3                        |
	| 106                 | MONTH([[z]])                                               | 1                        |
	| 107                 | MROUND([[x]],[[y]])                                        | 2                        |
	| 108                 | MULTINOMIAL(10,[[x]],2)                                    | 858                      |
	| 109                 | N([[s]])                                                   | -1                       |
 # move to alternate | 110                                                        | NA()                     | #N/A |
	| 111                 | NETWORKDAYS(2014,2015)                                     | 2                        |
	| 112                 | NOT([[[[a]]]])                                             | False                    |
	| 113                 | NOW()                                                      | [Now]                    |
	| 114                 | NPER(0.1, 100, 1000, 999,0)                                | -79.74911468163210       |
	| 115                 | NPV([[z]],[[z]],[[rc(1).set]],[[y]])                       | 0.951915852742299        |
 #                   | 116                                                        | NULL()                   |      |
	| 117                 | OCT2BIN([[z]],[[z]])                                       | 0000001000               |
	| 118                 | OCT2DEC([[z]])                                             | 8                        |
	| 119                 | OCT2HEX(10,2)                                              | 08                       |
	| 120                 | ODD([[z]])                                                 | 11                       |
	| 121                 | OR(1,2)                                                    | True                     |
	| 122                 | PI()                                                       | 3.14159265358979         |
	| 123                 | PMT([[z]],[[x]],100,200,[[t]])                             | -1300                    |
	| 124                 | POWER([[z]],[[y]])                                         | 100                      |
	| 125                 | PPMT(10,1,1,1000,500,0)                                    | -1500                    |
	| 126                 | PRODUCT([[x]],[[y]],[[z]])                                 | 20                       |
	| 127                 | PV(5,1,10,100,0)                                           | -18.3333333333333        |
	| 128                 | QUOTIENT([[z]],[[rc(1).set]])                              | 2                        |
	| 129                 | RADIANS([[z]])                                             | 0.174532925199433        |
	| 130                 | RANDBETWEEN([[x]],10)                                      | [Int]                    |
	| 131                 | RATE(360,-600,100000,0,1)                                  | 0.00504500404584643      |
	| 132                 | REPT([[y]],[[y]])                                          | 22                       |
	| 133                 | RIGHT([[x]],[[x]])                                         | 1                        |
	| 134                 | ROMAN(10,0)                                                | X                        |
	| 135                 | ROUND([[rc(1).set]],3)                                     | 5                        |
	| 136                 | ROUNDDOWN([[rc(1).set]],3)                                 | 5                        |
	| 137                 | ROUNDUP(5,2)                                               | 5                        |
	| 138                 | SEARCH(1,[[x]],1)                                          | 1                        |
	| 139                 | SEARCHB(1,1,1)                                             | 1                        |
	| 140                 | SECOND([[e]])                                              | 0                        |
	| 141                 | SERIESSUM(1000,10,10,10)                                   | 1E+31                    |
	| 142                 | SIGN(50)                                                   | 1                        |
	| 143                 | SIN([[z]])                                                 | -0.54402111088937        |
	| 144                 | SINH([[z]])                                                | 11013.2328747034         |
	| 145                 | SLN(100,10,[[x]])                                          | 90                       |
	| 146                 | SQRT(16)                                                   | 4                        |
	| 147                 | SQRTPI(16)                                                 | 7.08981540362206         |
	| 148                 | STDEV(1,10,5)                                              | 4.50924975282289         |
	| 149                 | SUBTOTAL(2,[[x]],[[z]],0)                                  | 3                        |
	| 150                 | SUM([[rc(1).set]],[[z]],[[s]])                             | 14                       |
	| 151                 | SYD(1000,[[z]],[[rc(1).set]],[[x]])                        | 330                      |
	| 152                 | TAN([[z]])                                                 | 0.648360827459087        |
	| 153                 | TANH([[z]])                                                | 0.999999995877693        |
	| 154                 | TEXT([[y]],[[rc(1).set]])                                  | 5                        |
	| 155                 | TIME(24,[[x]],[[x]])                                       | 0.000706018518518518     |
	| 156                 | TIMEVALUE("2:24 AM")                                       | 0.1                      |
	| 157                 | TODAY()                                                    | [Today]                  |
	| 158                 | TRIM(10)                                                   | 10                       |
	| 159                 | TRUNC(1000,[[rc(1).set]])                                  | 1000                     |
	| 160                 | TYPE(-1)                                                   | 1                        |
	| 161                 | UPPER(-1)                                                  | -1                       |
	| 162                 | VALUE([[x]])                                               | 1                        |
	| 163                 | VAR([[rc(1).set]],[[z]])                                   | 12.5                     |
	| 164                 | WEEKDAY(11011,[[x]])                                       | 7                        |
	| 165                 | WEEKNUM(11011,[[y]])                                       | 8                        |
	| 166                 | WORKDAY([[rc(1).set]],[[rc(1).set]],[[z]])                 | 1/15/1900 12:00:00.000 AM|
	| 167                 | YEAR(11011)                                                | 1930                     |
#	| 168                 | FALSE                                                      | FALSE                    |
#	| 169                 | TRUE                                                       | TRUE                     |

Scenario Outline: Calculate using Recordset input in aggregate functions like SUM
	Given I have a calculate variable "[[var().int]]" equal to 
	| var().int	|
	| 1			|
	| 2			|
	| 3			|
	And I have a calculate variable "[[val]]" equal to "3"
	And I have the formula "<fx>"
	When the calculate tool is executed
	Then the calculate result should be "<value>"
	And the execution has "NO" error
	Examples: 
		| No | fx                                             |  value |
		| 1  | SUM([[var([[val]]).int]])                      |  3     |
		| 2  | SUM([[var([[val]]).int]],[[var([[val]]).int]]) |  6     |
		| 3  | SUM([[var([[val]]).int]],[[var([[val]]).int]]) |  6     |

Scenario: Calculate using variables with a null value
	Given I have a calculate variable "[[a]]" equal to "NULL"
	And I have a calculate variable "[[b]]" equal to "NULL"
	And I have the formula "SUM([[a]],[[b]])"
	When the calculate tool is executed
	Then the execution has "An" error
	And the debug inputs as  
	| fx =                       |
	| SUM([[a]],[[b]]) = SUM(,) |

Scenario: Variable that does not exist
	Given I have a calculate variable "[[a]]" equal to "1"
	And I have a calculate variable "[[b]]" equal to "20"
	And I have the formula "Sum([[a]],[[b]],[[c]])"
	When the calculate tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| fx =                             |
	| SUM([[var().int]]) = SUM(3) |	
		And the debug output as 
	|                |
	| [[rs().a]] = 3 |
Scenario: Calculate using variables with a no existent value
	Given I have the formula "SUM([[a]],[[b]])"
	When the calculate tool is executed
	Then the execution has "An" error
	And the debug inputs as  
	| fx =                       |
	| SUM([[a]],[[b]]) =  |

